using Microsoft.EntityFrameworkCore;
using TabTogetherApi.Entities;
using System;

namespace TabTogetherApi.DbContexts
{
    public class TabTogetherContext : DbContext
    {
        public TabTogetherContext(DbContextOptions<TabTogetherContext> options) : base(options) { }

        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }
        public DbSet<Friend> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Receipt configuration
            modelBuilder.Entity<Receipt>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.CreatedAt).IsRequired();
                b.Property(r => r.Total).HasPrecision(18, 2);
                b.Property(r => r.ImageUrl).HasMaxLength(2048);
                b.HasMany(r => r.Items)
                 .WithOne(i => i.Receipt)
                 .HasForeignKey(i => i.ReceiptId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ReceiptItem configuration
            modelBuilder.Entity<ReceiptItem>(b =>
{
    b.HasKey(i => i.Id);
    b.Property(i => i.Name).HasMaxLength(500).IsRequired();
    b.Property(i => i.Price).HasPrecision(18, 2);
    b.Property(i => i.Amount);
    b.Property(i => i.Confidence);
    b.HasOne(i => i.Friend)
     .WithMany() // or WithMany(f => f.AssignedItems) if you add a collection
     .HasForeignKey(i => i.FriendId)
     .OnDelete(DeleteBehavior.Restrict); // use Restrict (or DeleteBehavior.Cascade)
});

            // Friend configuration
            modelBuilder.Entity<Friend>(b =>
            {
                b.HasKey(f => f.Id);
                b.Property(f => f.FirstName).HasMaxLength(100).IsRequired();
                b.Property(f => f.LastName).HasMaxLength(100);
                b.Property(f => f.PhoneNumber).HasMaxLength(32);
            });

            // --- Seed data for testing ---
            var friendAliceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var friendBobId   = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var receiptId     = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var item1Id       = Guid.Parse("aaaa1111-1111-1111-1111-aaaaaaaaaaaa");
            var item2Id       = Guid.Parse("bbbb2222-2222-2222-2222-bbbbbbbbbbbb");

            modelBuilder.Entity<Friend>().HasData(
                new Friend
                {
                    Id = friendAliceId,
                    FirstName = "Alice",
                    LastName = "Smith",
                    PhoneNumber = "555-0101"
                },
                new Friend
                {
                    Id = friendBobId,
                    FirstName = "Bob",
                    LastName = "Jones",
                    PhoneNumber = "555-0202"
                }
            );

            modelBuilder.Entity<Receipt>().HasData(
                new Receipt
                {
                    Id = receiptId,
                    CreatedAt = new DateTimeOffset(2025, 10, 23, 10, 0, 0, TimeSpan.Zero),
                    ImageUrl = "https://tabtogether.blob.core.windows.net/images/sample-receipt.jpg",
                    Total = 18.97m
                }
            );

            modelBuilder.Entity<ReceiptItem>().HasData(
                new
                {
                    Id = item1Id,
                    Name = "Pizza",
                    Amount = 1,
                    Price = 12.99m,
                    Confidence = 0.98,
                    ReceiptId = receiptId,
                    FriendId = friendAliceId
                },
                new
                {
                    Id = item2Id,
                    Name = "Soda",
                    Amount = 2,
                    Price = 2.99m,
                    Confidence = 0.95,
                    ReceiptId = receiptId,
                    FriendId = friendBobId
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}