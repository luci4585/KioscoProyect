using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

public class CajaAperturaValidator : AbstractValidator<CajaAperturaDto>
{
    public CajaAperturaValidator()
    {
        RuleFor(x => x.MontoInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El monto inicial no puede ser negativo");
    }
}

public class CajaCierreValidator : AbstractValidator<CajaCierreDto>
{
    public CajaCierreValidator()
    {
        RuleFor(x => x.MontoFinalInformado)
            .GreaterThanOrEqualTo(0).WithMessage("El monto final no puede ser negativo");
    }
}

public class MovimientoCajaCreateValidator : AbstractValidator<MovimientoCajaCreateDto>
{
    public MovimientoCajaCreateValidator()
    {
        RuleFor(x => x.TipoMovimiento)
            .NotEmpty().WithMessage("El tipo de movimiento es requerido")
            .Must(t => new[] { "Ingreso", "Egreso" }.Contains(t))
            .WithMessage("El tipo de movimiento debe ser 'Ingreso' o 'Egreso'");

        RuleFor(x => x.Monto)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0");
    }
}
