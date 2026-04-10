namespace AgentFrameworkDemo.Models;

/// <summary>
/// Shared context passed between agents containing project information
/// </summary>
public class ProjectContext
{
    /// <summary>
    /// Name of the project/app being designed
    /// </summary>
    public string ProjectName { get; set; } = "";
    
    /// <summary>
    /// High-level description of the project
    /// </summary>
    public string ProjectDescription { get; set; } = "";
    
    /// <summary>
    /// User's answers to questions from all agents
    /// </summary>
    public Dictionary<string, List<QuestionAnswer>> QuestionsAndAnswers { get; set; } = new();
    
    /// <summary>
    /// Business Requirements Document content
    /// </summary>
    public string? BusinessRequirementsDocument { get; set; }
    
    /// <summary>
    /// Architecture Review Document content
    /// </summary>
    public string? ArchitectureReviewDocument { get; set; }
    
    /// <summary>
    /// Product Requirements Document content
    /// </summary>
    public string? ProductRequirementsDocument { get; set; }
    
    /// <summary>
    /// Technical Requirements Document content
    /// </summary>
    public string? TechnicalRequirementsDocument { get; set; }
    
    /// <summary>
    /// GitHub Issues Report content
    /// </summary>
    public string? GitHubIssuesReport { get; set; }
    
    /// <summary>
    /// Information gathered from the ticketing system via MCP
    /// </summary>
    public TicketingSystemInfo? TicketingInfo { get; set; }
}

/// <summary>
/// Question and answer pair
/// </summary>
public class QuestionAnswer
{
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Information from the ticketing system
/// </summary>
public class TicketingSystemInfo
{
    public List<string> CommonIssues { get; set; } = new();
    public List<string> FeatureRequests { get; set; } = new();
    public Dictionary<string, int> TicketStatistics { get; set; } = new();
    public List<string> CustomerFeedback { get; set; } = new();
}
