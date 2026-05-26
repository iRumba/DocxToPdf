using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DocxToPdf.Server.Tests;

public class ConvertEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConvertEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Endpoint_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Convert_WithoutFormData_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var content = new StringContent("not a form", Encoding.UTF8, "text/plain");

        var response = await client.PostAsync("/api/convert", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Request must be multipart/form-data", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Convert_WithoutFileField_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("some data"));
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        var content = new MultipartFormDataContent
        {
            { fileContent, "otherField", "data.txt" }
        };

        var response = await client.PostAsync("/api/convert", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("No file provided", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Convert_WithWrongExtension_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not a docx"));
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var content = new MultipartFormDataContent
        {
            { fileContent, "file", "test.txt" }
        };

        var response = await client.PostAsync("/api/convert", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Only .docx files", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Convert_WithEmptyFile_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        using var stream = new MemoryStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        var content = new MultipartFormDataContent
        {
            { fileContent, "file", "test.docx" }
        };

        var response = await client.PostAsync("/api/convert", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("No file provided", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Convert_WithOversizedFile_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var largeContent = new byte[60 * 1024 * 1024];
        using var stream = new MemoryStream(largeContent);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        var content = new MultipartFormDataContent
        {
            { fileContent, "file", "test.docx" }
        };

        var response = await client.PostAsync("/api/convert", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("size exceeds", body, StringComparison.OrdinalIgnoreCase);
    }
}
