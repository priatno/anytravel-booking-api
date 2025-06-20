namespace AnyTravel.BookingApi.Models;

// NOTE: Passenger is the only entity mapped through EF Core today. Bookings
// and Payments are still read/written via SqlDataAccess (raw ADO.NET) — this
// was a partial migration attempt that never got finished.
public class Passenger
{
    public int PassengerId { get; set; }
    public int BookingId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "PASSPORT";
}
