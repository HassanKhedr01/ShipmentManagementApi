using FluentAssertions;

using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.ValueObjects;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Mappings;

public class AddressMappingExtensionsTest
{
    [Theory,AutoMoqData]
    public void ToOwnedType_WhenCalled_ShouldMapAddressDtoToAddress(AddressDto dto)
    {
        // Act
        var address = dto.ToOwnedType();

        // Assert
        address.Should().BeEquivalentTo(dto);
    }
    
    [Theory,AutoMoqData]
    public void ToDto_WhenCalled_ShouldMapAddressToAddressDto(Address address)
    {
        // Act
        var addressDto = address.ToDto();

        // Assert
        addressDto.Should().BeEquivalentTo(address);
    }
}