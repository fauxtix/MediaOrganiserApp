using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace MediaOrganizerApp.Application.Exceptions
{
    public class CustomValidationException : Exception
    {
        public IList<ValidationFailure> Errors { get; }

        public CustomValidationException(IList<ValidationFailure> errors)
        {
            Errors = errors;
        }

        public CustomValidationException(string message, IList<ValidationFailure> errors) : base(message)
        {
            Errors = errors;
        }
    }
}