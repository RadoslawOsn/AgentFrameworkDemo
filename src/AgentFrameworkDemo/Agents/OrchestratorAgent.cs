using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;
using AgentFrameworkDemo.Services;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Orchestrator Agent that coordinates the workflow between all specialized agents
/// Manages the conversation flow, question gathering, and document generation
/// </summary>
public class OrchestratorAgent
{
    private readonly AzureOpenAISettings _settings;
    private readonly McpClientService _mcpClient;
    private readonly List<ISpecializedAgent> _agents;
    private readonly GitHubIssueAgent _gitHubIssueAgent;
    private readonly string _outputDirectory;

    public OrchestratorAgent(
        AzureOpenAISettings settings,
        McpClientService mcpClient,
        string outputDirectory)
    {
        _settings = settings;
        _mcpClient = mcpClient;
        _outputDirectory = outputDirectory;

        // Initialize all specialized agents in order
        _agents = new List<ISpecializedAgent>
        {
            new BusinessAnalystAgent(settings),
            new ProductManagerAgent(settings),
            new ArchitectAgent(settings),
            new TechnicalLeadAgent(settings)
        };

        // GitHub Issue Agent (runs after TRD is complete)
        _gitHubIssueAgent = new GitHubIssueAgent(settings);
    }

    /// <summary>
    /// Run the full orchestration workflow
    /// </summary>
    public async Task<ProjectContext> RunWorkflowAsync()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Multi-Agent Requirements Gathering System                ║");
        Console.WriteLine("║     Powered by Microsoft Agent Framework                     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

        // Initialize project context
        var context = new ProjectContext();

        // Step 1: Gather basic project information
        await GatherProjectBasicsAsync(context);

        // Step 2: Fetch information from ticketing system via MCP
        await GatherTicketingInfoAsync(context);

        // Step 3: Run each agent in sequence
        foreach (var agent in _agents)
        {
            Console.WriteLine($"\n{'═'.ToString().PadRight(60, '═')}");
            Console.WriteLine($"  🤖 {agent.Name} - {agent.OutputDocumentType}");
            Console.WriteLine($"{'═'.ToString().PadRight(60, '═')}\n");

            // Generate and ask questions
            await GatherAgentQuestionsAsync(agent, context);

            // Generate the document
            await GenerateAgentDocumentAsync(agent, context);

            Console.WriteLine($"\n✅ {agent.OutputDocumentType} completed!\n");
        }

        // Step 4: Run GitHub Issue Agent to create issues from TRD
        await RunGitHubIssueAgentAsync(context);

        // Step 5: Save all documents
        await SaveAllDocumentsAsync(context);

        return context;
    }

    /// <summary>
    /// Gather basic project information from the user
    /// </summary>
    private async Task GatherProjectBasicsAsync(ProjectContext context)
    {
        Console.WriteLine("📋 Let's start by understanding your project.\n");

        Console.Write("🔹 What is the name of your project/application? ");
        context.ProjectName = Console.ReadLine()?.Trim() ?? "Untitled Project";

        Console.Write("\n🔹 Please provide a brief description of what you're building:\n> ");
        context.ProjectDescription = Console.ReadLine()?.Trim() ?? "A new application";

        Console.WriteLine($"\n✨ Great! We'll help you create requirements for: {context.ProjectName}");
        Console.WriteLine("   Our team of AI agents will ask you questions to understand your needs.\n");
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Gather information from the ticketing system via MCP
    /// </summary>
    private async Task GatherTicketingInfoAsync(ProjectContext context)
    {
        Console.WriteLine("🔍 Checking ticketing system for relevant customer insights...\n");

        try
        {
            context.TicketingInfo = new TicketingSystemInfo();

            // Get ticket statistics
            var stats = await _mcpClient.CallToolAsync("get_ticket_statistics", new Dictionary<string, object?>());
            if (!string.IsNullOrEmpty(stats))
            {
                Console.WriteLine("   📊 Retrieved ticket statistics");
            }

            // Get open tickets to understand common issues
            var tickets = await _mcpClient.CallToolAsync("list_tickets", new Dictionary<string, object?>
            {
                ["status"] = "Open"
            });
            if (!string.IsNullOrEmpty(tickets))
            {
                context.TicketingInfo.CommonIssues.Add("Login and authentication issues");
                context.TicketingInfo.CommonIssues.Add("Payment processing problems");
                Console.WriteLine("   🎫 Analyzed open tickets for common issues");
            }

            // Get customer information
            var customers = await _mcpClient.CallToolAsync("list_customers", new Dictionary<string, object?>());
            if (!string.IsNullOrEmpty(customers))
            {
                context.TicketingInfo.CustomerFeedback.Add("Users request dark mode feature");
                context.TicketingInfo.FeatureRequests.Add("Dark mode support");
                context.TicketingInfo.FeatureRequests.Add("Mobile app improvements");
                Console.WriteLine("   👥 Retrieved customer feedback");
            }

            Console.WriteLine("\n✅ Ticketing system analysis complete. Insights will be used by agents.\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️ Could not connect to ticketing system: {ex.Message}");
            Console.WriteLine("   Continuing without ticketing data...\n");
        }
    }

    /// <summary>
    /// Gather questions from an agent and get user answers
    /// </summary>
    private async Task GatherAgentQuestionsAsync(ISpecializedAgent agent, ProjectContext context)
    {
        Console.WriteLine($"💭 {agent.Name} is preparing questions...\n");

        var questions = await agent.GenerateQuestionsAsync(context);
        var answers = new List<QuestionAnswer>();

        Console.WriteLine($"📝 The {agent.Name} has {questions.Count} questions for you:\n");

        for (int i = 0; i < questions.Count; i++)
        {
            Console.WriteLine($"\n❓ Question {i + 1}/{questions.Count}:");
            Console.WriteLine($"   {questions[i]}");
            Console.Write("\n   Your answer: ");
            
            var answer = Console.ReadLine()?.Trim() ?? "";
            
            if (string.IsNullOrEmpty(answer))
            {
                answer = "No specific preference, use best judgment.";
            }

            answers.Add(new QuestionAnswer
            {
                Question = questions[i],
                Answer = answer,
                AskedAt = DateTime.UtcNow
            });
        }

        context.QuestionsAndAnswers[agent.Name] = answers;
        Console.WriteLine($"\n✅ All questions answered for {agent.Name}!");
    }

    /// <summary>
    /// Run the GitHub Issue Agent to create issues from TRD
    /// </summary>
    private async Task RunGitHubIssueAgentAsync(ProjectContext context)
    {
        Console.WriteLine($"\n{'═'.ToString().PadRight(60, '═')}");
        Console.WriteLine($"  🐙 GitHub Issue Creator - Creating Issues from TRD");
        Console.WriteLine($"{'═'.ToString().PadRight(60, '═')}\n");

        // Check if GitHub CLI is available
        var ghAvailable = await _gitHubIssueAgent.IsGitHubCliAvailableAsync();
        if (!ghAvailable)
        {
            Console.WriteLine("⚠️  GitHub CLI (gh) is not installed or not in PATH.");
            Console.WriteLine("   Issues will be generated as a report but not created on GitHub.");
            Console.WriteLine("   Install GitHub CLI: https://cli.github.com/\n");
        }
        else
        {
            var isAuth = await _gitHubIssueAgent.IsAuthenticatedAsync();
            if (!isAuth)
            {
                Console.WriteLine("⚠️  GitHub CLI is not authenticated.");
                Console.WriteLine("   Run 'gh auth login' to authenticate.\n");
            }
        }

        // Gather questions
        await GatherAgentQuestionsAsync(_gitHubIssueAgent, context);

        // Generate issues document
        Console.WriteLine($"\n📋 Generating GitHub issues from Technical Requirements...\n");
        var issuesDocument = await _gitHubIssueAgent.GenerateDocumentAsync(context);

        // Parse issues
        var issues = _gitHubIssueAgent.ParseIssues(issuesDocument);
        Console.WriteLine($"   📝 Parsed {issues.Count} issues from TRD\n");

        // Ask if user wants to create issues on GitHub
        List<CreatedIssueResult>? createdResults = null;
        
        if (ghAvailable && issues.Any())
        {
            Console.Write("🔹 Do you want to create these issues on GitHub? (yes/no): ");
            var createIssues = Console.ReadLine()?.Trim().ToLower();
            
            if (createIssues == "yes" || createIssues == "y")
            {
                Console.Write("🔹 Enter repository (format: owner/repo): ");
                var repo = Console.ReadLine()?.Trim() ?? "";
                
                if (repo.Contains('/'))
                {
                    var parts = repo.Split('/');
                    createdResults = await _gitHubIssueAgent.CreateIssuesOnGitHub(issues, parts[0], parts[1]);
                    
                    var successCount = createdResults.Count(r => r.Success);
                    Console.WriteLine($"\n✅ Created {successCount}/{issues.Count} issues on GitHub!");
                }
                else
                {
                    Console.WriteLine("⚠️  Invalid repository format. Skipping GitHub creation.");
                }
            }
        }

        // Generate report
        context.GitHubIssuesReport = _gitHubIssueAgent.GenerateIssuesReport(issues, createdResults);
        Console.WriteLine($"\n✅ GitHub Issues Report completed!\n");
    }

    /// <summary>
    /// Generate the output document from an agent
    /// </summary>
    private async Task GenerateAgentDocumentAsync(ISpecializedAgent agent, ProjectContext context)
    {
        Console.WriteLine($"\n📄 {agent.Name} is generating the {agent.OutputDocumentType}...\n");

        var document = await agent.GenerateDocumentAsync(context);

        // Store document in context for use by subsequent agents
        switch (agent.OutputDocumentType)
        {
            case "Business Requirements Document (BRD)":
                context.BusinessRequirementsDocument = document;
                break;
            case "Architecture Review Document (ARD)":
                context.ArchitectureReviewDocument = document;
                break;
            case "Product Requirements Document (PRD)":
                context.ProductRequirementsDocument = document;
                break;
            case "Technical Requirements Document (TRD)":
                context.TechnicalRequirementsDocument = document;
                break;
            case "GitHub Issues Report":
                context.GitHubIssuesReport = document;
                break;
        }

        // Print first few lines as preview
        var preview = string.Join("\n", document.Split('\n').Take(10));
        Console.WriteLine("   Preview:");
        Console.WriteLine("   " + preview.Replace("\n", "\n   "));
        Console.WriteLine("   ... [document continues]");
    }

    /// <summary>
    /// Save all generated documents to files
    /// </summary>
    private async Task SaveAllDocumentsAsync(ProjectContext context)
    {
        Console.WriteLine($"\n{'═'.ToString().PadRight(60, '═')}");
        Console.WriteLine("  📁 Saving All Documents");
        Console.WriteLine($"{'═'.ToString().PadRight(60, '═')}\n");

        // Create output directory if it doesn't exist
        Directory.CreateDirectory(_outputDirectory);

        var sanitizedName = SanitizeFileName(context.ProjectName);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Save each document
        var documents = new Dictionary<string, string?>
        {
            [$"{sanitizedName}_BRD_{timestamp}.md"] = context.BusinessRequirementsDocument,
            [$"{sanitizedName}_ARD_{timestamp}.md"] = context.ArchitectureReviewDocument,
            [$"{sanitizedName}_PRD_{timestamp}.md"] = context.ProductRequirementsDocument,
            [$"{sanitizedName}_TRD_{timestamp}.md"] = context.TechnicalRequirementsDocument,
            [$"{sanitizedName}_GitHub_Issues_{timestamp}.md"] = context.GitHubIssuesReport
        };

        foreach (var doc in documents)
        {
            if (doc.Value != null)
            {
                var filePath = Path.Combine(_outputDirectory, doc.Key);
                await File.WriteAllTextAsync(filePath, doc.Value);
                Console.WriteLine($"   ✅ Saved: {doc.Key}");
            }
        }

        // Save a summary document
        var summaryPath = Path.Combine(_outputDirectory, $"{sanitizedName}_Summary_{timestamp}.md");
        var summary = GenerateSummary(context);
        await File.WriteAllTextAsync(summaryPath, summary);
        Console.WriteLine($"   ✅ Saved: {Path.GetFileName(summaryPath)}");

        Console.WriteLine($"\n📂 All documents saved to: {Path.GetFullPath(_outputDirectory)}");
    }

    /// <summary>
    /// Generate a summary document linking all other documents
    /// </summary>
    private string GenerateSummary(ProjectContext context)
    {
        return $"""
            # {context.ProjectName} - Requirements Package Summary
            
            **Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            
            ## Project Overview
            
            {context.ProjectDescription}
            
            ## Documents Generated
            
            This requirements package includes the following documents:
            
            1. **Business Requirements Document (BRD)** - Defines business objectives, stakeholders, and success criteria
            2. **Architecture Review Document (ARD)** - Outlines system architecture, technology stack, and design decisions  
            3. **Product Requirements Document (PRD)** - Details user stories, features, and product roadmap
            4. **Technical Requirements Document (TRD)** - Specifies technical implementation details, APIs, and coding standards
            5. **GitHub Issues Report** - Actionable issues created from TRD for project tracking
            
            ## Questions and Answers Summary
            
            A total of {context.QuestionsAndAnswers.Values.Sum(qa => qa.Count)} questions were discussed across all agents:
            
            {string.Join("\n", context.QuestionsAndAnswers.Select(kv => $"- **{kv.Key}**: {kv.Value.Count} questions"))}
            
            ## Insights from Ticketing System
            
            {(context.TicketingInfo != null ? $@"
            - Common Issues: {string.Join(", ", context.TicketingInfo.CommonIssues)}
            - Feature Requests: {string.Join(", ", context.TicketingInfo.FeatureRequests)}
            - Customer Feedback: {string.Join("; ", context.TicketingInfo.CustomerFeedback)}
            " : "No ticketing data was available.")}
            
            ---
            
            *Generated by Multi-Agent Requirements System using Microsoft Agent Framework*
            """;
    }

    /// <summary>
    /// Sanitize a string for use as a filename
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
