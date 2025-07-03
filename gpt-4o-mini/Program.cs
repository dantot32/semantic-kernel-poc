using Gpt4oMini;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using Spectre.Console;
using NAudio.Wave;
using OpenAI.Chat;

AnsiConsole.MarkupLine("[bold yellow]Premi Alt+R per iniziare la registrazione e di nuovo Alt+R per terminare...[/]");

string apiKey = "< api key >";

// 1. add kernel to collection
ServiceCollection services = new();
services.AddOpenAIChatCompletion("gpt-4o-mini", apiKey);
#pragma warning disable SKEXP0010
services.AddOpenAIAudioToText("whisper-1", apiKey);
services.AddOpenAITextToAudio("tts-1", apiKey);
services.AddKernel();
IServiceProvider provider = services.BuildServiceProvider();

// 2.create and register the plugin
Kernel kernel = provider.GetRequiredService<Kernel>();
kernel.ImportPluginFromType<TestPlugin>();

IChatCompletionService chatService = provider.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0001
IAudioToTextService audioToTextService = provider.GetRequiredService<IAudioToTextService>();
ITextToAudioService textToAudioService = provider.GetRequiredService<ITextToAudioService>();

// temp file
string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); ;
string name = Guid.NewGuid().ToString() + "-console-ai.wav";
string fullPath = Path.Combine(path, name);

// 3. specify prompt settings and pass to chat service
PromptExecutionSettings chatSettings = new OpenAIPromptExecutionSettings()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};
OpenAIAudioToTextExecutionSettings audioToTextSettings = new(fullPath)
{
    Language = "it",
    ResponseFormat = "json",
    Filename = name
};
OpenAITextToAudioExecutionSettings textToAudioSettings = new()
{
    Voice = "fable", // Supported voices are alloy, echo, fable, onyx, nova, and shimmer.
    ResponseFormat = "mp3", // Supported formats are mp3, opus, aac, and flac
    Speed = 0.93F
};

// statefull logic
ChatHistory history = new();

history.AddUserMessage("Sei un agente turistico. Il tuo scopo è prendere prenotazioni alberghiere. Nella prenotazione ti deve essere indicato: il nome e cognome del prenotante, data di arrivo e partenza, albergo.");

while (true)
{

    Console.Write("User > ");
    string? prompt = "";

    ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

    if (keyInfo.Modifiers == ConsoleModifiers.Alt && keyInfo.Key == ConsoleKey.R)
    {
        if (NAudioHelper.RecordAudio(fullPath))
        {
            // audio to text
            var bytes = File.ReadAllBytes(fullPath);
            var promptAudioContent = new AudioContent(bytes, mimeType: null);
            var transcript = await audioToTextService.GetTextContentAsync(promptAudioContent, audioToTextSettings);
            prompt = transcript.Text;

            // cleanup temp data
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            Console.WriteLine($"User > {prompt}");

        }
    }
    else
    {
        prompt = Console.ReadLine();
    }

    if (string.IsNullOrWhiteSpace(prompt))
        continue;

    if (prompt == "esci")
        break;

    // text to text
    history.AddUserMessage(prompt);
    var result = await chatService.GetChatMessageContentAsync(history, chatSettings, kernel);
    history.Add(result);

    // text to audio
    var mp3Path = Path.ChangeExtension(fullPath, ".mp3");
    AudioContent resultAudioContent = await textToAudioService.GetAudioContentAsync(result.ToString(), textToAudioSettings);
    await File.WriteAllBytesAsync(mp3Path, resultAudioContent.Data?.ToArray());
    NAudioHelper.PlayAudio(mp3Path);

    // cleanup temp data
    if (File.Exists(mp3Path))
        File.Delete(mp3Path);

    Console.WriteLine($"Assistant > {result}");
}

