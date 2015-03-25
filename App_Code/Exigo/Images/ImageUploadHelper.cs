using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

public class ImageUploadHelper
{
	public ImageUploadHelper()
	{
		
	}

    public string UploadImage(byte[] imageBytes, string caption, bool setAsAvatar)
    {
        // First, save the image as-is to the 'My Photos' gallery.
        var filename = Path.GetRandomFileName().Replace(".", "") + ".png";
        var photoAlbumName = "My Photos";
        if(!string.IsNullOrEmpty(caption))
        {
            caption = caption.Replace(" ", "_");
            caption = Regex.Replace(caption, @"[^a-zA-Z0-9_]", "");
            filename = caption + "~" + filename;
        }

        var context = ExigoApiContext.CreateImagesContext();

        // Save the image and the thumbnail
        context.SaveImage(
            GlobalUtilities.GetCustomerPhotoPath(Identity.Current.CustomerID, photoAlbumName), 
            filename,
            GlobalUtilities.ResizeImage(imageBytes, GlobalSettings.CustomerImages.MaxImageWidth, GlobalSettings.CustomerImages.MaxImageHeight));

        context.SaveImage(
            GlobalUtilities.GetCustomerPhotoPath(Identity.Current.CustomerID, photoAlbumName + "/" + GlobalSettings.CustomerImages.CustomerImagesThumbnailFolderName), 
            filename,
            GlobalUtilities.ResizeImage(imageBytes, GlobalSettings.CustomerImages.MaxThumbnailImageWidth, GlobalSettings.CustomerImages.MaxThumbnailImageHeight));



            // If we are also saving this as our default image, let's save it again, once for each size we use.
        if(setAsAvatar)
        {
            context.SaveImage(
                GlobalUtilities.GetCustomerAvatarPath(Identity.Current.CustomerID),
                GlobalSettings.CustomerImages.TinyAvatarImageName,
                GlobalUtilities.ResizeImage(imageBytes, GlobalSettings.CustomerImages.TinyAvatarWidth, GlobalSettings.CustomerImages.TinyAvatarHeight));

            context.SaveImage(
                GlobalUtilities.GetCustomerAvatarPath(Identity.Current.CustomerID),
                GlobalSettings.CustomerImages.LargeAvatarImageName,
                GlobalUtilities.ResizeImage(imageBytes, GlobalSettings.CustomerImages.LargeAvatarWidth, GlobalSettings.CustomerImages.LargeAvatarHeight));
        }

        return GlobalUtilities.GetCustomerPhotoUrl(Identity.Current.CustomerID, photoAlbumName, filename);
    }
}