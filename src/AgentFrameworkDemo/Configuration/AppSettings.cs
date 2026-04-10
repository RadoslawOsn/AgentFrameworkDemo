namespace AgentFrameworkDemo.Configuration;

/// <summary>
/// Configuration settings for Azure OpenAI
/// </summary>
public class AzureOpenAISettings
{
    public string Endpoint { get; set; } = "";
    public string DeploymentName { get; set; } = "gpt-4o";
    public string ApiKey { get; set; } = "";
}

/// <summary>
/// Configuration settings for MCP Server
/// </summary>
public class McpServerSettings
{
    public string Command { get; set; } = "dotnet";
    public string Arguments { get; set; } = "";
}

/// <summary>
/// Root configuration
/// </summary>
public class AppSettings
{
    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public McpServerSettings McpServer { get; set; } = new();
    public string OutputDirectory { get; set; } = "./output";
}
