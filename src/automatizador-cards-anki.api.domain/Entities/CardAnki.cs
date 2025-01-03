namespace automatizador_cards_anki.api.domain.Entities;

public class CardAnki(string word, string phrase, string meaning, string? urlImage)
{
    public string Word = word;
    public string Phrase = phrase;
    public string Meaning = meaning;
    public string? UrlImage = urlImage;
}
