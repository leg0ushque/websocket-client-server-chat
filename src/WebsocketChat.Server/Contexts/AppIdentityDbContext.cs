using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using WebsocketChat.Library.Models;
using WebsocketChat.Server.Identity;

namespace WebsocketChat.Server.Contexts
{
    public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ROLES

            var adminRoleId = "1";

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = Identity.IdentityConstants.AdminRole, NormalizedName = Identity.IdentityConstants.AdminRole.ToUpper() },
                new IdentityRole { Id = "2", Name = Identity.IdentityConstants.UserRole.ToUpper(), NormalizedName = Identity.IdentityConstants.UserRole.ToUpper() }
            );

            // FIRST USER

            var adminUserId = "admin-user-id";
            var adminUser = new User
            {
                Id = adminUserId,
                UserName = "admin@mail.com",
                Email = "admin@mail.com",
                NormalizedUserName = "ADMIN@MAIL.COM",
                NormalizedEmail = "ADMIN@MAIL.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var hasher = new PasswordHasher<User>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, Constants.AdminFirstPassword);

            builder.Entity<User>().HasData(adminUser);

            // Add admin role to the first user!
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            });

            // Messages table

            builder.Entity<Message>(entity =>
            {
                entity.Property(m => m.MessageText).IsRequired().HasMaxLength(500);
                entity.HasOne<IdentityUser>()
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .IsRequired();
            });
        }

    }
}
