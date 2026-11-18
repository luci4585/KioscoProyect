using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

/// <summary>
/// Validación de ingresos de mercadería — Etapa 8 (RoadMap §11; RF-036, RF-039,
/// RF-076, RF-077).
/// </summary>
public class IngresoMercaderiaCreateValidator : AbstractValidator<IngresoMercaderiaCreateDto>
{
    public IngresoMercaderiaCreateValidator()
    {
        RuleFor(x => x.ProveedorId)
            .GreaterThan(0).WithMessage("El proveedor es requerido (RF-037)");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("El ingreso debe incluir al menos un producto");

        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.ProductoId)
                .GreaterThan(0).WithMessage("El producto es requerido");

            detalle.RuleFor(d => d.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");

            detalle.RuleFor(d => d.CostoUnitario)
                .GreaterThanOrEqualTo(0).WithMessage("El costo unitario no puede ser negativo (RF-039)");
        });
    }
}
