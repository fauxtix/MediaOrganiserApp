namespace MediaOrganizerApp.WebApi.Helpers.Validations
{
    using FluentValidation.Results;
    using MediaOrganizerApp.Application.Interfaces.Services.Validators;
    using System.Linq;

    public class ValidationServiceAdapter : IValidationService
    {
        private readonly IValidationService _validationService;

        public ValidationServiceAdapter(IValidationService validationService)
        {
            _validationService = validationService;
        }

        public ValidationResult Validate<T>(T model)
        {
            return _validationService.Validate(model);
        }
    }
}
