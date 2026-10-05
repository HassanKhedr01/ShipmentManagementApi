using Moq;

using FluentAssertions;

using AutoFixture.Xunit2;

using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Domain.Entities;
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
        Assert.Equal(packageId, result.Id);
        Assert.Equal(package.Name, result.Name);
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
        currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        repoMock.Setup(repo => repo.GetByIdAsync(userId, packageId)).ReturnsAsync(package);

        // Act
        var result = await service.GetUserPackageByIdAsync(packageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageId, result.Id);
        Assert.Equal(package.Name, result.Name);
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

    #endregion
}