using Microsoft.Extensions.Logging;

using Moq;

using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;

namespace ShipmentManagement.Application.Tests.Services;

public class PackagesServiceTest
{
    private readonly Mock<IPackagesRepository> _packagesRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly PackagesService _service;

    public PackagesServiceTest()
    {
        _packagesRepositoryMock = new Mock<IPackagesRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        Mock<ILogger<PackagesService>> loggerMock = new();

        _service = new PackagesService(_packagesRepositoryMock.Object, _currentUserServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task GetPackageByIdAsync_PackageExists_ReturnsPackage()
    {
        // Arrange
        int packageId = 1;
        var package = new Package { Id = packageId, Name = "Test Package" };
        _packagesRepositoryMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync(package);

        // Act
        var result = await _service.GetPackageByIdAsync(packageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(packageId, result.Id);
        Assert.Equal("Test Package", result.Name);
    }

    [Fact]
    public async Task GetPackageByIdAsync_PackageDoesNotExist_ReturnsNull()
    {
        // Arrange
        int packageId = 1;
        _packagesRepositoryMock.Setup(repo => repo.GetByIdAsync(packageId)).ReturnsAsync((Package?)null);

        // Act
        var result = await _service.GetPackageByIdAsync(packageId);

        // Assert
        Assert.Null(result);
    }
}