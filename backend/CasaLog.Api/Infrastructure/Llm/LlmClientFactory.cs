using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace CasaLog.Api.Infrastructure.Llm;

public static class LlmClientFactory
{
    public static IChatClient Create(IConfiguration config)
    {
        var provider = config["Llm:Provider"] ?? "ollama";

        if (provider == "groq")
        {
            var apiKey = config["Llm:Groq:ApiKey"]
                ?? throw new InvalidOperationException("Llm:Groq:ApiKey not configured");
            var model = config["Llm:Groq:Model"] ?? "llama-3.3-70b-versatile";
            var endpoint = config["Llm:Groq:Endpoint"] ?? "https://api.groq.com/openai/v1/";

            return new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) }
            ).GetChatClient(model).AsIChatClient();
        }

        var ollamaModel = config["Llm:Ollama:Model"] ?? "llama3.2";
        var ollamaEndpoint = config["Llm:Ollama:Endpoint"] ?? "http://localhost:11434/v1/";

        return new OpenAIClient(
            new ApiKeyCredential("ollama"),
            new OpenAIClientOptions { Endpoint = new Uri(ollamaEndpoint) }
        ).GetChatClient(ollamaModel).AsIChatClient();
    }
}
