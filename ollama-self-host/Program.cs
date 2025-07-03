using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = Kernel.CreateBuilder();

// Configure Ollama (OpenAI-compatible API)
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://localhost:11434"),
    apiKey: "dummy key"
);

var kernel = builder.Build();

// Run prompt
var result = await kernel.InvokePromptAsync("Ciao come stai?");
Console.WriteLine(result);