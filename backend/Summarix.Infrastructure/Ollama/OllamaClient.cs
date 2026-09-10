using Microsoft.Extensions.Options;
using Summarix.Application.Interfaces;
using Summarix.Domain.Exceptions;
using Summarix.Infrastructure.Contracts;
using System.Net.Http.Json;

namespace Summarix.Infrastructure.Ollama;

public class OllamaClient(HttpClient httpClient, IOptions<OllamaSettings> settings) : IOllamaClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OllamaSettings _settings = settings.Value;

    public async Task<string> GetSummaryFromTextAsync(
        string text,
        string? promptInstruction = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var systemPrompt = promptInstruction ?? _settings.DefaultSystemPrompt;
        var userContent = $"Transcript:\n\"\"\"\n{text}\n\"\"\"\n\nPlease provide a clear summary and key takeaways.";

        var requestPayload = new OllamaGenerateRequest(
            Model: _settings.Model,
            Prompt: userContent,
            System: systemPrompt,
            Stream: false
        );

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/generate",
                requestPayload,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SummarizationFailedException(
                    $"Ollama error: {response.StatusCode} - {errorBody}"
                );
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken
            );

            if (result is null || string.IsNullOrWhiteSpace(result.Response))
            {
                throw new SummarizationFailedException("Ollama returned an empty response.");
            }

            return result.Response.Trim();
        }
        catch (HttpRequestException ex)
        {
            throw new SummarizationFailedException("Failed to connect to Ollama service.", ex);
        }
    }
}