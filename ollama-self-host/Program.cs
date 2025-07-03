using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

// OllamaFunctionCalling -> https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Demos/OllamaFunctionCalling/Program.cs
// https://github.com/microsoft/semantic-kernel/tree/main/dotnet/src/Connectors/Connectors.Ollama
// https://www.nuget.org/packages/Microsoft.SemanticKernel.Connectors.Ollama/1.35.0-alpha
// dotnet add package Microsoft.SemanticKernel.Connectors.Ollama --version 1.35.0-alpha

var builder = Kernel.CreateBuilder();
var modelId = "mistral";
var endpoint = new Uri("http://localhost:11434");

#pragma warning disable SKEXP0070 
builder.Services.AddOllamaChatCompletion(modelId, endpoint);
#pragma warning restore SKEXP0070

var kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

#pragma warning disable SKEXP0070
var settings = new OllamaPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
#pragma warning restore SKEXP0070

// statefull logic
ChatHistory history = new();

history.AddSystemMessage(" some system prompt ");

Console.Write("> ");

string? prompt = null;

while ((prompt = Console.ReadLine()) is not null)
{
    Console.WriteLine();

    try
    {
        history.AddUserMessage(prompt);

        var response = chatCompletionService.GetStreamingChatMessageContentsAsync(history, settings, kernel);

        await foreach (var chunk in response)
            Console.Write(chunk);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n\n> ");
    }

    // add new line
    Console.Write("\n\n> ");
}
