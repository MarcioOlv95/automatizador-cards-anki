using automatizador_cards_anki.api.application.Common;
using Serilog;
using automatizador_cards_anki.api.Middleware;
using automatizador_cards_anki.api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly));

builder.Services.ResolveDependencies();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:4200",
                                "https://localhost")
                .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseRequestLog();

app.UseAuthorization();

app.MapControllers();

app.Run();
