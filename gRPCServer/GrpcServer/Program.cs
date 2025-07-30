using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar o Kestrel para não usar SSL
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
        // Não configurar TLS para desenvolvimento sem certificado
    });
});

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<StudyService>();

app.MapGet("/", () => "Servidor gRPC em execução...");

app.Run();