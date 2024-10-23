using automatizador_cards_anki.api.domain.Shared;
using MediatR;

namespace automatizador_cards_anki.api.application.Cards.Messaging.Requests
{
    public record InsertCardsRequest : IRequest<Result>
    {
        public List<string> Words { get; set; }
    }
}
