using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

public class ProveedorCreateValidator : AbstractValidator<ProveedorCreateDto>
{
    public ProveedorCreateValidator()
    {
        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es requerida")
            .MaximumLength(150).WithMessage("La razón social no puede exceder 150 caracteres");

        RuleFor(x => x.Cuit)
            .MaximumLength(11).WithMessage("El CUIT no puede exceder 11 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Cuit));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no es válido")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
