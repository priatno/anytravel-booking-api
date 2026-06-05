using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;

namespace AnyTravel.BookingApi.Integrations.Notifications;

public class BookingNotificationOptions
{
    public string? TopicArn { get; set; }
}

/// <summary>
/// Publishes a booking-confirmed event to SNS so downstream systems (loyalty
/// points, email/SMS notifications, the future reporting pipeline) can react
/// without booking-api calling each of them directly.
///
/// This was added ahead of the SNS/SQS multi-shard design work discussed in
/// the August architecture review — right now there's a single, unsharded
/// topic (when configured at all) and no dead-letter queue. Treat this as
/// the seed of that migration, not the finished version. If TopicArn isn't
/// configured, publishing is skipped and logged rather than failing the
/// booking request.
/// </summary>
public class BookingNotifier
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly BookingNotificationOptions _options;
    private readonly ILogger<BookingNotifier> _logger;

    public BookingNotifier(IAmazonSimpleNotificationService sns, IOptions<BookingNotificationOptions> options, ILogger<BookingNotifier> logger)
    {
        _sns = sns;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishBookingConfirmedAsync(int bookingId, string pnrCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.TopicArn))
        {
            _logger.LogInformation("BookingNotifications:TopicArn not configured - skipping SNS publish for booking {BookingId}", bookingId);
            return;
        }

        var message = System.Text.Json.JsonSerializer.Serialize(new { bookingId, pnrCode, eventType = "BookingConfirmed" });

        await _sns.PublishAsync(new PublishRequest
        {
            TopicArn = _options.TopicArn,
            Message = message,
        }, ct);
    }
}
