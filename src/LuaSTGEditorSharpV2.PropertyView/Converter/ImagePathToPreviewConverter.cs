using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LuaSTGEditorSharpV2.PropertyView.Converter;

public sealed record ImagePreview(ImageSource? Source)
{
    public static ImagePreview Unavailable { get; } = new((ImageSource?)null);

    public bool IsAvailable => Source is not null;
}

[ValueConversion(typeof(string), typeof(ImagePreview))]
public sealed class ImagePathToPreviewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var path = value?.ToString();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return ImagePreview.Unavailable;
        }

        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return new ImagePreview(image);
        }
        catch (Exception exception) when (exception is IOException
                                               or UnauthorizedAccessException
                                               or NotSupportedException
                                               or ArgumentException
                                               or FormatException)
        {
            return ImagePreview.Unavailable;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
