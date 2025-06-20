namespace AnyTravel.BookingApi.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public string Status { get; set; } = "PENDING";
    public decimal FareAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
