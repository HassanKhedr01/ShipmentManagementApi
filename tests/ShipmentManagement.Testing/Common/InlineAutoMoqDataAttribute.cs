using AutoFixture.Xunit2;

namespace ShipmentManagement.Testing.Common;

public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values) : base(new AutoMoqDataAttribute(), values) { }
}