using MediaOrganizerApp.Application.DTOs;
using Microsoft.Extensions.Localization;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Web.Blazor.Services.DocumentHelpers
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _Uri;
        private readonly IConfiguration _env;
        private readonly IStringLocalizer<App> _localizer;

        private string cultureCookie;

        public PdfGeneratorService(HttpClient httpClient, IConfiguration env, IStringLocalizer<App> localizer)
        {
            _httpClient = httpClient;
            _env = env;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("ApiKey", _env["ApiKey"]);
            _Uri = $"{_env["BaseUrl"]}/api/authors";
            _localizer = localizer;
        }

        private const float MarginLeft = 10;
        private const float MarginTop = 10;
        private const float RowHeight = 20;
        private const float LineSpacing = 5;
        private const float ColumnPadding = 10;
        private const float VerticalPadding = 5;
        private const float PageWidth = 500;


        public byte[] GeneratePublicacoesPdf(List<PubsWithDescriptions> publicacoes)
        {
            using var document = new PdfDocument();
            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            var contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;

            var columns = new List<(string ColumnName, float XPosition)>
            {
                (_localizer["lblTitulo"], 20),
                (_localizer["Category"], 190),
                (_localizer["Year"], 360),
                (_localizer["lblPaginas"], 420)
            };

            float yPosition = DrawGenericHeader(graphics, titleFont, contentFont, _localizer["frmPreviewLivrosGenerico"], columns);

            foreach (var pub in publicacoes)
            {
                if (yPosition > page.GetClientSize().Height - 40)
                {
                    AddNewPage(document, ref page, ref graphics, ref yPosition, titleFont, contentFont, (g, t, c) => DrawGenericHeader(g, t, c, "Publica��es Report", columns));
                }

                var rowColumns = new List<(string Value, float XPosition, float Width)>
                {
                    (pub.Titulo, 20, 170),
                    (pub.Categoria, 190, 170),
                    (pub.AnoEdicao, 360, 50),
                    (pub.Paginas.ToString(), 420, 50)
                };

                yPosition = DrawGenericRow(graphics, contentFont, rowColumns, yPosition);
            }

            DrawFooters(document, contentFont);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        public byte[] CreateMediaPdf(List<MediaVM> mediaList)
        {
            try
            {
                using var document = new PdfDocument();
                var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
                var contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

                int pageNumber = 1;
                PdfPage page = document.Pages.Add();
                PdfGraphics graphics = page.Graphics;

                var columns = new List<(string ColumnName, float XPosition)>
                {
                    (_localizer["lblTitulo"], 20),
                    (_localizer["lblAutor"], 150),
                    (_localizer["lblCategoria"], 280),
                    (_localizer["lblTipoUtilizador"], 410)
                };

                float yPosition = DrawGenericHeader(graphics, titleFont, contentFont, _localizer["MediaReport"], columns);

                foreach (var media in mediaList)
                {
                    if (yPosition > page.GetClientSize().Height - 40)
                    {
                        AddNewPage(document, ref page, ref graphics, ref yPosition, titleFont, contentFont, (g, t, c) => DrawGenericHeader(g, t, c, "Media Report", columns));
                    }

                    string mediaType = DetermineMediaType(media);

                    var rowColumns = new List<(string Value, float XPosition, float Width)>
                    {
                        (media.FileName ?? "N/A", 20, 120),
                        (media.Autor ?? "N/A", 150, 120),
                        (media.Genero ?? "N/A", 280, 120),
                        (mediaType, 410, 80)
                    };

                    yPosition = DrawGenericRow(graphics, contentFont, rowColumns, yPosition);
                }

                DrawFooters(document, contentFont);

                using var stream = new MemoryStream();
                document.Save(stream);
                return stream.ToArray();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating PDF: {ex.Message}");
                throw;
            }
        }

        public byte[] CreateAuthorsPdf(List<AutorDTO> authorsList)
        {
            using var document = new PdfDocument();
            var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
            var contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 9);

            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;
            float yPosition = DrawHeader(graphics, titleFont, contentFont);
            int pageNumber = 1;

            foreach (var author in authorsList)
            {
                if (yPosition > page.GetClientSize().Height - 100)
                {
                    page = document.Pages.Add();
                    graphics = page.Graphics;
                    yPosition = DrawHeader(graphics, titleFont, contentFont);
                    pageNumber++;
                }

                float parentStartY = yPosition;

                float rowCenterY = yPosition + (RowHeight - titleFont.MeasureString(author.Descricao).Height) / 2;
                graphics.DrawString(author.Descricao, titleFont, PdfBrushes.Black, new PointF(MarginLeft + ColumnPadding, rowCenterY));
                yPosition += RowHeight + LineSpacing;

                graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(MarginLeft, yPosition, PageWidth - MarginLeft * 2, RowHeight));
                graphics.DrawString(_localizer["lblTitulo"], contentFont, PdfBrushes.Black, new PointF(20 + ColumnPadding, yPosition + VerticalPadding));
                graphics.DrawString(_localizer["lblEditora"], contentFont, PdfBrushes.Black, new PointF(260 + ColumnPadding, yPosition + VerticalPadding));
                yPosition += RowHeight;

                var uri = $"{_Uri}/PublicacoesCatalogadas/{author.Id}";
                List<PublicacoesVM> authorPubs = _httpClient
                    .GetFromJsonAsync<IEnumerable<PublicacoesVM>>(uri).Result
                    .ToList();

                foreach (var pub in authorPubs)
                {
                    if (yPosition > page.GetClientSize().Height - 40)
                    {
                        graphics.DrawRectangle(new PdfPen(PdfBrushes.Black, 1f), new RectangleF(MarginLeft, parentStartY, PageWidth - MarginLeft * 2, yPosition - parentStartY));

                        page = document.Pages.Add();
                        graphics = page.Graphics;
                        yPosition = DrawHeader(graphics, titleFont, contentFont);
                        pageNumber++;
                        parentStartY = yPosition;
                    }

                    var titleBounds = new RectangleF(20 + ColumnPadding, yPosition + VerticalPadding, 240 - 2 * ColumnPadding, RowHeight * 2);
                    var publisherBounds = new RectangleF(260 + ColumnPadding, yPosition + VerticalPadding, 240 - 2 * ColumnPadding, RowHeight * 2);

                    var titleHeight = contentFont.MeasureString(pub.Titulo, titleBounds.Width).Height;
                    var publisherHeight = contentFont.MeasureString(pub.Editora, publisherBounds.Width).Height;

                    float rowHeight = Math.Max(Math.Max(titleHeight, publisherHeight) + 2 * VerticalPadding, RowHeight);

                    var thickPen = new PdfPen(PdfBrushes.Black, 1f);
                    graphics.DrawRectangle(thickPen, new RectangleF(MarginLeft, yPosition, PageWidth - MarginLeft * 2, rowHeight));

                    graphics.DrawString(pub.Titulo, contentFont, PdfBrushes.Black, titleBounds);
                    graphics.DrawString(pub.Editora, contentFont, PdfBrushes.Black, publisherBounds);

                    yPosition += rowHeight + LineSpacing;
                }

                graphics.DrawRectangle(new PdfPen(PdfBrushes.Black, 1f), new RectangleF(MarginLeft, parentStartY, PageWidth - MarginLeft * 2, yPosition - parentStartY));

                yPosition += LineSpacing;
            }

            DrawFooters(document, contentFont);

            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private float DrawHeader(PdfGraphics graphics, PdfFont titleFont, PdfFont contentFont)
        {
            graphics.DrawString(
                _localizer["frmPreviewAutoresGenerico"],
                titleFont,
                PdfBrushes.Black,
                new PointF(MarginLeft, MarginTop)
            );

            float y = MarginTop + 30;

            graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(MarginLeft, y, PageWidth - MarginLeft * 2, RowHeight));
            graphics.DrawString(_localizer["lblNomeAutor"], contentFont, PdfBrushes.Black, new PointF(20 + ColumnPadding, y + VerticalPadding));

            return y + RowHeight;
        }

        private string DetermineMediaType(MediaVM media)
        {
            if (media == null)
            {
                return "Outra";
            }

            string ext = string.IsNullOrEmpty(media.FilePath) ? string.Empty : Path.GetExtension(media.FilePath).ToLower();
            string mediaFormat = media.FormatoMedia?.ToLower() ?? string.Empty;

            if (!string.IsNullOrEmpty(media.FilePath) && !IsPhysicalMedia(mediaFormat))
            {
                return DetermineMediaTypeByExtension(ext, media.IdFormato_Media);
            }

            if (!string.IsNullOrEmpty(media.FileUrl) && !IsPhysicalMedia(mediaFormat))
            {
                return "Youtube";
            }

            if (string.IsNullOrEmpty(media.FileUrl) && IsPhysicalMedia(mediaFormat))
            {
                return DeterminePhysicalMediaType(media.IdFormato_Media);
            }

            return "Outra";
        }

        private bool IsPhysicalMedia(string mediaFormat)
        {
            return mediaFormat == "cd" || mediaFormat == "lp" || mediaFormat.StartsWith("dvd");
        }

        private string DetermineMediaTypeByExtension(string ext, int? idFormatoMedia)
        {
            return ext switch
            {
                ".mp3" => "MP3",
                ".mp4" => DetermineMp4MediaType(idFormatoMedia),
                _ => "Outra"
            };
        }

        private string DetermineMp4MediaType(int? idFormatoMedia)
        {
            return idFormatoMedia switch
            {
                1 => "MP4",
                2 => "CD",
                3 => "LP",
                4 => "DVD",
                9 => "Outra",
                _ => "Outra"
            };
        }

        private string DeterminePhysicalMediaType(int? idFormatoMedia)
        {
            return idFormatoMedia switch
            {
                2 => "CD",
                3 => "LP",
                4 => "DVD - Filme",
                5 => "DVD - Audio",
                9 => "Local",
                _ => "Outra"
            };
        }

        private void DrawFooter(PdfGraphics graphics, PdfFont font, int pageNumber, int totalPages)
        {
            string footerText = $"{_localizer["lblPagina"]} {pageNumber} / {totalPages}";
            var footerSize = font.MeasureString(footerText);
            float xPosition = (PageWidth - footerSize.Width) / 2;
            float yPosition = graphics.ClientSize.Height - 20;

            graphics.DrawString(footerText, font, PdfBrushes.Gray, new PointF(xPosition, yPosition));
        }

        private void DrawFooters(PdfDocument document, PdfFont font)
        {
            int totalPages = document.Pages.Count;
            for (int i = 0; i < totalPages; i++)
            {
                var page = document.Pages[i];
                var graphics = page.Graphics;
                DrawFooter(graphics, font, i + 1, totalPages);
            }
        }

        private void AddNewPage(PdfDocument document, ref PdfPage page, ref PdfGraphics graphics, ref float yPosition, PdfFont titleFont, PdfFont contentFont, Func<PdfGraphics, PdfFont, PdfFont, float> drawHeader)
        {
            page = document.Pages.Add();
            graphics = page.Graphics;
            yPosition = drawHeader(graphics, titleFont, contentFont);
        }

        private float DrawGenericHeader(PdfGraphics graphics, PdfFont titleFont, PdfFont contentFont, string reportTitle, List<(string ColumnName, float XPosition)> columns)
        {
            graphics.DrawString(
                reportTitle,
                titleFont,
                PdfBrushes.Black,
                new PointF(MarginLeft, MarginTop)
            );

            float y = MarginTop + 30;

            graphics.DrawRectangle(PdfBrushes.LightGray, new RectangleF(MarginLeft, y, PageWidth - MarginLeft * 2, RowHeight));

            foreach (var column in columns)
            {
                graphics.DrawString(column.ColumnName, contentFont, PdfBrushes.Black, new PointF(column.XPosition + ColumnPadding, y + VerticalPadding));
            }

            return y + RowHeight;
        }

        private float DrawGenericRow(PdfGraphics graphics, PdfFont font, List<(string Value, float XPosition, float Width)> columns, float y)
        {
            float maxHeight = RowHeight;

            foreach (var column in columns)
            {
                var bounds = new RectangleF(column.XPosition + ColumnPadding, y + VerticalPadding, column.Width - 2 * ColumnPadding, RowHeight * 2);
                var textHeight = font.MeasureString(column.Value, bounds.Width).Height;
                maxHeight = Math.Max(maxHeight, textHeight + 2 * VerticalPadding);
            }

            var thickPen = new PdfPen(PdfBrushes.Black, 1f);
            graphics.DrawRectangle(thickPen, new RectangleF(MarginLeft, y, PageWidth - MarginLeft * 2, maxHeight));

            foreach (var column in columns)
            {
                var bounds = new RectangleF(column.XPosition + ColumnPadding, y + VerticalPadding, column.Width - 2 * ColumnPadding, RowHeight * 2);
                graphics.DrawString(column.Value, font, PdfBrushes.Black, bounds);
            }

            return y + maxHeight + LineSpacing;
        }

    }
}
