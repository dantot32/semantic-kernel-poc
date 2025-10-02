using System.Text.Json.Serialization;

namespace PhiOnnxFuncCalling;

public class Message
{
    [JsonPropertyName("role")] public string Role { get; set; } = ""; // system, assistant or user
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("tools")] public string? Tools { get; set; } // available list of tools in json
}

#region tool definition

// Functions versus tools: A function is a specific kind of tool, defined by a JSON schema
// example:
//{
//    "type": "function",
//    "name": "get_weather",
//    "description": "Retrieves current weather for the given location.",
//    "parameters": {
//        "type": "object",
//        "properties": {
//            "location": {
//                "type": "string",
//                "description": "City and country e.g. Bogotá, Colombia"
//            },
//            "units": {
//    "type": "string",
//                "enum": ["celsius", "fahrenheit"],
//                "description": "Units the temperature will be returned in."
//            }
//        },
//        "required": ["location", "units"],
//        "additionalProperties": false
//    },
//    "strict": true
//}

public class ToolDefinition
{
    [JsonPropertyName("type")] public string Type { get; set; } = "function";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("parameters")] public ParametersDefinition Parameters { get; set; } = new();
    [JsonPropertyName("strict")] public bool Strict { get; set; } = true;
}

public class ParametersDefinition
{
    [JsonPropertyName("type")] public string Type { get; set; } = "object";
    [JsonPropertyName("properties")] public Dictionary<string, ParameterDefinition> Properties { get; set; } = new();
    [JsonPropertyName("required")] public string[]? Required { get; set; }
    [JsonPropertyName("additionalProperties")] public bool AdditionalProperties { get; set; } = false;
}

public class ParameterDefinition
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("enum")] public string[]? Enum { get; set; }
}

#endregion

#region tool call (response from model)
public class ToolCall
{
    [JsonPropertyName("type")] public string Type { get; set; } = "function_call";
    [JsonPropertyName("call_id")] public string CallId { get; set; } = ""; // used later to submit the function result
    [JsonPropertyName("name")] public string Name { get; set; } = ""; // deserialized from model response
    [JsonPropertyName("arguments")] public Dictionary<string, object> Arguments { get; set; } = new(); // deserialized from model response
}

#endregion

#region tool call output

public class ToolCallOutput
{
    [JsonPropertyName("type")] public string Type { get; set; } = "function_call_output";
    [JsonPropertyName("call_id")] public string CallId { get; set; } = "";
    [JsonPropertyName("output")] public string Output { get; set; } = "";
}

#endregion