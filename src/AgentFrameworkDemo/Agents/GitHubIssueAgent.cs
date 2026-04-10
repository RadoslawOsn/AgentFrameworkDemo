using System.Diagnostics;
using System.Text.RegularExpressions;
using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// GitHub Issue Creator Agent
/// Parses the Technical Requirements Document and creates GitHub issues
/// </summary>
public class GitHubIssueAgent : BaseAgent
{
    private readonly string _repoOwner;
    private readonly string _repoName;

    public override string Name => "GitHub Issue Creator";
    public override string Role => "DevOps Engineer responsible for creating actionable GitHub issues from technical requirements";
    public override string OutputDocumentType => "GitHub Issues Report";

    public GitHubIssueAgent(AzureOpenAISettings settings, string repoOwner = "", string repoName = "") 
        : base(settings)
    {
        _repoOwner = repoOwner;
        _repoName = repoName;
    }

    protected override string GetSystemPrompt()
    {
        return """
            You are an experienced DevOps Engineer who specializes in breaking down technical requirements into actionable GitHub issues.
            
            Your responsibilities:
            - Parse technical requirements documents
            - Identify discrete work items and tasks
            - Create well-structured GitHub issues with clear titles
            - Add appropriate labels and milestones
            - Define acceptance criteria for each issue
            - Estimate complexity using labels (small, medium, large)
            - Group related issues into epics
            
            When asking questions:
            - Understand priority of different features
            - Clarify which items should be in MVP vs later phases
            - Understand team capacity and sprint planning
            - Identify dependencies between tasks
            
            When creating issues:
            - Use clear, actionable titles
            - Include detailed descriptions
            - Add acceptance criteria as checkboxes
            - Suggest appropriate labels
            - Reference related issues
            """;
    }

    protected override string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Technical Requirements Document:
            {context.TechnicalRequirementsDocument ?? "Not yet created"}
            
            As a DevOps Engineer preparing to create GitHub issues, generate 3-5 questions to understand:
            
            1. Repository details (owner/name) for creating issues
            2. Label preferences (bug, feature, enhancement, etc.)
            3. Milestone/sprint structure
            4. Priority of different feature areas
            5. Any existing issues to reference or avoid duplicates
            
            Format your response as a numbered list of specific questions.
            """;
    }

    protected override string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Technical Requirements Document:
            {context.TechnicalRequirementsDocument ?? "Not available"}
            
            Product Requirements Document:
            {context.ProductRequirementsDocument ?? "Not available"}
            
            Information gathered:
            {FormatAllQuestionsAndAnswers(context)}
            
            Based on the technical requirements, generate a list of GitHub issues.
            Format each issue EXACTLY as follows (this format will be parsed programmatically):
            
            ---ISSUE---
            TITLE: [Clear, actionable title]
            LABELS: [comma-separated labels like: feature, backend, priority-high]
            MILESTONE: [MVP, Phase 2, or Future]
            BODY:
            ## Description
            [Detailed description of what needs to be done]
            
            ## Acceptance Criteria
            - [ ] [Criterion 1]
            - [ ] [Criterion 2]
            - [ ] [Criterion 3]
            
            ## Technical Notes
            [Any technical considerations]
            
            ## Dependencies
            [List any dependencies on other issues or external factors]
            ---END---
            
            Create issues for:
            1. Core infrastructure setup
            2. API endpoints (one issue per major endpoint group)
            3. Database schema and migrations
            4. Authentication and authorization
            5. Frontend components (grouped logically)
            6. Testing requirements
            7. DevOps/CI/CD setup
            8. Documentation tasks
            
            Generate at least 10-15 well-defined issues that cover the main technical requirements.
            """;
    }

    /// <summary>
    /// Parse issues from the generated document
    /// </summary>
    public List<GitHubIssue> ParseIssues(string document)
    {
        var issues = new List<GitHubIssue>();
        var issueBlocks = Regex.Split(document, @"---ISSUE---");

        foreach (var block in issueBlocks)
        {
            if (string.IsNullOrWhiteSpace(block) || !block.Contains("TITLE:"))
                continue;

            var issue = new GitHubIssue();

            // Parse title
            var titleMatch = Regex.Match(block, @"TITLE:\s*(.+?)(?=\n|LABELS:)", RegexOptions.Singleline);
            if (titleMatch.Success)
                issue.Title = titleMatch.Groups[1].Value.Trim();

            // Parse labels
            var labelsMatch = Regex.Match(block, @"LABELS:\s*(.+?)(?=\n|MILESTONE:)", RegexOptions.Singleline);
            if (labelsMatch.Success)
                issue.Labels = labelsMatch.Groups[1].Value.Trim().Split(',').Select(l => l.Trim()).ToList();

            // Parse milestone
            var milestoneMatch = Regex.Match(block, @"MILESTONE:\s*(.+?)(?=\n|BODY:)", RegexOptions.Singleline);
            if (milestoneMatch.Success)
                issue.Milestone = milestoneMatch.Groups[1].Value.Trim();

            // Parse body
            var bodyMatch = Regex.Match(block, @"BODY:\s*(.+?)(?=---END---|$)", RegexOptions.Singleline);
            if (bodyMatch.Success)
                issue.Body = bodyMatch.Groups[1].Value.Trim();

            if (!string.IsNullOrEmpty(issue.Title))
                issues.Add(issue);
        }

        return issues;
    }

    /// <summary>
    /// Create issues on GitHub using the gh CLI
    /// </summary>
    public async Task<List<CreatedIssueResult>> CreateIssuesOnGitHub(
        List<GitHubIssue> issues, 
        string repoOwner, 
        string repoName)
    {
        var results = new List<CreatedIssueResult>();
        var repo = $"{repoOwner}/{repoName}";

        Console.WriteLine($"\n📝 Creating {issues.Count} issues on {repo}...\n");

        foreach (var issue in issues)
        {
            try
            {
                var labelArgs = issue.Labels.Any() 
                    ? $"--label \"{string.Join(",", issue.Labels)}\"" 
                    : "";

                // Create a temp file for the body to handle multiline content
                var bodyFile = Path.GetTempFileName();
                await File.WriteAllTextAsync(bodyFile, issue.Body);

                var args = $"issue create --repo {repo} --title \"{EscapeForShell(issue.Title)}\" --body-file \"{bodyFile}\" {labelArgs}";

                var result = await RunGhCommandAsync(args);

                File.Delete(bodyFile);

                if (result.Success)
                {
                    Console.WriteLine($"   ✅ Created: {issue.Title}");
                    results.Add(new CreatedIssueResult
                    {
                        Title = issue.Title,
                        Success = true,
                        IssueUrl = result.Output
                    });
                }
                else
                {
                    Console.WriteLine($"   ❌ Failed: {issue.Title} - {result.Error}");
                    results.Add(new CreatedIssueResult
                    {
                        Title = issue.Title,
                        Success = false,
                        Error = result.Error
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {issue.Title} - {ex.Message}");
                results.Add(new CreatedIssueResult
                {
                    Title = issue.Title,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Check if GitHub CLI is available
    /// </summary>
    public async Task<bool> IsGitHubCliAvailableAsync()
    {
        try
        {
            var result = await RunGhCommandAsync("--version");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if user is authenticated with GitHub CLI
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var result = await RunGhCommandAsync("auth status");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<(bool Success, string Output, string Error)> RunGhCommandAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "gh",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode == 0, output.Trim(), error.Trim());
    }

    private string EscapeForShell(string input)
    {
        return input.Replace("\"", "\\\"").Replace("`", "\\`");
    }

    /// <summary>
    /// Generate a report of created issues
    /// </summary>
    public string GenerateIssuesReport(List<GitHubIssue> issues, List<CreatedIssueResult>? createdResults = null)
    {
        var report = $"""
            # GitHub Issues Report
            
            **Project:** {_repoOwner}/{_repoName}
            **Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            **Total Issues:** {issues.Count}
            
            ## Issues Summary
            
            | # | Title | Labels | Milestone | Status |
            |---|-------|--------|-----------|--------|
            """;

        for (int i = 0; i < issues.Count; i++)
        {
            var issue = issues[i];
            var status = "📋 Pending";
            
            if (createdResults != null && i < createdResults.Count)
            {
                status = createdResults[i].Success ? "✅ Created" : "❌ Failed";
            }

            report += $"\n| {i + 1} | {issue.Title} | {string.Join(", ", issue.Labels)} | {issue.Milestone} | {status} |";
        }

        report += "\n\n## Issue Details\n";

        for (int i = 0; i < issues.Count; i++)
        {
            var issue = issues[i];
            report += $"""
                
                ### {i + 1}. {issue.Title}
                
                **Labels:** {string.Join(", ", issue.Labels)}
                **Milestone:** {issue.Milestone}
                
                {issue.Body}
                
                ---
                """;
        }

        return report;
    }
}

/// <summary>
/// Represents a GitHub issue to be created
/// </summary>
public class GitHubIssue
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public List<string> Labels { get; set; } = new();
    public string Milestone { get; set; } = "";
}

/// <summary>
/// Result of creating an issue on GitHub
/// </summary>
public class CreatedIssueResult
{
    public string Title { get; set; } = "";
    public bool Success { get; set; }
    public string? IssueUrl { get; set; }
    public string? Error { get; set; }
}
