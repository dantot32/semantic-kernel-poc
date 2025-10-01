using Microsoft.ML.OnnxRuntimeGenAI;
using SkOnnx;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "models",
           "Phi-4-mini-instruct-onnx", "cpu_and_mobile", "cpu-int4-rtn-block-32-acc-level-4");

using OgaHandle ogaHandle = new OgaHandle();
using Config config = new Config(modelPath);
using Model model = new Model(config);
using Tokenizer tokenizer = new Tokenizer(model);

var toolList = JsonSerializer.Serialize(AvailableFunctions.GetTools());

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
    Content = $@"You are a helpful assistant with these tools.

{toolList}

In addition to plain text responses, you can chose to call one or more of the provided functions.

Use the following rule to decide when to call a function:
  * if the response can be generated from your internal knowledge (e.g., as in the case of queries like ""What is the capital of Poland?""), do so
  * if you need external information that can be obtained by calling one or more of the provided functions, generate a function calls

If you decide to call functions:
  * prefix function calls with functools marker (no closing marker required)
  * all function calls should be generated in a single JSON list formatted as [{{""name"": [function name], ""arguments"": [function arguments as JSON]}}, ...]
  * follow the provided JSON schema. Do not hallucinate arguments or values. Do to blindly copy values from the provided samples
  * respect the argument type formatting. E.g., if the type if number and format is float, write value 7 as 7.0
  * make sure you pick the right functions that match the user intent",
    Tools = toolList
};
history.Add(systemMessage);

while (true)
{
    Console.Write("User > ");
    string? prompt = Console.ReadLine();

    // add message to history
    history.Add(new Message
    {
        Role = "user",
        Content = prompt
    });

    var messagesJson = JsonSerializer.Serialize(history.ToArray());

    var chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);

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

    // for example model should return syimply: Functools[{"name": "get_current_time", "arguments": {}}]
    var toolCalls = ToolCallHelper.ExtractToolCalls(response);

    if (toolCalls.Any())
    {

        Console.WriteLine($"[Tool Call] {response}");

        // 1. add assistant message with tool calls
        history.Add(new Message
        {
            Role = "assistant",
            Content = JsonSerializer.Serialize(toolCalls.ToArray())
        });

        // 2. execute each call
        List<ToolCallOutput> outputs = new();
        foreach (var toolCall in toolCalls)
        {
            try
            {
                var toolCallOutput = AvailableFunctions.ExecuteTool(toolCall);
                Console.WriteLine($"[Tool Call Output: {toolCall.Name}] {toolCallOutput.Output}");
                outputs.Add(toolCallOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Function execution failed: {ex.Message}");
            }
        }

        // 3. return final response
        history.Add(new Message
        {
            Role = "assistant",
            Content = JsonSerializer.Serialize(outputs.ToArray())
        });

        messagesJson = JsonSerializer.Serialize(history.ToArray());

        chatTemplate = tokenizer.ApplyChatTemplate("", messagesJson, "", true);

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