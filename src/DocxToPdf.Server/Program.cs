using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors();

// Health check
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

// Convert endpoint
app.MapPost("/api/convert", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest(new ErrorResponse("Request must be multipart/form-data"));

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file is null || file.Length == 0)
        return Results.BadRequest(new ErrorResponse("No file provided"));

    if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new ErrorResponse("Only .docx files are supported"));

    if (file.Length > 50 * 1024 * 1024)
        return Results.BadRequest(new ErrorResponse("File size exceeds 50 MB limit"));

    var tempDir = Path.Combine(Path.GetTempPath(), "DocxToPdf", Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);

    try
    {
        var inputPath = Path.Combine(tempDir, file.FileName);
        await using (var stream = new FileStream(inputPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var outputFileName = Path.GetFileNameWithoutExtension(file.FileName) + ".pdf";
        var outputPath = Path.Combine(tempDir, outputFileName);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = $"--headless --convert-to pdf --outdir \"{tempDir}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await process.WaitForExitAsync(cts.Token);

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            return Results.Problem(
                title: "Conversion failed",
                detail: error,
                statusCode: 500
            );
        }

        if (!File.Exists(outputPath))
            return Results.Problem(
                title: "Output file not found",
                detail: "LibreOffice did not produce the expected output file",
                statusCode: 500
            );

        var pdfBytes = await File.ReadAllBytesAsync(outputPath);
        return Results.File(pdfBytes, "application/pdf", outputFileName);
    }
    finally
    {
        if (Directory.Exists(tempDir))
        {
            try { Directory.Delete(tempDir, true); }
            catch { /* best effort cleanup */ }
        }
    }
});

app.Run();

public record ErrorResponse(string Message);

