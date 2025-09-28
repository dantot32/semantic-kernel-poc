using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkOnnx;

public class ParameterDefinition
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("default")]
    public object? Default { get; set; }
}

public class ToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("parameters")]
    public Dictionary<string, ParameterDefinition> Parameters { get; set; } = new();
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("arguments")]
    public Dictionary<string, object> Arguments { get; set; } = new();
}

public class ToolCall
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("function")]
    public FunctionCall Function { get; set; } = new();
}

public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tools")]
    public string? Tools { get; set; }

    [JsonPropertyName("tool_calls")]
    public ToolCall[]? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
public static class AvailableFunctions
{
    public static List<ToolDefinition> GetTools()
    {
        return new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "get_weather",
                Description = "Get current weather information for a specific city",
                Parameters = new Dictionary<string, ParameterDefinition>
                {
                    ["city"] = new ParameterDefinition
                    {
                        Description = "The name of the city",
                        Type = "str",
                        Default = "London"
                    },
                    ["unit"] = new ParameterDefinition
                    {
                        Description = "Temperature unit",
                        Type = "str",
                        Default = "celsius"
                    }
                }
            },
            new ToolDefinition
            {
                Name = "calculate",
                Description = "Perform mathematical calculations",
                Parameters = new Dictionary<string, ParameterDefinition>
                {
                    ["expression"] = new ParameterDefinition
                    {
                        Description = "Mathematical expression to evaluate",
                        Type = "str"
                    }
                }
            },
            new ToolDefinition
            {
                Name = "get_current_time",
                Description = "Get the current date and time",
                Parameters = new Dictionary<string, ParameterDefinition>()
            }
        };
    }

    public static string ExecuteFunction(string functionName, Dictionary<string, object> arguments)
    {
        return functionName switch
        {
            "get_weather" => GetWeather(arguments),
            "calculate" => Calculate(arguments),
            "get_current_time" => GetCurrentTime(),
            _ => JsonSerializer.Serialize(new { error = "Function not found" })
        };
    }

    private static string GetWeather(Dictionary<string, object> arguments)
    {
        var city = arguments.GetValueOrDefault("city", "unknown city").ToString()!;
        var unit = arguments.GetValueOrDefault("unit", "celsius").ToString()!;

        var random = new Random();
        var temp = unit == "fahrenheit" ? random.Next(32, 100) : random.Next(0, 38);

        return $"The weather in {city} is {temp}°{(unit == "celsius" ? "C" : "F")} and sunny.";
    }

    private static string Calculate(Dictionary<string, object> arguments)
    {
        var expression = arguments.GetValueOrDefault("expression", "").ToString()!;

        try
        {
            var result = EvaluateExpression(expression);
            return $"The result of {expression} is {result}";
        }
        catch (Exception ex)
        {
            return $"Error calculating {expression}: {ex.Message}";
        }
    }

    private static string GetCurrentTime()
    {
        var now = DateTime.Now;
        return $"Current time is {now:yyyy-MM-dd HH:mm:ss} ({TimeZoneInfo.Local.DisplayName})";
    }
    private static double EvaluateExpression(string expression)
    {
        var dataTable = new System.Data.DataTable();
        var result = dataTable.Compute(expression, "");
        return Convert.ToDouble(result);
    }
}