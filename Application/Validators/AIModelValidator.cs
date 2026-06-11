using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaOrganizerApp.Application.Validators
{
    public class AIModelValidator : AbstractValidator<AIModelVM>
    {
        public AIModelValidator()
        {
            RuleFor(p => p.Descricao)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty().WithMessage("Preencha {PropertyName}, p.f.")
                .Length(2, 256).WithMessage("Tamanho ({TotalLength}) inválido na {PropertyName}");
        }

        #region Custom Validators
        protected bool BeAValidDescricao(string descricao)
        {
            descricao = descricao.Replace("'", " ").Replace("-", "").Replace(" ", "");
            return descricao.All(char.IsLetterOrDigit) || descricao.All(char.IsPunctuation) || descricao.All(char.IsSymbol);
        }
        #endregion
    }
}
