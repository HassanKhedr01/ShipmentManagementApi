using FluentAssertions;

using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Mappings;

public class TrackingEventMappingExtensionsTest
{
    [Theory, AutoMoqData]
    public void ToResult_WhenCalled_ShouldMapTrackingEventToTrackingEventResult(
        TrackingEvent trackingEvent)
    {
        // Act
        var result = trackingEvent.ToResult();

        // Assert
        result.Should().BeEquivalentTo(trackingEvent, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToDto_WhenCalled_ShouldMapTrackingEventResultToTrackingEventDto(
        TrackingEventResult trackingEventResult)
    {
        // Act
        var result = trackingEventResult.ToDto();

        // Assert
        result.Should().BeEquivalentTo(trackingEventResult, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToEntity_WhenCalled_ShouldMapCreateTrackingEventRequestToTrackingEvent(
        CreateTrackingEventRequest request)
    {
        // Act
        var result = request.ToEntity();

        // Assert
        result.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToCreateRequest_WhenCalled_ShouldMapCreateTrackingEventDtoToCreateTrackingEventRequest(
        CreateTrackingEventDto createDto)
    {
        // Act
        var result = createDto.ToCreateRequest();

        // Assert
        result.Should().BeEquivalentTo(createDto, options => options.ExcludingMissingMembers());
    }
}