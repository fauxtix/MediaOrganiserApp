using System;

namespace MediaOrganizerApp.Application.Exceptions
{
    public class PublicationException : Exception
    {
        public PublicationException() : base() { }

        public PublicationException(string message) : base(message) { }

        public PublicationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
