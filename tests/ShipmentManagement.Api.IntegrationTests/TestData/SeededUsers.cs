namespace ShipmentManagement.Api.IntegrationTests.TestData;

public sealed class SeededUsers
{
    public Guid UserId { get; }
    public Guid AdminId { get; }

    public SeededUsers(Guid userId, Guid adminId)
    {
        UserId = userId;
        AdminId = adminId;
    }
}