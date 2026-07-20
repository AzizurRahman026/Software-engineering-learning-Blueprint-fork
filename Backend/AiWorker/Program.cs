using AiWorker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<CalculatorService>();
app.MapGet("/", () => "gRPC server is running. Use a gRPC client to talk to me.");

app.Run();
