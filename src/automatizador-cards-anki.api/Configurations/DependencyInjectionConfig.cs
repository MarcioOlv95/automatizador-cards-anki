using automatizador_cards_anki.api.application.Cards.Validators;
using automatizador_cards_anki.api.application.Shared;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;
using automatizador_cards_anki.api.domain.Shared.Interface;
using automatizador_cards_anki.api.integrations.anki;
using automatizador_cards_anki.api.integrations.azure_openai;
using FluentValidation.AspNetCore;

namespace automatizador_cards_anki.api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void ResolveDependencies(this IServiceCollection services)
        {
            services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<InsertCardsRequestValidator>());
            services.AddScoped<IOpenAiApiManager, OpenAiApiManager>();
            services.AddScoped<IAnkiApiManager, AnkiApiManager>();
            services.AddScoped<IImageService, ImageService>();
        }
    }
}
