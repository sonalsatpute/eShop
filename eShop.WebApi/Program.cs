using eShop.Observability.Configurations;
using eShop.WebApi.Database;
using eShop.WebApi.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddDomainServices();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add OpenTelemetry services.
builder.Services.AddObservability(settings: builder.Configuration, forWebApp: true);
// builder.Services.AddSingleton<IStartupFilter, ObservabilityStartupFilter>();
// builder.AddOpenTelemetry();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();