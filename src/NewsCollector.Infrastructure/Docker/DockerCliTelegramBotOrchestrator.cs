using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Docker;

public sealed class DockerCliTelegramBotOrchestrator : ITelegramBotOrchestrator
{
    private readonly TelegramBotOrchestratorOptions _options;
    private readonly ILogger<DockerCliTelegramBotOrchestrator> _logger;

    public DockerCliTelegramBotOrchestrator(
        IOptions<TelegramBotOrchestratorOptions> options,
        ILogger<DockerCliTelegramBotOrchestrator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<OrchestratorResult> StartBotContainerAsync(Guid botId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return new OrchestratorResult(false, true, "Docker orchestrator disabled");
        }

        if (string.IsNullOrWhiteSpace(_options.WorkerConnectionString))
        {
            return new OrchestratorResult(false, false, "TelegramBotOrchestrator:WorkerConnectionString is not configured");
        }

        var containerName = BuildContainerName(botId);

        try
        {
            await RunDockerAsync($"rm -f {containerName}", cancellationToken);

            var runArgs =
                $"run -d --name {containerName} --restart unless-stopped " +
                $"--network {_options.DockerNetwork} " +
                $"-e TelegramBot__BotId={botId} " +
                $"-e TelegramBot__PollingIntervalSeconds=5 " +
                $"-e ConnectionStrings__DefaultConnection={Quote(_options.WorkerConnectionString)} " +
                _options.DockerImage;

            var containerId = await RunDockerAsync(runArgs, cancellationToken);
            _logger.LogInformation("Started Telegram bot container {ContainerName} ({ContainerId})", containerName, containerId.Trim());
            return new OrchestratorResult(true, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Telegram bot container for {BotId}", botId);
            return new OrchestratorResult(false, false, ex.Message);
        }
    }

    public async Task StopBotContainerAsync(Guid botId, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var containerName = BuildContainerName(botId);

        try
        {
            await RunDockerAsync($"rm -f {containerName}", cancellationToken);
            _logger.LogInformation("Stopped Telegram bot container {ContainerName}", containerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to stop Telegram bot container {ContainerName}", containerName);
        }
    }

    public static string BuildContainerName(Guid botId) => $"nc-telegram-bot-{botId:N}";

    private async Task<string> RunDockerAsync(string arguments, CancellationToken cancellationToken)
    {
        var dockerBinary = ResolveDockerBinary();
        var psi = new ProcessStartInfo
        {
            FileName = dockerBinary,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start docker process");

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var message = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
            throw new InvalidOperationException(message.Trim());
        }

        return stdout;
    }

    private static string ResolveDockerBinary()
    {
        foreach (var candidate in new[] { "/usr/bin/docker", "/usr/local/bin/docker", "docker" })
        {
            if (candidate == "docker" || File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            "Docker CLI not found in the API container. Rebuild the api image (Dockerfile.api installs docker.io).");
    }

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";
}
