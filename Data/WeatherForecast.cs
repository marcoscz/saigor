using System.ComponentModel.DataAnnotations;

namespace Saigor.Data;

public class WeatherForecast
{
    /// <summary>
    /// The date of the forecast.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Temperature in Celsius.
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Temperature in Fahrenheit (calculated).
    /// </summary>
    public int TemperatureF { get; set; }

    /// <summary>
    /// Weather summary.
    /// </summary>
    [StringLength(100)]
    public string? Summary { get; set; }

    /// <summary>
    /// Inicializa uma nova instância de WeatherForecast.
    /// </summary>
    public WeatherForecast() { }
}