using FluentValidation.Results;

namespace MediaOrganizerApp.Application.Interfaces.Services.Validators
{
    public interface IValidationService
    {
        ValidationResult Validate<T>(T model);
    }
}