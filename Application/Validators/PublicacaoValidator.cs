using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Formatting;
using System;
using System.IO;
using System.Linq;

namespace MediaOrganizerApp.Application.Validators
{
    public class PublicacaoValidator : AbstractValidator<PubsWithDescriptions>
    {
        public PublicacaoValidator()
        {
            RuleFor(p => p.Titulo)
                .NotNull().WithMessage("Título requerido")
                .NotEmpty().WithMessage("Título requerido")
                .Length(2, 256).WithMessage("Tamanho ({TotalLength}) inválido no Título");
            RuleFor(p => p.IdAutor)
                .NotNull()
                .GreaterThan(0).WithMessage("Autor requerido");
            RuleFor(p => p.IdCategoria)
                .NotNull()
                .GreaterThan(0).WithMessage("Categoria requerida");
            RuleFor(p => p.IdEditora)
                .NotNull()
                .GreaterThan(0).WithMessage("Editora requerida");
            RuleFor(p => p.IdFormato)
                .NotNull()
                .GreaterThan(0).WithMessage("Formato requerido");
            RuleFor(p => p.IdLocalArquivo)
                .NotNull()
                .GreaterThan(0).WithMessage("Local de Arquivo requerido");
            RuleFor(p => p.AnoEdicao)
                .Must(BeAValidYear)
                .When(p => !string.IsNullOrEmpty(p.AnoEdicao) ||
                        !DataFormat.IsNumeric(p.AnoEdicao) ||
                        int.Parse(p.AnoEdicao) < 1900 ||
                        int.Parse(p.AnoEdicao) > DateTime.Now.Year)
                .WithMessage("Ano de Edição inválido");
        }

        #region Custom Validators
        protected bool BeAValidDescription(string description)
        {
            description = description.Replace("'", " ").Replace("-", "").Replace(" ", "").Replace(".", "");
            return description.All(char.IsLetterOrDigit) || description.All(char.IsSymbol);
        }

        protected bool BeAValidYear(string year)
        {
            bool isYearOk = true;
            if (year.Length == 4)
            {
                if (!DataFormat.IsNumeric(year))
                    isYearOk = false;
                else if (int.Parse(year) > DateTime.Now.Year)
                    isYearOk = false;
            }
            else
                isYearOk = false;

            return isYearOk;
        }

        protected bool ExistInDiskIfFilled(string ficheiro)
        {
            if (!string.IsNullOrEmpty(ficheiro))
                return File.Exists(path: ficheiro);

            return true;
        }
        protected bool BeAValidTitle(string titulo)
        {
            titulo.Replace("'", " ").Replace("-", "");
            return titulo.All(char.IsLetter);
        }

        #endregion
    }
}
