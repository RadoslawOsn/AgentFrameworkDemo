using AgentFrameworkDemo.Agents;
using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Services;
using Microsoft.Extensions.Configuration;

namespace AgentFrameworkDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        try
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var settings = new AppSettings();
            configuration.Bind(settings);

            // Validate Azure OpenAI settings
            if (string.IsNullOrEmpty(settings.AzureOpenAI.Endpoint) || 
                settings.AzureOpenAI.Endpoint == "YOUR_AZURE_OPENAI_ENDPOINT")
            {
                Console.WriteLine("⚠️  Azure OpenAI endpoint not configured!");
                Console.WriteLine("   Please update appsettings.json with your Azure OpenAI credentials.");
                Console.WriteLine("\n   Required settings:");
                Console.WriteLine("   - AzureOpenAI:Endpoint - Your Azure OpenAI endpoint URL");
                Console.WriteLine("   - AzureOpenAI:ApiKey - Your Azure OpenAI API key");
                Console.WriteLine("   - AzureOpenAI:DeploymentName - Your model deployment name (default: gpt-4o)");
                
                // For demo purposes, allow proceeding with mock mode
                Console.WriteLine("\n   Press Enter to continue in demo mode (mock responses)...");
                Console.ReadLine();
                
                await RunDemoModeAsync(settings);
                return;
            }

            await RunProductionModeAsync(settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine("\nStack trace:");
            Console.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Run with actual Azure OpenAI and MCP server
    /// </summary>
    static async Task RunProductionModeAsync(AppSettings settings)
    {
        // Initialize MCP client for ticketing system
        await using var mcpClient = new McpClientService(settings.McpServer);
        
        // Try to connect to MCP server
        try
        {
            await mcpClient.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ MCP server not available: {ex.Message}");
            Console.WriteLine("   Continuing without ticketing system integration...\n");
        }

        // Create and run the orchestrator
        var orchestrator = new OrchestratorAgent(
            settings.AzureOpenAI,
            mcpClient,
            settings.OutputDirectory
        );

        var context = await orchestrator.RunWorkflowAsync();

        // Final summary
        PrintCompletionSummary(context);
    }

    /// <summary>
    /// Run in demo mode without actual AI calls
    /// </summary>
    static async Task RunDemoModeAsync(AppSettings settings)
    {
        Console.WriteLine("\n🎭 Running in DEMO MODE (no actual AI calls)\n");
        
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Multi-Agent Requirements Gathering System                ║");
        Console.WriteLine("║     [DEMO MODE - Simulated Responses]                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("📋 In production mode, this application would:");
        Console.WriteLine("   1. Connect to Azure OpenAI for AI-powered conversations");
        Console.WriteLine("   2. Connect to the Ticketing MCP Server for customer insights");
        Console.WriteLine("   3. Run 4 specialized AI agents in sequence:");
        Console.WriteLine("      - 🔹 Business Analyst → Creates Business Requirements Document");
        Console.WriteLine("      - 🔹 Solution Architect → Creates Architecture Review Document");
        Console.WriteLine("      - 🔹 Product Manager → Creates Product Requirements Document");
        Console.WriteLine("      - 🔹 Technical Lead → Creates Technical Requirements Document");
        Console.WriteLine("   4. Each agent asks questions and uses previous documents as context");
        Console.WriteLine("   5. Save all generated documents to the output folder\n");

        // Create sample output files
        Directory.CreateDirectory(settings.OutputDirectory);
        
        var sampleBrd = """
            # Business Requirements Document
            
            ## 1. Executive Summary
            This is a sample BRD generated in demo mode.
            
            ## 2. Business Objectives
            - Primary Goal: Demonstrate multi-agent collaboration
            - Success Metric: Generate comprehensive requirements documents
            
            ## 3. Stakeholders
            - Product Owner
            - Development Team
            - End Users
            
            *[Demo mode - actual document would be AI-generated based on your answers]*
            """;

        await File.WriteAllTextAsync(
            Path.Combine(settings.OutputDirectory, "DEMO_BRD.md"), 
            sampleBrd
        );

        Console.WriteLine($"📁 Sample output created in: {Path.GetFullPath(settings.OutputDirectory)}");
        Console.WriteLine("\n✅ Demo mode completed. Configure Azure OpenAI to enable full functionality.");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Print final completion summary
    /// </summary>
    static void PrintCompletionSummary(AgentFrameworkDemo.Models.ProjectContext context)
    {
        Console.WriteLine("\n");
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    🎉 WORKFLOW COMPLETE! 🎉                   ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"\n📊 Project: {context.ProjectName}");
        Console.WriteLine($"📝 Total questions asked: {context.QuestionsAndAnswers.Values.Sum(qa => qa.Count)}");
        Console.WriteLine("\n📄 Documents generated:");
        Console.WriteLine("   ✅ Business Requirements Document (BRD)");
        Console.WriteLine("   ✅ Architecture Review Document (ARD)");
        Console.WriteLine("   ✅ Product Requirements Document (PRD)");
        Console.WriteLine("   ✅ Technical Requirements Document (TRD)");
        Console.WriteLine("\n🚀 Your requirements package is ready for development!");
    }
}
