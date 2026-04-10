using System.Diagnostics;
using System.Text.Json;
using AgentFrameworkDemo.Configuration;
using ModelContextProtocol.Client;

namespace AgentFrameworkDemo.Services;

/// <summary>
/// Service for connecting to and calling MCP servers
/// </summary>
public class McpClientService : IAsyncDisposable
{
    private readonly McpServerSettings _settings;
    private IMcpClient? _client;

    public McpClientService(McpServerSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Connect to the MCP server
    /// </summary>
    public async Task ConnectAsync()
    {
        Console.WriteLine("🔌 Connecting to Ticketing MCP Server...");

        try
        {
            // Start the MCP server process
            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = _settings.Command,
                Arguments = _settings.Arguments.Split(' ').ToList(),
                Name = "ticketing-mcp-server"
            });
            
            _client = await McpClientFactory.CreateAsync(clientTransport);

            Console.WriteLine("✅ Connected to Ticketing MCP Server");

            // List available tools
            var tools = await _client.ListToolsAsync();
            Console.WriteLine($"   Available tools: {tools.Count}");
            foreach (var tool in tools)
            {
                Console.WriteLine($"   - {tool.Name}: {tool.Description}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Failed to connect to MCP server: {ex.Message}");
            Console.WriteLine("   The demo will continue without MCP integration.");
        }
    }

    /// <summary>
    /// Call a tool on the MCP server
    /// </summary>
    public async Task<string> CallToolAsync(string toolName, Dictionary<string, object?> arguments)
    {
        if (_client == null)
        {
            return "{}"; // Return empty JSON if not connected
        }

        try
        {
            var result = await _client.CallToolAsync(toolName, arguments);
            
            // Extract text content from the result
            if (result.Content != null && result.Content.Any())
            {
                var firstContent = result.Content.First();
                // Try to get text property using reflection or casting
                var contentJson = JsonSerializer.Serialize(firstContent);
                return contentJson;
            }

            return "{}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️ Error calling tool {toolName}: {ex.Message}");
            return "{}";
        }
    }

    /// <summary>
    /// Get all available tools from the MCP server
    /// </summary>
    public async Task<int> GetAvailableToolCountAsync()
    {
        if (_client == null)
        {
            return 0;
        }

        try
        {
            var result = await _client.ListToolsAsync();
            return result.Count;
        }
        catch
        {
            return 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
