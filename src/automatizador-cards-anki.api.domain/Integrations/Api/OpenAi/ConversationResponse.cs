using automatizador_cards_anki.api.domain.Helper;

namespace automatizador_cards_anki.api.domain.Integrations.Api.OpenAi;

public class ConversationResponse
{
    public string Text { get; set; } = string.Empty;
    public string MeaningPart { get; set; } = string.Empty;
    public string PhrasePart { get; set; } = string.Empty;

    public void CleanText()
    {
        Text = Text.Replace("\\n", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\"", string.Empty)
                    .Replace(@"\", string.Empty)
                    .Replace("*", string.Empty)
                    .Trim();
    }

    public void FormatPhrase(string word)
    {
        PhrasePart = StringHelper.ToFirstLetterUpperCase(PhrasePart.Replace(word, $"<b>{word}</b>"));
    }

    public void FormatMeaning()
    {
        MeaningPart = MeaningPart.ToLowerInvariant();
        MeaningPart = MeaningPart.Replace("meaning:", string.Empty)
                                .Replace("simple", string.Empty)
                                .Trim();
        MeaningPart = StringHelper.ToFirstLetterUpperCase(MeaningPart);
    }

    public void GetMeaningPart()
    {
        GetIndexPhraseMarker(out _, out int idx);

        if (idx >= 0)
            MeaningPart = Text.Substring(0, idx).Trim();
        else
        {
            var firstPeriod = Text.IndexOf('.');
            
            if (firstPeriod > 0)
                MeaningPart = Text.Substring(0, firstPeriod + 1).Trim();
            else
                MeaningPart = Text;
        }
    }

    public void GetPhrasePart()
    {
        GetIndexPhraseMarker(out string phraseMarker, out int idx);

        if (idx >= 0)
            PhrasePart = Text.Substring(idx + phraseMarker.Length).Trim();
        else
        {
            var firstPeriod = Text.IndexOf('.');
            if (firstPeriod > 0)
                PhrasePart = Text.Substring(firstPeriod + 1).Trim();
            else
                PhrasePart = string.Empty;
        }
    }

    private void GetIndexPhraseMarker(out string phraseMarker, out int idx)
    {
        phraseMarker = "phrase:";
        idx = Text.IndexOf(phraseMarker, StringComparison.OrdinalIgnoreCase);
    }
}
