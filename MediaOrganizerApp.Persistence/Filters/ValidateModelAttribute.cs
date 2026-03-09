using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Interfaces.Services.Validators;
using System;
using System.Linq;

namespace MediaOrganizerApp.API.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly IValidationService _validationService;

        public ValidateModelAttribute(IValidationService validationService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var model = context.ActionArguments.Values.FirstOrDefault();
            if (model == null)
            {
                context.Result = new BadRequestObjectResult(new ApiResponse<object>(false, "Model is null", null));
                return;
            }

            var validationResult = _validationService.Validate(model);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var errorResponse = new ApiResponse<object>(false, "Validation failed", errors);
                context.Result = new BadRequestObjectResult(errorResponse);
            }
        }
    }
}
