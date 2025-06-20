using AnyTravel.BookingApi.Data;
using AnyTravel.BookingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnyTravel.BookingApi.Controllers;

/// <summary>
/// Handles payment confirmations coming from the payment gateway webhook.
/// NOTE: overlaps with BookingsController.UpdatePaymentStatus — both end up
/// calling sp_UpdatePaymentStatus with slightly different validation. This
/// duplication predates the current team and hasn't been consolidated.
/// </summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly SqlDataAccess _data;

    public PaymentsController(SqlDataAccess data)
    {
        _data = data;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleGatewayWebhook([FromBody] Payment payment)
    {
        if (payment.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be positive." });
        }

        await _data.UpdatePaymentStatusAsync(payment.BookingId, payment.PaymentStatus, payment.Amount, payment.Notes);
        return Ok();
    }
}
