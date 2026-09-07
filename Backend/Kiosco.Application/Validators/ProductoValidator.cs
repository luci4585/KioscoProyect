using FluentValidation;
using Kiosco.Application.DTOs;

namespace Kiosco.Application.Validators;

public class ProductoCreateValidator : AbstractValidator<ProductoCreateDto>
{
    public ProductoCreateValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.RubroId)
            .GreaterThan(0).WithMessage("El rubro es requerido");

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo");

        RuleFor(x => x.Costo)
            .GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
    }
}

public class ProductoUpdateValidator : AbstractValidator<ProductoUpdateDto>
{
    public ProductoUpdateValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres");

        RuleFor(x => x.RubroId)
            .GreaterThan(0).WithMessage("El rubro es requerido");

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo");

        RuleFor(x => x.Costo)
            .GreaterThanOrEqualTo(0).WithMessage("El costo no puede ser negativo");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
    }
}
