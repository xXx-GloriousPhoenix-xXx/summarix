using Microsoft.Extensions.Options;
using Summarix.Application.Interfaces;
using Summarix.Domain.Exceptions;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Summarix.Infrastructure.Textify;

public class TextifyClient(HttpClient httpClient, IOptions<TextifySettings> options) : ITextifyClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly TextifySettings _settings = options.Value;

    public async Task<string> GetTextFromVideoAsync(
        Stream videoStream,
        string fileName,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var safeFileName = $"upload_{Guid.NewGuid():N}{extension}";
        var contentType = extension switch
        {
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".ogg" => "audio/ogg",
            _ => "application/octet-stream"
        };

        var streamContent = new StreamContent(videoStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "media", safeFileName);

        var targetLanguage = language ?? _settings.DefaultLanguage;
        content.Add(new StringContent(targetLanguage), "language");
        content.Add(new StringContent(_settings.Model), "model");
        content.Add(new StringContent("none"), "translation");
        content.Add(new StringContent("none"), "language_translation");

        try
        {
            using var response = await _httpClient.PostAsync(
                "/transcribe",
                content,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (errorBody.Contains("Failed to start transcription process", StringComparison.OrdinalIgnoreCase) ||
                    errorBody.Contains("does not contain any stream", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidMediaAudioException(
                        "The uploaded media file does not contain an audio stream or the audio track is corrupted."
                    );
                }

                throw new TranscriptionFailedException($"Txtify error: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(responseJson);

            var pid = 0;
            if (doc.RootElement.TryGetProperty("pid", out var pidProp))
            {
                pid = pidProp.GetInt32();
            }
            else
            {
                throw new TranscriptionFailedException($"Failed to obtain PID from Txtify response: {responseJson}");
            }

            await WaitForCompletionAsync(pid, cancellationToken);

            return await ExtractTranscriptionTextFromZipAsync(pid, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new TranscriptionFailedException("Failed to connect to Txtify service.", ex);
        }
    }

    private async Task WaitForCompletionAsync(int pid, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromSeconds(2);
        var timeout = TimeSpan.FromMinutes(_settings.TimeoutMinutes);
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        while (!linkedCts.Token.IsCancellationRequested)
        {
            await Task.Delay(delay, linkedCts.Token);

            using var statusResponse = await _httpClient.GetAsync($"/status?pid={pid}", linkedCts.Token);
            if (!statusResponse.IsSuccessStatusCode)
            {
                continue;
            }

            var statusBody = await statusResponse.Content.ReadAsStringAsync(linkedCts.Token);

            if (statusBody.Contains("completed", StringComparison.OrdinalIgnoreCase) ||
                statusBody.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                statusBody.Contains("done", StringComparison.OrdinalIgnoreCase) ||
                statusBody.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                statusBody.Contains("100%", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (statusBody.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                statusBody.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                throw new TranscriptionFailedException($"Transcription process failed: {statusBody}");
            }
        }

        throw new TranscriptionFailedException("Transcription process timed out.");
    }

    private async Task<string> ExtractTranscriptionTextFromZipAsync(int pid, CancellationToken cancellationToken)
    {
        using var zipResponse = await _httpClient.GetAsync($"/download?pid={pid}", cancellationToken);

        if (!zipResponse.IsSuccessStatusCode)
        {
            throw new TranscriptionFailedException($"Failed to download transcript zip: {zipResponse.StatusCode}");
        }

        await using var zipStream = await zipResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var memoryStream = new MemoryStream();
        await zipStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        await using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        var txtEntry = archive.Entries.FirstOrDefault(e => 
            e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && e.Length > 0);

        txtEntry ??= archive.Entries.FirstOrDefault(e => 
            (e.Name.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) || 
            e.Name.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase)) && e.Length > 0);

        txtEntry ??= archive.Entries.FirstOrDefault(e => e.Length > 0);

        if (txtEntry is null)
        {
            var entryNames = string.Join(", ", archive.Entries.Select(e => $"{e.FullName} ({e.Length} bytes)"));
            throw new TranscriptionFailedException($"Zip archive contains no readable text. Found entries: [{entryNames}]");
        }

        await using var entryStream = await txtEntry.OpenAsync(cancellationToken);
        using var reader = new StreamReader(entryStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
