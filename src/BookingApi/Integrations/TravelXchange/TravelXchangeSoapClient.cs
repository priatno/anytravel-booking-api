using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace AnyTravel.BookingApi.Integrations.TravelXchange;

/// <summary>
/// Hand-rolled client for the TravelXchange GDS SOAP/XML availability service.
/// No WSDL-generated proxy exists — the envelope is built and parsed by hand.
/// There is no retry policy or circuit breaker; a slow/unavailable GDS will
/// block the calling request until the HttpClient timeout is hit.
/// </summary>
public class TravelXchangeSoapClient
{
    private readonly HttpClient _httpClient;
    private readonly TravelXchangeOptions _options;

    public TravelXchangeSoapClient(HttpClient httpClient, IOptions<TravelXchangeOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AvailabilityResult> CheckAvailabilityAsync(AvailabilityRequest request, CancellationToken ct = default)
    {
        var envelope = BuildRequestEnvelope(request);

        using var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
        using var response = await _httpClient.PostAsync(_options.BaseUrl, content, ct);
        var responseXml = await response.Content.ReadAsStringAsync(ct);

        return ParseResponse(responseXml);
    }

    private string BuildRequestEnvelope(AvailabilityRequest request)
    {
        var envelope = new XElement("soap:Envelope",
            new XAttribute(XNamespace.Xmlns + "soap", "http://schemas.xmlsoap.org/soap/envelope/"),
            new XElement("soap:Body",
                new XElement("AvailabilityRequest",
                    new XElement("AgentCode", _options.AgentCode),
                    new XElement("FlightNumber", request.FlightNumber),
                    new XElement("DepartureDate", request.DepartureDate.ToString("yyyy-MM-dd"))
                )
            )
        );

        return envelope.ToString();
    }

    private AvailabilityResult ParseResponse(string responseXml)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);
            var available = doc.Descendants("Available").FirstOrDefault()?.Value == "true";
            var seats = int.TryParse(doc.Descendants("SeatsRemaining").FirstOrDefault()?.Value, out var s) ? s : 0;

            return new AvailabilityResult
            {
                IsAvailable = available,
                SeatsRemaining = seats,
                RawResponseXml = responseXml,
            };
        }
        catch (Exception)
        {
            // TODO: TravelXchange occasionally returns a non-XML error page during
            // maintenance windows. Swallowing this for now — revisit when we
            // build proper resiliency around this integration.
            return new AvailabilityResult { IsAvailable = false, SeatsRemaining = 0, RawResponseXml = responseXml };
        }
    }
}
