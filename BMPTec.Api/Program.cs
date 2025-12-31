using BMPTec.Application.Interfaces.Repositories;
using BMPTec.Application.Interfaces.Services;
using BMPTec.Application.Services;
using BMPTec.Infrastructure.Data;
using BMPTec.Infrastructure.Data.Repositories;
using BMPTec.Infrastructure.Services;
using ChuBank.Application.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting BMPTec API...");
    
    // 1. Controllers
    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
    
    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        // Detecta se está rodando em Docker
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        var connectionString = isDocker 
            ? builder.Configuration.GetConnectionString("DockerConnection")
            : builder.Configuration.GetConnectionString("DefaultConnection");
        
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
        
        // Log detalhado apenas em desenvolvimento
        if (builder.Environment.IsDevelopment())
        {
            options.LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors();
        }
    });
    
    // 3. Repositories
    builder.Services.AddScoped<IContaRepository, ContaRepository>();
    builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
    
    // 4. Application Services
    builder.Services.AddScoped<IContaService, ContaService>();
    builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();

    builder.Services.AddScoped<IExtratoAppService, ExtratoAppService>();
    
    // 5. Infrastructure Services
    builder.Services.AddScoped<ISequenceGenerator, DatabaseSequenceGenerator>();
    builder.Services.AddScoped<IExtratoService, ExtratoService>();
    builder.Services.AddMemoryCache();
    
    // 6. AutoMapper
    builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
    
    //// 7. MediatR (se estiver usando)
    //builder.Services.AddMediatR(cfg => 
    //    cfg.RegisterServicesFromAssembly(typeof(ContaService).Assembly));

    // ========== CONFIGURAÇÃO DE VERSIONAMENTO ==========
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        
        // Para versionamento por URL, configure assim:
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });
    
    // Necessário para versionamento por URL
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    
    // 8. Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "BMPTec API", 
            Version = "v1",
            Description = "API para gerenciamento de contas bancárias"
        });
    });
    
    // 9. CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    // ========== BUILD APP ==========
    
    var app = builder.Build();
    
    // ========== CONFIGURE PIPELINE ==========
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "BMPTec API v1");
            c.RoutePrefix = "swagger"; // Para acessar em /swagger
        });
    }
    
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.MapControllers();
    
    // ========== CREATE DATABASE IF NOT EXISTS ==========
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated(); // Para desenvolvimento
        // Ou para produção: await dbContext.Database.MigrateAsync();
    }
    
    Log.Information("BMPTec API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}