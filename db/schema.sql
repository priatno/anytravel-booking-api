-- BookingDB schema
-- Shared by booking-api today. In the real AnyTravel estate this same
-- database is also touched by ~30 other services and by fare-calc-batch.

CREATE TABLE Bookings (
    BookingId       INT IDENTITY(1,1) PRIMARY KEY,
    PnrCode         VARCHAR(10) NOT NULL,
    FlightNumber    VARCHAR(10) NOT NULL,
    DepartureDate   DATETIME2 NOT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    FareAmount      DECIMAL(12,2) NOT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    -- legacy audit columns, not mapped by any current model class
    LegacySourceSystem VARCHAR(20) NULL,
    MigratedFromBatchId INT NULL
);

CREATE TABLE Passengers (
    PassengerId     INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL FOREIGN KEY REFERENCES Bookings(BookingId),
    FullName        VARCHAR(120) NOT NULL,
    DocumentNumber  VARCHAR(40) NOT NULL,
    DocumentType    VARCHAR(20) NOT NULL DEFAULT 'PASSPORT'
);

CREATE TABLE Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    BookingId       INT NOT NULL FOREIGN KEY REFERENCES Bookings(BookingId),
    PaymentStatus   VARCHAR(20) NOT NULL DEFAULT 'UNPAID',
    Amount          DECIMAL(12,2) NOT NULL,
    Notes           VARCHAR(500) NULL,
    UpdatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
