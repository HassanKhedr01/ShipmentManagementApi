using Moq;

using FluentAssertions;

using AutoFixture.Xunit2;

using Microsoft.Extensions.Time.Testing;

using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Enums;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Services;

public class PackagesServiceTest
{
    #region GetPackageByIdAsync

    [Theory, AutoMoqData]
    public async Task GetPackageById_PackageExists_ReturnsPackage(
        int packageId, Package package,
        [Frozen] Mock<IPackagesRepository> reopMock,
        PackagesService service)
    {
        // Arrange
        package.Id = packageId;
        reopMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync(package);

        // Act
        var result = await service.GetPackageByIdAsync(packageId);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(package, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetPackageById_PackageDoesNotExist_ReturnsNull(
        int packageId,
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        repoMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((Package?)null);

        // Act
        var result = await service.GetPackageByIdAsync(packageId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserPackageByIdAsync

    [Theory, AutoMoqData]
    public async Task GetUserPackageById_PackageExists_ReturnsPackage(
        int packageId,
        Package package,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        package.Id = packageId;
        Guid userId = Guid.NewGuid();
        package.ApplicationUserId = userId;
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync(package);

        // Act
        var result = await service.GetUserPackageByIdAsync(packageId);

        // Assert
        Assert.NotNull(result);
        result.Should().BeEquivalentTo(package, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetUserPackageById_PackageDoesNotExist_ReturnsNull(
        int packageId,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync((Package?)null);

        // Act
        var result = await service.GetUserPackageByIdAsync(packageId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetAllPackagesAsync

    [Theory, AutoMoqData]
    public async Task GetAllPackages_PackagesExist_ReturnsPackages(
        List<Package> packages,
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(packages);

        // Act
        var resultList = await service.GetAllPackagesAsync();

        // Assert
        Assert.NotNull(resultList);
        Assert.Equal(packages.Count, resultList.Count);
        resultList.Should().BeEquivalentTo(packages, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetAllPackages_NoPackagesExist_ReturnsEmptyList(
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Package>());

        // Act
        var resultList = await service.GetAllPackagesAsync();

        // Assert
        Assert.NotNull(resultList);
        Assert.Empty(resultList);
    }

    #endregion

    #region GetAllUserPackagesAsync

    [Theory, AutoMoqData]
    public async Task GetAllUserPackages_PackagesExist_ReturnsPackages(
        List<Package> packages,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        foreach (var package in packages)
        {
            package.ApplicationUserId = userId;
        }

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetAllAsync(userId)).ReturnsAsync(packages);

        // Act
        var resultList = await service.GetAllUserPackagesAsync();

        // Assert
        Assert.NotNull(resultList);
        Assert.Equal(packages.Count, resultList.Count);
        resultList.Should().BeEquivalentTo(packages, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetAllUserPackages_NoPackagesExist_ReturnsEmptyList(
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetAllAsync(userId)).ReturnsAsync(new List<Package>());

        // Act
        var resultList = await service.GetAllUserPackagesAsync();

        // Assert
        Assert.NotNull(resultList);
        Assert.Empty(resultList);
    }

    #endregion

    #region GetPackageByTrackingNumberAsync

    [Theory, AutoMoqData]
    public async Task GetPackageByTrackingNumber_PackageExists_ReturnsPackage(
        string trackingNumber,
        Package package,
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        package.TrackingNumber = trackingNumber;
        repoMock.Setup(repo => repo.GetByTrackingNumberAsync(trackingNumber)).ReturnsAsync(package);

        // Act
        var result = await service.GetPackageByTrackingNumberAsync(trackingNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(trackingNumber, result.TrackingNumber);
        result.Should().BeEquivalentTo(package, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetPackageByTrackingNumber_PackageDoesNotExist_ReturnsNull(
        string trackingNumber,
        [Frozen] Mock<IPackagesRepository> repoMock,
        PackagesService service)
    {
        // Arrange
        repoMock.Setup(repo => repo.GetByTrackingNumberAsync(trackingNumber)).ReturnsAsync((Package?)null);

        // Act
        var result = await service.GetPackageByTrackingNumberAsync(trackingNumber);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region CanAcceptTrackingEvents

    [Theory]
    [InlineAutoMoqData(Status.Created)]
    [InlineAutoMoqData(Status.PickedUp)]
    [InlineAutoMoqData(Status.InTransit)]
    [InlineAutoMoqData(Status.ArrivedAtFacility)]
    [InlineAutoMoqData(Status.OutForDelivery)]
    [InlineAutoMoqData(Status.Delayed)]
    public void CanAcceptTrackingEvents_PackageCanAccept_ReturnsTrue(Status validStatus, PackagesService service)
    {
        // Arrange
        var package = new Package { CurrentStatus = validStatus };

        // Act
        var result = service.CanAcceptTrackingEvents(package);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineAutoMoqData(Status.Cancelled)]
    [InlineAutoMoqData(Status.Delivered)]
    public void CanAcceptTrackingEvents_PackageCannotAccept_ReturnsFalse(Status validStatus, PackagesService service)
    {
        // Arrange
        var package = new Package { CurrentStatus = validStatus };

        // Act
        var result = service.CanAcceptTrackingEvents(package);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CanTransitionToStatus

    [Theory]
    [InlineAutoMoqData(Status.Created, Status.PickedUp)]
    [InlineAutoMoqData(Status.Created, Status.Cancelled)]
    [InlineAutoMoqData(Status.PickedUp, Status.InTransit)]
    [InlineAutoMoqData(Status.InTransit, Status.ArrivedAtFacility)]
    [InlineAutoMoqData(Status.InTransit, Status.Delayed)]
    [InlineAutoMoqData(Status.InTransit, Status.OutForDelivery)]
    [InlineAutoMoqData(Status.ArrivedAtFacility, Status.InTransit)]
    [InlineAutoMoqData(Status.Delayed, Status.InTransit)]
    [InlineAutoMoqData(Status.Delayed, Status.OutForDelivery)]
    [InlineAutoMoqData(Status.OutForDelivery, Status.Delayed)]
    [InlineAutoMoqData(Status.OutForDelivery, Status.Delivered)]
    public void CanTransitionToStatus_ValidTransitions_ReturnsTrue(Status currentStatus, Status targetStatus,
        PackagesService service)
    {
        // Act
        var result = service.CanTransitionToStatus(currentStatus, targetStatus);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineAutoMoqData(Status.Created, Status.InTransit)] // skips state
    [InlineAutoMoqData(Status.Created, Status.Delivered)] // skips all states
    [InlineAutoMoqData(Status.PickedUp, Status.Delivered)] // skips states
    [InlineAutoMoqData(Status.InTransit, Status.Delivered)] // skips OutForDelivery
    [InlineAutoMoqData(Status.ArrivedAtFacility, Status.Delivered)] // invalid destination
    [InlineAutoMoqData(Status.Delayed, Status.Delivered)] // invalid destination
    [InlineAutoMoqData(Status.Delivered, Status.OutForDelivery)] // terminal state
    [InlineAutoMoqData(Status.Delivered, Status.InTransit)] // terminal state
    [InlineAutoMoqData(Status.PickedUp, Status.Created)] // backwards transition
    public void CanTransitionToStatus_InvalidTransitions_ReturnsFalse(Status currentStatus, Status targetStatus,
        PackagesService service)
    {
        // Act
        var result = service.CanTransitionToStatus(currentStatus, targetStatus);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CreatePackageAsync

    [Theory, AutoMoqData]
    public async Task CreatePackageAsync_CreatesPackage_ReturnsCreatedPackage(
        CreatePackageRequest createRequest,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        Package? addedPackage = null;
        repoMock.Setup(repo => repo.AddAsync(It.IsAny<Package>()))
            .Callback((Package package) => addedPackage = package)
            .ReturnsAsync((Package package) => package);

        // Act
        var result = await service.CreatePackageAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        addedPackage.Should().NotBeNull();
        addedPackage!.ApplicationUserId.Should().Be(userId);
        result.Should().BeEquivalentTo(addedPackage, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task CreatePackageAsync_CreatesPackage_SetsInitialStatusToCreated(
        CreatePackageRequest createRequest,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        Package? addedPackage = null;
        repoMock.Setup(repo => repo.AddAsync(It.IsAny<Package>()))
            .Callback((Package package) => addedPackage = package)
            .ReturnsAsync((Package package) => package);

        // Act
        var result = await service.CreatePackageAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        addedPackage.Should().NotBeNull();
        result.CurrentStatus.Should().Be(DTOs.Common.Status.Created);
    }

    [Theory, AutoMoqData]
    public async Task CreatePackageAsync_CreatesPackage_AddInitialTrackingEvent(
        CreatePackageRequest createRequest,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        Package? addedPackage = null;
        repoMock.Setup(repo => repo.AddAsync(It.IsAny<Package>()))
            .Callback((Package package) => addedPackage = package)
            .ReturnsAsync((Package package) => package);

        // Act
        await service.CreatePackageAsync(createRequest);

        // Assert
        addedPackage.Should().NotBeNull();
        addedPackage.TrackingEvents.Should().ContainSingle();

        var addedEvent = addedPackage.TrackingEvents.Single();
        addedEvent.Status.Should().Be(Status.Created);
        addedEvent.PackageId.Should().Be(addedPackage.Id);
        addedEvent.PackageName.Should().Be(addedPackage.Name);
    }

    [Theory, AutoMoqData]
    public async Task CreatePackageAsync_StandardPackage_CreatesPackageWithStandardDeliveryType(
        CreatePackageRequest createRequest,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTime,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var frozenTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        fakeTime.SetUtcNow(frozenTime);
        createRequest.DeliveryType = DTOs.Common.DeliveryType.Standard;
        Package? addedPackage = null;

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.AddAsync(It.IsAny<Package>()))
            .Callback((Package package) => addedPackage = package)
            .ReturnsAsync((Package package) => package);

        // Act
        var result = await service.CreatePackageAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        addedPackage.Should().NotBeNull();
        result.DeliveryType.Should().Be(DTOs.Common.DeliveryType.Standard);
        result.EstimatedDeliveryDate.Should().Be(frozenTime.UtcDateTime.AddDays(3));
    }

    [Theory, AutoMoqData]
    public async Task CreatePackageAsync_ExpressPackage_CreatesPackageWithExpressDeliveryType(
        CreatePackageRequest createRequest,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider fakeTime,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        var frozenTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        fakeTime.SetUtcNow(frozenTime);
        createRequest.DeliveryType = DTOs.Common.DeliveryType.Express;
        Package? addedPackage = null;

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.AddAsync(It.IsAny<Package>()))
            .Callback((Package package) => addedPackage = package)
            .ReturnsAsync((Package package) => package);

        // Act
        var result = await service.CreatePackageAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        addedPackage.Should().NotBeNull();
        result.DeliveryType.Should().Be(DTOs.Common.DeliveryType.Express);
        result.EstimatedDeliveryDate.Should().Be(frozenTime.UtcDateTime.AddDays(1));
    }

    #endregion

    #region CancelPackageAsync

    [Theory, AutoMoqData]
    public async Task CancelPackageAsync_PackageExistsAndCancellable_ReturnsTrue(
        int packageId,
        Package package,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        package.Id = packageId;
        package.ApplicationUserId = userId;
        package.CurrentStatus = Status.Created;
        Package? cancelledPackage = null;

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync(package);
        repoMock.Setup(repo => repo.UpdateAsync(package))
            .Callback((Package p) => cancelledPackage = p)
            .ReturnsAsync(true);

        // Act
        var result = await service.CancelPackageAsync(packageId);

        // Assert
        Assert.True(result);
        cancelledPackage.Should().NotBeNull();
        cancelledPackage!.CurrentStatus.Should().Be(Status.Cancelled);
    }

    [Theory, AutoMoqData]
    public async Task CancelPackageAsync_PackageExistsAndCancellable_CreatesTrackingEvent(
        int packageId,
        Package package,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        package.Id = packageId;
        package.ApplicationUserId = userId;
        package.CurrentStatus = Status.Created;
        Package? cancelledPackage = null;

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync(package);
        repoMock.Setup(repo => repo.UpdateAsync(package))
            .Callback((Package p) => cancelledPackage = p)
            .ReturnsAsync(true);

        // Act
        var result = await service.CancelPackageAsync(packageId);

        // Assert
        Assert.True(result);
        cancelledPackage.Should().NotBeNull();
        cancelledPackage!.TrackingEvents.Should().ContainSingle(te => te.Status == Status.Cancelled);
        var trackingEvent = cancelledPackage.TrackingEvents.Single(te => te.Status == Status.Cancelled);
        trackingEvent.PackageId.Should().Be(cancelledPackage.Id);
        trackingEvent.PackageName.Should().Be(cancelledPackage.Name);
    }

    [Theory, AutoMoqData]
    public async Task CancelPackageAsync_PackageDoesNotExist_ReturnsFalse(
        int packageId,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync((Package?)null);

        // Act
        var result = await service.CancelPackageAsync(packageId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineAutoMoqData(Status.PickedUp)]
    [InlineAutoMoqData(Status.InTransit)]
    [InlineAutoMoqData(Status.ArrivedAtFacility)]
    [InlineAutoMoqData(Status.OutForDelivery)]
    [InlineAutoMoqData(Status.Delayed)]
    [InlineAutoMoqData(Status.Delivered)]
    [InlineAutoMoqData(Status.Cancelled)]
    public async Task CancelPackageAsync_PackageExistsAndNotCancellable_ReturnsFalse(
        int packageId,
        Package package,
        Status currentStatus,
        [Frozen] Mock<IPackagesRepository> repoMock,
        [Frozen] Mock<ICurrentUserService> currentUserServiceMock,
        PackagesService service)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        package.Id = packageId;
        package.ApplicationUserId = userId;
        package.CurrentStatus = currentStatus; // Not cancellable

        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync(package);
        repoMock.Setup(repo => repo.UpdateAsync(package)).ReturnsAsync(true);

        // Act
        var result = await service.CancelPackageAsync(packageId);

        // Assert
        Assert.False(result);
    }

    #endregion
}