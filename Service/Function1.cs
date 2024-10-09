using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Service;

public class Function1
{
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _serviceBusQueueName = Environment.GetEnvironmentVariable("ServiceBusQueueName");

    public Function1(ILoggerFactory loggerFactory, JsonSerializerOptions jsonSerializerOptions,ServiceBusClient serviceBusClient)
    {
        _logger = loggerFactory.CreateLogger<Function1>();
        _serializerOptions = jsonSerializerOptions;
        _serviceBusClient = serviceBusClient;
    }

    [Function("Function1")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger received a request.");

        // Read and deserialize the incoming JSON payload
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        InputPayload payload;
        try
        {
            payload = JsonSerializer.Deserialize<InputPayload>(requestBody, _serializerOptions);
            if (payload == null || string.IsNullOrEmpty(payload.MessageId) || string.IsNullOrEmpty(payload.Content))
            {
                return await CreateBadRequestResponse(req, "Invalid payload. MessageId and Content are required.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize input JSON.");
            return await CreateBadRequestResponse(req, "Invalid JSON format.");
        }

        // Send the message to the Azure Service Bus
        try
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(_serviceBusQueueName);

            var message = new ServiceBusMessage(JsonSerializer.Serialize(payload, _serializerOptions))
            {
                MessageId = payload.MessageId
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation($"Message sent to Service Bus with ID: {payload.MessageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to Service Bus.");
            return req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
        }

        // Return success response
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync("Message processed successfully.");
        return response;
    }

    private async Task<HttpResponseData> CreateBadRequestResponse(HttpRequestData req, string errorMessage)
    {
        var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
        await response.WriteStringAsync(errorMessage);
        return response;
    }
}
