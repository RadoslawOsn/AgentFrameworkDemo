using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace TicketingMcpServer.Tools;

/// <summary>
/// MCP Tools for customer management
/// </summary>
[McpServerToolType]
public class CustomerTools
{
    private static readonly List<Customer> _customers = new()
    {
        new Customer { Id = "CUST-001", Name = "Acme Corporation", Email = "support@acme.com", Plan = "Enterprise", ActiveTickets = 2 },
        new Customer { Id = "CUST-002", Name = "TechStart Inc", Email = "help@techstart.io", Plan = "Professional", ActiveTickets = 1 },
        new Customer { Id = "CUST-003", Name = "Global Services Ltd", Email = "it@globalservices.com", Plan = "Enterprise", ActiveTickets = 0 }
    };

    [McpServerTool, Description("List all customers in the system with optional filtering by plan type")]
    public static string ListCustomers(
        [Description("Filter by plan type (Basic, Professional, Enterprise)")] string? plan = null)
    {
        var filtered = _customers.AsEnumerable();
        
        if (!string.IsNullOrEmpty(plan))
            filtered = filtered.Where(c => c.Plan.Equals(plan, StringComparison.OrdinalIgnoreCase));
        
        return JsonSerializer.Serialize(filtered.ToList(), new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get detailed information about a specific customer")]
    public static string GetCustomer(
        [Description("The customer ID (e.g., CUST-001)")] string customerId)
    {
        var customer = _customers.FirstOrDefault(c => c.Id.Equals(customerId, StringComparison.OrdinalIgnoreCase));
        
        if (customer == null)
            return JsonSerializer.Serialize(new { error = $"Customer {customerId} not found" });
        
        return JsonSerializer.Serialize(customer, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get the support history for a customer including all their tickets")]
    public static string GetCustomerHistory(
        [Description("The customer ID")] string customerId)
    {
        var customer = _customers.FirstOrDefault(c => c.Id.Equals(customerId, StringComparison.OrdinalIgnoreCase));
        
        if (customer == null)
            return JsonSerializer.Serialize(new { error = $"Customer {customerId} not found" });
        
        // Simulated history
        var history = new
        {
            customer = customer,
            totalTicketsAllTime = customer.ActiveTickets + 5, // Simulated
            averageResolutionTime = "2.5 days",
            satisfactionScore = 4.2,
            lastInteraction = DateTime.UtcNow.AddDays(-3)
        };
        
        return JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
    }
}

public class Customer
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Plan { get; set; } = "Basic";
    public int ActiveTickets { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddMonths(-6);
}
