using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

/// <summary>
/// Validación de movimientos manuales de stock — Etapa 7 (RoadMap §10;
/// RF-076, RF-077).
/// </summary>
public class StockMovimientoValidator : AbstractValidator<StockMovimientoDto>
{
    public StockMovimientoValidator()
    {
        RuleFor(x => x.ProductoId)
            .GreaterThan(0).WithMessage("El producto es requerido");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");
    }
}
