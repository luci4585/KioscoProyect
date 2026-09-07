namespace Kiosco.Application.DTOs;

public class CajaDto
{
    public int Id { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal? MontoFinal { get; set; }
    public decimal? Diferencia { get; set; }
    public bool Abierta { get; set; }
    public string UsuarioAperturaNombre { get; set; } = string.Empty;
    public string? UsuarioCierreNombre { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal SaldoCalculado { get; set; }
}

public class CajaAperturaDto
{
    public decimal MontoInicial { get; set; }
}

public class CajaCierreDto
{
    public decimal MontoFinalInformado { get; set; }
}

public class MovimientoCajaDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
}

public class MovimientoCajaCreateDto
{
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
}
