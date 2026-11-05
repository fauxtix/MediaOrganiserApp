#!/bin/bash
# ──────────────────────────────────────────────────────────────────
# db-init: prepara a base de dados do contentor SQL.
#  1. espera pelo SQL Server;
#  2. restaura Publicacoes_Prod a partir do .bak indicado (se não existir);
#  3. normaliza ApplicationSettings para o ambiente Docker
#     (caminhos com '/' e URLs internas dos serviços).
# Idempotente: se a base de dados já existir, só reaplica os settings.
# ──────────────────────────────────────────────────────────────────
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
SERVER=sql
USER=sa
DB=Publicacoes_Prod
BACKUP_DIR=${SQL_BACKUP_DIR:-/backup}
BACKUP_FILE=${SQL_BACKUP_FILE:-}

if [ -z "${SQL_SA_PASSWORD:-}" ]; then
    echo "[db-init] SQL_SA_PASSWORD não está definida" >&2
    exit 1
fi

run_sql() {
    "$SQLCMD" -S "$SERVER" -U "$USER" -P "$SQL_SA_PASSWORD" -C -b -l 15 "$@"
}

echo "[db-init] à espera do SQL Server em $SERVER..."
ready=0
for _ in $(seq 1 60); do
    if run_sql -Q "SELECT 1" >/dev/null 2>&1; then
        ready=1
        break
    fi
    sleep 2
done
if [ "$ready" -ne 1 ]; then
    echo "[db-init] o SQL Server não ficou disponível a tempo" >&2
    exit 1
fi
echo "[db-init] SQL Server disponível."

exists=$(run_sql -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'$DB'" | tr -d '[:space:]')
if [ "$exists" = "1" ]; then
    echo "[db-init] a base de dados $DB já existe; restore ignorado."
else
    if [ -z "$BACKUP_FILE" ]; then
        echo "[db-init] $DB não existe e SQL_BACKUP_FILE está vazio" >&2
        exit 1
    fi
    if [ ! -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
        echo "[db-init] ficheiro de backup não encontrado: $BACKUP_DIR/$BACKUP_FILE" >&2
        echo "[db-init] define SQL_BACKUP_FILE com um ficheiro de Web.Blazor/wwwroot/Files/Backup/Database" >&2
        exit 1
    fi

    echo "[db-init] a restaurar $BACKUP_FILE para $DB..."
    run_sql -Q "
        RESTORE DATABASE [$DB]
        FROM DISK = N'$BACKUP_DIR/$BACKUP_FILE'
        WITH MOVE N'$DB'     TO N'/var/opt/mssql/data/$DB.mdf',
             MOVE N'${DB}_log' TO N'/var/opt/mssql/data/${DB}_log.ldf',
             REPLACE,
             RECOVERY;"
fi

echo "[db-init] a normalizar ApplicationSettings para o ambiente Docker..."
# Em Linux os caminhos guardados com '\' tornam-se nomes de ficheiro literais
# (e não são sequer criáveis num bind mount do Windows). As URLs internas
# apontam para os nomes dos serviços na rede do compose.
run_sql -d "$DB" <<'SQL'
UPDATE ApplicationSettings
SET PastaDownloadsPdf           = REPLACE(PastaDownloadsPdf, '\', '/'),
    PastaDownloadsMedia         = REPLACE(PastaDownloadsMedia, '\', '/'),
    PastaImagens                = REPLACE(PastaImagens, '\', '/'),
    PastaBackupUploads          = REPLACE(PastaBackupUploads, '\', '/'),
    PastaBackupBD               = REPLACE(PastaBackupBD, '\', '/'),
    PastaPublicacoes            = REPLACE(PastaPublicacoes, '\', '/'),
    PastaListagens              = REPLACE(PastaListagens, '\', '/'),
    PastaImagensAutores         = REPLACE(PastaImagensAutores, '\', '/'),
    PastaImagensCapasLivros     = REPLACE(PastaImagensCapasLivros, '\', '/'),
    PastaImagemFundo            = REPLACE(PastaImagemFundo, '\', '/'),
    ModelBaseUrl                = 'http://ollama:11434',
    ApiBaseUrl_Production       = 'http://webapi:5000',
    FileManagerApiBaseUrl_Production = 'http://webapi:5000/api/FileManager';
SQL

echo "[db-init] a normalizar caminhos de imagens/ficheiros ('\\' -> '/')..."
# As colunas de caminhos foram gravadas em Windows. Em Linux '\' é um caractere
# literal, por isso File.Exists falha e as grids caem no ícone "sem imagem".
# Os guards COL_LENGTH evitam falhas se a coluna/tabela não existir neste .bak.
run_sql -d "$DB" <<'SQL'
IF COL_LENGTH('dbo.Publicacao', 'ImagemCapa') IS NOT NULL
    UPDATE dbo.Publicacao SET ImagemCapa = REPLACE(ImagemCapa, '\', '/')
    WHERE ImagemCapa LIKE '%\%';

IF COL_LENGTH('dbo.Publicacao', 'DigitalPath') IS NOT NULL
    UPDATE dbo.Publicacao SET DigitalPath = REPLACE(DigitalPath, '\', '/')
    WHERE DigitalPath LIKE '%\%';

IF COL_LENGTH('dbo.Autor', 'ImagemAutor') IS NOT NULL
    UPDATE dbo.Autor SET ImagemAutor = REPLACE(ImagemAutor, '\', '/')
    WHERE ImagemAutor LIKE '%\%';

IF COL_LENGTH('dbo.Media', 'FilePath') IS NOT NULL
    UPDATE dbo.Media SET FilePath = REPLACE(FilePath, '\', '/')
    WHERE FilePath LIKE '%\%';

IF COL_LENGTH('dbo.Media', 'FileUrl') IS NOT NULL
    UPDATE dbo.Media SET FileUrl = REPLACE(FileUrl, '\', '/')
    WHERE FileUrl LIKE '%\%';

IF COL_LENGTH('dbo.Media', 'CoverFile') IS NOT NULL
    UPDATE dbo.Media SET CoverFile = REPLACE(CoverFile, '\', '/')
    WHERE CoverFile LIKE '%\%';
SQL

echo "[db-init] concluído."
