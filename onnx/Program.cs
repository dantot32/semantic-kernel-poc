using Microsoft.ML.OnnxRuntimeGenAI;
using SkOnnx;
using System.Text.Json;

var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "models",
           "Phi-4-mini-instruct-onnx", "cpu_and_mobile", "cpu-int4-rtn-block-32-acc-level-4");

using OgaHandle ogaHandle = new OgaHandle();
using Config config = new Config(modelPath);
using Model model = new Model(config);
using Tokenizer tokenizer = new Tokenizer(model);

using GeneratorParams generatorParams = new GeneratorParams(model);
generatorParams.SetSearchOption("max_length", 4096);
generatorParams.SetSearchOption("temperature", 0.00001);
generatorParams.SetSearchOption("top_p", 1.0);
generatorParams.SetSearchOption("do_sample", false);
using var tokenizerStream = tokenizer.CreateStream();
using var generator = new Generator(model, generatorParams);

List<Message> history = new();
var systemMessage = new Message
{
    Role = "system",
    Content = "You are a helpful assistant with these tools. Don't apologize, never",
    Tools = JsonSerializer.Serialize(AvailableFunctions.GetTools())
};
history.Add(systemMessage);

while (true)
{
    Console.Write("User > ");
    string? prompt = Console.ReadLine();

    history.Add(new Message
    {
        Role = "user",
        Content = prompt
    });

    var messages = history.Select(msg => new
    {
        role = msg.Role,
        content = msg.Content,
        tools = msg.Tools,
        tool_calls = msg.ToolCalls,
        tool_call_id = msg.ToolCallId,
        name = msg.Name
    }).Where(msg => !string.IsNullOrEmpty(msg.content) ||
                   !string.IsNullOrEmpty(msg.tools) ||
                   msg.tool_calls != null ||
                   !string.IsNullOrEmpty(msg.tool_call_id));

    var messagesJson = JsonSerializer.Serialize(messages.ToArray());

    var chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);
    //Console.WriteLine($"{chatTemplate}");
    var sequences = tokenizer.Encode(chatTemplate);

    generator.AppendTokenSequences(sequences);
    var response = "";
    while (!generator.IsDone())
    {
        generator.GenerateNextToken();
        var lastTokenId = generator.GetSequence(0)[^1];
        var token = tokenizerStream.Decode(lastTokenId);
        response += token;
    }

    var toolCalls = ToolCallHelper.ExtractToolCalls(response);

    if (toolCalls.Any())
    {
        // Aggiungi messaggio assistant con tool calls
        var toolCallId = ToolCallHelper.GenerateRandomId();
        history.Add(new Message
        {
            Role = "assistant",
            ToolCalls = new[] { new ToolCall
                    {
                        Type = "function",
                        Id = toolCallId,
                        Function = new FunctionCall { Name = "placeholder" }
                    }}
        });

        // Esegui ogni function call
        foreach (var toolCall in toolCalls)
        {
            try
            {
                var result = AvailableFunctions.ExecuteFunction(toolCall.Name, toolCall.Arguments);

                Console.WriteLine($"[Function {toolCall.Name}] {result}");

                // Aggiungi risultato tool
                history.Add(new Message
                {
                    Role = "tool",
                    ToolCallId = toolCallId,
                    Name = toolCall.Name,
                    Content = result
                });

                //var finalResponse = GenerateResponse();
                //Console.WriteLine($"Assistant > {finalResponse}");

                //history.Add(new Message
                //{
                //    Role = "assistant",
                //    Content = finalResponse
                //});
                messages = history.Select(msg => new
                {
                    role = msg.Role,
                    content = msg.Content,
                    tools = msg.Tools,
                    tool_calls = msg.ToolCalls,
                    tool_call_id = msg.ToolCallId,
                    name = msg.Name
                }).Where(msg => !string.IsNullOrEmpty(msg.content) ||
                               !string.IsNullOrEmpty(msg.tools) ||
                               msg.tool_calls != null ||
                               !string.IsNullOrEmpty(msg.tool_call_id));

                messagesJson = JsonSerializer.Serialize(messages.ToArray());

                chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);
                //Console.WriteLine($"{chatTemplate}");
                sequences = tokenizer.Encode(chatTemplate);

                generator.AppendTokenSequences(sequences);
                while (!generator.IsDone())
                {
                    generator.GenerateNextToken();
                    var lastTokenId = generator.GetSequence(0)[^1];
                    var token = tokenizerStream.Decode(lastTokenId);
                    Console.Write(token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Function execution failed: {ex.Message}");
            }
        }
    }
    else
    {
        // Risposta normale senza function calls
        Console.WriteLine($"Assistant > {response}");
        history.Add(new Message
        {
            Role = "assistant",
            Content = response
        });
    }

    Console.WriteLine();
}