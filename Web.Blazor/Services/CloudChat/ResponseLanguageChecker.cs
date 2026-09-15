using System.Text.RegularExpressions;

namespace Web.Blazor.Services.CloudChat
{
    public enum DetectedLanguage
    {
        Portuguese,
        English,
        Inconclusive
    }

    public static partial class ResponseLanguageChecker
    {
        private const int PalavrasMinimas = 12;
        private const int MargemMinima = 2;

        public const int OcorrenciasMinimasMistura = 4;

        public const double FracaoMinimaMistura = 0.08;

        [GeneratedRegex("```.*?```", RegexOptions.Singleline)]
        private static partial Regex RegexCodigo();

        [GeneratedRegex(@"https?://\S+")]
        private static partial Regex RegexUrl();

        private static readonly string[] StopwordsPt =
        [
            "não", "que", "de", "para", "com", "uma", "como", "mais", "isso", "este",
            "esta", "são", "dos", "das", "porque", "está", "pode", "muito", "também",
            "mas", "seu", "sua", "quando", "entre", "sobre", "será", "então", "assim"
        ];

        private static readonly string[] StopwordsEn =
        [
            "the", "and", "is", "of", "to", "you", "with", "for", "that", "this",
            "are", "from", "your", "have", "will", "can", "not", "but", "they",
            "which", "their", "there", "would", "should", "about", "into", "than"
        ];

        private static readonly HashSet<string> IntrusoesEmTextoPt =
            StopwordsEn.Where(p => p != "for").ToHashSet(StringComparer.Ordinal);

        private static readonly HashSet<string> IntrusoesEmTextoEn = StopwordsPt.ToHashSet(StringComparer.Ordinal);

        private static readonly char[] DiacriticosPt = ['ã', 'õ', 'ç', 'á', 'é', 'í', 'ó', 'ú', 'â', 'ê', 'ô'];

        public static DetectedLanguage Detect(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DetectedLanguage.Inconclusive;

            string limpo = RegexUrl().Replace(RegexCodigo().Replace(text, " "), " ");
            string[] palavras = limpo.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            if (palavras.Length < PalavrasMinimas)
                return DetectedLanguage.Inconclusive;

            int pontosPt = 0;
            int pontosEn = 0;

            foreach (string palavraBruta in palavras)
            {
                string palavra = Normalizar(palavraBruta);

                if (StopwordsPt.Contains(palavra)) pontosPt++;
                else if (StopwordsEn.Contains(palavra)) pontosEn++;

                if (palavra.IndexOfAny(DiacriticosPt) >= 0) pontosPt++;
            }

            if (Math.Abs(pontosPt - pontosEn) < MargemMinima)
                return DetectedLanguage.Inconclusive;

            return pontosPt > pontosEn ? DetectedLanguage.Portuguese : DetectedLanguage.English;
        }

        public static bool ContemMisturaSignificativa(string? texto, string? codigoIdiomaBase, out int ocorrenciasIntrusao)
        {
            ocorrenciasIntrusao = 0;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            bool basePortuguesa = !string.Equals(codigoIdiomaBase?.Trim(), "en", StringComparison.OrdinalIgnoreCase);
            var intrusoes = basePortuguesa ? IntrusoesEmTextoPt : IntrusoesEmTextoEn;

            string limpo = RegexUrl().Replace(RegexCodigo().Replace(texto, " "), " ");
            string[] palavras = limpo.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (palavras.Length < PalavrasMinimas)
                return false;

            foreach (string palavraBruta in palavras)
            {
                if (intrusoes.Contains(Normalizar(palavraBruta)))
                    ocorrenciasIntrusao++;
            }

            return ocorrenciasIntrusao >= OcorrenciasMinimasMistura
                && ocorrenciasIntrusao >= palavras.Length * FracaoMinimaMistura;
        }

        private static string Normalizar(string palavraBruta) =>
            palavraBruta.Trim().ToLowerInvariant().Trim('.', ',', ';', ':', '!', '?', '"', '\'', '(', ')');

        public static bool IsMismatch(DetectedLanguage detected, string expectedTwoLetterCode)
        {
            var esperado = expectedTwoLetterCode?.ToLowerInvariant() switch
            {
                "pt" => DetectedLanguage.Portuguese,
                "en" => DetectedLanguage.English,
                _ => DetectedLanguage.Portuguese
            };

            return detected != DetectedLanguage.Inconclusive && detected != esperado;
        }
    }
}