namespace automatizador_cards_anki.api.domain.Helper;

public static class StringHelper
{
    public static string ToFirstLetterUpperCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return $"{input[0].ToString().ToUpper()}{input.Substring(1)}";
    }
}
