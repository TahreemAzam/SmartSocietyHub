using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartSocietyHub.Infrastructure.Identity;
using SmartSocietyHub.Infrastructure.Persistence.Models;

namespace SmartSocietyHub.Infrastructure.Persistence
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }

        public DbSet<Resident> Residents { get; set; }

        public DbSet<FamilyMember> FamilyMembers { get; set; }

        public DbSet<Complaint> Complaints { get; set; }

        public DbSet<Facility> Facilities { get; set; }

        public DbSet<FacilityBooking> FacilityBookings { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<BillingSettings> BillingSettings { get; set; }

        public DbSet<Update> Updates { get; set; }

        public DbSet<UpdateReadStatus> UpdateReadStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ============================================================
            // PROPERTY
            // ============================================================

            // Property: HouseNumber + Block must be unique
            builder.Entity<Property>()
                .HasIndex(p => new { p.HouseNumber, p.Block })
                .IsUnique();

            // ============================================================
            // RESIDENT
            // ============================================================

            // Resident: CNIC must be unique
            builder.Entity<Resident>()
                .HasIndex(r => r.CNIC)
                .IsUnique();

            // Resident: DateOfBirth is stored as a date only
            builder.Entity<Resident>()
                .Property(r => r.DateOfBirth)
                .HasColumnType("date");

            // ============================================================
            // FAMILY MEMBER
            // ============================================================

            // FamilyMember: DateOfBirth is stored as a date only
            builder.Entity<FamilyMember>()
                .Property(f => f.DateOfBirth)
                .HasColumnType("date");

            // ============================================================
            // RESIDENT -> PROPERTY
            // ============================================================

            builder.Entity<Resident>()
                .HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // RESIDENT -> FAMILY MEMBERS
            // ============================================================

            builder.Entity<FamilyMember>()
                .HasOne(f => f.Resident)
                .WithMany(r => r.FamilyMembers)
                .HasForeignKey(f => f.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================================
            // RESIDENT -> APPLICATION USER
            // ============================================================

            // One ApplicationUser can be linked to one Resident/Owner.
            // ApplicationUserId is nullable because existing residents
            // may not have a login account.
            builder.Entity<Resident>()
                .HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Resident>(r => r.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Each ApplicationUser can be linked to only one Resident.
            builder.Entity<Resident>()
                .HasIndex(r => r.ApplicationUserId)
                .IsUnique();

            // ============================================================
            // COMPLAINT -> RESIDENT
            // ============================================================

            // Every complaint belongs to the resident who submitted it.
            builder.Entity<Complaint>()
                .HasOne(c => c.Resident)
                .WithMany()
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // COMPLAINT -> PROPERTY
            // ============================================================

            // Every complaint is associated with a property.
            builder.Entity<Complaint>()
                .HasOne(c => c.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // COMPLAINT -> MAINTENANCE STAFF
            // ============================================================

            // AssignedToUserId points to ApplicationUser.
            // The application will ensure that only a user with the
            // MaintenanceStaff role can be assigned to a complaint.
            builder.Entity<Complaint>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============================================================
            // FACILITY -> BOOKINGS
            // ============================================================

            // One facility can have many bookings.
            builder.Entity<FacilityBooking>()
                .HasOne(b => b.Facility)
                .WithMany(f => f.Bookings)
                .HasForeignKey(b => b.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================================
            // RESIDENT -> FACILITY BOOKINGS
            // ============================================================

            // One resident can have many facility bookings.
            builder.Entity<FacilityBooking>()
                .HasOne(b => b.Resident)
                .WithMany()
                .HasForeignKey(b => b.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // FACILITY BOOKING
            // ============================================================

            // Store booking date without a time component.
            builder.Entity<FacilityBooking>()
                .Property(b => b.BookingDate)
                .HasColumnType("date");

            // Store the booking status as a string.
            builder.Entity<FacilityBooking>()
                .Property(b => b.Status)
                .HasMaxLength(20);

            // Facility name should be unique.
            builder.Entity<Facility>()
                .HasIndex(f => f.Name)
                .IsUnique();

            // Facility name length.
            builder.Entity<Facility>()
                .Property(f => f.Name)
                .HasMaxLength(100);

            // Facility description length.
            builder.Entity<Facility>()
                .Property(f => f.Description)
                .HasMaxLength(500);

            // Facility location length.
            builder.Entity<Facility>()
                .Property(f => f.Location)
                .HasMaxLength(200);

            // ============================================================
            // BILLING
            // ============================================================

            // Invoice number should be unique.
            builder.Entity<Bill>()
                .HasIndex(b => b.InvoiceNumber)
                .IsUnique();

            // Store billing month as a date.
            builder.Entity<Bill>()
                .Property(b => b.BillingMonth)
                .HasColumnType("date");

            // Store issue date as a date.
            builder.Entity<Bill>()
                .Property(b => b.IssueDate)
                .HasColumnType("date");

            // Store due date as a date.
            builder.Entity<Bill>()
                .Property(b => b.DueDate)
                .HasColumnType("date");

            // Store monetary values with two decimal places.
            builder.Entity<Bill>()
                .Property(b => b.MaintenanceAmount)
                .HasPrecision(18, 2);

            builder.Entity<Bill>()
                .Property(b => b.SecurityAmount)
                .HasPrecision(18, 2);

            builder.Entity<Bill>()
                .Property(b => b.WaterAmount)
                .HasPrecision(18, 2);

            builder.Entity<Bill>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            // Store bill status as a string.
            builder.Entity<Bill>()
                .Property(b => b.Status)
                .HasMaxLength(30);

            // One resident can have many bills.
            builder.Entity<Bill>()
                .HasOne(b => b.Resident)
                .WithMany()
                .HasForeignKey(b => b.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // PAYMENT -> BILL
            // ============================================================

            // One bill can have multiple payment submissions.
            // This allows a resident to submit a new proof if an
            // earlier payment proof is rejected.
            builder.Entity<Payment>()
                .HasOne(p => p.Bill)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment monetary amount.
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // Payment method.
            builder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(20);

            // Payment status.
            builder.Entity<Payment>()
                .Property(p => p.Status)
                .HasMaxLength(30);

            // Payment proof path.
            builder.Entity<Payment>()
                .Property(p => p.PaymentProofPath)
                .HasMaxLength(500);

            // Rejection reason.
            builder.Entity<Payment>()
                .Property(p => p.RejectionReason)
                .HasMaxLength(500);

            // ============================================================
            // BILLING SETTINGS
            // ============================================================

            // Standard monthly billing charges.
            // There will be one current settings record managed
            // through the Billing Charges/Settings feature.
            builder.Entity<BillingSettings>()
                .Property(b => b.MaintenanceAmount)
                .HasPrecision(18, 2);

            builder.Entity<BillingSettings>()
                .Property(b => b.SecurityAmount)
                .HasPrecision(18, 2);

            builder.Entity<BillingSettings>()
                .Property(b => b.WaterAmount)
                .HasPrecision(18, 2);

            // ============================================================
            // COMMUNITY UPDATES & EVENTS
            // ============================================================

            // Update title length.
            builder.Entity<Update>()
                .Property(u => u.Title)
                .HasMaxLength(100)
                .IsRequired();

            // Update description length.
            builder.Entity<Update>()
                .Property(u => u.Description)
                .HasMaxLength(2000)
                .IsRequired();

            // Store UpdateType as a readable string
            // instead of an integer in PostgreSQL.
            builder.Entity<Update>()
                .Property(u => u.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Event date is stored without a time component.
            builder.Entity<Update>()
                .Property(u => u.EventDate)
                .HasColumnType("date");

            // Event location length.
            builder.Entity<Update>()
                .Property(u => u.Location)
                .HasMaxLength(200);

            // Useful index for retrieving published updates.
            builder.Entity<Update>()
                .HasIndex(u => new { u.IsPublished, u.PublishedAt });

            // ============================================================
            // UPDATE -> READ STATUS
            // ============================================================

            // One update can have many read-status records.
            builder.Entity<UpdateReadStatus>()
                .HasOne(r => r.Update)
                .WithMany()
                .HasForeignKey(r => r.UpdateId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user can mark a particular update as seen only once.
            // This is also used to calculate the user's unseen-update count.
            builder.Entity<UpdateReadStatus>()
                .HasIndex(r => new { r.UpdateId, r.UserId })
                .IsUnique();
        }
    }
}