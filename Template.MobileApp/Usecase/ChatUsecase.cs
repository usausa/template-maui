namespace Template.MobileApp.Usecase;

using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.AI;

using OllamaSharp;
using OllamaSharp.Models.Exceptions;

using Template.MobileApp.Services;

using AiMessage = Microsoft.Extensions.AI.ChatMessage;

//--------------------------------------------------------------------------------
// Tool
//--------------------------------------------------------------------------------

public sealed class DeviceStatusFunction : AIFunction
{
    private readonly IDeviceInfo deviceInfo;

    private readonly DeviceState deviceState;

    public override string Name => "get_device_status";

    public override string Description => "Gets the battery, network or hardware information of this device.";

    public override JsonElement JsonSchema { get; } = JsonElement.Parse(
        """
        {
          "type": "object",
          "properties": {
            "kind": { "type": "string", "enum": ["battery", "network", "device"] }
          },
          "required": ["kind"]
        }
        """);

    public DeviceStatusFunction(
        IDeviceInfo deviceInfo,
        DeviceState deviceState)
    {
        this.deviceInfo = deviceInfo;
        this.deviceState = deviceState;
    }

    protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        arguments.TryGetValue("kind", out var kind);
        var result = kind?.ToString() switch
        {
            "battery" => $"Level: {deviceState.BatteryChargeLevel * 100:F0}%, State: {deviceState.BatteryState}, Power source: {deviceState.BatteryPowerSource}",
            "network" => $"Access: {deviceState.NetworkAccess}, Profile: {deviceState.NetworkProfile}, Wi-Fi signal (dBm): {deviceState.WiFiSignalStrength?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            "device" => $"Manufacturer: {deviceInfo.Manufacturer}, Model: {deviceInfo.Model}, OS: {deviceInfo.Platform} {deviceInfo.VersionString}",
            _ => "Unknown kind."
        };
        return ValueTask.FromResult<object?>(result);
    }
}

public sealed class TodoListFunction : AIFunction
{
    private readonly TimeProvider timeProvider;

    private readonly DataService dataService;

    public override string Name => "list_todos";

    public override string Description => "Lists the to-do items of this app.";

    public override JsonElement JsonSchema { get; } = JsonElement.Parse(
        """
        {
          "type": "object",
          "properties": {}
        }
        """);

    public TodoListFunction(
        TimeProvider timeProvider,
        DataService dataService)
    {
        this.timeProvider = timeProvider;
        this.dataService = dataService;
    }

    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        builder.Append("Today: ").AppendLine(timeProvider.GetLocalNow().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        foreach (var entity in await dataService.QueryTodoListAsync().ConfigureAwait(false))
        {
            builder.Append(entity.IsDone ? "[x] " : "[ ] ").Append(entity.Title);
            if (entity.DueDate is { } dueDate)
            {
                builder.Append(", due ").Append(dueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            if (entity.IsImportant)
            {
                builder.Append(", important");
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }
}

public sealed class TodoAddFunction : AIFunction
{
    private readonly TimeProvider timeProvider;

    private readonly DataService dataService;

    public override string Name => "add_todo";

    public override string Description => "Adds a to-do item to this app.";

    public override JsonElement JsonSchema { get; } = JsonElement.Parse(
        """
        {
          "type": "object",
          "properties": {
            "title": { "type": "string" }
          },
          "required": ["title"]
        }
        """);

    public TodoAddFunction(
        TimeProvider timeProvider,
        DataService dataService)
    {
        this.timeProvider = timeProvider;
        this.dataService = dataService;
    }

    protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
    {
        var title = arguments.TryGetValue("title", out var value) ? value?.ToString()?.Trim() : null;
        if (String.IsNullOrEmpty(title))
        {
            return "Title is required.";
        }

        var now = timeProvider.GetLocalNow().DateTime;
        await dataService.InsertTodoAsync(new TodoEntity { Title = title, Note = string.Empty, CreatedAt = now, UpdatedAt = now }).ConfigureAwait(false);
        return $"Added: {title}";
    }
}

//--------------------------------------------------------------------------------
// Session
//--------------------------------------------------------------------------------

public sealed record ChatAnswer(
    string Text,
    bool Canceled,
    string? Error);

public sealed class ChatSession : IDisposable
{
    private readonly IOllamaApiClient ollama;

    private readonly IChatClient client;

    private readonly ChatOptions options;

    private readonly List<AiMessage> history = [];

    private bool? toolSupported;

    public string Model { get; }

    internal ChatSession(string endPoint, string model, ChatOptions options)
    {
        var api = new OllamaApiClient(new Uri(endPoint), model);
        ollama = api;
        client = new ChatClientBuilder(api).UseFunctionInvocation().Build();
        this.options = options;
        Model = model;
    }

    public void Dispose() => client.Dispose();

    public async ValueTask<ChatAnswer> SendAsync(string text, IProgress<string> progress, CancellationToken cancel)
    {
        history.Add(new AiMessage(ChatRole.User, text));

        var builder = new StringBuilder();
        string? error = null;
        try
        {
            await Task.Run(
                async () =>
                {
                    toolSupported ??= (await ollama.ShowModelAsync(Model, cancel).ConfigureAwait(false)).Capabilities?.Contains("tools") ?? false;
                    await foreach (var update in client.GetStreamingResponseAsync(history, toolSupported.Value ? options : null, cancel).ConfigureAwait(false))
                    {
                        if (update.Text.Length == 0)
                        {
                            continue;
                        }

                        builder.Append(update.Text);
                        progress.Report(builder.ToString());
                    }
                },
                cancel).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex) when (ex is HttpRequestException or OllamaException or WebException or IOException)
        {
            if (!cancel.IsCancellationRequested)
            {
                error = ex.Message;
            }
        }

        var answer = builder.ToString();
        if (error is not null)
        {
            history.RemoveAt(history.Count - 1);
            return new ChatAnswer(answer, false, error);
        }

        if (!cancel.IsCancellationRequested || (answer.Length > 0))
        {
            history.Add(new AiMessage(ChatRole.Assistant, answer));
        }
        else
        {
            history.RemoveAt(history.Count - 1);
        }

        return new ChatAnswer(answer, cancel.IsCancellationRequested, null);
    }
}

//--------------------------------------------------------------------------------
// Usecase
//--------------------------------------------------------------------------------

public sealed class ChatUsecase
{
    private readonly Settings settings;

    private readonly ChatOptions options;

    public ChatUsecase(
        TimeProvider timeProvider,
        IDeviceInfo deviceInfo,
        Settings settings,
        DeviceState deviceState,
        DataService dataService)
    {
        this.settings = settings;
        options = new ChatOptions
        {
            Tools =
            [
                new DeviceStatusFunction(deviceInfo, deviceState),
                new TodoListFunction(timeProvider, dataService),
                new TodoAddFunction(timeProvider, dataService)
            ]
        };
    }

    public ChatSession CreateSession() =>
        new(settings.OllamaEndPoint, settings.OllamaModel, options);
}
