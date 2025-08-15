using AnyTravel.BookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AnyTravel.BookingApi.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Passenger> Passengers => Set<Passenger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Passenger>().ToTable("Passengers");
    }
}
