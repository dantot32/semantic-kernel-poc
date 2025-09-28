using Microsoft.ML.OnnxRuntimeGenAI;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SkOnnx;

public class ToolCallHelper
{
    //public static string GenerateResponse(List<Message> history)
    //{
        //// Crea array di messaggi come nello script Python
        //var messages = history.Select(msg => new
        //{
        //    role = msg.Role,
        //    content = msg.Content,
        //    tools = msg.Tools,
        //    tool_calls = msg.ToolCalls,
        //    tool_call_id = msg.ToolCallId,
        //    name = msg.Name
        //}).Where(msg => !string.IsNullOrEmpty(msg.content) ||
        //                !string.IsNullOrEmpty(msg.tools) ||
        //                msg.tool_calls != null ||
        //                !string.IsNullOrEmpty(msg.tool_call_id));

        //var messagesJson = JsonSerializer.Serialize(messages.ToArray());
        ////Console.WriteLine($"[DEBUG] Messages: {messagesJson}");

        //// Usa il chat template come nello script Python
        //var chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);
        //var sequences = tokenizer.Encode(chatTemplate);

        //using var generator = new Generator(model, generatorParams);
        //generator.AppendTokenSequences(sequences);

        //var response = "";
        //while (!generator.IsDone())
        //{
        //    generator.GenerateNextToken();
        //    var lastTokenId = generator.GetSequence(0)[^1];
        //    var token = tokenizerStream.Decode(lastTokenId);
        //    response += token;
        //}

        //return response.Trim();
    //}

    public static List<FunctionCall> ExtractToolCalls(string response)
    {
        var toolCalls = new List<FunctionCall>();

        try
        {
            // Prova prima il parsing JSON diretto
            if (response.TrimStart().StartsWith("["))
            {
                var functionCalls = JsonSerializer.Deserialize<FunctionCall[]>(response);
                if (functionCalls != null)
                    toolCalls.AddRange(functionCalls);
            }
            else
            {
                // Cerca pattern JSON nell'output come nello script Python
                var jsonMatch = Regex.Match(response, @"\[.*?\]", RegexOptions.Singleline);
                if (jsonMatch.Success)
                {
                    var jsonPart = jsonMatch.Value;
                    var functionCalls = JsonSerializer.Deserialize<FunctionCall[]>(jsonPart);
                    if (functionCalls != null)
                        toolCalls.AddRange(functionCalls);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Could not parse tool calls: {ex.Message}");
        }

        return toolCalls;
    }

    public static string GenerateRandomId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 9)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
