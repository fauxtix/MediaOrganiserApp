namespace Web.Blazor.Services.ApiWrappers
{
    public sealed class GoogleBooksKeyNotConfiguredException : Exception
    {
        public GoogleBooksKeyNotConfiguredException(string message) : base(message)
        {
        }
    }
}