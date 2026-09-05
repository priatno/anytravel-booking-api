using AnyTravel.BookingApi.Data;
using AnyTravel.BookingApi.Integrations.Notifications;
using AnyTravel.BookingApi.Integrations.TravelXchange;
using AnyTravel.BookingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnyTravel.BookingApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly SqlDataAccess _data;
    private readonly TravelXchangeSoapClient _travelXchange;
    private readonly BookingNotifier _notifier;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(SqlDataAccess data, TravelXchangeSoapClient travelXchange, BookingNotifier notifier, ILogger<BookingsController> logger)
    {
        _data = data;
        _travelXchange = travelXchange;
        _notifier = notifier;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
    {
        // Quick validation pass - we were getting garbage flight numbers and
        // past-dated bookings from a couple of partner integrations. Doesn't
        // cover fare amount yet, follow-up ticket pending.
        if (string.IsNullOrWhiteSpace(booking.FlightNumber))
        {
            return BadRequest(new { message = "flightNumber is required." });
        }

        if (booking.DepartureDate < DateTime.UtcNow)
        {
            return BadRequest(new { message = "departureDate cannot be in the past." });
        }

        // Business logic lives here rather than in a service layer — this
        // controller talks directly to BookingDB and to the GDS.
        var availability = await _travelXchange.CheckAvailabilityAsync(new AvailabilityRequest
        {
            FlightNumber = booking.FlightNumber,
            DepartureDate = booking.DepartureDate,
        });

        if (!availability.IsAvailable)
        {
            return Conflict(new { message = "No seats available on TravelXchange for this flight/date." });
        }

        booking.PnrCode = GeneratePnrCode();
        booking.CreatedAtUtc = DateTime.UtcNow;
        booking.Status = "CONFIRMED";

        var bookingId = await _data.CreateBookingAsync(booking);

        _logger.LogInformation("Booking {BookingId} created with PNR {Pnr}", bookingId, booking.PnrCode);

        await _notifier.PublishBookingConfirmedAsync(bookingId, booking.PnrCode);

        return CreatedAtAction(nameof(GetBooking), new { id = bookingId }, new { bookingId, pnrCode = booking.PnrCode });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _data.GetBookingByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPut("{id:int}/payment-status")]
    public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] Payment payment)
    {
        // NOTE: fare-calc-batch (the nightly batch job) also writes to this
        // same status via a direct DB update, outside of this API. If that
        // job's fare adjustment logic changes, this endpoint's amount
        // validation may need to change too.
        await _data.UpdatePaymentStatusAsync(id, payment.PaymentStatus, payment.Amount, payment.Notes);
        return NoContent();
    }

    private static string GeneratePnrCode() => Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
}
