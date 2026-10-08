using FluentAssertions;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Enums;

namespace ShipmentManagement.Application.Tests.Mappings;

public class EnumsMappingExtensionsTests
{
    [Theory]
    [InlineData(DTOs.Common.Status.Created, Status.Created)]
    [InlineData(DTOs.Common.Status.PickedUp, Status.PickedUp)]
    [InlineData(DTOs.Common.Status.InTransit, Status.InTransit)]
    [InlineData(DTOs.Common.Status.ArrivedAtFacility, Status.ArrivedAtFacility)]
    [InlineData(DTOs.Common.Status.OutForDelivery, Status.OutForDelivery)]
    [InlineData(DTOs.Common.Status.Delivered, Status.Delivered)]
    [InlineData(DTOs.Common.Status.Delayed, Status.Delayed)]
    [InlineData(DTOs.Common.Status.Cancelled, Status.Cancelled)]
    public void ToDomain_ShouldMapStatusCorrectly(DTOs.Common.Status value, Status expected)
    {
        // Act
        var result = value.ToDomain();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDomain_ForInvalidStatus_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (DTOs.Common.Status)999;

        // Act
        var act = () => value.ToDomain();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(Status.Created, DTOs.Common.Status.Created)]
    [InlineData(Status.PickedUp, DTOs.Common.Status.PickedUp)]
    [InlineData(Status.InTransit, DTOs.Common.Status.InTransit)]
    [InlineData(Status.ArrivedAtFacility, DTOs.Common.Status.ArrivedAtFacility)]
    [InlineData(Status.OutForDelivery, DTOs.Common.Status.OutForDelivery)]
    [InlineData(Status.Delivered, DTOs.Common.Status.Delivered)]
    [InlineData(Status.Delayed, DTOs.Common.Status.Delayed)]
    [InlineData(Status.Cancelled, DTOs.Common.Status.Cancelled)]
    public void ToDto_ShouldMapStatusCorrectly(Status value, DTOs.Common.Status expected)
    {
        // Act
        var result = value.ToDto();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDto_ForInvalidStatus_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (Status)999;

        // Act
        var act = () => value.ToDto();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(DTOs.Common.Result.Successful, Result.Successful)]
    [InlineData(DTOs.Common.Result.Failed, Result.Failed)]
    public void ToDomain_ShouldMapResultCorrectly(DTOs.Common.Result value, Result expected)
    {
        // Act
        var result = value.ToDomain();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDomain_ForInvalidResult_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (DTOs.Common.Result)999;

        // Act
        var act = () => value.ToDomain();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(Result.Successful, DTOs.Common.Result.Successful)]
    [InlineData(Result.Failed, DTOs.Common.Result.Failed)]
    public void ToDto_ShouldMapResultCorrectly(Result value, DTOs.Common.Result expected)
    {
        // Act
        var result = value.ToDto();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDto_ForInvalidResult_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (Result)999;

        // Act
        var act = () => value.ToDto();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(DTOs.Common.DeliveryType.Standard, DeliveryType.Standard)]
    [InlineData(DTOs.Common.DeliveryType.Express, DeliveryType.Express)]
    public void ToDomain_ShouldMapDeliveryTypeCorrectly(DTOs.Common.DeliveryType value, DeliveryType expected)
    {
        // Act
        var result = value.ToDomain();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDomain_ForInvalidDeliveryType_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (DTOs.Common.DeliveryType)999;

        // Act
        var act = () => value.ToDomain();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(DeliveryType.Standard, DTOs.Common.DeliveryType.Standard)]
    [InlineData(DeliveryType.Express, DTOs.Common.DeliveryType.Express)]
    public void ToDto_ShouldMapDeliveryTypeCorrectly(DeliveryType value, DTOs.Common.DeliveryType expected)
    {
        // Act
        var result = value.ToDto();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToDto_ForInvalidDeliveryType_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = (DeliveryType)999;

        // Act
        var act = () => value.ToDto();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}