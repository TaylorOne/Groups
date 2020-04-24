using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Groups.Areas.Identity.Data;

namespace Groups.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Group> Groups { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<FileUpload> Files { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UsersToJoin> UsersToJoin { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Group>().ToTable("Group");
            builder.Entity<Post>().ToTable("Post");
            builder.Entity<Image>().ToTable("Image");
            builder.Entity<FileUpload>().ToTable("File");
            builder.Entity<Event>().ToTable("Event");
            builder.Entity<UserGroup>().ToTable("UserGroup");
            builder.Entity<UsersToJoin>().ToTable("UserToJoin");

            builder.Entity<UserGroup>()
                .HasKey(t => new { t.UserId, t.GroupId });

            builder.Entity<UsersToJoin>()
                .HasKey(t => new { t.UserId, t.GroupId });

            builder.Entity<Image>()
                .HasOne(p => p.Group)
                .WithMany(b => b.Images);

            builder.Entity<Group>()
                .HasOne(p => p.BannerImg)
                .WithOne();

        }
    }
}
