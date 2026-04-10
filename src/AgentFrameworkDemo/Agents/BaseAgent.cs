using System.ClientModel;
using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Base class for specialized agents providing common AI functionality
/// </summary>
public abstract class BaseAgent : ISpecializedAgent
{
    protected readonly AzureOpenAISettings _settings;
    protected readonly ChatClient _chatClient;

    public abstract string Name { get; }
    public abstract string Role { get; }
    public abstract string OutputDocumentType { get; }

    protected BaseAgent(AzureOpenAISettings settings)
    {
        _settings = settings;
        
        var azureClient = new AzureOpenAIClient(
            new Uri(_settings.Endpoint),
            new AzureKeyCredential(_settings.ApiKey)
        );
        
        _chatClient = azureClient.GetChatClient(_settings.DeploymentName);
    }

    /// <summary>
    /// System prompt specific to each agent type
    /// </summary>
    protected abstract string GetSystemPrompt();

    /// <summary>
    /// Generate questions to ask the user
    /// </summary>
    public virtual async Task<List<string>> GenerateQuestionsAsync(ProjectContext context)
    {
        var systemPrompt = GetSystemPrompt();
        var userPrompt = BuildQuestionGenerationPrompt(context);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var response = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 1000
        });

        var content = response.Value.Content[0].Text;
        var questions = ParseQuestions(content);
        
        return questions;
    }

    /// <summary>
    /// Generate the output document
    /// </summary>
    public virtual async Task<string> GenerateDocumentAsync(ProjectContext context)
    {
        var systemPrompt = GetSystemPrompt();
        var userPrompt = BuildDocumentGenerationPrompt(context);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var response = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            Temperature = 0.3f,
            MaxOutputTokenCount = 4000
        });

        return response.Value.Content[0].Text;
    }

    /// <summary>
    /// Build prompt for question generation
    /// </summary>
    protected virtual string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        var prompt = $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Based on your role as {Role}, generate 5-7 detailed and specific questions to ask the user 
            about the project to gather information needed for the {OutputDocumentType}.
            
            Consider what has already been discussed:
            {FormatPreviousQuestionsAndAnswers(context)}
            
            {GetAdditionalContext(context)}
            
            Format your response as a numbered list of questions.
            Focus on questions that will help you create a comprehensive {OutputDocumentType}.
            Be specific and avoid generic questions.
            """;
        
        return prompt;
    }

    /// <summary>
    /// Build prompt for document generation
    /// </summary>
    protected virtual string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        var prompt = $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Based on all the information gathered, create a comprehensive {OutputDocumentType}.
            
            Information gathered from questions:
            {FormatAllQuestionsAndAnswers(context)}
            
            {GetPreviousDocumentsContext(context)}
            
            {GetTicketingSystemContext(context)}
            
            Create a well-structured, professional {OutputDocumentType} in Markdown format.
            Include all relevant sections, be thorough and detailed.
            """;
        
        return prompt;
    }

    /// <summary>
    /// Get additional context specific to the agent type
    /// </summary>
    protected virtual string GetAdditionalContext(ProjectContext context) => "";

    /// <summary>
    /// Get context from previous documents
    /// </summary>
    protected virtual string GetPreviousDocumentsContext(ProjectContext context) => "";

    /// <summary>
    /// Get context from ticketing system
    /// </summary>
    protected string GetTicketingSystemContext(ProjectContext context)
    {
        if (context.TicketingInfo == null) return "";
        
        return $"""
            
            Information from Ticketing System:
            - Common Issues: {string.Join(", ", context.TicketingInfo.CommonIssues)}
            - Feature Requests: {string.Join(", ", context.TicketingInfo.FeatureRequests)}
            - Customer Feedback: {string.Join("; ", context.TicketingInfo.CustomerFeedback)}
            """;
    }

    /// <summary>
    /// Format previous Q&A for context
    /// </summary>
    protected string FormatPreviousQuestionsAndAnswers(ProjectContext context)
    {
        if (!context.QuestionsAndAnswers.Any()) return "No previous discussions.";
        
        var formatted = new List<string>();
        foreach (var agentQa in context.QuestionsAndAnswers)
        {
            if (agentQa.Key == Name) continue; // Skip own questions
            formatted.Add($"\n{agentQa.Key} discussed:");
            foreach (var qa in agentQa.Value)
            {
                formatted.Add($"Q: {qa.Question}\nA: {qa.Answer}");
            }
        }
        
        return formatted.Any() ? string.Join("\n", formatted) : "No previous discussions.";
    }

    /// <summary>
    /// Format all Q&A for document generation
    /// </summary>
    protected string FormatAllQuestionsAndAnswers(ProjectContext context)
    {
        if (!context.QuestionsAndAnswers.Any()) return "No information gathered.";
        
        var formatted = new List<string>();
        foreach (var agentQa in context.QuestionsAndAnswers)
        {
            formatted.Add($"\n### {agentQa.Key} Discussion:");
            foreach (var qa in agentQa.Value)
            {
                formatted.Add($"**Q: {qa.Question}**\nA: {qa.Answer}\n");
            }
        }
        
        return string.Join("\n", formatted);
    }

    /// <summary>
    /// Parse questions from AI response
    /// </summary>
    protected List<string> ParseQuestions(string content)
    {
        var questions = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            // Remove numbering patterns like "1.", "1)", "1:", "-", "*"
            if (trimmed.Length > 2)
            {
                var cleaned = System.Text.RegularExpressions.Regex.Replace(trimmed, @"^[\d\-\*\)\.]+\s*", "");
                if (!string.IsNullOrWhiteSpace(cleaned) && cleaned.Contains('?'))
                {
                    questions.Add(cleaned);
                }
            }
        }
        
        return questions;
    }
}
