using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetCore_backend.Entities;

namespace NetCore_backend.DataAccess
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tablolarının (AspNetRoles, AspNetUsers vb.) doğru şekilde
            // yapılandırılması için bu satır MUTLAKA en başta olmalıdır.
            base.OnModelCreating(builder);

            SeedRolesAndUsers(builder);
        }

        private static void SeedRolesAndUsers(ModelBuilder builder)
        {
            // =================================================================
            // Roller (Roles)
            // =================================================================
            const string ADMIN_ROLE_ID = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
            const string USER_ROLE_ID = "f1e2d3c4-b5a6-9870-fedc-ba0987654321";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = ADMIN_ROLE_ID,
                    Name = "Admin",
                    NormalizedName = "ADMIN", // Identity tarafından kullanılır
                    ConcurrencyStamp = ADMIN_ROLE_ID
                },
                new IdentityRole
                {
                    Id = USER_ROLE_ID,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = USER_ROLE_ID
                }
            );

            // =================================================================
            // Kullanıcılar (Users)
            // =================================================================
            const string ADMIN_USER_ID = "fedcba98-7654-3210-fedc-ba9876543210";
            const string BASIC_USER_ID = "01234567-89ab-cdef-0123-456789abcdef";

            // Admin Kullanıcısı
            var adminUser = new AppUser
            {
                Id = ADMIN_USER_ID,
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };

            // Standart Kullanıcı
            var basicUser = new AppUser
            {
                Id = BASIC_USER_ID,
                UserName = "user@example.com",
                NormalizedUserName = "USER@EXAMPLE.COM",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                ConcurrencyStamp = Guid.NewGuid().ToString("D")
            };

            // Parolaları hash'liyoruz. Asla düz metin parola saklamayın!
            var passwordHasher = new PasswordHasher<AppUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "AdminPassword123!");
            basicUser.PasswordHash = passwordHasher.HashPassword(basicUser, "UserPassword123!");

            builder.Entity<AppUser>().HasData(adminUser, basicUser);

            // =================================================================
            // Kullanıcıları Rollere Atama (User-Role Relationship)
            // =================================================================
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = ADMIN_ROLE_ID,
                    UserId = ADMIN_USER_ID
                },
                new IdentityUserRole<string>
                {
                    RoleId = USER_ROLE_ID,
                    UserId = BASIC_USER_ID
                }
            );
        }
    }
}
