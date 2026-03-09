using System;

namespace MediaOrganizerApp.Application.Exceptions
{
    public class PublicationNotFoundException : Exception
    {
        public PublicationNotFoundException()
        {
        }

        public PublicationNotFoundException(string message)
            : base(message)
        {
        }

        public PublicationNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
