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

    public async Task<RegistrationResult> CreateRegistrationAsync(string fullName, string email, string eventTitle, CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The DefaultConnection connection string is not configured.");
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        const string eventQuery = @"
            SELECT event_id, capacity
            FROM dbo.Events WITH (UPDLOCK, HOLDLOCK)
            WHERE title = @Title;";
        await using var eventCommand = new SqlCommand(eventQuery, connection, transaction);
        eventCommand.Parameters.Add("@Title", SqlDbType.VarChar, 150).Value = eventTitle;
        await using var eventReader = await eventCommand.ExecuteReaderAsync(cancellationToken);

        if (!await eventReader.ReadAsync(cancellationToken))
        {
            throw new EventNotFoundException("That event is not available for registration.");
        }

        var eventId = eventReader.GetInt32(0);
        var capacity = eventReader.GetInt32(1);
        await eventReader.DisposeAsync();

        const string countQuery = "SELECT COUNT(*) FROM dbo.Registrations WHERE event_id = @EventId;";
        await using var countCommand = new SqlCommand(countQuery, connection, transaction);
        countCommand.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
        var registrationCount = (int)(await countCommand.ExecuteScalarAsync(cancellationToken) ?? 0);
        if (registrationCount >= capacity)
        {
            throw new RegistrationConflictException("This event is full.");
        }

        const string userQuery = @"
            SELECT user_id
            FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
            WHERE email = @Email;";
        await using var userCommand = new SqlCommand(userQuery, connection, transaction);
        userCommand.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = email;
        var userResult = await userCommand.ExecuteScalarAsync(cancellationToken);
        int userId;

        if (userResult is null or DBNull)
        {
            const string roleQuery = "SELECT role_id FROM dbo.Roles WHERE role_name = 'Student';";
            await using var roleCommand = new SqlCommand(roleQuery, connection, transaction);
            var roleResult = await roleCommand.ExecuteScalarAsync(cancellationToken);
            if (roleResult is null or DBNull)
            {
                throw new InvalidOperationException("The Student role is missing from dbo.Roles.");
            }

            const string insertUserQuery = @"
                INSERT INTO dbo.Users (full_name, email, role_id)
                OUTPUT INSERTED.user_id
                VALUES (@FullName, @Email, @RoleId);";
            await using var insertUserCommand = new SqlCommand(insertUserQuery, connection, transaction);
            insertUserCommand.Parameters.Add("@FullName", SqlDbType.VarChar, 100).Value = fullName;
            insertUserCommand.Parameters.Add("@Email", SqlDbType.VarChar, 150).Value = email;
            insertUserCommand.Parameters.Add("@RoleId", SqlDbType.Int).Value = Convert.ToInt32(roleResult);
            userId = Convert.ToInt32(await insertUserCommand.ExecuteScalarAsync(cancellationToken));
        }
        else
        {
            userId = Convert.ToInt32(userResult);
        }

        const string duplicateQuery = @"
            SELECT registration_id
            FROM dbo.Registrations
            WHERE user_id = @UserId AND event_id = @EventId;";
        await using var duplicateCommand = new SqlCommand(duplicateQuery, connection, transaction);
        duplicateCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        duplicateCommand.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
        if (await duplicateCommand.ExecuteScalarAsync(cancellationToken) is not null)
        {
            throw new RegistrationConflictException("You are already registered for this event.");
        }

        const string insertRegistrationQuery = @"
            INSERT INTO dbo.Registrations (user_id, event_id)
            OUTPUT INSERTED.registration_id
            VALUES (@UserId, @EventId);";
        await using var insertRegistrationCommand = new SqlCommand(insertRegistrationQuery, connection, transaction);
        insertRegistrationCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        insertRegistrationCommand.Parameters.Add("@EventId", SqlDbType.Int).Value = eventId;
        var registrationId = Convert.ToInt32(await insertRegistrationCommand.ExecuteScalarAsync(cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return new RegistrationResult(registrationId, eventTitle);
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

public sealed record RegistrationResult(int RegistrationId, string EventTitle);

public sealed class EventNotFoundException(string message) : Exception(message);

public sealed class RegistrationConflictException(string message) : Exception(message);
