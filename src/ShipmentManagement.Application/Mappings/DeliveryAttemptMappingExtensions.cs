using ShipmentManagement.Application.DTOs.DeliveryAttempts;
using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Application.Mappings;

public static class DeliveryAttemptMappingExtensions
{
    public static DeliveryAttemptResult ToResult(this DeliveryAttempt deliveryAttempt)
    {
        return new DeliveryAttemptResult
        {
            Id = deliveryAttempt.Id,
            PackageId = deliveryAttempt.PackageId,
            AttemptedAt = deliveryAttempt.AttemptedAt,
            DeliveryResult = deliveryAttempt.DeliveryResult.ToDto(),
            FailureReason = deliveryAttempt.FailureReason
        };
    }
    
    public static DeliveryAttemptDto ToDto(this DeliveryAttemptResult result)
    {
        return new DeliveryAttemptDto
        {
            Id = result.Id,
            PackageId = result.PackageId,
            AttemptedAt = result.AttemptedAt,
            DeliveryResult = result.DeliveryResult,
            FailureReason = result.FailureReason
        };
    }
}