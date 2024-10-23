using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using automatizador_cards_anki.api.domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace automatizador_cards_anki.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnkiController : ControllerBase
    {
        protected readonly ISender _sender;

        public AnkiController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("insert-cards")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<Result> InserirCardsAnkiAsync(InsertCardsRequest words, CancellationToken cancellationToken)
        {
            return await _sender.Send(words, cancellationToken);
        }
    }
}
