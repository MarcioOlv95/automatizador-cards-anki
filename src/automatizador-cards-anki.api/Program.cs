using automatizador_cards_anki.api.integrations.azure_openai;
using automatizador_cards_anki.api.application.Common;
using automatizador_cards_anki.api.integrations.anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.application.Cards.Validators;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly));
builder.Services.AddScoped<IOpenAiApiManager, OpenAiApiManager>();
builder.Services.AddScoped<IAnkiApiManager, AnkiApiManager>();
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<InsertCardsRequestValidator>());

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

app.UseAuthorization();

app.MapControllers();

app.Run();
