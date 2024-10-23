using automatizador_cards_anki.api.application.Cards.Messaging.Requests;
using FluentValidation;

namespace automatizador_cards_anki.api.application.Cards.Validators;

public class InsertCardsRequestValidator : AbstractValidator<InsertCardsRequest>
{
    public InsertCardsRequestValidator()
    {
        RuleFor(x => x.Words)
            .NotEmpty()
            .WithMessage("Campo obrigatório para preenchimento");
    }
}
