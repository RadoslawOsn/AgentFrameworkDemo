using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using TicketingMcpServer.Tools;

namespace TicketingMcpServer;

/// <summary>
/// MCP Server for Customer Ticketing System
/// Provides tools for managing service requests, tickets, and customer information
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.Error.WriteLine("Starting Ticketing MCP Server...");

        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
        
        builder.Logging.SetMinimumLevel(LogLevel.Warning);

        var host = builder.Build();
        
        Console.Error.WriteLine("Ticketing MCP Server ready. Waiting for connections...");
        
        await host.RunAsync();
    }
}
