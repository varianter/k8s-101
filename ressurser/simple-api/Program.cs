var builder = WebApplication.CreateBuilder(args);

// Add swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger(); 
app.UseSwaggerUI();

app.UseHttpsRedirection();

// This isn't how you usually do this in .NET. See options-patterns instead. 
var favoriteLunch = Environment.GetEnvironmentVariable("FAV_LUNCH");
var machineName = Environment.MachineName;

var isHealthy = true;

app.MapGet("/info", () =>
    {
        if (isHealthy)
            return new
            {
                newField = "Some new field",
                favoriteLunch,
                machineName
            };

        throw new Exception("Sorry I am broken :(");
    })
    .WithName("SystemInfo")
    .WithOpenApi();

// Boss said we needed this done ASAP
app.MapPost("/experimental", () =>
{
    isHealthy = false;
    return "I think this went OK";
}).WithName("BETA: Experimental feature")
.WithOpenApi();

// Endpoint to probe weather or not this API is working properly
app.MapGet("/health", () =>
    {
        if (isHealthy) return "OK";
        throw new Exception("OH NO I AM UNHEALTHY");
    })
.WithName("Health")
.WithOpenApi();

// This app requires some time to get started. 
// Maybe you should use a readiness-probe? 
Thread.Sleep(3000);

throw new Exception("OH NO I CRASHED");

// app.Run();
