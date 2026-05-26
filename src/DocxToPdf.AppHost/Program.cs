var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.DocxToPdf_Server>("api")
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_URLS", "http://+:8080");

builder.AddNpmApp("client", "../DocxToPdf.Client")
    .WithReference(apiService)
    .WithEndpoint(port: 5173, targetPort: 5173, scheme: "http", env: "PORT")
    .WithEnvironment("VITE_API_URL", apiService.GetEndpoint("http"))
    .PublishAsDockerFile();

builder.Build().Run();
