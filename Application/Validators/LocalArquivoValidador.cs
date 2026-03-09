using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Domain.Entities;
using System.Linq;

namespace MediaOrganizerApp.Application.Validators
{
    public class LocalArquivoValidador : AbstractValidator<LocalArquivoVM>
    {
        public LocalArquivoValidador()
        {
            RuleFor(p => p.Descricao)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty().WithMessage("Preencha {PropertyName}, p.f.")
                .Length(2, 256).WithMessage("Tamanho ({TotalLength}) inválido na {PropertyName}")
                // implementar em validação à parte
                //.Must(NotExistInDatabase).WithMessage("{PropertyName} já existe na base de dados")
                .Must(BeAValidDescricao).WithMessage("{PropertyName} contém carateres inválidos");

        }

        #region Custom Validators
        protected bool BeAValidDescricao(string descricao)
        {
            descricao = descricao.Replace("'", " ").Replace("-", "").Replace(" ", "");
            return descricao.All(char.IsLetter);
        }
        #endregion
    }
}
