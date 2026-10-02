using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class RegistrationService
{
    private readonly IConfiguration _configuration;

    public RegistrationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GetUserRegistration(string inputEmail)
    {
        // Reject invalid input before it reaches the database.
        if (string.IsNullOrWhiteSpace(inputEmail))
        {
            throw new ArgumentException("Email cannot be null, empty, or whitespace.", nameof(inputEmail));
        }

        // Read the connection string from configuration instead of embedding credentials.
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        const string query = @"
            SELECT TOP 1 r.registration_id AS RegistrationId
            FROM dbo.Registrations AS r
            INNER JOIN dbo.Users AS u ON u.user_id = r.user_id
            WHERE u.email = @Email;";

        // Dispose database resources on all exit paths.
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(query, connection);

        // Bind the email with a bounded NVARCHAR parameter to prevent SQL injection.
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255)
        {
            Value = inputEmail
        });

        connection.Open();
        var result = command.ExecuteScalar();

        // Return null when no registration matches the email.
        return result is null or DBNull ? null : result.ToString();
    }
}
