using StackExchange.Redis;

// ⚠️  WARNING: THIS CODE IS FOR REFERENCE AND LEARNING PURPOSES ONLY
// ⚠️  NOT INTENDED FOR PRODUCTION USE WITHOUT PROPER SECURITY REVIEW
// ⚠️  MISSING: Security middleware, HTTPS enforcement, CORS configuration
// ⚠️  MISSING: Authentication, Authorization, Rate limiting
// ⚠️  MISSING: Health checks, monitoring, logging configuration
// ⚠️  MISSING: Connection pooling, retry policies, circuit breakers

var builder = WebApplication.CreateBuilder(args);

// Configure URLs
builder.WebHost.UseUrls("http://localhost:5001");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();

    // Localhost Redis (commented out)
    // var connectionString = configuration?.GetConnectionString("Redis") ?? "localhost:6379";
    // return ConnectionMultiplexer.Connect(connectionString);

    // Cloud Redis configuration
    var configurationOptions = new ConfigurationOptions
    {
        EndPoints = { {configuration?.GetConnectionString("RedisHost") ?? "localhost",
                      int.Parse(configuration?.GetConnectionString("RedisPort") ?? "6379")} },
        User = configuration?.GetConnectionString("RedisUser") ?? "default",
        Password = configuration?.GetConnectionString("RedisPassword") ?? ""
    };

    return ConnectionMultiplexer.Connect(configurationOptions);
});

builder.Services.AddScoped<IDatabase>(provider =>
{
    var multiplexer = provider.GetService<IConnectionMultiplexer>();
    return multiplexer?.GetDatabase() ?? throw new InvalidOperationException("Redis connection not available");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
