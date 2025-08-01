﻿using Common.Domain.Interface;
using Microsoft.Extensions.Logging;

namespace Common.Domain.Services;

public class NatiaHealthCheck : INatiaHealthCheck
{
    private readonly ILogger<NatiaHealthCheck> _logger;

    private const string BaseUrl = "http://192.168.0.79:2000/api/Controll/checkrobot";

    public NatiaHealthCheck(ILogger<NatiaHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task CheckHealth()
    {
        try
        {
            _logger.LogInformation("Initiating health check for Natia robot at {Url}", BaseUrl);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            var client = new HttpClient(handler);

            var response = await client.GetAsync(BaseUrl);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Natia robot is healthy: {Result}", result);
                await Console.Out.WriteLineAsync(result);
            }
            else
            {
                _logger.LogWarning("Health check failed with status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while checking robot health.");
            await Console.Out.WriteLineAsync("შეცდომა რობოტთან დაკავშირების მცდელობისას.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during health check.");
            await Console.Out.WriteLineAsync("შეცდომა რობოტთან დაკავშირების მცდელობისას.");
        }
    }
}
