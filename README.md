# Multi-Agent Requirements Gathering System

A demonstration of Microsoft Agent Framework in .NET featuring multiple AI agents working together to create comprehensive requirements documentation for software projects.

## 🎯 Overview

This demo showcases a team of 5 specialized AI agents that collaborate to gather requirements from users and generate professional documentation:

| Agent | Role | Output |
|-------|------|--------|
| **Business Analyst** | Gathers business objectives, stakeholder needs, and success criteria | Business Requirements Document (BRD) |
| **Solution Architect** | Designs system architecture and technology decisions | Architecture Review Document (ARD) |
| **Product Manager** | Defines user stories, features, and product roadmap | Product Requirements Document (PRD) |
| **Technical Lead** | Specifies technical implementation details and APIs | Technical Requirements Document (TRD) |
| **GitHub Issue Creator** | Parses TRD and creates actionable GitHub issues | GitHub Issues (via `gh` CLI) |

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Orchestrator Agent                        │
│         (Coordinates workflow between all agents)            │
└─────────────────────────────────────────────────────────────┘
                              │
    ┌─────────────┬───────────┼───────────┬─────────────┐
    ▼             ▼           ▼           ▼             ▼
┌────────┐  ┌──────────┐  ┌─────────┐  ┌────────┐  ┌────────┐
│Business│─▶│ Solution │─▶│ Product │─▶│Technical│─▶│ GitHub │
│Analyst │  │ Architect│  │ Manager │  │  Lead   │  │ Issues │
└────────┘  └──────────┘  └─────────┘  └────────┘  └────────┘
    │
        └───────────────────────────────────────┘
                           │
                    ┌──────┴──────┐
                    ▼             ▼
          ┌─────────────┐  ┌─────────────┐
          │ Azure OpenAI│  │ MCP Server  │
          │   (AI)      │  │ (Ticketing) │
          └─────────────┘  └─────────────┘
```

## 📁 Project Structure

```
AgentFrameworkDemo/
├── AgentFrameworkDemo.sln
├── src/
│   ├── AgentFrameworkDemo/           # Main console application
│   │   ├── Agents/
│   │   │   ├── ISpecializedAgent.cs  # Agent interface
│   │   │   ├── BaseAgent.cs          # Common agent functionality
│   │   │   ├── BusinessAnalystAgent.cs
│   │   │   ├── ArchitectAgent.cs
│   │   │   ├── ProductManagerAgent.cs
│   │   │   ├── TechnicalLeadAgent.cs
│   │   │   ├── GitHubIssueAgent.cs   # Creates GitHub issues from TRD
│   │   │   └── OrchestratorAgent.cs  # Workflow coordinator
│   │   ├── Configuration/
│   │   │   └── AppSettings.cs
│   │   ├── Models/
│   │   │   └── ProjectContext.cs     # Shared context between agents
│   │   ├── Services/
│   │   │   └── McpClientService.cs   # MCP server client
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── TicketingMcpServer/           # Custom MCP Server
│       ├── Program.cs
│       └── Tools/
│           ├── TicketTools.cs        # Ticket management tools
│           ├── CustomerTools.cs      # Customer management tools
│           └── ServiceRequestTools.cs # Service request tools
└── output/                           # Generated documents
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Azure OpenAI Service with a deployed model (e.g., gpt-4o)
- GitHub CLI (`gh`) - Optional, for creating issues on GitHub
  - Install: https://cli.github.com/
  - Authenticate: `gh auth login`

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/RadoslawOsn/AgentFrameworkDemo.git
   cd AgentFrameworkDemo
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Configure Azure OpenAI:**
   Edit `src/AgentFrameworkDemo/appsettings.json`:
   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "https://your-resource.openai.azure.com/",
       "DeploymentName": "gpt-4o",
       "ApiKey": "your-api-key"
     }
   }
   ```

### Running the Demo

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Run the main application:**
   ```bash
   cd src/AgentFrameworkDemo
   dotnet run
   ```

3. **Follow the prompts:**
   - Enter your project name and description
   - Answer questions from each agent
   - Review generated documents in the `output/` folder

### Running the MCP Server Separately

The ticketing MCP server can be run independently:

```bash
cd src/TicketingMcpServer
dotnet run
```

## 🔧 MCP Server Tools

The ticketing MCP server provides the following tools:

### Ticket Management
- `list_tickets` - List all tickets with optional filtering
- `get_ticket` - Get details of a specific ticket
- `create_ticket` - Create a new support ticket
- `update_ticket_status` - Update ticket status
- `get_ticket_statistics` - Get ticket statistics

### Customer Management
- `list_customers` - List all customers
- `get_customer` - Get customer details
- `get_customer_history` - Get customer support history

### Service Requests
- `list_service_requests` - List service requests
- `create_service_request` - Create new service request
- `get_service_request` - Get request details
- `schedule_service_request` - Schedule a service
- `get_service_capacity` - Check service availability

## 🔄 Workflow

1. **User Input**: Provide project name and description
2. **MCP Integration**: System queries ticketing system for customer insights
3. **Agent Workflow**:
   - Business Analyst asks 5-7 questions → Generates BRD
   - Product Manager reviews BRD, asks questions → Generates PRD
   - Solution Architect reviews BRD+PRD, asks questions → Generates ARD
   - Technical Lead reviews all docs, asks questions → Generates TRD
   - GitHub Issue Creator parses TRD → Creates issues on GitHub (optional)
4. **Output**: All documents saved to `output/` folder

## 📄 Sample Output

Each run generates:
- `{ProjectName}_BRD_{timestamp}.md` - Business Requirements
- `{ProjectName}_ARD_{timestamp}.md` - Architecture Review
- `{ProjectName}_PRD_{timestamp}.md` - Product Requirements
- `{ProjectName}_TRD_{timestamp}.md` - Technical Requirements
- `{ProjectName}_GitHub_Issues_{timestamp}.md` - GitHub Issues Report
- `{ProjectName}_Summary_{timestamp}.md` - Overview linking all docs

## 🐙 GitHub Integration

The GitHub Issue Creator agent can automatically create issues on GitHub:

1. **Parses TRD** - Extracts actionable work items from technical requirements
2. **Generates Issues** - Creates well-structured issues with:
   - Clear titles and descriptions
   - Acceptance criteria as checkboxes
   - Labels (feature, backend, priority-high, etc.)
   - Milestone assignments (MVP, Phase 2, Future)
3. **Creates on GitHub** - Uses `gh` CLI to create issues (requires authentication)

### Prerequisites for GitHub Integration

```bash
# Install GitHub CLI
winget install GitHub.cli

# Authenticate
gh auth login
```

If GitHub CLI is not available, issues are still generated as a report in the output folder.

## 🛠️ Technologies Used

- **Microsoft Agent Framework** - AI agent orchestration
- **Azure OpenAI Service** - LLM backend
- **Model Context Protocol (MCP)** - Tool integration standard
- **GitHub CLI** - Issue creation on GitHub
- **.NET 8.0** - Runtime platform
- **Azure.AI.OpenAI** - Azure OpenAI SDK

## 📚 Key Concepts

### Orchestrator Pattern
The `OrchestratorAgent` coordinates the workflow, managing:
- Agent sequencing
- Context sharing between agents
- Question gathering from users
- Document generation and storage

### Shared Context
The `ProjectContext` class maintains shared state:
- Project information
- Q&A from all agents
- Generated documents
- Ticketing system insights

### MCP Integration
The custom MCP server demonstrates:
- Tool definition with attributes
- JSON schema generation
- Stdio transport for local communication

## 🔒 Security Notes

- Store API keys in environment variables for production
- Never commit `appsettings.json` with actual credentials
- Use Azure Key Vault for production deployments

## 📝 License

MIT License - See [LICENSE](LICENSE) for details

## 🤝 Contributing

Contributions welcome! Please read the contributing guidelines first.

---

*Built with Microsoft Agent Framework - Demonstrating multi-agent AI collaboration*

