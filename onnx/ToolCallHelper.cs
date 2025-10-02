using System.Text.Json;
using System.Text.RegularExpressions;

namespace PhiOnnxFuncCalling;

public class ToolCallHelper
{
    public static List<ToolDefinition> GetTools()
    {
        return new List<ToolDefinition>
        {
            new ToolDefinition
            {
                Name = "get_weather",
                Description = "Get current weather information for a specific city",
                Parameters = new ParametersDefinition
                {
                    Properties = new Dictionary<string, ParameterDefinition>
                    {
                        ["city"] = new ParameterDefinition
                        {
                            Description = "The name of the city",
                            Type = "str"
                        },
                        ["unit"] = new ParameterDefinition
                        {
                            Description = "Temperature unit",
                            Type = "str"
                        }
                    },
                    Required = new[] { "city" }
                }
            },
            new ToolDefinition
            {
                Name = "get_current_time",
                Description = "Get the current date and time",
                Parameters = new ParametersDefinition()
            }
        };
    }

    public static List<ToolCall> TryExtractCalls(string response)
    {
        
        List<ToolCall> result = new();

        try
        {
            ToolCall[] output = [];

            // try direct parsing for clean reponse
            if (response.TrimStart().StartsWith("["))
                output = JsonSerializer.Deserialize<ToolCall[]>(response);
            else
            {
                // use regex to parse dirty response
                var jsonMatch = Regex.Match(response, @"\[.*?\]", RegexOptions.Singleline);
                if (jsonMatch.Success)
                    output = JsonSerializer.Deserialize<ToolCall[]>(jsonMatch.Value);
            }

            for (int i = 0; i < output.Length; i++)
            {
                var tool = output[i];
                tool.CallId = GenerateRandomId();
                result.Add(tool);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Tool Call Attempt] {response}");
            Console.WriteLine($"[DEBUG] Could not parse tool calls: {ex.Message}");
        }

        return result;
    }

    public static ToolCallOutput TryExecuteTool(ToolCall call)
    {
        return new ToolCallOutput
        {
            CallId = call.CallId,
            Output = call.Name switch
            {
                "get_weather" => GetWeather(call.Arguments),
                "get_current_time" => GetCurrentTime(),
                _ => JsonSerializer.Serialize(new { error = "Function not found" })
            }
        };
    }

    #region tool implementation

    private static string GetWeather(Dictionary<string, object> arguments)
    {
        var city = arguments.GetValueOrDefault("city", "unknown city").ToString()!;
        var unit = arguments.GetValueOrDefault("unit", "celsius").ToString()!;

        var random = new Random();
        var temp = unit == "fahrenheit" ? random.Next(32, 100) : random.Next(0, 38);

        return $"The weather in {city} is {temp}°{(unit == "celsius" ? "C" : "F")} and sunny.";
    }

    private static string GetCurrentTime()
    {
        var now = DateTime.Now;
        return $"Current time is {now:yyyy-MM-dd HH:mm:ss} ({TimeZoneInfo.Local.DisplayName})";
    }

    #endregion

    #region utils

    public static string GenerateRandomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 9)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion
}
