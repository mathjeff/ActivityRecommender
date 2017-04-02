using System.Windows.Media.Imaging;
using System;


public class ImageLoader
{
    public static BitmapImage loadImage(System.String fileName)
    {
        String testString = fileName;
        if (System.IO.File.Exists(testString))
        {
            fileName = testString;
        }
        else
        {
            fileName = "../../../Images/" + fileName;
        }
        if (!System.IO.File.Exists(fileName))
        {
            return null;
        }
        else
        {
            BitmapImage image = new BitmapImage();
            //image.BeginInit();
            image.UriSource = new Uri(fileName, UriKind.Relative);
            //image.CacheOption = BitmapCacheOption.OnLoad;
            //image.EndInit();
            return image;
        }
    }
}