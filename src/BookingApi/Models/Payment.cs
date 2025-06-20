namespace AnyTravel.BookingApi.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public string PaymentStatus { get; set; } = "UNPAID";
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
