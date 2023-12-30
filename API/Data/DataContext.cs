using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<User, Role, int,
IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
 IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        buildUserRoleModels(builder);
        buildLikeModel(builder);
        buildMessageModel(builder);
    }

    private static void buildUserRoleModels(ModelBuilder builder)
    {
        builder.Entity<User>()
       .HasMany(ur => ur.UserRoles)
       .WithOne(u => u.User)
       .HasForeignKey(ur => ur.UserId)
       .IsRequired();

        builder.Entity<Role>()
        .HasMany(ur => ur.UserRoles)
        .WithOne(u => u.Role)
        .HasForeignKey(ur => ur.RoleId)
        .IsRequired();
    }

    private static void buildLikeModel(ModelBuilder builder)
    {

        builder.Entity<UserLike>()
        .HasKey(k => new { k.SourceUserId, k.TargetUserId });

        builder.Entity<UserLike>()
        .HasOne(s => s.SourceUser)
        .WithMany(l => l.LikedUsers)
        .HasForeignKey(s => s.SourceUserId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLike>()
        .HasOne(s => s.TargetUser)
        .WithMany(l => l.LikedByUsers)
        .HasForeignKey(s => s.TargetUserId)
        .OnDelete(DeleteBehavior.Cascade);
    }

    private static void buildMessageModel(ModelBuilder builder)
    {
        builder.Entity<Message>()
        .HasOne(u => u.Recipient)
        .WithMany(m => m.MessagesRecived)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
        .HasOne(u => u.Sender)
        .WithMany(m => m.MessagesSent)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
