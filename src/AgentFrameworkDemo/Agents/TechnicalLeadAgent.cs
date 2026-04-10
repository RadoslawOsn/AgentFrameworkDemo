using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Technical Requirements Document Agent
/// Defines detailed technical specifications, API contracts, and implementation guidelines
/// </summary>
public class TechnicalLeadAgent : BaseAgent
{
    public override string Name => "Technical Lead";
    public override string Role => "Technical Lead responsible for defining technical specifications, API contracts, and implementation guidelines";
    public override string OutputDocumentType => "Technical Requirements Document (TRD)";

    public TechnicalLeadAgent(AzureOpenAISettings settings) : base(settings)
    {
    }

    protected override string GetSystemPrompt()
    {
        return """
            You are an experienced Technical Lead with deep expertise in software development and technical specifications.
            
            Your responsibilities:
            - Define detailed technical specifications
            - Design API contracts and data models
            - Create implementation guidelines
            - Define coding standards and practices
            - Specify testing requirements
            - Document technical dependencies
            - Define DevOps and CI/CD requirements
            
            When asking questions:
            - Focus on technical implementation details
            - Understand coding standards and practices
            - Explore testing and quality requirements
            - Identify technical dependencies
            - Understand DevOps and deployment needs
            
            When creating the TRD:
            - Include detailed API specifications
            - Document data models and schemas
            - Define technical implementation approach
            - Specify coding standards
            - Include testing strategy
            - Document deployment procedures
            """;
    }

    protected override string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            As a Technical Lead, review all previous documents to define technical requirements.
            
            Previous Discussions:
            {FormatPreviousQuestionsAndAnswers(context)}
            
            {(context.BusinessRequirementsDocument != null ? "Business Requirements: Defined\n" : "")}
            {(context.ArchitectureReviewDocument != null ? "Architecture: Designed\n" : "")}
            {(context.ProductRequirementsDocument != null ? "Product Requirements: Defined\n" : "")}
            
            Generate 5-7 specific questions to understand:
            
            1. Preferred programming languages and frameworks
            2. Database and data storage requirements
            3. API design preferences (REST, GraphQL, gRPC)
            4. Authentication and authorization implementation
            5. Testing requirements and coverage targets
            6. CI/CD and deployment pipeline requirements
            7. Logging, monitoring, and observability needs
            8. Code quality standards and review processes
            
            {GetTicketingSystemContext(context)}
            
            Format your response as a numbered list of specific, implementation-focused questions.
            """;
    }

    protected override string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Create a comprehensive Technical Requirements Document based on all gathered information.
            
            Business Requirements Document:
            {context.BusinessRequirementsDocument ?? "Not yet created"}
            
            Architecture Review Document:
            {context.ArchitectureReviewDocument ?? "Not yet created"}
            
            Product Requirements Document:
            {context.ProductRequirementsDocument ?? "Not yet created"}
            
            Information gathered from all discussions:
            {FormatAllQuestionsAndAnswers(context)}
            
            {GetTicketingSystemContext(context)}
            
            Structure the TRD with these sections:
            
            # Technical Requirements Document
            
            ## 1. Executive Summary
            - Technical Overview
            - Key Technical Decisions
            
            ## 2. Technology Stack
            - Frontend Technologies
            - Backend Technologies
            - Database Technologies
            - Infrastructure Technologies
            - Third-party Services
            
            ## 3. API Specifications
            ### API Overview
            - API Style (REST/GraphQL/gRPC)
            - Base URL Structure
            - Versioning Strategy
            
            ### Endpoints
            #### Module 1: [Name]
            ```
            GET /api/v1/resource
            POST /api/v1/resource
            ...
            ```
            - Request/Response Schemas
            - Error Handling
            
            ## 4. Data Models
            ### Entity 1: [Name]
            - Fields and Types
            - Relationships
            - Constraints
            
            ### Entity 2: [Name]
            ...
            
            ## 5. Database Design
            - Schema Design
            - Indexing Strategy
            - Data Migration Approach
            - Backup and Recovery
            
            ## 6. Security Implementation
            - Authentication Flow
            - Authorization Model
            - Data Encryption
            - API Security
            - Secrets Management
            
            ## 7. Integration Specifications
            - External API Integrations
            - Event/Message Contracts
            - Webhook Specifications
            
            ## 8. Testing Requirements
            - Unit Testing Standards
            - Integration Testing
            - End-to-End Testing
            - Performance Testing
            - Security Testing
            - Coverage Requirements
            
            ## 9. DevOps Requirements
            - CI/CD Pipeline
            - Deployment Strategy
            - Environment Configuration
            - Infrastructure as Code
            - Container Specifications
            
            ## 10. Monitoring and Observability
            - Logging Standards
            - Metrics Collection
            - Alerting Rules
            - Tracing Implementation
            - Health Checks
            
            ## 11. Coding Standards
            - Code Style Guidelines
            - Documentation Requirements
            - Code Review Process
            - Version Control Practices
            
            ## 12. Performance Requirements
            - Response Time Targets
            - Throughput Requirements
            - Resource Limits
            - Optimization Guidelines
            
            ## 13. Error Handling
            - Error Codes
            - Exception Handling Strategy
            - Retry Policies
            - Circuit Breaker Patterns
            
            ## 14. Technical Debt and Considerations
            - Known Limitations
            - Future Technical Improvements
            - Technical Risks
            
            ## 15. Development Environment
            - Local Setup Requirements
            - Development Tools
            - Dependencies
            
            Generate the complete document in Markdown format.
            Include code examples and technical details where appropriate.
            Be thorough and implementation-ready.
            """;
    }

    protected override string GetPreviousDocumentsContext(ProjectContext context)
    {
        var docs = new List<string>();
        
        if (context.BusinessRequirementsDocument != null)
        {
            docs.Add($"Business Requirements Document:\n{context.BusinessRequirementsDocument[..Math.Min(1500, context.BusinessRequirementsDocument.Length)]}...");
        }
        
        if (context.ArchitectureReviewDocument != null)
        {
            docs.Add($"Architecture Review Document:\n{context.ArchitectureReviewDocument[..Math.Min(1500, context.ArchitectureReviewDocument.Length)]}...");
        }
        
        if (context.ProductRequirementsDocument != null)
        {
            docs.Add($"Product Requirements Document:\n{context.ProductRequirementsDocument[..Math.Min(1500, context.ProductRequirementsDocument.Length)]}...");
        }
        
        return docs.Any() ? string.Join("\n\n", docs) : "";
    }
}
