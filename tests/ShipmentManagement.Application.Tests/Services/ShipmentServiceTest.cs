using AutoFixture.Xunit2;

using FluentAssertions;

using Microsoft.Extensions.Time.Testing;

using Moq;

using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Enums;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Services;

public class ShipmentServiceTest
{
    #region GetEventsByTrackingNumberAsync

    [Theory, AutoMoqData]
    public async Task GetEventsByTrackingNumber_EventsExists_ReturnsEvents(
        string trackingNumber,
        List<TrackingEvent> events,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        ShipmentService service)
    {
        // Arrange
        trackingRepoMock.Setup(repo => repo.GetAllByTrackingNumberAsync(trackingNumber)).ReturnsAsync(events);

        // Act
        var results = await service.GetEventsByTrackingNumberAsync(trackingNumber);

        // Assert
        results.Should().HaveCount(events.Count);
        results.Should().BeEquivalentTo(events, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetEventsByTrackingNumber_EventsDoesNotExists_ReturnsEmptyEventsList(
        string trackingNumber,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        ShipmentService service)
    {
        // Arrange
        trackingRepoMock.Setup(repo => repo.GetAllByTrackingNumberAsync(trackingNumber))
            .ReturnsAsync(new List<TrackingEvent>());

        // Act
        var results = await service.GetEventsByTrackingNumberAsync(trackingNumber);

        // Assert
        results.Should().HaveCount(0);
    }

    #endregion

    #region GetEventsByPackageIdAsync

    [Theory, AutoMoqData]
    public async Task GetEventsByPackageId_EventsExists_ReturnsEvents(
        int packageId,
        List<TrackingEvent> events,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        ShipmentService service)
    {
        // Arrange
        trackingRepoMock.Setup(repo => repo.GetAllByPackageIdAsync(packageId)).ReturnsAsync(events);

        // Act
        var results = await service.GetEventsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(events.Count);
        results.Should().BeEquivalentTo(events, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetEventsByPackageId_EventsDoesNotExists_ReturnsEmptyEventsList(
        int packageId,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        ShipmentService service)
    {
        // Arrange
        trackingRepoMock.Setup(repo => repo.GetAllByPackageIdAsync(packageId))
            .ReturnsAsync(new List<TrackingEvent>());

        // Act
        var results = await service.GetEventsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(0);
    }

    #endregion

    #region GetUserEventsByPackageIdAsync

    [Theory, AutoMoqData]
    public async Task GetUserEventsByPackageId_EventsExists_ReturnsUserEvents(
        int packageId,
        List<TrackingEvent> events,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        ShipmentService service)
    {
        // Arrange
        var userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        trackingRepoMock.Setup(repo => repo.GetAllByPackageIdAsync(userId, packageId)).ReturnsAsync(events);

        // Act
        var results = await service.GetUserEventsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(events.Count);
        results.Should().BeEquivalentTo(events, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetUserEventsByPackageId_EventsDoesNotExists_ReturnsEmptyEventsList(
        int packageId,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        ShipmentService service)
    {
        // Arrange
        var userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        trackingRepoMock.Setup(repo => repo.GetAllByPackageIdAsync(userId, packageId))
            .ReturnsAsync(new List<TrackingEvent>());

        // Act
        var results = await service.GetUserEventsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(0);
    }

    #endregion

    #region GetDeliveryAttemptsByPackageIdAsync

    [Theory, AutoMoqData]
    public async Task GetDeliveryAttemptsByPackageId_AttemptsExists_ReturnsAttempts(
        int packageId,
        List<DeliveryAttempt> attempts,
        [Frozen] Mock<IAttemptsRepository> attemptsRepoMock,
        ShipmentService service)
    {
        // Arrange
        attemptsRepoMock.Setup(repo => repo.GetByPackageIdAsync(packageId)).ReturnsAsync(attempts);

        // Act
        var results = await service.GetDeliveryAttemptsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(attempts.Count);
        results.Should().BeEquivalentTo(attempts, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetDeliveryAttemptsByPackageId_AttemptsDoesNotExists_ReturnsEmptyAttemptsList(
        int packageId,
        [Frozen] Mock<IAttemptsRepository> attemptsRepoMock,
        ShipmentService service)
    {
        // Arrange
        attemptsRepoMock.Setup(repo => repo.GetByPackageIdAsync(packageId))
            .ReturnsAsync(new List<DeliveryAttempt>());

        // Act
        var results = await service.GetDeliveryAttemptsByPackageIdAsync(packageId);

        // Assert
        results.Should().HaveCount(0);
    }

    #endregion

    #region RecordEventAsync

    [Theory, AutoMoqData]
    public async Task RecordEvent_ValidRequest_ReturnsSuccessfulRecordEventResult(
        int packageId,
        Package package,
        FacilityResult facilityResult,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(frozenTime);

        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync(package);
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync(facilityResult);
        facilitiesServiceMock.Setup(fs => fs.IsFacilityAvailableForOperations(facilityResult)).Returns(true);
        packagesServiceMock.Setup(ps => ps.CanTransitionToStatus(It.IsAny<Status>(), It.IsAny<Status>())).Returns(true);
        trackingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<TrackingEvent>()))
            .ReturnsAsync((TrackingEvent trackingEvent) => trackingEvent);
        trackingRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.TrackingEventResult.PackageId.Should().Be(packageId);
        result.TrackingEventResult.PackageName.Should().Be(package.Name);
        result.TrackingEventResult.FacilityName.Should().Be(facilityResult.Name);
        result.TrackingEventResult.OccuredAt.Should().Be(frozenTime.UtcDateTime);
        result.TrackingEventResult.Should().BeEquivalentTo(createRequest, options => options.ExcludingMissingMembers());
        trackingRepoMock.Verify(repo => repo.AddAsync(It.IsAny<TrackingEvent>()), Times.Once);
        trackingRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task RecordEvent_PackageNotFound_ReturnsFailedRecordEventResult(
        int packageId,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        ShipmentService service)
    {
        // Arrange
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((Package?)null);

        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Package not found.");
    }

    [Theory, AutoMoqData]
    public async Task RecordEvent_PackageCannotAcceptTrackingEvents_ReturnsFailedRecordEventResult(
        int packageId,
        Package package,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(false);

        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Package is not in a state to accept tracking events.");
    }

    [Theory, AutoMoqData]
    public async Task RecordEvent_FacilityNotFound_ReturnsFailedRecordEventResult(
        int packageId,
        Package package,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync((FacilityResult?)null);

        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Facility not found.");
    }

    [Theory, AutoMoqData]
    public async Task RecordEvent_FacilityNotAvailableForOperations_ReturnsFailedRecordEventResult(
        int packageId,
        Package package,
        FacilityResult facilityResult,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync((facilityResult));
        facilitiesServiceMock.Setup(fs => fs.IsFacilityAvailableForOperations(facilityResult)).Returns(false);
        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Facility is not available for operations.");
    }
    
    [Theory, AutoMoqData]
    public async Task RecordEvent_CanNotTransitionToNewStatus_ReturnsFailedRecordEventResult(
        int packageId,
        Package package,
        FacilityResult facilityResult,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync((facilityResult));
        facilitiesServiceMock.Setup(fs => fs.IsFacilityAvailableForOperations(facilityResult)).Returns(true);
        packagesServiceMock.Setup(ps => ps.CanTransitionToStatus(It.IsAny<Status>(), It.IsAny<Status>())).Returns(false);
        
        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid status transition.");
    }
    
    [Theory, AutoMoqData]
    public async Task RecordEvent_PackageDelivered_CreatesSuccessfulDeliveryAttempt(
        int packageId,
        Package package,
        FacilityResult facilityResult,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        [Frozen] Mock<IAttemptsRepository> attemptsRepoMock,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(frozenTime);
        createRequest.NewStatus = DTOs.Common.Status.Delivered;
        DeliveryAttempt? addedAttempt = null;
        
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync((facilityResult));
        facilitiesServiceMock.Setup(fs => fs.IsFacilityAvailableForOperations(facilityResult)).Returns(true);
        packagesServiceMock.Setup(ps => ps.CanTransitionToStatus(It.IsAny<Status>(), It.IsAny<Status>())).Returns(true);
        attemptsRepoMock.Setup(repo => repo.AddAsync(It.IsAny<DeliveryAttempt>()))
            .Callback((DeliveryAttempt attempt) => addedAttempt = attempt)
            .ReturnsAsync((DeliveryAttempt attempt) => attempt);
        trackingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<TrackingEvent>()))
            .ReturnsAsync((TrackingEvent trackingEvent) => trackingEvent);
        trackingRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        addedAttempt.Should().NotBeNull();
        addedAttempt!.PackageId.Should().Be(packageId);
        addedAttempt.AttemptedAt.Should().Be(frozenTime.UtcDateTime);
        addedAttempt.DeliveryResult.Should().Be(Result.Successful);
        trackingRepoMock.Verify(repo => repo.AddAsync(It.IsAny<TrackingEvent>()), Times.Once);
        trackingRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task RecordEvent_PackageDelayed_CreatesFailedDeliveryAttempt(
        int packageId,
        Package package,
        FacilityResult facilityResult,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IFacilitiesService> facilitiesServiceMock,
        [Frozen] Mock<IAttemptsRepository> attemptsRepoMock,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTimeProvider,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(frozenTime);
        createRequest.NewStatus = DTOs.Common.Status.Delayed;
        DeliveryAttempt? addedAttempt = null;
        
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        facilitiesServiceMock.Setup(fs => fs.GetFacilityByIdAsync(createRequest.FacilityId))
            .ReturnsAsync((facilityResult));
        facilitiesServiceMock.Setup(fs => fs.IsFacilityAvailableForOperations(facilityResult)).Returns(true);
        packagesServiceMock.Setup(ps => ps.CanTransitionToStatus(It.IsAny<Status>(), It.IsAny<Status>())).Returns(true);
        attemptsRepoMock.Setup(repo => repo.AddAsync(It.IsAny<DeliveryAttempt>()))
            .Callback((DeliveryAttempt attempt) => addedAttempt = attempt)
            .ReturnsAsync((DeliveryAttempt attempt) => attempt);
        trackingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<TrackingEvent>()))
            .ReturnsAsync((TrackingEvent trackingEvent) => trackingEvent);
        trackingRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        addedAttempt.Should().NotBeNull();
        addedAttempt!.PackageId.Should().Be(packageId);
        addedAttempt.AttemptedAt.Should().Be(frozenTime.UtcDateTime);
        addedAttempt.DeliveryResult.Should().Be(Result.Failed);
        trackingRepoMock.Verify(repo => repo.AddAsync(It.IsAny<TrackingEvent>()), Times.Once);
        trackingRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Theory, AutoMoqData]
    public async Task RecordEvent_ValidRequestWithoutAFacility_CreatesSuccessfulEventRecord(
        int packageId,
        Package package,
        CreateTrackingEventRequest createRequest,
        [Frozen] Mock<IPackagesRepository> packagesRepoMock,
        [Frozen] Mock<IPackagesService> packagesServiceMock,
        [Frozen] Mock<IAttemptsRepository> attemptsRepoMock,
        [Frozen] Mock<ITrackingRepository> trackingRepoMock,
        ShipmentService service)
    {
        // Arrange
        package.Id = packageId;
        createRequest.FacilityId = null;
        
        packagesRepoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((package));
        packagesServiceMock.Setup(ps => ps.CanAcceptTrackingEvents(package)).Returns(true);
        packagesServiceMock.Setup(ps => ps.CanTransitionToStatus(It.IsAny<Status>(), It.IsAny<Status>())).Returns(true);
        attemptsRepoMock.Setup(repo => repo.AddAsync(It.IsAny<DeliveryAttempt>()))
            .ReturnsAsync((DeliveryAttempt attempt) => attempt);
        trackingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<TrackingEvent>()))
            .ReturnsAsync((TrackingEvent trackingEvent) => trackingEvent);
        trackingRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        // Act
        var result = await service.RecordEventAsync(packageId, createRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.TrackingEventResult.FacilityName.Should().BeNull();
        trackingRepoMock.Verify(repo => repo.AddAsync(It.IsAny<TrackingEvent>()), Times.Once);
        trackingRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    #endregion
}