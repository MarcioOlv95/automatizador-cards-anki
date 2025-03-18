using automatizador_cards_anki.api.domain.Shared.Interface;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace automatizador_cards_anki.api.application.Shared;

public class ImageService : IImageService
{
    private const int IMAGE_WIDTH = 400;
    private const int IMAGE_HEIGHT = 500;
    private const string FOLDER_NAME = "images";

    public async Task<string> ResizeImageAsync(string url, string word)
    {
        var pathImages = Path.Combine(Directory.GetCurrentDirectory()!.ToString(), FOLDER_NAME);

        if (!Directory.Exists(pathImages))
            Directory.CreateDirectory(pathImages);

        var pathFile = Path.Combine(pathImages, word + ".png");

        var response = await new HttpClient().GetAsync(url);
        using (var fs = new FileStream(pathFile, FileMode.Create))
        {
            await response.Content.CopyToAsync(fs);
        }

        var img = Image.Load(pathFile);
        img.Mutate(x => x.Resize(IMAGE_WIDTH, IMAGE_HEIGHT));
        await img.SaveAsPngAsync(pathFile);

        return pathFile;
    }
}
