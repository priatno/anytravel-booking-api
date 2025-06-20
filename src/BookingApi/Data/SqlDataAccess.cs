using System.Data;
using AnyTravel.BookingApi.Models;
using Microsoft.Data.SqlClient;

namespace AnyTravel.BookingApi.Data;

/// <summary>
/// Direct ADO.NET access to BookingDB. Predates the EF Core adoption — most
/// of the API still goes through here rather than through a repository
/// abstraction. Kept as-is pending the ECS/Aurora PostgreSQL migration.
/// </summary>
public class SqlDataAccess
{
    private readonly string _connectionString;

    public SqlDataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> CreateBookingAsync(Booking booking)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_CreateBooking", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PnrCode", booking.PnrCode);
        cmd.Parameters.AddWithValue("@FlightNumber", booking.FlightNumber);
        cmd.Parameters.AddWithValue("@DepartureDate", booking.DepartureDate);
        cmd.Parameters.AddWithValue("@FareAmount", booking.FareAmount);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<Booking?> GetBookingByIdAsync(int bookingId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_GetBookingById", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@BookingId", bookingId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Booking
        {
            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
            PnrCode = reader.GetString(reader.GetOrdinal("PnrCode")),
            FlightNumber = reader.GetString(reader.GetOrdinal("FlightNumber")),
            DepartureDate = reader.GetDateTime(reader.GetOrdinal("DepartureDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            FareAmount = reader.GetDecimal(reader.GetOrdinal("FareAmount")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
        };
    }

    public async Task UpdatePaymentStatusAsync(int bookingId, string status, decimal amount, string? notes)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_UpdatePaymentStatus", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@BookingId", bookingId);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}
