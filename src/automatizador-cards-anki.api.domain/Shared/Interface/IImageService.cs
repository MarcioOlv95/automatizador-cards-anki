namespace automatizador_cards_anki.api.domain.Shared.Interface;

public interface IImageService
{
    Task<string> ResizeImageAsync(string url, string word);
}
