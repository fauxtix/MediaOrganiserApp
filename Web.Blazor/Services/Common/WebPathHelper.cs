using Microsoft.AspNetCore.Hosting;

namespace Web.Blazor.Services.Common
{
    /// <summary>
    /// Normaliza caminhos guardados na base de dados. Os caminhos foram gravados
    /// em Windows (com '\') e, em contentores Linux, '\' é um caractere literal —
    /// <c>Path.Combine</c> + <c>File.Exists</c> falham e a grid cai no ícone
    /// "sem imagem". Estes helpers tornam a resolução independente da plataforma.
    /// </summary>
    public static class WebPathHelper
    {
        /// <summary>
        /// Converte um caminho relativo da BD num URL servível (sempre com '/').
        /// URLs absolutas (ex.: capas do Google Books) ficam intactas.
        /// </summary>
        public static string ToWebUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return path;

            return path.Replace('\\', '/').TrimStart('/');
        }

        /// <summary>
        /// Resolve um caminho relativo da BD para o caminho físico dentro de wwwroot.
        /// Trata '\' como separador e remove o '/' inicial (no Linux um caminho
        /// iniciado por '/' é absoluto e <c>File.Exists</c> falharia).
        /// </summary>
        public static string ToPhysicalPath(IWebHostEnvironment env, string? path)
        {
            var relative = ToWebUrl(path);
            if (string.IsNullOrEmpty(relative))
                return string.Empty;

            return Path.Combine(env.WebRootPath, relative);
        }
    }
}
