using FluentValidation;
using MediaOrganizerApp.Application.DTOs;
using MediaOrganizerApp.Application.Formatting;
using System;
using System.Linq;

namespace MediaOrganizerApp.Application.Validators
{
    public class MediaValidator : AbstractValidator<MediaVM>
    {
        public MediaValidator()
        {
            RuleFor(p => p.FileName)
                .NotNull()
                .NotEmpty().WithMessage("Título deverá ser preenchido")
                .Length(2, 256).WithMessage("Tamanho do Título inválido");
            RuleFor(p => p.AnoEdicao)
                .Must(BeAValidYear)
                .When(p => !string.IsNullOrEmpty(p.AnoEdicao))
                .WithMessage("Ano de Edição inválido");
            RuleFor(p => p.DataMov)
                .Must(BeAValidDate).WithMessage("Data inválida");
            RuleFor(p => p.Autor)
                .NotNull()
                .NotEmpty().WithMessage("Campo 'Autor' deverá ser preenchido");
            RuleFor(p => p.IdFormato_Media)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Campo 'Formato' deverá ser preenchido");
            RuleFor(p => p.IdGenero)
                .GreaterThan(0)
                .NotNull().WithMessage("Campo 'Género/Categoria' deverá ser preenchido");
            RuleFor(p => (int)p.TipoMedia)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Campo 'Tipo' (Áudio/Vídeo) deverá ser preenchido");
        }

        #region Custom Validators
        protected bool BeAValidDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return false;

            description = description.Replace("'", " ").Replace("-", "").Replace(" ", "").Replace(".", "");
            return description.All(char.IsAscii);
        }

        protected bool BeAValidDate(DateTime dataMov)
        {
            bool isDateOk = true;
            if (!DataFormat.IsValidDate(dataMov))
            {
                isDateOk = false;
            }
            else if (dataMov > DateTime.Now)
            {
                isDateOk = false;
            }

            return isDateOk;
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
        #endregion
    }
}
