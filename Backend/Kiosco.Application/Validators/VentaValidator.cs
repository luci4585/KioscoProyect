using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

public class VentaCreateValidator : AbstractValidator<VentaCreateDto>
{
    public VentaCreateValidator()
    {
        RuleFor(x => x.MetodoPago)
            .NotEmpty().WithMessage("El método de pago es requerido")
            .Must(m => new[] { "Efectivo", "Débito", "Crédito", "Transferencia" }.Contains(m))
            .WithMessage("El método de pago no es válido");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La venta debe tener al menos un producto");

        RuleForEach(x => x.Detalles).ChildRules(detalle =>
        {
            detalle.RuleFor(d => d.ProductoId)
                .GreaterThan(0).WithMessage("El producto es requerido");

            detalle.RuleFor(d => d.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0");
        });
    }
}
