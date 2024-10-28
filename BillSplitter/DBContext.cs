using Microsoft.EntityFrameworkCore;
using BillSplitter.Models;
using System.Data.Common;
namespace BillSplitter.DBContext;
public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<IOU> IOUs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Initiator)
                .WithMany()
                .HasForeignKey("InitiatorId");

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey("UserId");

            modelBuilder.Entity<IOU>()
                .HasOne(i => i.Creditor)
                .WithMany()
                .HasForeignKey("CreditorId");

            modelBuilder.Entity<IOU>()
                .HasOne(i => i.Debtor)
                .WithMany()
                .HasForeignKey("DebtorId");
        }
    }