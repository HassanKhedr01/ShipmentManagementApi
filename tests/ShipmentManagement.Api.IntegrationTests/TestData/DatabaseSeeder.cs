using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Enums;
using ShipmentManagement.Domain.ValueObjects;
using ShipmentManagement.Infrastructure.Identity;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Api.IntegrationTests.TestData;

public static class DatabaseSeeder
{
    private static Guid UserId { get; set; }
    private static Guid AdminId { get; set; }

    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roles = new[] { "User", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = role });
                EnsureSucceeded(roleResult);
            }
        }

        const string userEmail = "testuser@example.com";
        const string userPassword = "UserPassword123!";
        const string adminEmail = "testadmin@example.com";
        const string adminPassword = "AdminPassword123!";

        var user = await userManager.FindByEmailAsync(userEmail);
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (user is null)
        {
            user = new ApplicationUser { UserName = userEmail, Email = userEmail };
            var userResult = await userManager.CreateAsync(user, userPassword);
            EnsureSucceeded(userResult);
            UserId = user.Id;
        }

        if (admin is null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            var userResult = await userManager.CreateAsync(admin, adminPassword);
            EnsureSucceeded(userResult);
            AdminId = admin.Id;
        }

        if (!await userManager.IsInRoleAsync(user, "User"))
        {
            var roleResult = await userManager.AddToRoleAsync(user, "User");
            EnsureSucceeded(roleResult);
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            EnsureSucceeded(roleResult);
        }
    }

    public static async Task SeedApplicationAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var facilities = new List<Facility>
        {
            new()
            {
                Id = 1,
                Name = "Cairo Central Hub",
                Address = "15 Ramses Street",
                City = "Cairo",
                IsActive = true
            },
            new()
            {
                Id = 2,
                Name = "Alexandria Distribution Center",
                Address = "20 Corniche Road",
                City = "Alexandria",
                IsActive = true
            },
            new()
            {
                Id = 3,
                Name = "Mansoura Sorting Facility",
                Address = "10 El Gomhoria Street",
                City = "Mansoura",
                IsActive = true
            },
            new()
            {
                Id = 4,
                Name = "Giza Logistics Center",
                Address = "8 Faisal Street",
                City = "Giza",
                IsActive = true
            },
            new()
            {
                Id = 5,
                Name = "Aswan Delivery Center",
                Address = "5 Corniche El Nile",
                City = "Aswan",
                IsActive = false
            }
        };

        var packages = new List<Package>
        {
            new()
            {
                Id = 1,
                Name = "Laptop Shipment",
                SenderName = "Ahmed Hassan",
                RecipientName = "Mohamed Ali",
                TrackingNumber = "PKG-4A6B7C9D",
                CurrentStatus = Status.PickedUp,
                DeliveryType = DeliveryType.Standard,
                CreatedAt = new DateTime(2026, 10, 1, 9, 0, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 4),
                ApplicationUserId = UserId,
                OriginAddress = new Address { Street = "Tahrir Street", City = "Cairo", PostalCode = "11511" },
                DestinationAddress = new Address { Street = "Corniche Road", City = "Alexandria", PostalCode = "21500" }
            },
            new()
            {
                Id = 2,
                Name = "Mobile Phone",
                SenderName = "Sara Mahmoud",
                RecipientName = "Omar Khaled",
                TrackingNumber = "PKG-3E2F1G4H",
                CurrentStatus = Status.Created,
                DeliveryType = DeliveryType.Express,
                CreatedAt = new DateTime(2026, 10, 10, 10, 30, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 11),
                ApplicationUserId = UserId,
                OriginAddress = new Address { Street = "El Nasr Street", City = "Cairo", PostalCode = "11765" },
                DestinationAddress =
                    new Address { Street = "El Gomhoria Street", City = "Mansoura", PostalCode = "35511" }
            },
            new()
            {
                Id = 3,
                Name = "Office Supplies",
                SenderName = "Nile Trading",
                RecipientName = "Delta Office",
                TrackingNumber = "PKG-9G8H7I6J",
                CurrentStatus = Status.PickedUp,
                DeliveryType = DeliveryType.Standard,
                CreatedAt = new DateTime(2026, 10, 12, 8, 0, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 15),
                ApplicationUserId = UserId,
                OriginAddress = new Address { Street = "Industrial Zone", City = "Giza", PostalCode = "12511" },
                DestinationAddress = new Address { Street = "El Bahr Street", City = "Mansoura", PostalCode = "35516" }
            },
            new()
            {
                Id = 4,
                Name = "Medical Equipment",
                SenderName = "MedTech Egypt",
                RecipientName = "Aswan Medical Center",
                TrackingNumber = "PKG-5K4L3M2N",
                CurrentStatus = Status.Created,
                DeliveryType = DeliveryType.Express,
                CreatedAt = new DateTime(2026, 10, 11, 11, 0, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 12),
                ApplicationUserId = AdminId,
                OriginAddress = new Address { Street = "Dokki Street", City = "Giza", PostalCode = "12611" },
                DestinationAddress =
                    new Address { Street = "Corniche El Nile", City = "Aswan", PostalCode = "81511" }
            },
            new()
            {
                Id = 5,
                Name = "Books Collection",
                SenderName = "Knowledge Books",
                RecipientName = "Heba Ibrahim",
                TrackingNumber = "PKG-1O2P3Q4R",
                CurrentStatus = Status.PickedUp,
                DeliveryType = DeliveryType.Standard,
                CreatedAt = new DateTime(2026, 10, 13, 7, 45, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 16),
                ApplicationUserId = AdminId,
                OriginAddress =
                    new Address { Street = "Saad Zaghloul Street", City = "Alexandria", PostalCode = "21525" },
                DestinationAddress =
                    new Address { Street = "El Geish Street", City = "Cairo", PostalCode = "11311" }
            },
            new()
            {
                Id = 6,
                Name = "Computer Accessories",
                SenderName = "Tech Solutions",
                RecipientName = "Youssef Samir",
                TrackingNumber = "PKG-5S6T7U8V",
                CurrentStatus = Status.Created,
                DeliveryType = DeliveryType.Standard,
                CreatedAt = new DateTime(2026, 10, 8, 14, 0, 0),
                EstimatedDeliveryDate = new DateTime(2026, 10, 11),
                ApplicationUserId = AdminId,
                OriginAddress =
                    new Address { Street = "Abbas El Akkad Street", City = "Cairo", PostalCode = "11765" },
                DestinationAddress = new Address
                {
                    Street = "El Horreya Street", City = "Alexandria", PostalCode = "21526"
                }
            }
        };

        var trackingEvents = new List<TrackingEvent>
        {
            new()
            {
                Id = 1,
                PackageId = 1,
                PackageName = "Laptop Shipment",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 1, 9, 0, 0),
                FacilityId = null,
                FacilityName = null
            },
            new()
            {
                Id = 2,
                PackageId = 1,
                PackageName = "Laptop Shipment",
                Status = Status.PickedUp,
                Description = "Package picked up from the sender",
                OccuredAt = new DateTime(2026, 10, 1, 12, 0, 0),
                FacilityId = 1,
                FacilityName = "Cairo Central Hub"
            },
            new()
            {
                Id = 3,
                PackageId = 2,
                PackageName = "Mobile Phone",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 10, 10, 30, 0),
                FacilityId = null,
                FacilityName = null
            },
            new()
            {
                Id = 4,
                PackageId = 3,
                PackageName = "Office Supplies",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 12, 8, 0, 0),
                FacilityId = null,
                FacilityName = null
            },
            new()
            {
                Id = 5,
                PackageId = 3,
                PackageName = "Office Supplies",
                Status = Status.PickedUp,
                Description = "Package picked up from the sender",
                OccuredAt = new DateTime(2026, 10, 12, 11, 0, 0),
                FacilityId = 4,
                FacilityName = "Giza Logistics Center"
            },
            new()
            {
                Id = 6,
                PackageId = 4,
                PackageName = "Medical Equipment",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 11, 11, 0, 0),
                FacilityId = null,
                FacilityName = null
            },
            new()
            {
                Id = 7,
                PackageId = 5,
                PackageName = "Books Collection",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 13, 7, 45, 0),
                FacilityId = null,
                FacilityName = null
            },
            new()
            {
                Id = 8,
                PackageId = 5,
                PackageName = "Books Collection",
                Status = Status.PickedUp,
                Description = "Package picked up from the sender",
                OccuredAt = new DateTime(2026, 10, 13, 9, 0, 0),
                FacilityId = 2,
                FacilityName = "Alexandria Distribution Center"
            },
            new()
            {
                Id = 9,
                PackageId = 6,
                PackageName = "Computer Accessories",
                Status = Status.Created,
                Description = "Shipment registered",
                OccuredAt = new DateTime(2026, 10, 8, 14, 0, 0),
                FacilityId = null,
                FacilityName = null
            }
        };

        var refreshTokens = new List<RefreshToken>
        {
            new()
            {
                Id = 1,
                ApplicationUserId = UserId,
                TokenHash = "sample-token-hash-001",
                CreatedAt = new DateTime(2026, 10, 10, 9, 0, 0),
                ExpiresAt = new DateTime(2026, 10, 17, 9, 0, 0),
                RevokedAt = null
            },
            new()
            {
                Id = 2,
                ApplicationUserId = UserId,
                TokenHash = "sample-token-hash-002",
                CreatedAt = new DateTime(2026, 10, 5, 9, 0, 0),
                ExpiresAt = new DateTime(2026, 10, 12, 9, 0, 0),
                RevokedAt = new DateTime(2026, 10, 12, 10, 0, 0)
            },
            new()
            {
                Id = 3,
                ApplicationUserId = AdminId,
                TokenHash = "sample-token-hash-003",
                CreatedAt = new DateTime(2026, 10, 5, 9, 0, 0),
                ExpiresAt = new DateTime(2026, 10, 12, 9, 0, 0),
                RevokedAt = null
            }
        };

        await context.Packages.AddRangeAsync(packages, cancellationToken);
        await context.Facilities.AddRangeAsync(facilities, cancellationToken);
        await context.TrackingEvents.AddRangeAsync(trackingEvents, cancellationToken);
        await context.RefreshTokens.AddRangeAsync(refreshTokens, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));

            throw new InvalidOperationException($"Identity seeding failed: {errors}");
        }
    }
}