using FluentAssertions;

using ShipmentManagement.Application.DTOs.DeliveryAttempts;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Mappings;

public class DeliveryAttemptMappingExtensionsTest
{
    [Theory,AutoMoqData]
    public void ToResult_WhenCalled_ReturnsExpectedDeliveryAttemptResult(DeliveryAttempt deliveryAttempt)
    {
        // Act
        var result = deliveryAttempt.ToResult();

        // Assert
        result.Should().BeEquivalentTo(deliveryAttempt, options => options.ExcludingMissingMembers());
    }
    
    [Theory,AutoMoqData]
    public void ToDto_WhenCalled_ReturnsExpectedDeliveryAttemptDto(DeliveryAttemptResult deliveryAttemptResult)
    {
        // Act
        var result = deliveryAttemptResult.ToDto();

        // Assert
        result.Should().BeEquivalentTo(deliveryAttemptResult, options => options.ExcludingMissingMembers());
    }
}