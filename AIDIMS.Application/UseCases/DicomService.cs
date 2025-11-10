using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIDIMS.Application.UseCases;

public class DicomService : IDicomService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<DicomService> _logger;

    public DicomService(IHttpClientFactory httpClientFactory, ILogger<DicomService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OrthancClient");

        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public async Task<DicomUploadResultDto?> UploadInstanceAsync(DicomUploadDto dicom)
    {
        if (dicom.File.Length == 0)
        {
            throw new ArgumentException("The provided DICOM file is empty.");
        }

        using var content = new StreamContent(dicom.File.OpenReadStream());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/dicom");

        var response = await _httpClient.PostAsync("/instances", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload DICOM instance. Status Code: {response.StatusCode}, Body: {errorBody}");
        }

        try
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Orthanc Response JSON: {JsonResponse}", jsonString);
            var result = JsonSerializer.Deserialize<DicomUploadResultDto>(jsonString, _jsonOptions);

            return result;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
