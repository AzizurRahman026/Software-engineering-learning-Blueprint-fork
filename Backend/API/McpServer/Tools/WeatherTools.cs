using Application.Common.Interfaces.Publisher;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace API.McpServer.Tools;

[McpServerToolType]
public class WeatherTools
{
    private readonly IMessageBus _messageBus;
    public WeatherTools(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [McpServerTool]
    [Description("Gets current weather for any city. Use when user asks about weather, temperature, or conditions anywhere.")]
    public async Task<string> GetWeather(
        [Description("The name of the city to get weather for")]
        string city
    )
    {
        return null;
        //var query = new GetWeatherQuery(city);
        //var result = await _messageBus.SendAsync<GetWeatherQuery, WeatherResult>(query);

        //return JsonSerializer.Serialize(result,
        //    new JsonSerializerOptions { WriteIndented = true });
    }
}


