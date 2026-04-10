using AgentFrameworkDemo.Configuration;
using AgentFrameworkDemo.Models;

namespace AgentFrameworkDemo.Agents;

/// <summary>
/// Business Requirements Document Agent
/// First agent in the chain - gathers business requirements and stakeholder needs
/// </summary>
public class BusinessAnalystAgent : BaseAgent
{
    public override string Name => "Business Analyst";
    public override string Role => "Business Analyst responsible for understanding business needs, stakeholder requirements, and defining the business case";
    public override string OutputDocumentType => "Business Requirements Document (BRD)";

    public BusinessAnalystAgent(AzureOpenAISettings settings) : base(settings)
    {
    }

    protected override string GetSystemPrompt()
    {
        return """
            You are an experienced Business Analyst with expertise in gathering requirements and understanding business needs.
            
            Your responsibilities:
            - Understand the business problem being solved
            - Identify stakeholders and their needs
            - Define business objectives and success criteria
            - Analyze market and competitive landscape
            - Document business processes and workflows
            - Identify risks and constraints
            - Define ROI and business value
            
            When asking questions:
            - Focus on the "why" behind the project
            - Understand business goals and KPIs
            - Identify target users and their pain points
            - Explore budget and timeline constraints
            - Understand regulatory or compliance requirements
            
            When creating the BRD:
            - Include executive summary
            - Document business objectives
            - Define scope and boundaries
            - List stakeholders and their roles
            - Describe current state vs. desired state
            - Define success metrics
            - Identify assumptions, constraints, and risks
            """;
    }

    protected override string BuildQuestionGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            As a Business Analyst, you need to gather comprehensive business requirements.
            Generate 5-7 specific questions to understand:
            
            1. Business goals and objectives
            2. Target audience and user personas
            3. Current pain points and challenges
            4. Success metrics and KPIs
            5. Budget and timeline expectations
            6. Regulatory or compliance requirements
            7. Stakeholder priorities
            
            {GetTicketingSystemContext(context)}
            
            Based on any ticketing system data available, tailor your questions to address 
            common issues and feature requests from customers.
            
            Format your response as a numbered list of specific, actionable questions.
            Avoid generic questions - be specific to this project.
            """;
    }

    protected override string BuildDocumentGenerationPrompt(ProjectContext context)
    {
        return $"""
            Project Name: {context.ProjectName}
            Project Description: {context.ProjectDescription}
            
            Create a comprehensive Business Requirements Document (BRD) based on the gathered information.
            
            Information gathered:
            {FormatAllQuestionsAndAnswers(context)}
            
            {GetTicketingSystemContext(context)}
            
            Structure the BRD with these sections:
            
            # Business Requirements Document
            
            ## 1. Executive Summary
            
            ## 2. Business Objectives
            - Primary Goals
            - Success Metrics/KPIs
            
            ## 3. Stakeholders
            - Key Stakeholders
            - Roles and Responsibilities
            
            ## 4. Current State Analysis
            - Current Process/System
            - Pain Points
            - Gap Analysis
            
            ## 5. Proposed Solution Overview
            - High-level Solution
            - Benefits
            
            ## 6. Scope
            - In Scope
            - Out of Scope
            
            ## 7. Business Requirements
            - Functional Requirements (high-level)
            - Non-Functional Requirements (business perspective)
            
            ## 8. Constraints and Assumptions
            - Business Constraints
            - Assumptions
            
            ## 9. Risks and Mitigation
            - Identified Risks
            - Mitigation Strategies
            
            ## 10. Timeline and Budget
            - High-level Timeline
            - Budget Considerations
            
            ## 11. Success Criteria
            - Acceptance Criteria
            - Measurement Methods
            
            Generate the complete document in Markdown format.
            Be thorough and professional.
            """;
    }
}
