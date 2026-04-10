using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Base interface for all specialized agents
/// </summary>
public interface ISpecializedAgent
{
    /// <summary>
    /// Name of the agent
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Role description of the agent
    /// </summary>
    string Role { get; }
    
    /// <summary>
    /// Document type this agent produces
    /// </summary>
    string OutputDocumentType { get; }
    
    /// <summary>
    /// Ask relevant questions to the user about the project
    /// </summary>
    Task<List<string>> GenerateQuestionsAsync(ProjectContext context);
    
    /// <summary>
    /// Process the context and generate the output document
    /// </summary>
    Task<string> GenerateDocumentAsync(ProjectContext context);
}
