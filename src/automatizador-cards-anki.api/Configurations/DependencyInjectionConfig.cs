using automatizador_cards_anki.api.application.Cards.Validators;
using automatizador_cards_anki.api.domain.Integrations.Api.Anki;
using automatizador_cards_anki.api.domain.Integrations.Api.OpenAi.Interface;
using automatizador_cards_anki.api.integrations.anki;
using automatizador_cards_anki.api.integrations.openai;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace automatizador_cards_anki.api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static void ResolveDependencies(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<InsertCardsRequestValidator>();

            services.AddScoped<IOpenAiApiManager, OpenAiApiManager>();
            services.AddScoped<IAnkiApiManager, AnkiApiManager>();
        }
    }
}
