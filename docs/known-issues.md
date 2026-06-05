# Known issues (last updated ~8 months ago, probably stale by now)

Running list Platform started keeping after the last incident review. Nobody
owns keeping this current, so treat dates/details as approximate.

- **IAM on the batch VM is way too broad.** `anytravel-batch-vm-01` (runs
  fare-calc-batch and reporting-svc) has `AmazonEC2FullAccess` attached to
  its instance profile. Nobody remembers why - probably copied from another
  instance during setup. Should be scoped down to just what those two jobs
  actually need (RDS network access, nothing IAM-wise beyond that).
- **`sp_GetPassengerManifest` is dead code.** No application calls it
  anymore (the reporting feature it backed was cut before launch). Safe to
  drop during the schema conversion, but nobody's verified that for certain.
- **`Legacy/web.config.xml` in booking-api is unused.** Left over from the
  IIS days. The connection string in it is stale/wrong compared to what's
  actually used today. Should just be deleted, but keeps getting missed in
  cleanup PRs.
- **BookingsController and PaymentsController both update payment status.**
  Slightly different validation in each path. Whoever consolidates these
  needs to check both call sites (the gateway webhook and the direct
  booking API) don't regress.
- **TravelXchange integration has no retry/circuit breaker.** A slow GDS
  response blocks the booking request until the HttpClient timeout fires.
  The `.ebextensions` idle timeout bump was a band-aid for this, not a fix.
- **fare-calc-batch and reporting-svc both run as crontab entries on the
  same VM, with no monitoring.** If either job dies silently, we find out
  from Finance asking where the report is, not from an alert.
- **SNS publishing in booking-api is half-wired.** The `BookingNotifier`
  code path exists but `BookingNotifications:TopicArn` isn't set in any
  environment yet, so it's a no-op today. Don't assume booking-confirmed
  events are actually flowing anywhere downstream.
