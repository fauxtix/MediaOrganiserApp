# Manual de usuario — Media Organizer

> Guía completa de la aplicación **Media Organizer**: gestión de publicaciones (libros, libros electrónicos y documentos), autores, contenido multimedia (audio, vídeo y YouTube), galerías, funciones de inteligencia artificial, tablas auxiliares, administración, informes y herramientas.
>
> Este manual está dirigido a **usuarios finales** y **administradores**. No se necesitan conocimientos de programación.
>
> 💡 El manual está disponible **dentro de la aplicación**: haz clic en el botón de ayuda (`?`) de la **barra superior** para abrirlo en un diálogo sin salir de la página actual.

---

## Índice

- [0. Inicio rápido — empezar en 5 minutos](#0-quick-start--começar-em-5-minutos)
- [1. Visión general](#1-visão-geral)
- [2. Primeros pasos](#2-primeiros-passos)
- [3. Panel (Inicio)](#3-dashboard-home)
- [4. Publicaciones (libros, libros electrónicos y documentos)](#4-publicações-livros-e-books-e-documentos)
- [5. Autores](#5-autores)
- [6. Contenido multimedia (audio, vídeo y YouTube)](#6-média-áudio-vídeo-e-youtube)
- [7. Galerías](#7-galerias)
- [8. Inteligencia artificial](#8-inteligência-artificial)
- [9. Tablas auxiliares](#9-tabelas-de-apoio)
- [10. Administración](#10-administração)
- [11. Informes](#11-relatórios)
- [12. Herramientas](#12-ferramentas)
- [13. Consejos y atajos](#13-dicas-e-atalhos)
- [14. Solución de problemas](#14-resolução-de-problemas)
- [15. Glosario](#15-glossário)
- [16. Escenarios completos (paso a paso)](#16-cenários-completos-passo-a-passo)
- [Apéndice A — Rutas principales](#apêndice-a--rotas-principais)
- [Apéndice B — Lista de comprobación de mantenimiento periódico](#apêndice-b--checklist-de-manutenção-periódica)
- [Apéndice C — Preguntas frecuentes (FAQ)](#apêndice-c--perguntas-frequentes-faq)

---

## 0. Inicio rápido — empezar en 5 minutos {#0-quick-start--começar-em-5-minutos}

Si es la primera vez que utilizas la aplicación, empieza por estos pasos esenciales.

### 0.1 Iniciar sesión en la aplicación

1. Abre la dirección de la aplicación en el navegador (en un entorno local, normalmente `https://localhost:4110`).
2. Introduce el **correo electrónico** y la **contraseña** de tu cuenta.
3. Haz clic en **Entrar**.
4. Si todavía no tienes una cuenta, utiliza el enlace de **registro** en la pantalla de autenticación y espera a que se cree el usuario (en algunas instalaciones, el administrador se encarga del registro).

### 0.2 Elegir el idioma

1. Abre **Configuración** (en el pie del menú lateral) → **Idioma**.
2. Elige entre **Portugués (predeterminado)**, **Inglés**, **Francés** o **Español**.
3. La elección se guarda y se aplica a toda la aplicación (la página se vuelve a cargar).

> 💡 La barra superior **no** tiene selector de idioma: solo muestra el idioma actual (icono de globo + `PT`/`EN`/`FR`/`ES`), junto al nombre de usuario, el botón de ayuda (`?`), el botón del tema y **Salir**.

### 0.3 Elegir el tema

1. En la barra superior, haz clic en el icono del **tema** (sol/luna).
2. La aplicación alterna entre el **modo claro** y el **modo oscuro**.
3. La barra superior y el menú lateral siempre son oscuros; es intencional.

### 0.4 Crear o importar la primera publicación

1. En el menú lateral, abre **Publicaciones → Gestión** (`/publicacoes`).
2. Para crear una manualmente (por ejemplo, un libro impreso), haz clic en **Nuevo** (botón verde con `+`), rellena al menos **Título**, **Autor**, **Categoría**, **Editorial**, **Formato** y **Ubicación de archivo** y haz clic en **Guardar**.
3. Para importar un documento del PC (PDF, DOCX o mensaje de Outlook `.msg`), utiliza **Importar PDF/DOCX**, selecciona el archivo y deja que la aplicación lea los **metadatos**.
4. **Revisa** siempre los datos rellenados automáticamente antes de guardarlos.

> 💡 El área **Publicaciones** no sirve solo para libros: también gestiona **libros electrónicos (PDF)**, **documentos de texto (DOCX)**, **mensajes de Outlook** y otros documentos. El formulario es el mismo; cambia sobre todo el origen de los datos (manual o importado).

### 0.5 Crear el primer autor

1. Abre **Publicaciones → Autores** (`/autores`).
2. Haz clic en **Nuevo**.
3. Rellena el **nombre** y, si es posible, la **biografía**.
4. **Guarda**.
5. Opcional: carga una **fotografía** y utiliza la IA para generar/completar el **perfil del autor**.

### 0.6 Añadir contenido multimedia (audio/vídeo)

1. Abre **Media → Gestión** (`/media`).
2. Haz clic en **Nuevo** para crear un registro manual o en **Upload** para enviar archivos.
3. Elige el tipo (audio, vídeo o YouTube), rellena los datos y **guarda**.

### 0.7 Hablar con la IA

1. Abre **AI → Chat** (`/ai/openrouter-chat`) para el chat en la nube, o **AI → Chat (Ollama)** (`/ai/chat`) para el chat local.
2. Escribe la pregunta y pulsa **Enter**.
3. La respuesta aparece en streaming; puedes **copiarla**, valorarla con **👍/👎** y consultar el historial.

### 0.8 Tabla «quiero hacer X → voy a Y»

| Quiero… | Voy a… | Ruta |
|---|---|---|
| Ver el resumen general | Inicio | `/` |
| Gestionar publicaciones (libros, libros electrónicos, documentos) | Publicaciones → Gestión | `/publicacoes` |
| Ver las portadas en una galería | Publicaciones → Galería | `/imagegallery` |
| Gestionar autores | Publicaciones → Autores | `/autores` |
| Gestionar contenido multimedia | Media → Gestión | `/media` |
| Ver vídeos | Media → Galería | `/videogallery` |
| Crear listas de reproducción | Media → Listas de reproducción | `/media/playlists` |
| Editar prompts de IA | AI → Editor de Prompts | `/edit-prompts` |
| Chat en la nube | AI → Chat | `/ai/openrouter-chat` |
| Chat local | AI → Chat (Ollama) | `/ai/chat` |
| Gestionar usuarios | Mantenimiento → Usuarios | `/admin/manageusers` |
| Gestionar tablas auxiliares | Mantenimiento → Tablas auxiliares | `/admin/managelookuptables` |
| Ver registros | Registros | `/admin/viewapplogs` |
| Gestionar archivos | Gestor de archivos | `/admin/filemanager` |
| Copia de seguridad de la base de datos | Copia de seguridad → Base de datos | `/backupdatabase` |
| Configuración | Pie del menú → Configuración | `/admin/appsettings2` |

---

## 1. Visión general {#1-visão-geral}

### 1.1 Qué es Media Organizer

Media Organizer es una aplicación de **catálogo personal/organizativo** que reúne en un solo lugar:

- **Publicaciones** — colección mixta de **libros impresos, libros electrónicos (PDF), documentos de texto (DOCX), mensajes de Outlook (`.msg`)** y otros documentos, con portada, sinopsis, ISBN, editorial, categoría, formato, ubicación de archivo y valoración personal. Puedes crear un registro manualmente o **importar** el archivo y dejar que la aplicación rellene los datos a partir de sus metadatos.
- **Autores** — fichas completas con biografía, fotografía y lista de libros.
- **Contenido multimedia** — audio, vídeo y enlaces de YouTube, organizados en listas de reproducción y reproducidos desde la propia aplicación.
- **Galerías** — vistas visuales de imágenes, vídeos y música.
- **Inteligencia artificial** — traducciones, resúmenes, metadatos, perfiles de autor y dos chats (en la nube y local).

### 1.2 Cómo está organizada la aplicación (sin detalles técnicos)

La aplicación tiene dos partes que trabajan conjuntamente:

1. **Servicio/API** — guarda y procesa los datos (base de datos, archivos, IA).
2. **Interfaz** — lo que ves en el navegador.

Para el usuario, se comporta como un único sitio web. En un entorno local, los dos componentes pueden utilizar puertos diferentes; si una página no se carga, consulta [Solución de problemas](#14-resolução-de-problemas).

### 1.3 Roles de usuario

Hay dos roles:

| Rol | Qué puede ver | Qué puede hacer |
|---|---|---|
| **Admin** | **Todos** los datos de todos los usuarios | Todo: gestionar usuarios, tablas auxiliares, configuración, registros, copias de seguridad y borrar el historial de todos |
| **GeneralUser** | **Solo sus propios** datos | Crear, editar y borrar sus publicaciones, autores, contenido multimedia y listas de reproducción; usar la IA y gestionar su historial de chat |

> ⚠️ **Atención:** el aislamiento de datos se aplica en el servidor. Un GeneralUser **no puede** ver los registros de otro usuario, aunque cambie la dirección de la página. Si un registro «ha desaparecido», comprueba que has iniciado sesión con la cuenta correcta.

### 1.4 Mapa mental de navegación

```
Home (/)
├── Publicações
│   ├── Gestão (/publicacoes)
│   ├── Galeria (/imagegallery)
│   └── Autores (/autores)
├── Media
│   ├── Gestão (/media)
│   ├── Galeria (/videogallery)
│   └── Playlists (/media/playlists)
├── AI
│   ├── Editor de Prompts (/edit-prompts)
│   ├── Chat (Ollama) (/ai/chat)
│   └── Chat (/ai/openrouter-chat)
├── [Admin] Backup
│   ├── Base de Dados (/backupdatabase)
│   ├── Uploads Publicações (/backupuploads/1)
│   ├── Uploads Media (/backupuploads/2)
│   └── Uploads Imagens (/backupuploads/3)
├── [Admin] Manutenção
│   ├── Utilizadores (/admin/manageusers)
│   └── Tabelas Auxiliares (/admin/managelookuptables)
├── [Admin] Gestor de Ficheiros (/admin/filemanager)
├── [Admin] Logs (/admin/viewapplogs)
└── Definições (/admin/appsettings2)   ← rodapé do menu
```

### 1.5 Qué se guarda y dónde

- **Registros** (publicaciones, autores, contenido multimedia, listas de reproducción, conversaciones, valoraciones) — en la base de datos.
- **Archivos** (PDF, portadas, imágenes de autores, audio, vídeo) — en las carpetas configuradas en Configuración.
- **Preferencias locales** (tema) — en el navegador; el **idioma** se define en **Configuración → Idioma** y se aplica mediante una cookie de cultura.
- **Configuración de IA** — en la base de datos; se puede editar en el área **IA** de Configuración.

---

## 2. Primeros pasos {#2-primeiros-passos}

### 2.1 Autenticación

#### Iniciar sesión

1. Abre la aplicación.
2. Introduce el **correo electrónico** y la **contraseña**.
3. Haz clic en **Entrar**.
4. Si las credenciales son correctas, accederás directamente al **Panel**.

#### Recuperar la contraseña

1. En la pantalla de autenticación, haz clic en **He olvidado mi contraseña**.
2. Introduce el correo electrónico asociado a la cuenta.
3. Sigue el enlace que recibirás por correo electrónico.
4. Define la nueva contraseña.

> 💡 Si no recibes el correo de recuperación, comprueba la carpeta de spam o ponte en contacto con el administrador (el envío de correo depende de la configuración del servidor).

#### Cerrar sesión

- Utiliza la opción **Salir** de la barra superior. Se cerrará la sesión y volverás a la pantalla de autenticación.

### 2.2 Idiomas

La aplicación está disponible en **cuatro idiomas**:

| Idioma | Código | Observaciones |
|---|---|---|
| Portugués | `pt` | Idioma **predeterminado** |
| Inglés | `en` | — |
| Francés | `fr` | — |
| Español | `es` | — |

**Cómo cambiarlo:**

1. Abre **Configuración** (en el pie del menú lateral) → sección **Idioma**.
2. Elige el idioma que quieras en la lista.
3. La aplicación guarda la elección y vuelve a cargar la interfaz en el nuevo idioma.

> 💡 La **barra superior no tiene selector de idioma**. Solo muestra un indicador con el idioma actual (globo + `PT`/`EN`/`FR`/`ES`), junto al nombre de usuario, el botón de ayuda (`?`), el botón del tema y **Salir**. El idioma siempre se cambia desde **Configuración**.

**Qué se traduce:** menús, botones, etiquetas, mensajes de validación y textos de la interfaz.

**Qué no se traduce:** el **contenido** que introduces (títulos, sinopsis, biografías) y los nombres de los modelos de IA. Puedes tener la interfaz en inglés y tus publicaciones en portugués; es normal.

> 💡 La elección se guarda en Configuración y se aplica a toda la aplicación (no es una preferencia independiente para cada pestaña).

### 2.3 Tema claro/oscuro

1. Haz clic en el icono del **tema** de la barra superior.
2. La aplicación cambia al instante, sin volver a cargar.
3. El navegador recuerda la preferencia.

**Reglas visuales importantes:**

- La **barra lateral** y la **barra superior** son **siempre oscuras**, en ambos temas.
- Las tarjetas, cuadrículas, formularios y diálogos se adaptan al tema.
- Los gráficos del Panel también se adaptan.

> 💡 Si observas un artefacto visual en un gráfico después de pasar el puntero, sácalo del gráfico y vuelve a situarlo; la aplicación corregirá automáticamente el repintado.

### 2.4 Estructura de la barra lateral

La barra lateral tiene tres zonas:

1. **Parte superior** — elementos generales: Inicio, Publicaciones, Media, AI.
2. **Parte central** — elementos administrativos (solo para Admin): Copia de seguridad, Mantenimiento, Gestor de archivos, Registros.
3. **Pie** — **Configuración**.

**Comportamiento:**

- Los grupos con elementos secundarios se expanden o contraen al hacer clic.
- El elemento activo aparece resaltado.
- En pantallas pequeñas, se puede contraer la barra lateral.

### 2.5 Barra superior

Suele incluir:

- El **nombre o la marca** de la aplicación.
- El **idioma actual** (icono de globo + `PT`/`EN`/`FR`/`ES`); es solo un indicador, el idioma se cambia desde **Configuración**.
- El **nombre de usuario**.
- El botón de **ayuda** (`?`), que abre este manual en un diálogo sin salir de la página actual.
- El **botón del tema**.
- La opción de **sesión** (salir).

### 2.6 Lista de comprobación del primer acceso

- [ ] He podido iniciar sesión.
- [ ] He elegido el idioma.
- [ ] He elegido el tema.
- [ ] He creado o importado al menos un libro/documento.
- [ ] He creado al menos un autor.
- [ ] He comprobado que las tablas auxiliares esenciales contienen datos (categorías, formatos, editoriales).
- [ ] He probado una traducción o un resumen con IA.
- [ ] He abierto el chat y enviado un mensaje.

---

## 3. Panel (Inicio) {#3-dashboard-home}

El Panel es la página de inicio (`/`) y ofrece una vista general del catálogo y la actividad.

### 3.1 Tarjetas de totales

En la parte superior aparecen tarjetas con recuentos, por ejemplo:

- **Publicaciones**.
- **Autores**.
- **Editoriales**.
- **Formatos**.
- **Contenido multimedia**.

Se puede hacer clic en cada tarjeta (cuando corresponda) para ir a la página pertinente.

### 3.2 Gráficos circulares

Hay gráficos de distribución por:

- **Categorías de publicaciones**.
- **Formatos de publicaciones**.
- **Editoriales**.
- **Categorías de contenido multimedia**.
- **Formatos de contenido multimedia**.

**Cómo interpretarlos:**

- Cada porción representa una categoría, un formato o una editorial.
- Al pasar el puntero, aparecen el nombre y el valor.
- Las porciones muy pequeñas pueden quedar agrupadas visualmente; consulta las tablas auxiliares para ver el detalle.

> 💡 Si un gráfico está vacío, no hay suficientes registros (o no tienen esa clasificación indicada).

### 3.3 Sección «Estado» (calidad de los datos)

Indicadores de registros incompletos o problemas:

| Indicador | Significado | Qué hacer |
|---|---|---|
| **Publicaciones sin portada** | Publicaciones sin imagen de portada | Abre cada publicación y carga la portada |
| **Publicaciones sin páginas** | Publicaciones sin número de páginas | Completa el campo **Páginas** |
| **Autores sin publicaciones** | Autores sin publicaciones asociadas | Asocia publicaciones o elimina los autores huérfanos |
| **Editoriales sin publicaciones** | Editoriales sin uso | Reutilízalas o elimínalas en las tablas auxiliares |
| **Categorías sin publicaciones** | Categorías sin uso | Reutilízalas o elimínalas |
| **Formatos sin publicaciones** | Formatos sin uso | Reutilízalos o elimínalos |
| **Ubicaciones de archivo sin publicaciones** | Ubicaciones sin uso | Reutilízalas o elimínalas |
| **Títulos duplicados** | Posibles duplicados | Comprueba los registros y unifícalos si corresponde |

> ⚠️ Estos indicadores son **sugerencias**, no errores. Un registro puede estar incompleto intencionadamente.

### 3.4 Sección «Actividad»

Métricas de uso de la IA y el chat:

- **Conversaciones** — número de conversaciones.
- **Total de respuestas**.
- **Últimos 7 días**.
- **Latencia media**.
- **Tokens por segundo** (con una nota explicativa: es una medida aproximada).
- **Tamaño medio en tokens**.
- **Porcentaje de uso de la nube**.
- **Modelos más utilizados** (tabla).

### 3.5 Sección «IA de uso interno»

Muestra el uso de la IA **fuera del chat** (traducciones, resúmenes, metadatos, perfiles, etc.):

- Total de operaciones.
- Últimos 7 días.
- Llamadas mediante **OpenRouter**.
- Llamadas mediante **Ollama** (local).
- **Fallbacks** (cuántas veces ha fallado la nube y se ha recurrido al servicio local).
- Tabla **por operación** (traducción, resumen, metadatos…).
- Tabla de los **últimos registros**, con operación, proveedor, contexto, escenario, duración y fecha.

**Cómo interpretarlo:**

- Muchos **fallbacks** = la nube es inestable, no hay clave o se ha agotado el saldo; la aplicación está utilizando Ollama.
- Duraciones elevadas = modelos locales en equipos modestos o prompts extensos.

### 3.6 Sección «Auditoría»

- **Últimos inicios de sesión**.
- **Errores recientes** (con nivel y mensaje).
- Recuento total de errores.

> 💡 Para ver todos los detalles, abre **Registros** (`/admin/viewapplogs`).

### 3.7 Actualización de los datos

El Panel carga los datos al abrir la página. Si has hecho cambios en otra pestaña, vuelve a cargar la página (F5) para ver los valores actualizados.

---

## 4. Publicaciones {#4-publicações-livros-e-books-e-documentos}

Página de gestión: **Gestión de publicaciones** (`/publicacoes`).

### 4.1 La cuadrícula de publicaciones

La cuadrícula muestra las publicaciones a las que tienes acceso, con columnas como título, autor, editorial, categoría, formato, año, valoración y estado de lectura.

**Operaciones disponibles:**

| Acción | Cómo |
|---|---|
| Ordenar | Haz clic en el encabezado de la columna |
| Filtrar | Utiliza la fila de filtros del encabezado |
| Buscar | Cuadro de búsqueda de la cuadrícula |
| Agrupar | Arrastra el encabezado al área de agrupación |
| Cambiar el tamaño de las columnas | Arrastra el borde del encabezado |
| Cambiar de página | Controles de la parte inferior de la cuadrícula |

### 4.2 Crear una publicación

1. Haz clic en **Nuevo** (botón verde `+`).
2. Rellena el formulario (consulta [4.4 Campos](#44-campos-do-formulário)).
3. Haz clic en **Guardar**.
4. El registro aparecerá en la cuadrícula.

> 💡 **Título** y **Autor** son los campos más importantes. Para **guardar** una publicación, la aplicación también exige **Categoría**, **Editorial**, **Formato** y **Ubicación de archivo**; si faltan, créalos primero en las Tablas auxiliares.

### 4.3 Editar y borrar

**Editar:**

1. En la fila de la publicación, haz clic en el icono del **lápiz** (Editar).
2. Modifica los campos.
3. **Guarda**.

**Borrar:**

1. En la fila, haz clic en el icono de la **papelera** (Borrar).
2. Confirma en el diálogo.

> ⚠️ La eliminación es permanente. Si la publicación tiene archivos asociados, confirma si también quieres eliminarlos del disco (la aplicación te avisará cuando corresponda).

### 4.4 Campos del formulario {#44-campos-do-formulário}

| Campo | Obligatorio | Descripción |
|---|---|---|
| **ID** | — | Se genera automáticamente; solo lectura |
| **ISBN** | No | Identificador de la publicación; útil para buscar en Google Books |
| **Fecha de movimiento** | No | Fecha de adquisición/registro |
| **Título** | **Sí** | Nombre de la publicación (de 2 a 256 caracteres) |
| **Año de edición** | No | Si se rellena, debe tener **4 dígitos** y no puede ser posterior al año actual; la IA puede sugerirlo |
| **Páginas** | No | Número de páginas |
| **Precio** | No | Importe monetario |
| **Autor** | **Sí** | Se selecciona de la lista de autores (se puede buscar) |
| **Categoría** | **Sí** | Categoría de la publicación |
| **Editorial** | **Sí** | Editorial |
| **Formato** | **Sí** | Formato físico/digital |
| **Ubicación de archivo** | **Sí** | Lugar donde se guarda el ejemplar/archivo |
| **Valoración (Rating)** | No | Valoración personal (estrellas) |
| **Leído** | No | Indica si ya lo has leído |
| **Sinopsis** | No | Descripción/resumen; admite traducción con IA |

**Notas sobre los selectores:**

- Se puede buscar en los campos **Autor**, **Categoría**, **Editorial**, **Formato** y **Ubicación de archivo**: escribe parte del nombre para filtrar.
- Si falta una opción, créala primero en las **Tablas auxiliares** (o pide ayuda al Admin).

### 4.5 Importar publicaciones desde archivos (PDF/DOCX)

1. En la página de gestión, utiliza el botón **Importar PDF/DOCX**.
2. Selecciona uno o varios archivos.
3. La aplicación crea o actualiza registros a partir del contenido.
4. Revisa los registros creados y corrige lo que sea necesario.

> 💡 La importación intenta extraer el título, el autor y otros datos. **Revisa siempre** el resultado; la extracción automática no es perfecta.

### 4.6 Leer una publicación en la aplicación

1. En la fila de la publicación, haz clic en el comando **Leer PDF**.
2. El visor abre el archivo asociado.
3. Puedes **editar la publicación** desde el visor o **cerrarlo**.
4. Para los documentos de Word aparece un visor propio con las mismas acciones.

> ⚠️ Si el archivo no existe en la ubicación indicada, la aplicación te avisará. Utiliza las páginas de diagnóstico (consulta [4.9](#49-ficheiros-em-falta-e-caminhos-errados)).

### 4.7 Búsqueda en Google Books

1. En el formulario de la publicación (o en el flujo de búsqueda), abre **Buscar en línea**.
2. Introduce el **título** o el **autor**, o ambos.
3. La aplicación consulta **Google Books**.
4. Elige un resultado para rellenar los campos.
5. Si el resultado no tiene descripción, la aplicación utiliza el **fragmento** disponible.
6. Si no hay resultados, se puede recurrir a la **IA** para generar un resumen.
7. Confirma y **guarda**.

**Buenas prácticas:**

- Busca por el **título exacto** (sin comillas) y los **apellidos del autor**.
- Si la publicación es una traducción, prueba con el título original.
- Comprueba siempre el **año** y la **editorial** antes de guardar.

### 4.8 Funciones de IA en las publicaciones

En el formulario de la publicación puedes utilizar:

| Función | Qué hace | Dónde aparece el resultado |
|---|---|---|
| **Resumen IA** | Genera una sinopsis a partir del título, el autor y los datos | Campo **Sinopsis** |
| **Traducir** | Traduce la sinopsis/el texto entre idiomas | Diálogo de traducción → **Confirmar** |
| **Año de edición IA** | Sugiere el año de edición | Campo **Año de edición** |
| **Libros del autor** | Lista o genera libros del autor seleccionado | Panel **Libros del autor** |

El funcionamiento detallado (incluidos el **indicador del modelo** y el botón **Cancelar**) se explica en [8. Inteligencia artificial](#8-inteligência-artificial).

### 4.9 Archivos que faltan y rutas incorrectas {#49-ficheiros-em-falta-e-caminhos-errados}

Dos páginas de diagnóstico ayudan a mantener el catálogo coherente:

#### Publicaciones no encontradas en el disco (`/pubsnotfoundondisk`)

- Muestra las publicaciones cuyo **archivo referenciado no existe**.
- Acciones habituales: corregir la ruta, volver a cargar el archivo o eliminar el registro.

#### Publicaciones con una ruta incorrecta (`/pubswithwrongpath`)

- Muestra las publicaciones cuya ruta apunta a una ubicación no válida o antigua.
- Acciones habituales: actualizar la **Ubicación de archivo**, mover el archivo o corregir la configuración de las carpetas.

> 💡 Ejecuta estas comprobaciones después de mover carpetas, cambiar de equipo o restaurar copias de seguridad.

### 4.10 Estadísticas y «libros del autor»

- El botón **Estadísticas** abre un resumen con recuentos y distribuciones.
- El botón **Libros del autor** abre un panel con los libros del autor seleccionado y permite navegar rápidamente entre sus obras.

### 4.11 Buenas prácticas para las publicaciones

- Rellena **Autor**, **Categoría**, **Editorial** y **Formato** para mejorar los gráficos del Panel.
- Utiliza el **ISBN** para evitar duplicados.
- Asigna nombres coherentes a los archivos (por ejemplo, `ISBN - Título.pdf`).
- Guarda las **portadas** en la carpeta configurada.
- Revisa periódicamente la sección **Estado** del Panel.

---

## 5. Autores {#5-autores}

Página: **Publicaciones → Autores** (`/autores`).

### 5.1 Lista de autores

La cuadrícula muestra los autores a los que tienes acceso, con su nombre, número de libros y fotografía (si la hay).

**Operaciones:**

- Buscar por nombre.
- Ordenar y filtrar.
- Abrir la ficha (**Editar**).
- Ver en modo de **solo lectura**.
- Borrar (con confirmación).

### 5.2 Crear un autor

1. Haz clic en **Nuevo**.
2. Rellena el **Nombre** (obligatorio, de 2 a 128 caracteres).
3. Rellena **Acerca de** (biografía; opcional, pero recomendado).
4. Indica la **fecha de nacimiento** (opcional).
5. Carga una **fotografía** (opcional).
6. **Guarda**.

### 5.3 Ficha del autor

La ficha de edición incluye:

| Campo | Obligatorio | Descripción |
|---|---|---|
| **Nombre** | **Sí** | Nombre del autor (de 2 a 128 caracteres) |
| **Fecha de nacimiento** | No | Fecha de nacimiento del autor |
| **Acerca de / Biografía** | No | Texto libre (editor de texto con formato); admite traducción con IA |
| **Fotografía** | No | Imagen del autor |
| **Libros** | — | Lista de obras asociadas |
| **Perfil IA** | — | Perfil generado/completado mediante IA |

### 5.4 Fotografía del autor

1. En la ficha, utiliza el área de **carga**.
2. Selecciona la imagen (formatos habituales: JPG, PNG).
3. Espera a que termine la carga.
4. La imagen aparecerá en la ficha y en la lista.

> 💡 Para obtener una mejor presentación, utiliza imágenes cuadradas o de retrato y con buena resolución.

### 5.5 Biografía y traducción

1. Escribe o pega la biografía.
2. Utiliza **Traducir** para convertirla a otro idioma.
3. Revisa el texto en el diálogo.
4. Haz clic en **Confirmar** para aplicarlo.

> ⚠️ Al confirmar, la traducción sustituye el texto del campo. Si quieres conservar el original, cópialo antes en un bloc de notas.

### 5.6 Libros del autor

- En la ficha o en la lista, abre **Libros del autor**.
- Puedes **asociar** libros existentes y **crear** libros nuevos ya vinculados al autor.
- La lista refleja los datos que hay en la base de datos para ese autor.

### 5.7 Generar el perfil con IA

1. Abre la ficha del autor.
2. Haz clic en **Generar perfil IA** (o en la opción equivalente).
3. La aplicación intenta utilizar primero la **nube** (OpenRouter) y, si falla, recurre a **Ollama local**.
4. El resultado se muestra para que lo revises.
5. Edítalo si es necesario y **guárdalo**.

**Qué puede utilizar la IA:** el nombre, la biografía existente y las obras conocidas.

> 💡 Revisa siempre el resultado: la IA puede inventar fechas u obras. El perfil es un **punto de partida**, no una fuente de información fiable.

### 5.8 Vista de solo lectura

La vista **AuthorReadOnly** muestra la ficha sin permitir su edición; es útil para consultas rápidas o presentaciones.

### 5.9 Borrar autores

1. Haz clic en **Borrar** en la fila del autor.
2. Confirma la acción.

> ⚠️ Si el autor tiene libros asociados, la aplicación puede impedir que lo borres o mostrar una advertencia. Resuelve primero las asociaciones.

---

## 6. Contenido multimedia (audio, vídeo y YouTube) {#6-média-áudio-vídeo-e-youtube}

Página de gestión: **Media → Gestión** (`/media`).

### 6.1 Tipos de contenido multimedia

La aplicación distingue entre:

- **Audio** — MP3 y formatos similares.
- **Vídeo** — archivos de vídeo locales.
- **YouTube** — enlaces a vídeos alojados en YouTube (sin archivo local).

Cada tipo tiene sus propios campos y acciones, pero comparte la misma cuadrícula y el mismo sistema de edición.

### 6.2 La cuadrícula de contenido multimedia

Columnas habituales: título, artista/autor, categoría, formato, valoración, tipo y acciones.

**Acciones de cada fila:**

| Icono | Acción |
|---|---|
| ▶ | **Reproducir** (con el reproductor adecuado al tipo) |
| ℹ | **Información/detalles** |
| ✏ | **Editar** |
| 🗑 | **Borrar** |

### 6.3 Crear un registro multimedia

1. Haz clic en **Nuevo**.
2. Elige el **tipo** (audio, vídeo o YouTube).
3. Rellena los campos (consulta la tabla siguiente).
4. Para YouTube, introduce el **enlace** del vídeo.
5. **Guarda**.

**Campos y validación:**

| Campo | Obligatorio | Notas |
|---|---|---|
| **Título** (nombre del archivo/registro) | **Sí** | De 2 a 256 caracteres |
| **Tipo** | **Sí** | Audio o vídeo (YouTube se gestiona como un caso propio) |
| **Autor/Artista** | **Sí** | — |
| **Formato multimedia** | **Sí** | Por ejemplo, MP3, MP4 (tabla auxiliar) |
| **Género/Categoría** | **Sí** | Categoría multimedia (tabla auxiliar) |
| **Año de edición** | No | Si se rellena, debe tener 4 dígitos y no ser posterior al año actual |
| **Fecha de movimiento** | No | No puede ser una fecha futura |
| **Valoración** | No | Estrellas |
| **Notas/descripción** | No | Admite IA |

### 6.4 Cargar archivos (Upload)

1. Haz clic en **Upload**.
2. Selecciona uno o varios archivos.
3. Elige el destino/tipo, si se solicita.
4. Espera a que finalice la operación.
5. Revisa los registros creados.

**Recomendaciones:**

- Evita archivos enormes si no son necesarios.
- Utiliza nombres que no contengan caracteres especiales problemáticos.
- Confirma el **formato multimedia** (categoría) para que los gráficos sean correctos.

### 6.5 Metadatos y notas con IA

En el formulario o los detalles del contenido multimedia puedes utilizar:

- **Notas IA** — genera una descripción o notas sobre el elemento.
- **Metadatos IA** — sugiere datos como el género, el año y el artista.
- **Año/edición** — cuando corresponda.

Antes de guardar, **revisa siempre** el resultado.

### 6.6 Listas de reproducción

Página: **Media → Listas de reproducción** (`/media/playlists`).

**Crear una lista de reproducción:**

1. Haz clic en **Nuevo**.
2. Asígnale un **nombre** y, si quieres, una descripción.
3. **Guarda**.
4. Abre la lista de reproducción y **añade elementos** (contenido multimedia existente).

**Gestionar elementos:**

- Añadir/eliminar.
- Cambiar el orden (cuando esté disponible).
- Reproducir la lista completa.

**Reproducir:**

1. Abre la lista de reproducción.
2. Haz clic en **Reproducir**.
3. Se abrirá el **Reproductor de listas de reproducción** (`/media/playlists/{id}/player`) con la cola.
4. Utiliza los controles para avanzar o retroceder.

> 💡 Si un elemento no se reproduce, comprueba que el archivo existe en el disco o que el enlace de YouTube es válido.

### 6.7 Reproductor MP3

Página: **`/mp3player`**.

- Reproduce pistas de audio.
- Permite elegir una pista, pausarla, avanzar y ajustar el volumen.
- Muestra información de la pista actual.

> 💡 El reproductor funciona dentro de la página; para seguir escuchando mientras navegas, ábrelo en una pestaña independiente del navegador.

### 6.8 Reproductor de YouTube

Página: **`/youtubeplayer`**.

- Reproduce vídeos mediante el enlace guardado en el registro.
- Si el enlace no es válido o se ha eliminado el vídeo, aparecerá un error; corrige el enlace en el registro.

### 6.9 Detalles del contenido multimedia

- **Detalles de audio** (`Mp3Detail`) — información y reproducción.
- **Detalles de vídeo** (`VideoDetail`) — información y reproducción.

### 6.10 YouTube y suscripciones

La aplicación incluye un área de **suscripciones** (`/subscriptions`) para seguir canales o enlaces de YouTube, cuando esté configurada.

### 6.11 Buenas prácticas para el contenido multimedia

- Mantén la coherencia en los nombres de **artista** y **categoría**.
- Crea una lista de reproducción para cada contexto (por ejemplo, «Lecturas», «Viajes»).
- Para YouTube, es preferible utilizar enlaces públicos o «no listados» que sean estables.
- Comprueba los **formatos multimedia** de las tablas auxiliares antes de importar muchos elementos.

---

## 7. Galerías {#7-galerias}

Las galerías ofrecen vistas visuales alternativas a las cuadrículas.

### 7.1 Galería de imágenes (`/imagegallery`)

**Qué muestra:** portadas de libros e imágenes asociadas en una cuadrícula de miniaturas.

**Cómo utilizarla:**

1. Abre **Publicaciones → Galería**.
2. Explora las imágenes.
3. Haz clic en una imagen para ampliarla o ver sus detalles.
4. Utiliza el enlace para abrir el registro correspondiente.

**Comportamiento:**

- Si falta una imagen, se muestra un marcador de posición.
- La galería respeta el aislamiento de datos (solo ves los tuyos, salvo que seas Admin).

### 7.2 Galería de vídeos (`/videogallery`)

**Qué muestra:** miniaturas de los vídeos, con reproducción integrada.

**Cómo utilizarla:**

1. Abre **Media → Galería**.
2. Haz clic en un vídeo para reproducirlo.
3. Utiliza los detalles para editar los metadatos.

### 7.3 Galería de música (`/musicgallery`)

**Qué muestra:** una vista de las pistas/álbumes con estilo de **polaroid**.

**Interacciones:**

- Al pasar el puntero, se resalta el elemento (con un color de énfasis).
- Al hacer clic, se selecciona/reproduce.
- La selección se distingue visualmente.

> 💡 La galería de música es ideal para navegar por las portadas de los álbumes; para editar metadatos, utiliza la cuadrícula de **Media**.

### 7.4 Cuando una galería está vacía

Si no hay imágenes, vídeos o pistas, aparecerá el mensaje **«No hay datos disponibles»**. Comprueba lo siguiente:

1. Que haya registros del tipo correcto.
2. Que los archivos de imagen/vídeo estén asociados.
3. Que hayas iniciado sesión con la cuenta/usuario correctos.

---

## 8. Inteligencia artificial {#8-inteligência-artificial}

La IA es una de las áreas más completas de la aplicación. Esta sección explica **cómo funciona** y **cómo utilizar cada función**.

### 8.1 Arquitectura en dos capas (lo esencial)

La aplicación utiliza **dos proveedores**:

1. **Cloud (OpenRouter)** — **primera opción**. Es rápida y ofrece modelos más grandes, pero depende de internet, de una clave de API y de los límites de uso.
2. **Local (Ollama)** — **alternativa**. Se ejecuta en el equipo, sin coste por llamada, pero puede ser más lenta y depende del hardware.

**Flujo habitual:**

```
Solicitud del usuario
        │
        ▼
  Cloud (OpenRouter)  ──fallo/sin clave/límite──►  Local (Ollama)
        │                                              │
        └──────────── respuesta ───────────────────────┘
```

**Qué se considera un «fallo» de la nube:** falta de conexión, falta de clave, límite diario agotado, error HTTP o respuesta no válida o vacía.

> 💡 Este mecanismo es **automático**. El usuario no tiene que elegir el proveedor para cada operación, pero **puede ver cuál se ha utilizado** en el indicador del modelo (consulta [8.3](#83-o-chip-do-modelo)).

### 8.2 Dónde se utiliza la IA

| Operación | Dónde | Resultado |
|---|---|---|
| **Traducir** | Formularios de publicaciones/autores, herramienta de traducción | Texto traducido |
| **Resumen de publicación** | Edición de publicaciones/búsqueda en Google Books | Sinopsis |
| **Año de edición** | Edición de publicaciones | Año sugerido |
| **Notas multimedia** | Edición de contenido multimedia | Notas/descripción |
| **Metadatos multimedia** | Edición de contenido multimedia | Campos sugeridos |
| **Libros del autor** | Ficha del autor | Lista de obras |
| **Perfil del autor** | Ficha del autor | Perfil/biografía |
| **Chat en la nube** | `/ai/openrouter-chat` | Conversación |
| **Chat local** | `/ai/chat` | Conversación |

### 8.3 El indicador del modelo {#83-o-chip-do-modelo}

Al finalizar una operación de IA, puede aparecer un **indicador** (una pequeña etiqueta) con el modelo utilizado, por ejemplo:

```
⊘ OpenRouter · llama-3.3-70b
```

**Significado:**

- **OpenRouter · nombre-del-modelo** — la respuesta la ha generado ese modelo en la nube.
- **Ollama · nombre-del-modelo** — la respuesta la ha generado el modelo local.
- **Automático** — el router de la nube ha elegido el modelo (no se solicitó uno específico).
- **Modelo en curso** (en `title`) — aparece si se **cancela** la operación; indica qué modelo se estaba utilizando.

**Por qué es útil:** la aplicación puede probar varios modelos y recurrir al servicio local, así que no es posible prever cuál responderá. El indicador muestra la **procedencia** de la respuesta.

> 💡 En la traducción, el indicador aparece **debajo del botón Traducir** para no interferir con el mensaje de estado (cancelación o confianza).

### 8.4 Traducir texto

**Dónde encontrarlo:** formularios de libros (sinopsis), fichas de autor (biografía) y herramienta de traducción.

**Paso a paso:**

1. Abre el diálogo de traducción.
2. Elige los idiomas de **origen** y de **destino**.
3. Haz clic en **Traducir**.
4. Durante el proceso, aparece un mensaje de progreso que indica el **modelo** en uso.
5. Si tarda demasiado, haz clic en **Cancelar**.
6. Cuando termine correctamente, revisa el texto y haz clic en **Confirmar**.

**El botón Cancelar:**

- La interfaz se detiene **inmediatamente**.
- El modelo local puede seguir procesando en segundo plano, pero el resultado se **descarta**.
- Aparece el mensaje **«Cancelado por el usuario»** a la derecha.
- **Confirmar** queda desactivado (no hay texto que aplicar).

**Cerrar el diálogo:**

- Cancela en silencio cualquier traducción en curso.
- Una respuesta tardía **no** sobrescribirá el campo.

**Consejos:**

- Traduce textos revisados para evitar traducir errores.
- Si falla la traducción, vuelve a intentarlo; la aplicación puede elegir otro modelo.
- En textos largos, cancelar es más rápido que esperar a que se agote el tiempo de espera.

### 8.5 Resumen de una publicación (IA)

**Cómo utilizarlo:**

1. Abre o edita la publicación.
2. Haz clic en **Resumen IA**.
3. La aplicación intenta utilizar la nube y, si es necesario, el servicio local.
4. El resumen se coloca en el campo **Sinopsis**.
5. **Revísalo y edítalo** antes de guardar.

**Buenas prácticas:**

- Comprueba que el **título** y el **autor** sean correctos: son la base del resumen.
- Si ya hay una sinopsis, la IA puede ampliarla o sustituirla; comprueba el comportamiento en el diálogo.
- Los resúmenes generados pueden contener imprecisiones; verifica los datos.

### 8.6 Año de edición con IA

1. Abre la publicación.
2. Utiliza la acción **Año de edición IA**.
3. La aplicación sugerirá un año.
4. Acéptalo o corrígelo manualmente.

> ⚠️ La IA puede equivocarse con el año. Compruébalo con una fuente (ISBN, editorial, Google Books).

### 8.7 Notas y metadatos multimedia

**Notas:**

1. Abre el registro multimedia.
2. Haz clic en **Notas IA**.
3. Revisa el texto generado.
4. Guárdalo.

**Metadatos:**

1. Abre el registro.
2. Haz clic en **Metadatos IA**.
3. La IA sugiere campos (género, año, artista…).
4. Acepta los que tengan sentido.

> 💡 Para YouTube, incluye el título/artista correcto para mejorar las sugerencias.

### 8.8 Libros del autor (IA)

1. Abre la ficha del autor.
2. Utiliza **Libros del autor** o la generación con IA.
3. La aplicación intenta obtener o generar la lista de obras.
4. Revísala y asocia/crea los registros.

**Atención:** las listas generadas por IA pueden incluir obras inexistentes u omitir obras reales. Verifícalas antes de crear registros en lote.

### 8.9 Perfil del autor (IA)

1. Abre la ficha del autor.
2. Haz clic en **Generar perfil**.
3. La aplicación utiliza primero la nube y después el servicio local.
4. Se muestra el perfil para que lo revises.
5. Edítalo y guárdalo.

> ⚠️ Comprueba las fechas, los premios y las obras: son los aspectos en los que más suele equivocarse la IA.

### 8.10 Chat en la nube (OpenRouter)

Página: **AI → Chat** (`/ai/openrouter-chat`).

**Características:**

- Streaming en tiempo real.
- Sin alternativa local: si falla la nube, se muestra un error claro.
- Historial de conversaciones.
- Valoración 👍/👎 y opción de copiar.
- Bloque de **razonamiento** desplegable (cuando el modelo lo proporciona).

**Enviar un mensaje:**

1. Escribe en el cuadro de texto.
2. Pulsa **Enter** (o el botón de envío).
3. La respuesta aparece progresivamente.
4. Puedes **detener** la generación, si la opción está disponible.

**Razonamiento («💭 Razonamiento»):**

- Aparece cuando el modelo proporciona razonamiento y la opción está activada.
- Está **contraído de forma predeterminada**; puedes desplegarlo en cada mensaje.
- La elección (abierto/cerrado) se respeta y se guarda con la conversación.

**Historial:**

- Abre el panel **Historial**.
- Selecciona una conversación para retomarla.
- Las conversaciones están asociadas al usuario.

### 8.11 Chat local (Ollama)

Página: **AI → Chat (Ollama)** (`/ai/chat`).

**Diferencias con respecto al chat en la nube:**

- Se ejecuta **íntegramente en el equipo** (Ollama).
- Puede funcionar sin internet.
- La velocidad depende del hardware y del modelo.
- También admite historial, 👍/👎, copia y razonamiento.

**Modelos con razonamiento:**

- Modelos como Qwen3, DeepSeek-R1 y similares pueden generar un bloque de «pensamiento».
- Si la opción de razonamiento está **activada**, el bloque aparece separado de la respuesta.
- Si está **desactivada**, el bloque se descarta y **nunca** se muestra en la respuesta.
- Algunos modelos envían el pensamiento «inline» (dentro del texto, entre marcadores); la aplicación lo separa correctamente y no deja marcadores visibles.

**Respuesta vacía:**

- Si el modelo no genera texto, la aplicación muestra un aviso en lugar de una respuesta en blanco.
- Prueba a reformular la pregunta o a elegir otro modelo en Configuración.

### 8.12 Historial y borrar conversaciones

El panel **Historial** es común a ambos chats.

**Acciones:**

| Acción | Efecto |
|---|---|
| **Nueva conversación** | Inicia una conversación en blanco |
| **Seleccionar conversación** | Recupera el contexto |
| **Borrar historial** | Elimina las conversaciones (previa confirmación) |

**Borrar el historial:**

1. Abre **Historial**.
2. Haz clic en **Borrar historial** (botón de peligro, a la derecha).
3. Confirma en el diálogo, que indica **cuántas conversaciones** se borrarán y avisa de que la acción es **irreversible**.
4. La lista se vacía y se reinicia la conversación actual.

**Notas importantes:**

- La eliminación se realiza **conversación por conversación**.
- Para un **Admin**, el historial eliminado incluye el de **todos los usuarios**.
- La tabla de conversaciones no distingue entre interfaces: se borra el historial de **ambos chats**.
- Las valoraciones 👍/👎 asociadas quedan huérfanas y se ignoran (no se produce ningún error).
- Los botones quedan **desactivados** cuando la lista está vacía.

> ⚠️ No es posible recuperar las conversaciones después de borrarlas. Exporta lo que necesites antes.

### 8.13 Valorar respuestas (👍/👎) y copiar

**Valorar:**

1. En cada respuesta del asistente, haz clic en **👍** (me gusta) o **👎** (no me gusta).
2. Si vuelves a hacer clic en el mismo botón, se **elimina** la valoración.
3. La etiqueta de estado muestra **Me gusta / No me gusta / Sin valorar**.
4. La valoración se restaura al volver a abrir la conversación.

**Copiar:**

1. Haz clic en el icono de **copiar**.
2. El texto de la respuesta se copia al portapapeles.
3. Pégalo donde lo necesites (Ctrl+V).

> 💡 Las valoraciones sirven para medir la calidad de las respuestas y se guardan por conversación/mensaje.

### 8.14 Límites de los modelos gratuitos

- De forma predeterminada, la aplicación utiliza **modelos gratuitos**.
- Hay un **límite diario** de solicitudes gratuitas (configurable por el Admin).
- Cuando se alcanza el límite:
  - Las operaciones en la **nube** devuelven «sin respuesta» y la aplicación recurre a Ollama (cuando corresponde).
  - El **chat en la nube** muestra un mensaje claro (no tiene alternativa local).

**Cómo comprobarlo:** consulta la sección **IA de uso interno** del Panel y las tarjetas de uso de Configuración.

> 💡 Si necesitas más capacidad, el Admin puede habilitar **modelos de pago** o aumentar el límite.

### 8.15 Artefactos de seguridad de los modelos

Algunos modelos gratuitos devuelven ocasionalmente solo una línea de clasificación de seguridad (por ejemplo, «User Safety: safe;») en lugar de una respuesta.

**Qué hace la aplicación:**

- **Elimina** esas líneas de las respuestas válidas.
- Si la respuesta consiste **solo** en el artefacto, lo sustituye por un mensaje que pide que respondas **«continúa»**.

**Qué debes hacer:** responde **«continúa»** o reformula la pregunta.

### 8.16 Editor de Prompts

Página: **AI → Editor de Prompts** (`/edit-prompts`).

**Qué permite:**

- Consultar y editar las **plantillas de prompts** que utiliza la IA.
- Ver la descripción de cada plantilla.
- Guardar cambios (con validación).

**Recomendaciones:**

- Cambia un prompt cada vez.
- Mantén el objetivo y el formato solicitado (por ejemplo, «responde solo con el texto traducido»).
- Prueba con una operación sencilla antes de utilizarlo en lote.
- Si algo sale mal, restaura la versión anterior.

> ⚠️ Si editas mal los prompts, puedes perjudicar **todas** las operaciones que los utilizan. Si no sabes qué efecto tendrá un cambio, no lo hagas.

### 8.17 Configurar la IA (visión de Admin)

La configuración se encuentra en el área **IA** de Configuración (consulta [10.3](#103-área-ia)). Incluye:

- **Configuración común** — activar/desactivar el razonamiento.
- **OpenRouter** — clave de API, modelo predeterminado, permiso para modelos de pago, uso de la nube para publicaciones de autor, límite diario y tarjetas de uso.
- **Ollama (local)** — clave (si corresponde), dirección del modelo y nombre del modelo.

> 💡 Sin una clave de OpenRouter, la aplicación solo funciona con Ollama local.

---

## 9. Tablas auxiliares {#9-tabelas-de-apoio}

Las tablas auxiliares alimentan los selectores y los gráficos. Sin ellas, los formularios ofrecen pocas opciones y las estadísticas quedan incompletas.

Página: **Mantenimiento → Tablas auxiliares** (`/admin/managelookuptables`). También existe una nueva versión en `/admin/managelookuptables-new`, con el mismo contenido y un diseño diferente.

### 9.1 Categorías de publicaciones

- **Para qué sirven:** clasificar publicaciones (por ejemplo, novela, técnico, historia).
- **Campos:** nombre/descripción.
- **Impacto:** se utiliza en el formulario de publicación y en el gráfico de categorías.

### 9.2 Categorías multimedia

- **Para qué sirven:** clasificar audio/vídeo (por ejemplo, música, documental, pódcast).
- **Campos:** nombre/descripción (con detalles propios).
- **Impacto:** formulario multimedia y gráfico de categorías multimedia.

### 9.3 Formatos de publicaciones

- **Para qué sirven:** indicar el formato físico/digital de la publicación (por ejemplo, tapa dura, rústica, libro electrónico, PDF).
- **Impacto:** formulario de publicación y gráfico de formatos.

### 9.4 Formatos multimedia

- **Para qué sirven:** indicar el formato del archivo multimedia (por ejemplo, MP3, MP4, FLAC).
- **Impacto:** formulario multimedia y gráfico de formatos multimedia.

### 9.5 Editoriales

- **Para qué sirven:** indicar la editorial de la publicación.
- **Campos:** nombre y datos de contacto (cuando corresponda).
- **Impacto:** formulario de publicación, búsqueda en Google Books y gráfico de editoriales.

### 9.6 Ubicaciones de archivo

- **Para qué sirven:** indicar dónde se guarda el ejemplar/archivo (por ejemplo, estantería A, carpeta PDFs, nube).
- **Impacto:** formulario de publicación y diagnóstico de rutas.

### 9.7 Modelos de IA

- **Para qué sirven:** catalogar los modelos disponibles para Ollama local (y servir de referencia de configuración).
- **Campos:** nombre del modelo, dirección/base y parámetros.
- **Impacto:** selección del modelo local en Configuración y en el chat local.

### 9.8 Reglas para mantener las tablas

| Regla | Motivo |
|---|---|
| No eliminar valores **que se estén utilizando** | Se podrían romper asociaciones y dejar registros «huérfanos» |
| Es mejor **editar** que eliminar y volver a crear | Se mantienen las asociaciones existentes |
| Evitar duplicados | Confunden los gráficos y las búsquedas |
| Utilizar nombres breves y coherentes | Facilita el filtrado y la lectura |
| Revisar las tablas después de importaciones en lote | Los datos importados pueden generar duplicados |

> 💡 Si una categoría/editorial no se puede eliminar, es porque hay registros que la utilizan. Cambia primero la asociación.

### 9.9 Versión clásica y versión nueva

Ambas versiones ofrecen las mismas funciones:

- **Clásica** (`/admin/managelookuptables`) — pestañas.
- **Nueva** (`/admin/managelookuptables-new`) — barra lateral con grupos (Publicaciones, Media, IA) y tarjetas.

Puedes utilizar la que prefieras; los datos son los mismos.

---

## 10. Administración {#10-administração}

> Las siguientes secciones están dirigidas a usuarios con el rol **Admin**. Los GeneralUser no ven estos elementos en el menú.

### 10.1 Usuarios

Página: **Mantenimiento → Usuarios** (`/admin/manageusers`).

**Lista de usuarios:** muestra el usuario, el rol y las acciones disponibles.

**Crear un usuario:**

1. Haz clic en **Nuevo**.
2. Rellena el nombre, el correo electrónico y la contraseña.
3. Elige el **rol**: **Admin** o **GeneralUser**.
4. **Guarda**.

**Editar:**

1. Haz clic en **Editar** en la fila.
2. Modifica los datos o el rol.
3. **Guarda**.

**Borrar:**

1. Haz clic en **Borrar**.
2. Confirma la acción.

> ⚠️ Borrar un usuario puede eliminar u ocultar los datos asociados. Comprueba el impacto antes de hacerlo. Evita borrar al último Admin.

**Buenas prácticas:**

- Una cuenta de Admin por persona; no utilices una cuenta compartida.
- Utiliza GeneralUser para las tareas diarias.
- Desactiva o elimina las cuentas inactivas.
- Documenta quién tiene privilegios.

### 10.2 Configuración — visión general

Página: **Configuración** (`/admin/appsettings2`, en el pie del menú). También existe la versión clásica en `/admin/appsettings`.

La nueva versión organiza todo en una barra lateral con iconos y tarjetas a la derecha. Las secciones son:

| Sección | Contenido |
|---|---|
| **Descargas** | Carpeta de descargas multimedia |
| **Copias de seguridad** | Carpeta de copias de seguridad de las cargas |
| **Almacenamiento** | Carpetas de listados, imágenes de la aplicación, portadas de publicaciones e imágenes de autores |
| **Imágenes** | Configuración de imágenes |
| **Contactos** | Correo electrónico de soporte |
| **Otras opciones** | Tiempo de notificación, organización de archivos, eliminación de la imagen después de editarla |
| **Idioma** | Idioma predeterminado |
| **IA** | Configuración común, OpenRouter y Ollama (consulta 10.3) |
| **WebApi** | Dirección de la API |
| **Servicios** | Configuración de servicios |
| **Base de datos** | Conexión/estado |

> 💡 Los cambios de las carpetas afectan **al lugar desde el que se leen y en el que se escriben los archivos**. Cámbialas con cuidado y comprueba que las carpetas existan.

### 10.3 Área de IA {#103-área-ia}

El área **IA** tiene tres subsecciones:

#### Configuración común

- **Razonamiento** (activar/desactivar) — es común a ambos chats (nube y local). Cuando está activado, el razonamiento de los modelos aparece en el bloque «💭 Razonamiento».

#### OpenRouter

| Campo | Para qué sirve |
|---|---|
| **Clave de API** | Permite autenticarse en la nube |
| **Modelo predeterminado** | Modelo que se utilizará de forma predeterminada |
| **Permitir modelos de pago** | Si está desactivado, solo se utilizan modelos gratuitos |
| **Usar la nube para publicaciones de autor** | Habilita en la nube la generación de publicaciones de autor |
| **Límite diario de uso gratuito** | Número máximo de solicitudes gratuitas al día |
| **Tarjetas de uso** | Consumo actual e histórico |

**Notas:**

- Sin una clave, la nube no funciona y se recurre a Ollama.
- Si los **modelos de pago están desactivados**, la aplicación **no** los utiliza: elige modelos gratuitos equivalentes o el router gratuito.
- El límite se **aplica en el servidor**; al alcanzarlo, las operaciones en la nube devuelven «sin respuesta» y la aplicación utiliza el servicio local.

#### Ollama (local)

| Campo | Para qué sirve |
|---|---|
| **Clave de API** | Si Ollama la requiere |
| **Dirección base** | Ubicación donde se ejecuta Ollama |
| **Nombre del modelo** | Modelo local que se utilizará |

**Notas:**

- Ollama debe estar instalado y en ejecución en el equipo (o ser accesible desde la dirección indicada).
- Los modelos más grandes ofrecen mejores resultados, pero requieren más memoria y tardan más.

### 10.4 Registros

Página: **Registros** (`/admin/viewapplogs`).

**Qué muestra:** registros de la aplicación (fecha, nivel, mensaje), incluidos los errores.

**Cómo utilizarlos:**

1. Abre **Registros**.
2. Ordena por fecha (primero los más recientes).
3. Filtra por nivel (error, advertencia, información).
4. Utiliza el mensaje para diagnosticar el problema.

> 💡 El Panel solo muestra los errores recientes; en Registros puedes consultar el historial.

### 10.5 Gestor de archivos

Página: **Gestor de archivos** (`/admin/filemanager`).

**Qué permite:**

- Navegar por las carpetas de cargas y descargas.
- Ver los archivos y sus tamaños.
- **Cargar** archivos.
- **Descargar** archivos.
- **Crear carpetas**.
- **Borrar** archivos/carpetas (con confirmación).

**Precauciones:**

- Si borras archivos a los que hacen referencia los registros, estos se quedarán sin contenido.
- No muevas archivos manualmente sin actualizar sus rutas en la aplicación.
- Es preferible utilizar esta herramienta en vez de modificar directamente el sistema de archivos.

### 10.6 Copias de seguridad

#### Base de datos

Página: **Copia de seguridad → Base de datos** (`/backupdatabase`).

1. Abre la página.
2. Haz clic en el botón de **copia de seguridad**.
3. Confirma la operación.
4. Espera al mensaje de finalización.

**Qué copia:** la base de datos (registros, usuarios, configuración, conversaciones y valoraciones).

**Qué no copia:** los archivos (PDF, portadas y contenido multimedia).

#### Cargas

Hay tres páginas específicas:

| Página | Contenido | Archivo generado |
|---|---|---|
| `/backupuploads/1` | Cargas de **publicaciones** (PDF y portadas) | `PdfsAndCovers` (ZIP) |
| `/backupuploads/2` | Cargas de **contenido multimedia** | `MediaFiles` (ZIP) |
| `/backupuploads/3` | Cargas de **imágenes** | `AllAppImages` (ZIP) |

**Cómo utilizarlas:**

1. Abre la página del tipo que quieras.
2. Inicia la copia de seguridad.
3. Espera a que termine.
4. Guarda el archivo resultante en un lugar seguro.

> ⚠️ Una copia de seguridad completa incluye **la base de datos y las cargas**. Si solo haces una de las dos copias, el sistema quedará incoherente al restaurarlo.

### 10.7 Configuración de tablas de la aplicación

La aplicación incluye páginas auxiliares de configuración (`/admin/configapptables`, `/admin/adminpanel`) para situaciones avanzadas. **No son necesarias para el uso habitual.** Modifica sus opciones únicamente si sabes qué efecto tendrán.

### 10.8 Buenas prácticas de administración

- Haz copias de seguridad **periódicas** (base de datos + cargas).
- Prueba la restauración en una copia antes de que sea necesaria.
- Mantén en secreto la clave de OpenRouter.
- Revisa los registros cada semana.
- Limpia con cuidado las tablas auxiliares que no se utilicen.
- Documenta los cambios de configuración.
- Mantén Ollama y los modelos actualizados.

---

## 11. Informes {#11-relatórios}

La aplicación genera informes a partir de plantillas (RDLC) y los muestra en la propia interfaz.

### 11.1 Informe de autores

Página: **`/AuthorsPreview_Report`**.

**Cómo utilizarlo:**

1. Abre la página del informe.
2. Elige o escribe los parámetros (por ejemplo, un autor o un intervalo).
3. Haz clic en **Vista previa**.
4. Se genera y muestra el informe.
5. Utiliza las opciones de **exportación** disponibles (PDF/Excel, según la plantilla).

**Qué puedes esperar:**

- El informe utiliza los datos a los que tienes acceso (se aplica el aislamiento).
- Los informes grandes pueden tardar.
- Si no hay datos, el informe puede aparecer vacío.

### 11.2 Solución de problemas de informes

| Síntoma | Causa probable | Solución |
|---|---|---|
| Informe vacío | No hay datos para los parámetros indicados | Amplía el intervalo o elimina filtros |
| Error al generar | Falta la plantilla o hay datos no válidos | Comprueba los Registros; ponte en contacto con el Admin |
| Falla la exportación | Permisos o formato no compatible | Prueba otro formato; comprueba la carpeta |
| Tarda demasiado | Hay muchos registros | Limita los parámetros |

> 💡 El Admin mantiene las plantillas de informes. Si un informe da resultados incorrectos de forma sistemática, es necesario corregirlas.

---

## 12. Herramientas {#12-ferramentas}

### 12.1 Traductor

El traductor está disponible como **diálogo** en varias partes de la aplicación y como herramienta independiente.

**Funciones:**

- Selección de los idiomas de origen y destino.
- Indicador de progreso con el **nombre del modelo**.
- **Cancelar** (detiene la interfaz inmediatamente).
- **Confirmar** (aplica el texto).
- Un **indicador** del modelo que ha respondido.

**Buenas prácticas:**

- Traduce frases completas, no fragmentos.
- Revisa los nombres propios y los términos técnicos.
- Si cancelas, el texto original se mantiene intacto.

### 12.2 Prueba de rendimiento de IA

La aplicación recopila métricas de rendimiento de los modelos (latencia, tokens por segundo, uso) y las muestra en el Panel (sección **Actividad**) y en Configuración (tarjetas de uso).

**Para qué sirve:**

- Comparar modelos.
- Detectar una disminución del rendimiento.
- Ayudar a elegir el modelo predeterminado.

> 💡 La prueba de rendimiento es una herramienta de **observación**; no modifica el comportamiento de la aplicación.

### 12.3 El chat como herramienta de trabajo

Además de conversar, los chats pueden ayudar en las tareas cotidianas:

- Redactar sinopsis.
- Sugerir categorías.
- Traducir fragmentos.
- Resumir textos pegados.
- Corregir la ortografía.

> 💡 Para tareas estructuradas, utiliza las funciones específicas (Resumen, Traducir, Metadatos): los resultados se aplican directamente a los campos.

### 12.4 Recetas y contenidos auxiliares

Hay páginas auxiliares de recetas (`/recipes`, `/recipe_cards`) que se utilizan como contenido de demostración o ejemplo. No forman parte del flujo principal de catalogación.

---

## 13. Consejos y atajos {#13-dicas-e-atalhos}

### 13.1 Productividad

- **Enter** envía el mensaje en el chat.
- **Ctrl+V** pega texto/imágenes en los campos que lo admiten.
- Utiliza la **búsqueda de los selectores** (autor, editorial, categoría) en vez de recorrer listas largas.
- Ordena o filtra la cuadrícula antes de exportar o generar informes.
- Mantén **una conversación por tema** en el chat para que el historial resulte útil.

### 13.2 Calidad de los datos

- Rellena siempre **Título**, **Autor** y **Categoría**.
- Utiliza el **ISBN** para evitar confusiones.
- Evita los duplicados: busca antes de crear un registro.
- Mantén la coherencia de los nombres (por ejemplo, «J. R. R. Tolkien», no tres variantes).
- Revisa los indicadores de **Estado** del Panel.

### 13.3 Archivos y carpetas

- Evita caracteres problemáticos en los nombres (`/ \ : * ? " < > |`).
- Utiliza una carpeta para cada tipo (PDF, portadas, audio, vídeo).
- Haz copias de seguridad antes de realizar importaciones grandes.
- No muevas archivos fuera de la aplicación sin actualizar las rutas.

### 13.4 IA

- Revisa **siempre** lo que genere la IA.
- Para obtener resultados estructurados, utiliza las funciones específicas.
- Si la nube va lenta o no está disponible, la aplicación utiliza automáticamente el servicio local.
- En los modelos gratuitos puede aparecer el artefacto «User Safety»; responde «continúa».
- Si la respuesta está vacía, reformula la pregunta o cambia de modelo.

### 13.5 Seguridad

- No compartas la clave de OpenRouter.
- Cierra la sesión en los equipos compartidos.
- Utiliza contraseñas seguras y únicas.
- No uses la cuenta Admin para las tareas cotidianas.
- Desconfía de los prompts o archivos que soliciten datos sensibles.

### 13.6 Tema y accesibilidad

- El tema claro es mejor en lugares bien iluminados; el oscuro, en espacios con poca luz.
- El aspecto oscuro de la barra lateral y la barra superior es intencional y no cambia con el tema.
- Si el texto es pequeño, utiliza el zoom del navegador (Ctrl + / Ctrl -).
- Los principales contrastes cumplen las normas de accesibilidad (WCAG AA).

### 13.7 Tabla de acciones rápidas

| Quiero… | Acción |
|---|---|
| Abrir rápidamente una publicación | Buscar en la cuadrícula de publicaciones |
| Abrir este manual | Botón de ayuda (`?`) de la barra superior |
| Cambiar el idioma | Configuración → Idioma (la barra superior solo muestra el idioma actual) |
| Cambiar el tema | Icono del sol/luna |
| Cancelar una traducción | Botón **Cancelar** del diálogo |
| Copiar una respuesta de la IA | Icono de copia del mensaje |
| Valorar una respuesta | 👍 / 👎 en el mensaje |
| Borrar el historial | Historial → Borrar historial |
| Ver errores recientes | Panel → Auditoría / Registros |
| Hacer una copia de seguridad | Copia de seguridad → Base de datos + Cargas |

---

## 14. Solución de problemas {#14-resolução-de-problemas}

Esta sección cubre los problemas más habituales. Los síntomas se describen tal como los ve el usuario.

### 14.1 Autenticación y sesión

| Síntoma | Causa probable | Solución |
|---|---|---|
| «Credenciales no válidas» | Correo electrónico/contraseña incorrectos | Comprueba las mayúsculas y el teclado; restablece la contraseña |
| La sesión caduca rápidamente | Política de seguridad | Vuelve a iniciar sesión; ponte en contacto con el Admin |
| Se redirige al inicio de sesión desde todas las páginas | Cookie/sesión no válida | Borra las cookies del sitio; vuelve a iniciar sesión |
| No recibo el correo de recuperación | SMTP sin configurar/correo no deseado | Comprueba el correo no deseado; ponte en contacto con el Admin |

### 14.2 No aparecen los datos

| Síntoma | Causa probable | Solución |
|---|---|---|
| Han «desaparecido» publicaciones/autores | Cuenta incorrecta (aislamiento por usuario) | Comprueba el usuario; pide a un Admin que lo revise |
| La cuadrícula está vacía, pero hay registros | Hay una búsqueda o un filtro activos | Borra los filtros y la búsqueda |
| Los gráficos están vacíos | No se han rellenado los campos de clasificación | Rellena la categoría/formato/editorial |
| No se ven los registros de otro usuario | Comportamiento esperado para GeneralUser | Si es necesario, solicita acceso de Admin |

### 14.3 La IA no responde

| Síntoma | Causa probable | Solución |
|---|---|---|
| La traducción/el resumen no devuelve nada | La nube no está disponible y Ollama está detenido | Comprueba Ollama; vuelve a intentarlo |
| «Límite diario alcanzado» | Se ha alcanzado el límite de modelos gratuitos | Espera al día siguiente o pide al Admin que aumente el límite/habilite modelos de pago |
| Error en el chat en la nube | Falta la clave, se ha alcanzado el límite o hay inestabilidad | Comprueba la clave en Configuración; inténtalo más tarde; utiliza el chat local |
| Respuesta vacía | El modelo no ha generado contenido | Reformula la pregunta; elige otro modelo |
| «User Safety: safe;» | Artefacto del modelo | Responde «continúa» |
| La traducción tarda mucho | Modelo local grande | Cancela; utiliza la nube o un modelo más pequeño |
| No aparece el razonamiento | La opción está desactivada | Activa «Razonamiento» en Configuración común |
| Modelo inesperado | Selección/fallback automáticos | Consulta el indicador del modelo; fija el modelo predeterminado en Configuración |

### 14.4 Archivos

| Síntoma | Causa probable | Solución |
|---|---|---|
| «No se encuentra el archivo» | El archivo se ha movido o eliminado | Página `/pubsnotfoundondisk`; corrige la ruta o vuelve a cargarlo |
| Ruta incorrecta | La carpeta ha cambiado | Página `/pubswithwrongpath`; actualiza la ubicación |
| No aparece la portada | Falta la imagen o el formato no es válido | Vuelve a cargar la imagen |
| Falla la carga | Tamaño/formato/permisos | Reduce el archivo; comprueba el formato; consulta los Registros |
| El reproductor no reproduce | Códec/archivo no válido | Convierte el archivo a un formato compatible |

### 14.5 Informes

| Síntoma | Causa probable | Solución |
|---|---|---|
| Informe vacío | No hay datos o hay filtros | Amplía los criterios |
| Error al generar | Plantilla/datos | Consulta los Registros; ponte en contacto con el Admin |
| Falla la exportación | Permisos/carpeta | Prueba con otro formato o ubicación |

### 14.6 Entorno local (técnico, para Admin)

| Síntoma | Causa probable | Solución |
|---|---|---|
| La página no carga / conexión rechazada | La API o la interfaz están detenidas | Inicia ambos procesos |
| Conflicto en el puerto 5001 | De forma predeterminada, la API y la interfaz utilizan el mismo puerto | Utiliza perfiles/puertos distintos (por ejemplo, API 4100 e interfaz 4110) |
| Error de certificado en el navegador | Certificado autofirmado | Acepta el certificado; no actives HSTS hasta disponer de un certificado de confianza |
| `dotnet build` falla con MSB3027/MSB3021 | La aplicación en ejecución bloquea la DLL | Detén la aplicación y vuelve a compilar |
| Falla la restauración | Fuente NuGet/Syncfusion | Comprueba la configuración de la fuente y la clave |
| Falla el inicio de sesión SQL con `Trusted_Connection` | El grupo de aplicaciones tiene un nombre distinto del inicio de sesión | Haz coincidir el nombre del grupo de aplicaciones con el inicio de sesión SQL |

> 💡 Los detalles de implementación, puertos y SQL están documentados en las notas técnicas del proyecto (`AGENTS.md`), no en este manual de usuario.

### 14.7 Cuándo pedir ayuda

Recopila y envía esta información:

1. **Qué estabas haciendo** (página y acción).
2. **Qué esperabas** y **qué ocurrió**.
3. **Mensaje de error** (texto o captura de pantalla).
4. **Fecha/hora** aproximada.
5. **Usuario** (no envíes contraseñas).
6. Si es posible, los **Registros** correspondientes.

> ⚠️ No envíes nunca contraseñas ni la clave de OpenRouter en capturas de pantalla.

---

## 15. Glosario {#15-glossário}

| Término | Significado |
|---|---|
| **Admin** | Usuario con acceso completo, incluida la administración |
| **GeneralUser** | Usuario normal; solo ve sus propios datos |
| **Publicación** | Registro de una publicación/documento |
| **Media** | Registro de audio, vídeo o YouTube |
| **Lista de reproducción** | Colección ordenada de elementos multimedia |
| **Tabla auxiliar / de apoyo** | Lista de valores utilizada por los selectores (categorías, formatos, editoriales…) |
| **Lookup** | Sinónimo de tabla auxiliar |
| **OpenRouter** | Servicio en la nube que proporciona acceso a distintos modelos de IA |
| **Ollama** | Servicio local que ejecuta modelos de IA en el equipo |
| **Fallback** | Cambio automático al proveedor alternativo cuando falla el principal |
| **Cloud** | Proveedor remoto (OpenRouter) |
| **Local** | Proveedor que se ejecuta en el equipo (Ollama) |
| **Modelo** | Motor de IA (por ejemplo, llama-3.3-70b) |
| **Indicador del modelo** | Etiqueta que muestra qué modelo ha respondido |
| **Prompt** | Instrucción/texto que orienta a la IA |
| **Plantilla de prompt** | Prompt reutilizable y editable en el Editor de Prompts |
| **Streaming** | Respuesta que aparece progresivamente |
| **Razonamiento** | «Pensamiento» interno de algunos modelos, que se muestra por separado de la respuesta |
| **Historial** | Lista de conversaciones guardadas |
| **Rating** | Valoración personal (estrellas) |
| **Sinopsis** | Resumen/descripción de una publicación |
| **ISBN** | Identificador internacional de una publicación |
| **RDLC** | Formato de plantilla de informe |
| **Backup** | Copia de seguridad |
| **Log** | Registro de eventos/errores |
| **Aislamiento de datos** | Regla que impide que un usuario vea los datos de otro |
| **Tema** | Apariencia clara u oscura |
| **Cultura** | Idioma/región de la interfaz (pt, en, fr, es) |
| **Upload** | Carga de archivos en la aplicación |
| **Download** | Descarga de archivos desde la aplicación |
| **Cap** | Límite máximo (por ejemplo, solicitudes gratuitas al día) |
| **Token** | Unidad de texto utilizada por los modelos de IA |
| **Latencia** | Tiempo de respuesta |
| **TPS / Tokens por segundo** | Velocidad de generación del modelo |

---

## 16. Escenarios completos (paso a paso) {#16-cenários-completos-passo-a-passo}

Los siguientes escenarios combinan varias funciones y sirven como guía para realizar tareas reales.

### 16.1 Catalogar una publicación a partir del ISBN

**Objetivo:** crear en pocos minutos un registro completo, con datos oficiales y portada.

1. Abre **Publicaciones → Gestión** (`/publicacoes`).
2. Haz clic en **Nuevo**.
3. En el campo **ISBN**, introduce el número (con o sin guiones).
4. Abre **Buscar en línea** y busca por ISBN o título.
5. Elige el resultado correcto de la lista.
6. Comprueba los campos rellenados: **Título**, **Autor**, **Editorial**, **Año**, **Páginas**, **Sinopsis**.
7. Si falta el **Autor**, créalo primero en **Autores** y vuelve atrás (si sales sin guardar, perderás los datos del formulario; copia lo que ya hayas introducido).
8. Si falta la **Categoría**, el **Formato** o la **Ubicación de archivo**, elige una opción existente (o pide al Admin que la cree).
9. Si la sinopsis está en otro idioma, utiliza **Traducir** y confirma.
10. Carga la **portada** (si no se ha obtenido en la búsqueda).
11. Haz clic en **Guardar**.
12. Vuelve al Panel y comprueba que la publicación aparece en los recuentos y que la sección **Estado** ya no la indica como «sin portada/páginas».

**Errores habituales:**

| Error | Causa | Corrección |
|---|---|---|
| «Autor obligatorio» | No se ha seleccionado un autor | Selecciona o crea un autor |
| «Categoría obligatoria» | No se ha seleccionado una categoría | Selecciona una categoría |
| «Año de edición no válido» | El año tiene menos/más de 4 dígitos o es futuro | Corrígelo: debe tener 4 dígitos y ser ≤ el año actual |
| Registro duplicado | El ISBN ya existe | Busca antes de crear; unifica los registros |

### 16.2 Importar una carpeta de archivos PDF en lote

**Objetivo:** crear registros a partir de varios archivos y corregir después lo que falte.

1. Antes de importar, haz una **copia de seguridad** de la base de datos (`/backupdatabase`).
2. Prepara los archivos en una carpeta y asígnales nombres coherentes (`ISBN - Título.pdf`).
3. En **Publicaciones → Gestión**, utiliza **Importar PDF/DOCX** y selecciona los archivos.
4. Espera a que se procesen (puede tardar).
5. Abre la cuadrícula y ordena por **fecha** para ver los nuevos registros.
6. En cada registro:
   - Comprueba el **Título** y el **Autor**.
   - Completa la **Categoría**, la **Editorial**, el **Formato** y la **Ubicación de archivo**.
   - Añade la **portada** y la **sinopsis** (con IA si es necesario).
7. Al terminar, abre `/pubsnotfoundondisk` y `/pubswithwrongpath` y corrige las rutas.
8. Revisa la sección **Estado**: duplicados, sin portada y sin páginas.
9. Haz una nueva copia de seguridad.

**Consejos:**

- Importa lotes pequeños (10–20 archivos) para no perder trabajo si algo falla.
- Si no se reconoce un archivo, ábrelo primero y comprueba que no esté protegido o dañado.
- No borres los archivos originales hasta haber verificado los registros.

### 16.3 Preparar una ficha completa de autor

**Objetivo:** crear una ficha con fotografía, biografía revisada y perfil generado con IA.

1. Abre **Publicaciones → Autores** (`/autores`).
2. Haz clic en **Nuevo**, rellena el **Nombre** y haz clic en **Guardar**.
3. Abre la ficha del autor.
4. Carga la **fotografía** (de retrato y con buena resolución).
5. Escribe una **biografía básica** breve (2–4 frases) con los datos que conozcas.
6. Utiliza **Generar perfil IA** para completarla.
7. **Revisa** el resultado: nombres, fechas, obras y premios.
8. Corrige los datos erróneos y **guarda**.
9. Abre **Publicaciones del autor** y asocia o crea las obras.
10. Si la biografía está en otro idioma, utiliza **Traducir** y confirma.

**Atención:** la IA puede inventar fechas y obras. Antes de compartirlo, valida el perfil.

### 16.4 Crear una biblioteca de música y una lista de reproducción

**Objetivo:** cargar álbumes, completar los metadatos y escucharlos en una lista de reproducción.

1. Abre **Media → Gestión** (`/media`).
2. Haz clic en **Upload** y selecciona los archivos de audio.
3. En cada registro creado, abre **Editar** y comprueba:
   - **Título**, **Autor/Artista**, **Formato multimedia**, **Género/Categoría**, **Año**.
4. Utiliza **Metadatos IA** / **Notas IA** para completar lo que falte (revísalo siempre).
5. Abre **Media → Listas de reproducción** (`/media/playlists`).
6. Haz clic en **Nuevo**, asígnale un nombre (por ejemplo, «Jazz — noche») y haz clic en **Guardar**.
7. Abre la lista de reproducción → **Añadir elementos** → selecciona las pistas.
8. Cambia el orden si es necesario.
9. Haz clic en **Reproducir** para abrir el **Reproductor de listas de reproducción**.
10. Comprueba que se reproducen todas las pistas; corrige las que fallen.

**Consejos:**

- Es más útil crear una lista de reproducción para cada contexto que una lista enorme.
- Si no se reproduce una pista, comprueba el archivo en el **Gestor de archivos**.
- Comprueba que el **Formato multimedia** sea correcto para los gráficos del Panel.

### 16.5 Añadir y reproducir un vídeo de YouTube

1. Abre **Media → Gestión**.
2. Haz clic en **Nuevo** y elige el tipo **YouTube**.
3. Rellena el **Título**, el **Autor/Artista**, el **Género/Categoría** y el **Formato multimedia**.
4. Pega el **enlace** del vídeo (la URL completa).
5. **Guarda**.
6. En la cuadrícula, haz clic en **▶** para reproducirlo.
7. Si el reproductor muestra un error, comprueba lo siguiente:
   - El enlace es correcto y el vídeo no es privado.
   - El vídeo no se ha eliminado.
8. Para organizar el contenido, añade el elemento a una **lista de reproducción** (según la configuración, las listas pueden combinar audio, vídeo y YouTube).

### 16.6 Traducir y resumir sin perder el original

**Objetivo:** utilizar la IA sin alterar el texto original.

1. Abre la publicación o el autor y localiza el texto (sinopsis/biografía).
2. **Copia el texto original** en un bloc de notas o en el chat como referencia.
3. Abre **Traducir**, elige los idiomas y haz clic en **Traducir**.
4. Si tarda, haz clic en **Cancelar**; el campo permanecerá intacto.
5. Cuando termine, compara el resultado con el original.
6. Haz clic en **Confirmar** solo cuando estés conforme.
7. Para generar un resumen, utiliza **Resumen IA** y revísalo antes de guardarlo.
8. Guarda el registro.

> ⚠️ **Confirmar** aplica el texto al campo. Si quieres conservar las dos versiones, guarda primero el original en otro lugar.

### 16.7 Rutina de copias de seguridad y comprobación

**Objetivo:** asegurarte de que puedes recuperar el sistema.

1. Abre `/backupdatabase` y genera la copia de seguridad de la base de datos.
2. Abre `/backupuploads/1`, `/backupuploads/2` y `/backupuploads/3` y genera las copias de seguridad de las cargas.
3. Copia los archivos generados **fuera del equipo** (a un disco externo o a la nube).
4. Anota la fecha y el contenido de la copia.
5. **Prueba periódicamente la restauración** en una copia; una copia de seguridad que no se ha probado no es una copia de seguridad fiable.
6. Después de cualquier cambio, comprueba que las rutas de las carpetas de **Configuración** sigan siendo correctas.

### 16.8 Recuperarse de un cambio de carpetas

**Objetivo:** volver a tener todo en funcionamiento después de mover archivos.

1. Actualiza las carpetas en **Configuración** (Almacenamiento, Imágenes, Descargas, Copias de seguridad).
2. Abre `/pubsnotfoundondisk` y revisa qué archivos faltan.
3. Abre `/pubswithwrongpath` y corrige las rutas.
4. En el **Gestor de archivos**, comprueba que se vean las cargas.
5. Abre algunas publicaciones y comprueba que funcione **Leer PDF**.
6. Abre las galerías y comprueba que aparezcan las imágenes.
7. Haz una nueva copia de seguridad.

---

## Apéndice A — Rutas principales {#apêndice-a--rotas-principais}

| Ruta | Página |
|---|---|
| `/` | Panel |
| `/publicacoes` | Gestión de publicaciones |
| `/imagegallery` | Galería de imágenes |
| `/autores` | Autores |
| `/media` | Gestión multimedia |
| `/videogallery` | Galería de vídeos |
| `/media/playlists` | Listas de reproducción |
| `/media/playlists/{id}/player` | Reproductor de listas de reproducción |
| `/mp3player` | Reproductor MP3 |
| `/youtubeplayer` | Reproductor de YouTube |
| `/musicgallery` | Galería de música |
| `/edit-prompts` | Editor de prompts |
| `/ai/openrouter-chat` | Chat en la nube |
| `/ai/chat` | Chat local (Ollama) |
| `/pubsnotfoundondisk` | Publicaciones sin archivo |
| `/pubswithwrongpath` | Publicaciones con una ruta incorrecta |
| `/AuthorsPreview_Report` | Informe de autores |
| `/admin/manageusers` | Usuarios |
| `/admin/managelookuptables` | Tablas auxiliares (clásica) |
| `/admin/managelookuptables-new` | Tablas auxiliares (nueva) |
| `/admin/appsettings` | Configuración (clásica) |
| `/admin/appsettings2` | Configuración (nueva) |
| `/admin/filemanager` | Gestor de archivos |
| `/admin/viewapplogs` | Registros |
| `/backupdatabase` | Copia de seguridad de la base de datos |
| `/backupuploads/1` | Copia de seguridad de cargas de publicaciones |
| `/backupuploads/2` | Copia de seguridad de cargas multimedia |
| `/backupuploads/3` | Copia de seguridad de cargas de imágenes |

## Apéndice B — Lista de comprobación de mantenimiento periódico {#apêndice-b--checklist-de-manutenção-periódica}

**Diaria**

- [ ] Comprobar los errores recientes (Panel/Registros).
- [ ] Confirmar que la IA responde (en la nube o local).

**Semanal**

- [ ] Revisar los indicadores de **Estado**.
- [ ] Comprobar las copias de seguridad recientes.
- [ ] Limpiar las conversaciones antiguas, si es necesario.

**Mensual**

- [ ] Hacer una copia de seguridad completa (base de datos + cargas) y guardarla fuera del equipo.
- [ ] Revisar los usuarios y los roles.
- [ ] Revisar las tablas auxiliares (duplicados/huérfanos).
- [ ] Actualizar los modelos locales y comprobar el espacio en disco.

**Después de las importaciones o migraciones**

- [ ] Ejecutar `/pubsnotfoundondisk` y `/pubswithwrongpath`.
- [ ] Comprobar las portadas y los archivos.
- [ ] Revisar los títulos y autores duplicados.

---

## Apéndice C — Preguntas frecuentes (FAQ) {#apêndice-c--perguntas-frequentes-faq}

**P: ¿Por qué no aparece a otro usuario una publicación que he creado?**
R: Porque el aislamiento de datos se aplica por usuario. Solo un **Admin** puede ver los datos de todos. Un GeneralUser solo ve los suyos.

**P: He cambiado el idioma, pero mis publicaciones siguen en portugués. ¿Es un error?**
R: No. El selector de idioma traduce la **interfaz**. El **contenido** que has introducido (títulos, sinopsis, biografías) no se traduce automáticamente. Utiliza **Traducir** en esos campos.

**P: La IA tarda mucho. ¿Qué puedo hacer?**
R: Puedes **Cancelar** la operación (la interfaz se detiene inmediatamente). Si ocurre con frecuencia, probablemente estás usando el modelo **local** (Ollama); un modelo más pequeño o la nube responderán más rápido.

**P: La respuesta de la IA está vacía.**
R: La aplicación muestra un aviso en vez de una burbuja en blanco. Reformula la pregunta, vuelve a intentarlo o elige otro modelo en Configuración.

**P: Ha aparecido «User Safety: safe;» en una respuesta.**
R: Es un artefacto de algunos modelos gratuitos. Responde **«continúa»** o reformula la pregunta. La aplicación ya elimina esas líneas de las respuestas válidas.

**P: He agotado el límite diario de IA. ¿Y ahora qué?**
R: Las operaciones en la nube dejarán de funcionar y la aplicación recurrirá a **Ollama local** (si está disponible). El chat en la nube mostrará un mensaje claro. Espera al día siguiente o pide al Admin que revise el límite y los modelos de pago.

**P: El indicador muestra «Automático». ¿Qué modelo ha respondido?**
R: Significa que el router de la nube ha elegido el modelo. En ese caso quizá no se muestre el nombre exacto; el indicador señala el proveedor (OpenRouter) y que la selección ha sido automática.

**P: ¿Puedo recuperar una conversación borrada?**
R: No. Borrar el historial es **irreversible**. Las valoraciones asociadas quedan huérfanas y se ignoran.

**P: ¿Por qué la barra lateral sigue oscura cuando el tema es claro?**
R: Es intencional. La barra lateral y la barra superior tienen una identidad visual propia y no cambian con el tema.

**P: He borrado una categoría y las publicaciones se han quedado en un estado extraño.**
R: Probablemente la categoría estaba en uso. Es preferible **editarla** en vez de borrarla. Si es necesario, vuelve a crearla y asígnala de nuevo a las publicaciones.

**P: No se abre el archivo PDF de una publicación.**
R: Comprueba la ruta en `/pubsnotfoundondisk` y `/pubswithwrongpath`; revisa el **Gestor de archivos** y vuelve a cargar el archivo si es necesario.

**P: El informe aparece vacío.**
R: Comprueba los parámetros/filtros y que haya datos en el período indicado. Los informes respetan el aislamiento por usuario.

**P: ¿Puedo utilizar la aplicación sin internet?**
R: En parte. La aplicación y los datos locales funcionan, pero la IA en la **nube** no. **Ollama local** permite utilizar la IA sin internet, siempre que esté en ejecución.

**P: ¿Dónde está la clave de OpenRouter?**
R: En **Configuración → IA → OpenRouter** (solo para Admin). No la compartas ni la incluyas en capturas de pantalla.

**P: ¿Cómo puedo saber qué proveedor de IA se está utilizando?**
R: Consulta el **indicador del modelo** (OpenRouter/Ollama/Automático) y la sección **IA de uso interno** del Panel, donde también se muestran los fallbacks.

**P: ¿Qué es un «fallback»?**
R: Es el cambio automático de la nube al servicio local cuando falla la nube. Muchos fallbacks indican inestabilidad, falta de clave o que se ha agotado el límite.

---

*Fin del manual. Para obtener información técnica sobre la instalación, la implementación y la configuración del servidor, consulta la documentación técnica del proyecto.*
