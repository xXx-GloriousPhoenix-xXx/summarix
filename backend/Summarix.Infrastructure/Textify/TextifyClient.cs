using Microsoft.Extensions.Options;
using Summarix.Application.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using Summarix.Domain.Exceptions;

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
        content.Add(streamContent, "media", fileName);

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

                throw new TranscriptionFailedException(
                    $"Txtify error: {response.StatusCode} - {errorBody}"
                );
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("text", out var textElement))
            {
                return textElement.GetString() ?? string.Empty;
            }

            if (doc.RootElement.TryGetProperty("transcription", out var transcriptionElement))
            {
                return transcriptionElement.GetString() ?? string.Empty;
            }

            return responseJson;
        }
        catch (HttpRequestException ex)
        {
            throw new TranscriptionFailedException("Failed to connect to Txtify service.", ex);
        }
    }
}
