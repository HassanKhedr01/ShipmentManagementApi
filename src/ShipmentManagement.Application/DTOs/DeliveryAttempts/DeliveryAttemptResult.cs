using ShipmentManagement.Application.DTOs.Common;

namespace ShipmentManagement.Application.DTOs.DeliveryAttempts;

public class DeliveryAttemptResult
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public DateTime AttemptedAt { get; set; }
    public Result DeliveryResult { get; set; }
    public string? FailureReason { get; set; }
}