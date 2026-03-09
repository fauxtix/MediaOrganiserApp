namespace Web.Blazor.Static
{
    public static class Endpoints
    {
        public static string BaseUrl { get; private set; }

        public static string PublicationsEndpoint => $"{BaseUrl}/publications";
        public static string MediaEndpoint => $"{BaseUrl}/media";
        public static string AuthorsEndpoint => $"{BaseUrl}/authors";
        public static string CategoriesEndpoint => $"{BaseUrl}/categories";
        public static string PublishersEndpoint => $"{BaseUrl}/publishers";
        public static string PublicationsFormatsEndpoint => $"{BaseUrl}/formatospublicacoes";
        public static string MediaFormatsEndpoint => $"{BaseUrl}/formatosmedia";
        public static string StoragePlacesEndpoint => $"{BaseUrl}/storagelocations";
        public static string DashboardEndpoint => $"{BaseUrl}/dashboard";
        public static string PdfEndpoint => $"{BaseUrl}/pdf";
        public static string MostUsedEndpoint => $"{BaseUrl}/mostused";

        public static void Initialize(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
    }
}
