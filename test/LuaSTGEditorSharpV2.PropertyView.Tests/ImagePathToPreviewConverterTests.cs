using System;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

using LuaSTGEditorSharpV2.PropertyView.Converter;

using Xunit;

namespace LuaSTGEditorSharpV2.PropertyView.Tests;

public sealed class ImagePathToPreviewConverterTests
{
    private readonly ImagePathToPreviewConverter _converter = new();

    [Fact]
    public void MissingPathProducesUnavailablePreview()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

        var preview = Convert(path);

        Assert.False(preview.IsAvailable);
        Assert.Null(preview.Source);
    }

    [Fact]
    public void NonImageFileProducesUnavailablePreview()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "not an image");

            var preview = Convert(path);

            Assert.False(preview.IsAvailable);
            Assert.Null(preview.Source);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DecodableImageProducesLoadedPreview()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, System.Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));

            var preview = Convert(path);
            var bitmap = Assert.IsType<BitmapImage>(preview.Source);

            Assert.True(preview.IsAvailable);
            Assert.Equal(1, bitmap.PixelWidth);
            Assert.Equal(1, bitmap.PixelHeight);
            Assert.True(bitmap.IsFrozen);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private ImagePreview Convert(string path)
        => Assert.IsType<ImagePreview>(_converter.Convert(
            path,
            typeof(ImagePreview),
            null!,
            CultureInfo.InvariantCulture));
}
