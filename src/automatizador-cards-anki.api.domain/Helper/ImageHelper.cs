namespace automatizador_cards_anki.api.domain.Helper;

public static class ImageHelper
{
    private static readonly string FOLDER_NAME = "images";

    public static string SaveToPathAndGetUri(byte[] bytes, string fileName)
    {
        var pathImages = Path.Combine(Directory.GetCurrentDirectory(), FOLDER_NAME);

        if (!Directory.Exists(pathImages))
            Directory.CreateDirectory(pathImages);

        var pathFile = Path.Combine(pathImages, fileName + ".png");

        File.WriteAllBytes(pathFile, bytes);

        return new Uri(pathFile).AbsolutePath;
    }

    public static async Task RemoveFilesAsync()
    {
        var pathImages = Path.Combine(Directory.GetCurrentDirectory(), FOLDER_NAME);

        if (Directory.Exists(pathImages))
            Directory.Delete(pathImages, true);
    }
}
