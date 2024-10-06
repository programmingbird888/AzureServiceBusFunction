using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,      // Case-insensitive property name matching
                WriteIndented = true,                    // Pretty-print JSON output
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Camel-case property names
                //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,  // Ignore null properties
                //AllowTrailingCommas = true               // Allow trailing commas in the JSON
            };

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(Services =>
                {
                    Services.AddSingleton<ServiceBusClient>(provider =>
                    {
                        return new ServiceBusClient(
                            Environment.GetEnvironmentVariable("ConnectionString"));
                    });
                })
                .ConfigureServices(Services =>
                {
                    Services.AddSingleton(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        AllowTrailingCommas = true
                    });
                })
                .Build();

            host.Run();
        }
    }
}
