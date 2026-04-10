using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace TicketingMcpServer.Tools;

/// <summary>
/// MCP Tools for service request management
/// </summary>
[McpServerToolType]
public class ServiceRequestTools
{
    private static readonly List<ServiceRequest> _serviceRequests = new()
    {
        new ServiceRequest { Id = "SR-001", Type = "Installation", Status = "Scheduled", CustomerId = "CUST-001", ScheduledDate = DateTime.UtcNow.AddDays(2) },
        new ServiceRequest { Id = "SR-002", Type = "Maintenance", Status = "Completed", CustomerId = "CUST-002", ScheduledDate = DateTime.UtcNow.AddDays(-1) },
        new ServiceRequest { Id = "SR-003", Type = "Upgrade", Status = "Pending Approval", CustomerId = "CUST-003", ScheduledDate = null }
    };

    [McpServerTool, Description("List all service requests with optional filtering")]
    public static string ListServiceRequests(
        [Description("Filter by request type (Installation, Maintenance, Upgrade, Consultation)")] string? type = null,
        [Description("Filter by status (Pending Approval, Scheduled, In Progress, Completed, Cancelled)")] string? status = null)
    {
        var filtered = _serviceRequests.AsEnumerable();
        
        if (!string.IsNullOrEmpty(type))
            filtered = filtered.Where(sr => sr.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        
        if (!string.IsNullOrEmpty(status))
            filtered = filtered.Where(sr => sr.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        
        return JsonSerializer.Serialize(filtered.ToList(), new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Create a new service request")]
    public static string CreateServiceRequest(
        [Description("Type of service (Installation, Maintenance, Upgrade, Consultation)")] string type,
        [Description("Customer ID requesting the service")] string customerId,
        [Description("Detailed description of the service needed")] string description,
        [Description("Preferred date for the service (ISO format)")] string? preferredDate = null)
    {
        var newRequest = new ServiceRequest
        {
            Id = $"SR-{(_serviceRequests.Count + 1):D3}",
            Type = type,
            CustomerId = customerId,
            Description = description,
            Status = "Pending Approval",
            ScheduledDate = preferredDate != null ? DateTime.Parse(preferredDate) : null,
            CreatedAt = DateTime.UtcNow
        };
        
        _serviceRequests.Add(newRequest);
        
        return JsonSerializer.Serialize(new { success = true, serviceRequest = newRequest }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get detailed information about a specific service request")]
    public static string GetServiceRequest(
        [Description("The service request ID (e.g., SR-001)")] string requestId)
    {
        var request = _serviceRequests.FirstOrDefault(sr => sr.Id.Equals(requestId, StringComparison.OrdinalIgnoreCase));
        
        if (request == null)
            return JsonSerializer.Serialize(new { error = $"Service request {requestId} not found" });
        
        return JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Schedule a service request for a specific date")]
    public static string ScheduleServiceRequest(
        [Description("The service request ID")] string requestId,
        [Description("Scheduled date (ISO format)")] string scheduledDate,
        [Description("Assigned technician name")] string? assignedTo = null)
    {
        var request = _serviceRequests.FirstOrDefault(sr => sr.Id.Equals(requestId, StringComparison.OrdinalIgnoreCase));
        
        if (request == null)
            return JsonSerializer.Serialize(new { error = $"Service request {requestId} not found" });
        
        request.ScheduledDate = DateTime.Parse(scheduledDate);
        request.Status = "Scheduled";
        request.AssignedTo = assignedTo;
        request.UpdatedAt = DateTime.UtcNow;
        
        return JsonSerializer.Serialize(new { success = true, serviceRequest = request }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get available service capacity for scheduling")]
    public static string GetServiceCapacity(
        [Description("Start date for capacity check (ISO format)")] string startDate,
        [Description("End date for capacity check (ISO format)")] string endDate)
    {
        // Simulated capacity data
        var capacity = new
        {
            period = new { start = startDate, end = endDate },
            totalSlots = 20,
            bookedSlots = 8,
            availableSlots = 12,
            availableTechnicians = new[] { "John Smith", "Maria Garcia", "Alex Johnson" },
            peakDays = new[] { "Monday", "Friday" }
        };
        
        return JsonSerializer.Serialize(capacity, new JsonSerializerOptions { WriteIndented = true });
    }
}

public class ServiceRequest
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "Pending Approval";
    public string CustomerId { get; set; } = "";
    public string? Description { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
