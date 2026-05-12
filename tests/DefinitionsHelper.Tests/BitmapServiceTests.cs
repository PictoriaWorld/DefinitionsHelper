using DefinitionsHelper.Imaging;
using Xunit;

namespace DefinitionsHelper.Tests;

public class BitmapServiceTests
{
    [Theory]
    [InlineData(76, 55, 96, 76, 55)]   // books_1_preview.png at large thumbnail sizes
    [InlineData(64, 86, 96, 64, 86)]   // chair_2_preview.png at large thumbnail sizes
    [InlineData(64, 86, 48, 36, 48)]   // chair_2_preview.png at smaller thumbnail sizes
    [InlineData(150, 152, 96, 95, 96)] // bed_1_preview.png scaled down to fit
    public void CalculateThumbnailSize_MatchesWindowsPngThumbnailSizing(
        uint originalWidth,
        uint originalHeight,
        uint maxThumbnailSize,
        int expectedWidth,
        int expectedHeight)
    {
        var size = BitmapService.CalculateThumbnailSize(originalWidth, originalHeight, maxThumbnailSize);

        Assert.Equal(expectedWidth, size.Width);
        Assert.Equal(expectedHeight, size.Height);
    }
}
