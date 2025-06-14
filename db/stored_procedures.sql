-- Stored procedures called from src/BookingApi/Data/SqlDataAccess.cs

CREATE OR ALTER PROCEDURE sp_CreateBooking
    @PnrCode VARCHAR(10),
    @FlightNumber VARCHAR(10),
    @DepartureDate DATETIME2,
    @FareAmount DECIMAL(12,2)
AS
BEGIN
    INSERT INTO Bookings (PnrCode, FlightNumber, DepartureDate, FareAmount, Status)
    VALUES (@PnrCode, @FlightNumber, @DepartureDate, @FareAmount, 'CONFIRMED');

    SELECT SCOPE_IDENTITY() AS BookingId;
END
GO

CREATE OR ALTER PROCEDURE sp_GetBookingById
    @BookingId INT
AS
BEGIN
    SELECT BookingId, PnrCode, FlightNumber, DepartureDate, Status, FareAmount, CreatedAtUtc
    FROM Bookings
    WHERE BookingId = @BookingId;
END
GO

CREATE OR ALTER PROCEDURE sp_UpdatePaymentStatus
    @BookingId INT,
    @Status VARCHAR(20),
    @Amount DECIMAL(12,2),
    @Notes VARCHAR(500) = NULL
AS
BEGIN
    INSERT INTO Payments (BookingId, PaymentStatus, Amount, Notes)
    VALUES (@BookingId, @Status, @Amount, @Notes);

    UPDATE Bookings SET Status = @Status WHERE BookingId = @BookingId;
END
GO

-- Orphaned: no application code calls this anymore. Left over from a
-- reporting feature that was removed. Candidate for cleanup during
-- the AWS Transform schema-conversion assessment.
CREATE OR ALTER PROCEDURE sp_GetPassengerManifest
    @FlightNumber VARCHAR(10),
    @DepartureDate DATETIME2
AS
BEGIN
    SELECT p.FullName, p.DocumentNumber, p.DocumentType, b.FlightNumber, b.DepartureDate
    FROM Passengers p
    JOIN Bookings b ON b.BookingId = p.BookingId
    WHERE b.FlightNumber = @FlightNumber AND b.DepartureDate = @DepartureDate;
END
GO
