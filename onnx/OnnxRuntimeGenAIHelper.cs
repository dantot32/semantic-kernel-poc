using Microsoft.ML.OnnxRuntimeGenAI;
using SkOnnx;
using System.Text.Json;

namespace PhiOnnxFuncCalling;

public static class OnnxRuntimeGenAIHelper
{
    //private static void EnrichHistory(List<Message> history, Tokenizer tokenizer, Generator generator)
    //{
    //    var messages = history.Select(msg => new
    //    {
    //        role = msg.Role,
    //        content = msg.Content,
    //        tools = msg.Tools,
    //        tool_calls = msg.ToolCalls,
    //        tool_call_id = msg.ToolCallId,
    //        name = msg.Name
    //    }).Where(msg => !string.IsNullOrEmpty(msg.content) ||
    //                   !string.IsNullOrEmpty(msg.tools) ||
    //                   msg.tool_calls != null ||
    //                   !string.IsNullOrEmpty(msg.tool_call_id));

    //    var messagesJson = JsonSerializer.Serialize(messages.ToArray());
    //    var chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);
    //    var sequences = tokenizer.Encode(chatTemplate);
    //    generator.AppendTokenSequences(sequences);
    //}
    //public static string GenerateCompleteResponse(List<Message> history, Tokenizer tokenizer, Generator generator)
    //{

    //    EnrichHistory(history, tokenizer, generator);

    //    while (!generator.IsDone())
    //        generator.GenerateNextToken();

    //    var outputSequence = generator.GetSequence(0);
    //    return tokenizer.Decode(outputSequence);
    //}

    //public static string GenerateStreamingResponse(List<Message> history, Tokenizer tokenizer, Generator generator, bool print = false)
    //{

    //    EnrichHistory(history, tokenizer, generator);

    //    using var tokenizerStream = tokenizer.CreateStream();

    //    var response = ""; 
    //    while (!generator.IsDone())
    //    {
    //        generator.GenerateNextToken();
    //        var lastTokenId = generator.GetSequence(0)[^1];
    //        var token = tokenizerStream.Decode(lastTokenId);
    //        response += token;

    //        if(print)
    //            Console.Write(token);
    //    }

    //    return response;
    //}
}
