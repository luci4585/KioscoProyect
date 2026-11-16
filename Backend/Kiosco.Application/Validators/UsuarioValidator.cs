using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

public class UsuarioUpdateValidator : AbstractValidator<UsuarioUpdateDto>
{
    public UsuarioUpdateValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es requerido")
            .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

        RuleFor(x => x.NombreUsuario)
            .NotEmpty().WithMessage("El nombre de usuario es requerido")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El formato del email no es válido")
            .MaximumLength(150).WithMessage("El email no puede exceder 150 caracteres");
    }
}

public class UsuarioRolUpdateValidator : AbstractValidator<UsuarioRolUpdateDto>
{
    public UsuarioRolUpdateValidator()
    {
        RuleFor(x => x.RolId)
            .GreaterThan(0).WithMessage("El rol es requerido");
    }
}
