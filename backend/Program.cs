using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<RegistrationService>();

var app = builder.Build();
var frontendDirectory = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend"));
var frontendFiles = new PhysicalFileProvider(frontendDirectory);

app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = frontendFiles });
app.UseStaticFiles(new StaticFileOptions { FileProvider = frontendFiles });

var universityEmailPattern = new Regex(@"^[^\s@]+@univ\.edu\.ph$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

app.MapPost("/api/registrations", async (CreateRegistrationRequest request, RegistrationService registrations, CancellationToken cancellationToken) =>
{
    var fullName = request.FullName?.Trim();
    var email = request.Email?.Trim();
    var eventTitle = request.EventTitle?.Trim();
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 100)
    {
        errors[nameof(request.FullName)] = ["Enter your name (up to 100 characters)."];
    }

    if (string.IsNullOrWhiteSpace(email) || email.Length > 150 || !new EmailAddressAttribute().IsValid(email) || !universityEmailPattern.IsMatch(email))
    {
        errors[nameof(request.Email)] = ["Enter a valid email address ending in @univ.edu.ph."];
    }

    if (string.IsNullOrWhiteSpace(eventTitle) || eventTitle.Length > 150)
    {
        errors[nameof(request.EventTitle)] = ["Choose a valid event."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    try
    {
        var registration = await registrations.CreateRegistrationAsync(fullName!, email!, eventTitle!, cancellationToken);
        return Results.Created($"/api/registrations/{registration.RegistrationId}", registration);
    }
    catch (EventNotFoundException exception)
    {
        return Results.NotFound(new { message = exception.Message });
    }
    catch (RegistrationConflictException exception)
    {
        return Results.Conflict(new { message = exception.Message });
    }
});

app.Run();

public sealed record CreateRegistrationRequest(string? FullName, string? Email, string? EventTitle);