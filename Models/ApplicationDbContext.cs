using Microsoft.EntityFrameworkCore;

namespace hotel_system_backend.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> User { get; set; }
        public DbSet<Authorities> Authorities { get; set; }
        public DbSet<Guest> Guest { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Record> Record { get; set; }
        public DbSet<BusinessDay> BusinessDay { get; set; }
        
        public DbSet<DeletedTransactions> DeletedTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.HasSequence<long>("UserID_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("AuthorityId_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("GuestId_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("ReservationNumber_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("TransactionId_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("RecordNumber_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("BusinessDayId_seq", schema: "public").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<long>("DeletedTransactionId_seq", schema: "public").StartsAt(1000).IncrementsBy(1);

            
            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .HasDefaultValueSql("nextval('\"UserID_seq\"')")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            modelBuilder.Entity<Authorities>()
                .Property(a => a.AuthorityId)
                .HasDefaultValueSql("nextval('\"AuthorityId_seq\"')")
                .ValueGeneratedOnAdd();

            
            modelBuilder.Entity<Guest>()
                .Property(g => g.GuestId)
                .HasDefaultValueSql("nextval('\"GuestId_seq\"')")
                .ValueGeneratedOnAdd();

            
            modelBuilder.Entity<Reservation>()
                .Property(r => r.ReservationNumber)
                .HasDefaultValueSql("nextval('\"ReservationNumber_seq\"')")
                .ValueGeneratedOnAdd();

            
            modelBuilder.Entity<Transactions>()
                .Property(t => t.TransactionId)
                .HasDefaultValueSql("nextval('\"TransactionId_seq\"')")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<DeletedTransactions>()
                .Property(t => t.Id)
                .HasDefaultValueSql("nextval('\"DeletedTransactionId_seq\"')")
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<Record>()
                .Property(r => r.RecordNumber)
                .HasDefaultValueSql("nextval('\"RecordNumber_seq\"')")
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<BusinessDay>()
                .Property(r => r.Id)
                .HasDefaultValueSql("nextval('\"BusinessDayId_seq\"')")
                .ValueGeneratedOnAdd();
        }
    }
}
