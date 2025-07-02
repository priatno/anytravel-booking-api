namespace AnyTravel.BookingApi.Integrations.TravelXchange;

public class TravelXchangeOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AgentCode { get; set; } = string.Empty;
}

public class AvailabilityRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
}

public class AvailabilityResult
{
    public bool IsAvailable { get; set; }
    public int SeatsRemaining { get; set; }
    public string RawResponseXml { get; set; } = string.Empty;
}
