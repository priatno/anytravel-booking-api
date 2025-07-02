using AnyTravel.BookingApi.Integrations.TravelXchange;
using Microsoft.AspNetCore.Mvc;

namespace AnyTravel.BookingApi.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly TravelXchangeSoapClient _travelXchange;

    public AvailabilityController(TravelXchangeSoapClient travelXchange)
    {
        _travelXchange = travelXchange;
    }

    [HttpGet]
    public async Task<IActionResult> Check([FromQuery] string flightNumber, [FromQuery] DateTime departureDate)
    {
        var result = await _travelXchange.CheckAvailabilityAsync(new AvailabilityRequest
        {
            FlightNumber = flightNumber,
            DepartureDate = departureDate,
        });

        return Ok(result);
    }
}
