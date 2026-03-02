using GupyIntegration.Services;
using GupyIntegration.Configuration;
using GupyIntegration.Services.Interfaces;
using Microsoft.Extensions.Options;
using GupyIntegration.Models.Configurations;
using FirebaseAdmin;
using System.Security.Cryptography;
using GupyIntegration.Models;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Configure Firebase credentials
var firebaseConfig = builder.Configuration.GetSection("Firebase").Get<FirebaseConfig>();
Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    Path.Combine(Environment.CurrentDirectory, firebaseConfig?.CredentialsFile ?? "firebase.json")
);
builder.Services.AddSingleton(FirebaseApp.Create());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Coploy Gupy Integration API",
        Version = "v1",
        Description = "API for integrating with Gupy services",
        Contact = new OpenApiContact
        {
            Name = "Coploy",
            Url = new Uri("https://coploy.io")
        }
    });

    // Adiciona a configuração do ApiKey
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "ApiKey deve ser fornecida no header",
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };

    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };

    c.AddSecurityRequirement(requirement);

    // Add XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Injeção de Dependência
builder.Services.Configure<GupySettings>(builder.Configuration.GetSection("GupySettings"));
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.Configure<FirebaseConfig>(builder.Configuration.GetSection("Firebase"));
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<ICompanyInterviewService, CompanyInterviewService>();

builder.Services.AddHttpClient("GupyApi", (serviceProvider, client) =>
{
    var gulpySettings = serviceProvider.GetRequiredService<IOptions<GupySettings>>().Value;
    client.BaseAddress = new Uri(gulpySettings.BaseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {gulpySettings.ApiToken}");
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IDifyService, DifyService>();
builder.Services.AddScoped<IGupyApplicationService, GupyApplicationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configurar ApiKey
builder.Services.Configure<ApiKeySettings>(
    builder.Configuration.GetSection("ApiKey"));


var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
