using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Product Requirements Document Agent
/// Defines product features, user stories, and acceptance criteria
/// </summary>
public class ProductManagerAgent : BaseAgent
{
    public override string Name => "Product Manager";
    public override string Role => "Product Manager responsible for defining product features, user experience, and product roadmap";
    public override string OutputDocumentType => "Product Requirements Document (PRD)";

    public ProductManagerAgent(AzureOpenAISettings settings) : base(settings)
    {
    }

    protected override string GetSystemPrompt()
    {
        return """
            You are an experienced Product Manager with expertise in defining product requirements and user experience.
            
            Your responsibilities:
            - Define product vision and strategy
            - Create detailed user stories and acceptance criteria
            - Prioritize features based on business value
            - Define user journey and experience
            - Create product roadmap
            - Balance stakeholder needs with technical feasibility
            - Define MVP and future iterations
            
            When asking questions:
            - Focus on user needs and experience
            - Understand feature priorities
            - Explore user workflows and journeys
            - Identify must-have vs nice-to-have features
            - Understand competitive differentiation
            
            When creating the PRD:
            - Include product vision
            - Document detailed user stories
            - Define acceptance criteria
            - Create feature prioritization
            - Describe user personas and journeys
            - Define MVP scope
            - Include wireframe/UX concepts
            """;
    }

    protected override string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            As a Product Manager, review the business requirements and architecture to define product requirements.
            
            Previous Discussions:
            {FormatPreviousQuestionsAndAnswers(context)}
            
            {(context.BusinessRequirementsDocument != null ? $"Business Requirements have been defined.\n" : "")}
            {(context.ArchitectureReviewDocument != null ? $"Architecture has been designed.\n" : "")}
            
            Generate 5-7 specific questions to understand:
            
            1. Core features and functionality
            2. User personas and their specific needs
            3. User workflows and journeys
            4. Feature prioritization (MVP vs future phases)
            5. User experience expectations
            6. Accessibility requirements
            7. Mobile/responsive requirements
            8. Notification and communication preferences
            
            {GetTicketingSystemContext(context)}
            
            Consider customer feedback and feature requests from the ticketing system.
            Format your response as a numbered list of specific, user-focused questions.
            """;
    }

    protected override string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Create a comprehensive Product Requirements Document based on the gathered information.
            
            Business Requirements Document:
            {context.BusinessRequirementsDocument ?? "Not yet created"}
            
            Architecture Review Document:
            {context.ArchitectureReviewDocument ?? "Not yet created"}
            
            Information gathered from all discussions:
            {FormatAllQuestionsAndAnswers(context)}
            
            {GetTicketingSystemContext(context)}
            
            Structure the PRD with these sections:
            
            # Product Requirements Document
            
            ## 1. Product Overview
            - Product Vision
            - Product Goals
            - Target Users
            
            ## 2. User Personas
            - Primary Personas
            - Secondary Personas
            - Persona Details (goals, pain points, behaviors)
            
            ## 3. User Stories
            ### Epic 1: [Name]
            - User Story 1.1
              - As a [user], I want [feature] so that [benefit]
              - Acceptance Criteria
            - User Story 1.2
              ...
            
            ### Epic 2: [Name]
            ...
            
            ## 4. Feature Requirements
            ### Feature 1: [Name]
            - Description
            - User Value
            - Acceptance Criteria
            - Priority (Must Have / Should Have / Could Have / Won't Have)
            
            ### Feature 2: [Name]
            ...
            
            ## 5. MVP Definition
            - MVP Features
            - MVP Success Criteria
            - MVP Timeline
            
            ## 6. User Experience
            - User Flows (describe)
            - Key Screens/Pages
            - Navigation Structure
            - Accessibility Requirements
            
            ## 7. Non-Functional Requirements (Product Perspective)
            - Performance Expectations
            - Usability Requirements
            - Accessibility (WCAG compliance)
            - Localization/Internationalization
            
            ## 8. Product Roadmap
            - Phase 1 (MVP)
            - Phase 2
            - Phase 3
            - Future Considerations
            
            ## 9. Release Criteria
            - Definition of Done
            - Quality Criteria
            - Launch Requirements
            
            ## 10. Success Metrics
            - Key Performance Indicators
            - User Metrics
            - Business Metrics
            
            ## 11. Dependencies and Constraints
            - Dependencies
            - Constraints
            - Assumptions
            
            ## 12. Appendix
            - Glossary
            - References
            
            Generate the complete document in Markdown format.
            Be thorough with user stories and acceptance criteria.
            """;
    }

    protected override string GetPreviousDocumentsContext(ProjectContext context)
    {
        var docs = new List<string>();
        
        if (context.BusinessRequirementsDocument != null)
        {
            docs.Add($"Business Requirements Document:\n{context.BusinessRequirementsDocument[..Math.Min(2000, context.BusinessRequirementsDocument.Length)]}...");
        }
        
        if (context.ArchitectureReviewDocument != null)
        {
            docs.Add($"Architecture Review Document:\n{context.ArchitectureReviewDocument[..Math.Min(2000, context.ArchitectureReviewDocument.Length)]}...");
        }
        
        return docs.Any() ? string.Join("\n\n", docs) : "";
    }
}
