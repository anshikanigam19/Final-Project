using System.Text;
using System.IO;
using BloggingPlatform.API.Data;
using BloggingPlatform.API.Services;
using BloggingPlatform.API.Interfaces;
using BloggingPlatform.API.Repositories;
using BloggingPlatform.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
    
    // Add a more permissive policy for development
    options.AddPolicy("Development", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure DbContext with SQL Server and InMemory fallback
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // First try to connect to SQL Server
    try
    {
        options.UseSqlServer(connectionString, sqlServerOptions =>
        {
            // Increase command timeout to allow more time for connection
            sqlServerOptions.CommandTimeout(120); // Increased timeout to 2 minutes
            // Enable retry on failure with more aggressive settings
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
        Console.WriteLine("Configured for SQL Server with connection string: " + connectionString);
    }
    catch (Exception ex)
    {
        // Log the SQL Server configuration error
        Console.WriteLine($"Error configuring SQL Server: {ex.Message}");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] SQL Server configuration error: {ex.Message}\n");
        
        // Fall back to in-memory database for development/testing
        options.UseInMemoryDatabase("FallbackBlogDb");
        Console.WriteLine("WARNING: Using fallback in-memory database due to SQL Server connection issues");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Using fallback in-memory database\n");
    }
});

// Configure Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blogging Platform API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Migration error: {ex.Message}\n");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Always use the CORS policy that allows the Angular app
app.UseCors("AllowAngularApp");

// Log CORS configuration
Console.WriteLine("CORS configured to allow requests from http://localhost:4200");

// Ensure wwwroot directory exists
var webRootPath = builder.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    builder.Environment.WebRootPath = webRootPath;
}

if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

// Ensure uploads directory exists
var uploadsPath = Path.Combine(webRootPath, "uploads", "images");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Configure static files middleware to serve uploaded files
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize Database with enhanced error handling and fallback
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Attempting to initialize database...");
    Console.WriteLine("Attempting to initialize database...");
    
    try
    {
        // Test connection before creating database
        bool canConnect = false;
        try {
            canConnect = context.Database.CanConnect();
            Console.WriteLine($"Database connection test: {(canConnect ? "Successful" : "Failed")}");
            logger.LogInformation($"Database connection test: {(canConnect ? "Successful" : "Failed")}");
            
            // Log the database provider being used
            var dbProvider = context.Database.ProviderName;
            Console.WriteLine($"Using database provider: {dbProvider}");
            logger.LogInformation($"Using database provider: {dbProvider}");
            File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Using database provider: {dbProvider}\n");
        }
        catch (Exception connEx) {
            Console.WriteLine($"Connection test failed: {connEx.Message}");
            logger.LogError(connEx, "Connection test failed");
            File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Connection test failed: {connEx.Message}\n");
            if (connEx.InnerException != null) {
                File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Connection test inner exception: {connEx.InnerException.Message}\n");
            }
            // Don't throw - we'll continue with database creation regardless
            canConnect = false;
        }
        
        // Always try to create and seed the database, whether it's SQL Server or InMemory
        context.Database.EnsureCreated();
        
        // Seed initial data
        DbInitializer.Initialize(context);
        
        Console.WriteLine("Database initialized successfully.");
        logger.LogInformation("Database initialized successfully.");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Database initialized successfully.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while initializing the database.");
        Console.WriteLine(ex.Message);
        
        if (ex.InnerException != null)
        {
            Console.WriteLine("Inner exception: " + ex.InnerException.Message);
        }
        
        // Log to file with more details
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Database initialization error: {ex.Message}\n");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Exception type: {ex.GetType().FullName}\n");
        File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Stack trace: {ex.StackTrace}\n");
        
        if (ex.InnerException != null)
        {
            File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Inner exception: {ex.InnerException.Message}\n");
            File.AppendAllText("error_logs.txt", $"[{DateTime.Now}] Inner exception type: {ex.InnerException.GetType().FullName}\n");
        }
    }
}

app.Run();