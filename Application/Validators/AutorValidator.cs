using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using System.Linq;

namespace MediaOrganizerApp.Application.Validators
{
    public class AutorValidator : AbstractValidator<AutorDTO>
    {
        public AutorValidator()
        {
            RuleFor(p => p.Descricao)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .NotEmpty().WithMessage("Nome do Autor deverá ser preenchido")
                .Length(2, 128).WithMessage("Campo 'Nome' deverá ter entre 2 e 128 caracteres");

                // implementar em validação à parte
                //.Must(NotExistInDatabase).WithMessage("{PropertyName} já existe na base de dados")
        }

        #region Custom Validators
        protected bool BeAValidDescricao(string descricao)
        {
            descricao = descricao.Replace("'", " ").Replace("-", "").Replace(" ", "");
            var isLetterOrDigit = descricao.All(char.IsLetterOrDigit);
            var isPunctuation = descricao.All(char.IsPunctuation);
            return isLetterOrDigit || isPunctuation;
        }
        #endregion
    }
}
