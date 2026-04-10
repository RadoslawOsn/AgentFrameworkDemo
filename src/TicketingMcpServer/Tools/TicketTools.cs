using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace TicketingMcpServer.Tools;

/// <summary>
/// MCP Tools for ticket management
/// </summary>
[McpServerToolType]
public class TicketTools
{
    private static readonly List<Ticket> _tickets = new()
    {
        new Ticket { Id = "TKT-001", Title = "Login issues", Status = "Open", Priority = "High", CustomerId = "CUST-001", Description = "Customer unable to login to portal" },
        new Ticket { Id = "TKT-002", Title = "Payment failed", Status = "In Progress", Priority = "Critical", CustomerId = "CUST-002", Description = "Payment gateway returning errors" },
        new Ticket { Id = "TKT-003", Title = "Feature request", Status = "Open", Priority = "Low", CustomerId = "CUST-001", Description = "Request for dark mode" }
    };

    [McpServerTool, Description("List all tickets in the system with optional filtering by status or priority")]
    public static string ListTickets(
        [Description("Filter by ticket status (Open, In Progress, Resolved, Closed)")] string? status = null,
        [Description("Filter by priority (Low, Medium, High, Critical)")] string? priority = null)
    {
        var filtered = _tickets.AsEnumerable();
        
        if (!string.IsNullOrEmpty(status))
            filtered = filtered.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(priority))
            filtered = filtered.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
        
        return JsonSerializer.Serialize(filtered.ToList(), new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get detailed information about a specific ticket by its ID")]
    public static string GetTicket(
        [Description("The ticket ID (e.g., TKT-001)")] string ticketId)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
        
        if (ticket == null)
            return JsonSerializer.Serialize(new { error = $"Ticket {ticketId} not found" });
        
        return JsonSerializer.Serialize(ticket, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Create a new support ticket in the system")]
    public static string CreateTicket(
        [Description("Title of the ticket")] string title,
        [Description("Detailed description of the issue")] string description,
        [Description("Customer ID who reported the issue")] string customerId,
        [Description("Priority level (Low, Medium, High, Critical)")] string priority = "Medium")
    {
        var newTicket = new Ticket
        {
            Id = $"TKT-{(_tickets.Count + 1):D3}",
            Title = title,
            Description = description,
            CustomerId = customerId,
            Priority = priority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        
        _tickets.Add(newTicket);
        
        return JsonSerializer.Serialize(new { success = true, ticket = newTicket }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Update the status of an existing ticket")]
    public static string UpdateTicketStatus(
        [Description("The ticket ID to update")] string ticketId,
        [Description("New status (Open, In Progress, Resolved, Closed)")] string newStatus)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
        
        if (ticket == null)
            return JsonSerializer.Serialize(new { error = $"Ticket {ticketId} not found" });
        
        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;
        
        return JsonSerializer.Serialize(new { success = true, ticket }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get statistics about tickets in the system")]
    public static string GetTicketStatistics()
    {
        var stats = new
        {
            total = _tickets.Count,
            byStatus = _tickets.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.Count()),
            byPriority = _tickets.GroupBy(t => t.Priority).ToDictionary(g => g.Key, g => g.Count()),
            openCritical = _tickets.Count(t => t.Status == "Open" && t.Priority == "Critical")
        };
        
        return JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
    }
}

public class Ticket
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
    public string CustomerId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
