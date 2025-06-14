-- Minimal seed data for local/demo use.

INSERT INTO Bookings (PnrCode, FlightNumber, DepartureDate, Status, FareAmount)
VALUES ('AB12CD', 'QG-812', '2026-09-20T08:30:00', 'CONFIRMED', 1250000.00);

INSERT INTO Passengers (BookingId, FullName, DocumentNumber, DocumentType)
VALUES (1, 'Budi Santoso', 'P1234567', 'PASSPORT');

INSERT INTO Payments (BookingId, PaymentStatus, Amount)
VALUES (1, 'PAID', 1250000.00);
