using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Linq;

namespace MediaOrganizerApp.Application.Validators
{
    public class CategoriaValidator : AbstractValidator<CategoryVM>
    {
        public CategoriaValidator()
        {
            RuleFor(p => p.Descricao)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty().WithMessage("Preencha {PropertyName}, p.f.")
                .Length(2, 256).WithMessage("Tamanho ({TotalLength}) inválido na {PropertyName}");
                //.Matches("/^[\\w .,]+$/").WithMessage("Foram encontrados caracteres inválidos na Descrição");
                // implementar em validação à parte
                //.Must(NotExistInDatabase).WithMessage("{PropertyName} já existe na base de dados")
                //.Must(BeAValidDescricao).WithMessage("{PropertyName} contém carateres inválidos");

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
