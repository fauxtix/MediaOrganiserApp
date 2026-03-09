using FluentValidation;
using FluentValidation.Results;
using MediaOrganizerApp.Application.Interfaces.Services.Validators;
using System;
using System.Linq;

public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public ValidationResult Validate<T>(T model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        // Resolve the specific validator for type T
        var validator = (IValidator<T>)_serviceProvider.GetService(typeof(IValidator<T>));

        if (validator == null)
        {
            throw new InvalidOperationException($"No validator registered for type '{typeof(T).FullName}'");
        }

        var validationResult = validator.Validate(new ValidationContext<T>(model));

        // Filter out duplicate error messages
        var distinctFailures = validationResult.Errors
            .GroupBy(failure => failure.ErrorMessage)
            .Select(group => group.First())
            .ToList();

        return new ValidationResult(distinctFailures);
    }
}
