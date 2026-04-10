using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Architecture Review Document Agent
/// Reviews business requirements and designs the system architecture
/// </summary>
public class ArchitectAgent : BaseAgent
{
    public override string Name => "Solution Architect";
    public override string Role => "Solution Architect responsible for designing scalable, secure, and maintainable system architecture";
    public override string OutputDocumentType => "Architecture Review Document (ARD)";

    public ArchitectAgent(AzureOpenAISettings settings) : base(settings)
    {
    }

    protected override string GetSystemPrompt()
    {
        return """
            You are an experienced Solution Architect with expertise in designing enterprise-grade systems.
            
            Your responsibilities:
            - Design system architecture based on requirements
            - Select appropriate technologies and patterns
            - Ensure scalability, security, and performance
            - Define integration points and APIs
            - Create architecture diagrams concepts
            - Identify technical debt and risks
            - Ensure compliance with standards
            
            When asking questions:
            - Focus on technical constraints and requirements
            - Understand scalability and performance needs
            - Explore security and compliance requirements
            - Identify integration requirements
            - Understand deployment and infrastructure needs
            
            When creating the ARD:
            - Include architecture overview
            - Document design decisions and rationale
            - Define system components and interactions
            - Specify technology stack recommendations
            - Address non-functional requirements
            - Include security architecture
            - Document deployment architecture
            """;
    }

    protected override string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            As a Solution Architect, review the business requirements and generate questions to design the architecture.
            
            Previous Business Requirements Discussion:
            {FormatPreviousQuestionsAndAnswers(context)}
            
            {(context.BusinessRequirementsDocument != null ? $"Business Requirements Document Summary:\n{context.BusinessRequirementsDocument[..Math.Min(2000, context.BusinessRequirementsDocument.Length)]}..." : "")}
            
            Generate 5-7 specific questions to understand:
            
            1. Expected user load and scalability requirements
            2. Performance requirements (response times, throughput)
            3. Security and compliance requirements
            4. Integration with existing systems
            5. Deployment preferences (cloud provider, on-premise, hybrid)
            6. Data storage and processing requirements
            7. Availability and disaster recovery requirements
            8. Technology preferences or constraints
            
            {GetTicketingSystemContext(context)}
            
            Format your response as a numbered list of specific, technical questions.
            """;
    }

    protected override string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Create a comprehensive Architecture Review Document based on the gathered information.
            
            Business Requirements Document:
            {context.BusinessRequirementsDocument ?? "Not yet created"}
            
            Information gathered from all discussions:
            {FormatAllQuestionsAndAnswers(context)}
            
            {GetTicketingSystemContext(context)}
            
            Structure the Architecture Document with these sections:
            
            # Architecture Review Document
            
            ## 1. Executive Summary
            - Architecture Overview
            - Key Design Decisions
            
            ## 2. Architecture Principles
            - Guiding Principles
            - Design Standards
            
            ## 3. System Context
            - System Context Diagram (describe in text)
            - External Interfaces
            - Integration Points
            
            ## 4. Architecture Overview
            - High-Level Architecture
            - Component Diagram (describe in text)
            - Data Flow
            
            ## 5. Component Architecture
            - Frontend Architecture
            - Backend Architecture
            - Data Architecture
            - Integration Architecture
            
            ## 6. Technology Stack
            - Recommended Technologies
            - Technology Rationale
            - Alternatives Considered
            
            ## 7. Security Architecture
            - Authentication & Authorization
            - Data Security
            - Network Security
            - Compliance Considerations
            
            ## 8. Infrastructure Architecture
            - Deployment Architecture
            - Cloud Services (if applicable)
            - Scaling Strategy
            - High Availability Design
            
            ## 9. Non-Functional Requirements
            - Performance Requirements
            - Scalability Requirements
            - Availability Requirements
            - Disaster Recovery
            
            ## 10. Architecture Decisions
            - Key Decisions Made
            - Decision Rationale
            - Trade-offs
            
            ## 11. Risks and Mitigations
            - Technical Risks
            - Architecture Risks
            - Mitigation Strategies
            
            ## 12. Future Considerations
            - Extensibility
            - Technical Debt
            - Evolution Path
            
            Generate the complete document in Markdown format.
            Be thorough and technical while remaining clear.
            """;
    }

    protected override string GetPreviousDocumentsContext(ProjectContext context)
    {
        if (context.BusinessRequirementsDocument == null) return "";
        
        return $"""
            
            Business Requirements Document has been created. Key points to consider for architecture:
            {context.BusinessRequirementsDocument[..Math.Min(3000, context.BusinessRequirementsDocument.Length)]}
            """;
    }
}
