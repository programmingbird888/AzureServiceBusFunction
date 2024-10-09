using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Azure.Core.Diagnostics;
using Azure.Core;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FunctionsDebugger.Enable();

            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            //var credential = new DefaultAzureCredential();
            var credentials = new ChainedTokenCredential(
                 new ManagedIdentityCredential(),  // for Azure environment
                 new AzureCliCredential(),       // for local development
                 new VisualStudioCodeCredential()
            );
            // Set up a listener to monitor logged events.
            AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            //TokenCredential credential = new DefaultAzureCredential();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(Services =>
                {
                    Services.AddSingleton<ServiceBusClient>(provider =>
                    {
                        return new ServiceBusClient(connectionString, credentials);
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
