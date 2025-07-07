namespace Saigor.Data;

public class WeatherForecastService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Retorna previsões meteorológicas simuladas a partir de uma data inicial.
    /// </summary>
    /// <param name="startDate">Data inicial da previsão.</param>
    /// <returns>Array de WeatherForecast.</returns>
    public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
    {
        return Task.FromResult(Enumerable.Range(1, 5).Select(index =>
        {
            int tempC = Random.Shared.Next(-20, 55);
            return new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = tempC,
                TemperatureF = 32 + (int)(tempC / 0.5556),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }).ToArray());
    }
}