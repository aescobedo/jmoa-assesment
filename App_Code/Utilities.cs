using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using Exigo.WebService;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Drawing.Drawing2D;
using System.Data.SqlTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class GlobalUtilities
{
    /// <summary>
    /// Sets the value of the string to be the first non-nullable parameter found for the strings provided.
    /// </summary>
    /// <param name="strings"></param>
    /// <returns>The first non-null, non-empty string found.</returns>
    public static string Coalesce(params string[] strings)
    {
        return strings.Where(s => !string.IsNullOrEmpty(s)).FirstOrDefault();
    }

    /// <summary>
    /// Condenses the provided string to the provided max length of characters. If the content is longer than the max length, "..." will be appended to the end.
    /// </summary>
    /// <param name="content">The content to be condensed.</param>
    /// <param name="maxLength">The maximum number of allowable characters.</param>
    /// <returns>The content equal or less than the max length.</returns>
    public static string Condense(string content, int maxLength)
    {

        string contentText = Regex.Replace(content, @"<(.|\n)*?>", string.Empty);
        int length = contentText.Length;
        content = Regex.Match(contentText, @"^.{1," + (maxLength - 1) + @"}\b(?<!\s)").Value;
        if (length > maxLength) content += "...";

        return content;
    }

    /// <summary>
    /// Gets the start date for an autoship with the provided frequency type.
    /// </summary>
    /// <param name="frequency">How often the autoship will run</param>
    /// <returns>The start date for an autoship</returns>
    public static DateTime GetNewAutoOrderStartDate(FrequencyType frequency)
    {
        DateTime autoshipstartDate = new DateTime();
        var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        switch (frequency)
        {
            case FrequencyType.Weekly: autoshipstartDate = now.AddDays(7); break;
            case FrequencyType.BiWeekly: autoshipstartDate = now.AddDays(14); break;
            case FrequencyType.EveryFourWeeks: autoshipstartDate = now.AddDays(28); break;
            case FrequencyType.Monthly: autoshipstartDate = now.AddMonths(1); break;
            case FrequencyType.BiMonthly: autoshipstartDate = now.AddMonths(2); break;
            case FrequencyType.Quarterly: autoshipstartDate = now.AddMonths(3); break;
            case FrequencyType.SemiYearly: autoshipstartDate = now.AddMonths(6); break;
            case FrequencyType.Yearly: autoshipstartDate = now.AddYears(1); break;
            default: autoshipstartDate = now; break;
        }

        // Ensure we are not returning a day of 29, 30 or 31.
       autoshipstartDate = GetNextAvailableAutoOrderStartDate(autoshipstartDate);

        return autoshipstartDate;
    }

    /// <summary>
    /// Gets the next available date for an autoship starting with the provided date.
    /// </summary>
    /// <param name="date">The original start date</param>
    /// <returns>The nearest available start date for an autoship</returns>
    public static DateTime GetNextAvailableAutoOrderStartDate(DateTime date)
    {
        // Ensure we are not returning a day of 29, 30 or 31.
        if (date.Day > 28)
        {
            date = new DateTime(date.AddMonths(1).Year, date.AddMonths(1).Month, 1).Date;
        }
        
        return date;
    }

    /// <summary>
    /// Validates the provided credit card number using a Luhn algorithm.
    /// </summary>
    /// <param name="creditCardNumber">The credit card number to validate.</param>
    /// <returns>The validity of the credit card number. True = valid card, False = invalid card.</returns>
    public static bool ValidateCreditCard(string creditCardNumber)
    {
        const string allowed = "0123456789";
        int i;

        StringBuilder cleanNumber = new StringBuilder();
        for (i = 0; i < creditCardNumber.Length; i++)
        {
            if (allowed.IndexOf(creditCardNumber.Substring(i, 1)) >= 0)
                cleanNumber.Append(creditCardNumber.Substring(i, 1));
        }
        if (cleanNumber.Length < 13 || cleanNumber.Length > 16)
            return false;

        for (i = cleanNumber.Length + 1; i <= 16; i++)
            cleanNumber.Insert(0, "0");

        int multiplier, digit, sum, total = 0;
        string number = cleanNumber.ToString();

        for (i = 1; i <= 16; i++)
        {
            multiplier = 1 + (i % 2);
            digit = int.Parse(number.Substring(i - 1, 1));
            sum = digit * multiplier;
            if (sum > 9)
                sum -= 9;
            total += sum;
        }

        return (total % 10 == 0);
    }

    /// <summary>
    /// Gets the full product image path of the provided product image.
    /// </summary>
    /// <param name="productImage">The name of the product image</param>
    /// <returns>The absolute Uri of the provided product image</returns>
    public static string GetProductImagePath(string productImage)
    {
        if (productImage.Contains("nopic.gif"))
        {
            return "Assets/Images/imgProductImagePlaceholder.jpg";
        }
        else
        {
            if (productImage.Contains("http://") || productImage.Contains("https://"))
            {
                return productImage;
            }
            else
            {
                return GlobalSettings.Shopping.ProductImagePath + productImage;
            }
        }
    }

    /// <summary>
    /// Attempts to parse the provided object as the provided type. If the parsing is unsuccessful, it will reutrn the provided default value.
    /// </summary>
    /// <typeparam name="T">The type to parse your string to.</typeparam>
    /// <param name="s">The object to parse.</param>
    /// <param name="defaultValue">The value that will be returned if parsing is unsuccessful.</param>
    /// <returns></returns>
    public static T TryParse<T>(object s, object defaultValue)
    {
        try
        {
            return (T)Convert.ChangeType(s, typeof(T));
        }
        catch
        {
            return (T)defaultValue;
        }
    }

    /// <summary>
    /// Perform a deep Copy of the object.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T Clone<T>(T source, CloneType cloneType)
    {
        // Clone using serialization
        if(cloneType == CloneType.Serialization)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        // Clone using reflection
        if(cloneType == CloneType.Reflection)
        {
            var type = typeof(T);
            var clone = Activator.CreateInstance<T>();

            foreach(var property in type.GetProperties())
            {
                property.SetValue(clone, property.GetValue(source, null), null);
            }

            return clone;
        }

        return source;
    }

    /// <summary>
    /// Determines if the supplied year is a leap year.
    /// </summary>
    /// <param name="year">The year to check</param>
    /// <returns>True if the provided year is a leap year.</returns>
    public static bool IsLeapYear(int year) 
    { 
        if (year % 4 != 0) 
        { 
            return false; 
        } 
        if (year % 100 == 0) 
        { 
            return (year % 400 == 0); 
        } 
        return true; 
    }

    /// <summary>
    /// Returns the client's IP address, or (localhost) if there isn't one.
    /// </summary>
    /// <returns>The cleint's IP address</returns>
    public static string GetClientIP()
    {
        var ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        if(ip.Equals("::1", StringComparison.InvariantCultureIgnoreCase)) ip = "127.0.0.1";

        return ip;
    }

    /// <summary>
    /// Formats an error message (usually from an Exception) for use in Javascript by removing all line breaks and converting double-quotes to single-quotes.
    /// </summary>
    /// <param name="errormessage">The error message (usually from an Exception).</param>
    /// <returns>A formatted error message for use in Javascript.</returns>
    public static string FormatErrorMessageForJavascript(string errormessage) 
    {
        errormessage = errormessage.Replace("'", "");
        errormessage = errormessage.Replace(System.Environment.NewLine, " ");
        errormessage = errormessage.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        return errormessage;
    }
    
    /// <summary>
    /// Gets the formattted replicated site Url given the provided web alias.
    /// </summary>
    /// <param name="webAlias">The web alias to use.</param>
    /// <returns>The formatted website Url.</returns>
    public static string GetReplicatedSiteUrl(string webAlias)
    {
        return string.Format(GlobalSettings.Websites.ReplicatedSite, webAlias);
    }

    /// <summary>
    /// Gets the formattted signup Url given the provided web alias.
    /// </summary>
    /// <param name="webAlias">The web alias to use.</param>
    /// <returns>The formatted website Url.</returns>
    public static string GetSignupUrl(string webAlias)
    {
        return string.Format(GlobalSettings.Websites.Signup, webAlias);
    }

    /// <summary>
    /// Gets a formatted display name based on the company's rules. The default setting is that if a company name is found display the company name; otherwise, display the first and last name.
    /// </summary>
    /// <returns>The formatted display name.</returns>
    public static string GetDisplayName()
    {
        return GlobalUtilities.Coalesce(Identity.Current.Company, Identity.Current.FirstName + " " + Identity.Current.LastName);
    }

    /// <summary>
    /// Gets a formatted display name based on the company's rules. The default setting is that if a company name is found display the company name; otherwise, display the first and last name.
    /// </summary>
    /// <returns>The formatted display name.</returns>
    public static string GetDisplayName(string companyName, string firstName, string lastName)
    {
        return GlobalUtilities.Coalesce(companyName, firstName + " " + lastName);
    }

    /// <summary>
    /// Returns the supplied DateTime, or the earliest possible DateTime that SQL allows.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime GetSqlDateTime(DateTime? dateTime)
     {
         return dateTime ?? new DateTime(1753, 1, 1);
     }

    /// <summary>
    /// Fetches the current period ID from SQL.
    /// </summary>
    /// <returns>The current period ID</returns>
    public static int GetCurrentPeriodID()
    {
        var periodID = 0;

        var command = new SqlHelper();
        periodID = Convert.ToInt32(command.GetField(@"
                SELECT TOP 1 
                    p.PeriodID
                FROM
	                Periods p
                WHERE
	                p.PeriodTypeID = {0}
                    AND p.StartDate <= {1}
	                AND p.EndDate >= {1}
            ",  (int)PeriodTypes.Default,
                new SqlDateTime(DateTime.Now.Date)));

        return periodID;
    }

    /// <summary>
    /// Fetches the default period type ID. Used in other static classes.
    /// </summary>
    /// <returns>The current period ID</returns>
    public static int GetDefaultPeriodTypeID()
    {
        return PeriodTypes.Default;
    }

    public static bool IsCustomerInEnrollerTree(int customerID)
    {
        var helper = new SqlHelper();
        var result = helper.GetField("SELECT CustomerID FROM EnrollerTree WHERE EnrollerID = {0} AND CustomerID = {1}", Identity.Current.CustomerID, customerID);

        return (result != null);
    }

    #region Customer Photo Utilities
    public enum CustomerAvatarImageType
    {
        Tiny,
        Large
    }

    /// <summary>
    /// Returns the Url of a customer's tiny avatar image using the AvatarImageHandler. Returns a default image if one is not found.
    /// </summary>
    /// <param name="customerID">The customer ID of the requested avatar image.</param>
    /// <returns>The Url of the tiny avatar image, or a default image Url.</returns>
    public static string GetCustomerTinyAvatarUrl(int customerID) 
    {
        return string.Format("avatars/{0}/{1}", customerID, GlobalSettings.CustomerImages.TinyAvatarImageName);
    }

    /// <summary>
    /// Returns the Url of a customer's large avatar image using the AvatarImageHandler. Returns a default image if one is not found.
    /// </summary>
    /// <param name="customerID">The customer ID of the requested avatar image.</param>
    /// <returns>The Url of the large avatar image, or a default image Url.</returns>
    public static string GetCustomerLargeAvatarUrl(int customerID) 
    {
        return string.Format("avatars/{0}/{1}", customerID, GlobalSettings.CustomerImages.LargeAvatarImageName);
    }

    /// <summary>
    /// Returns the absolute Url of a customer's requested avatar image.
    /// </summary>
    /// <param name="customerID">The customer ID of the requested avatar image.</param>
    /// <param name="avatarImageType">The type of avatar image to return.</param>
    /// <returns>The absolute Url of the requested avatar image.</returns>
    public static string GetCustomerTinyAvatarUrl(int customerID, CustomerAvatarImageType avatarImageType) 
    {
        var avatarFileName = "";
        var avatarFileExtension = ".png";
        switch(avatarImageType)
        {
            case CustomerAvatarImageType.Tiny: avatarFileName = "tiny"; break;
            case CustomerAvatarImageType.Large: avatarFileName = "large"; break;
        }

        return string.Format("{0}/{1}/{2}/{3}/{4}{5}",
            GlobalSettings.CustomerImages.BasePath,
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesAvatarFolderName,
            avatarFileName,
            avatarFileExtension);
    }

    /// <summary>
    /// Returns the relative path of a customer's requested avatar image.
    /// </summary>
    /// <param name="customerID">The customer ID of the requested avatar image.</param>
    /// <param name="avatarImageType">The type of avatar image to return.</param>
    /// <returns>The relative path of the requested avatar image.</returns>
    public static string GetCustomerAvatarPath(int customerID) 
    {

        return string.Format("{0}/{1}/{2}",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesAvatarFolderName);
    }

    /// <summary>
    /// Returns the relative path of all a customer's photos given the customer's ID.
    /// </summary>
    /// <param name="customerID">The customer ID of the photo's owner.</param>
    /// <returns>The relative path of all of the customer's photos.</returns>
    public static string GetCustomerPhotoPath(int customerID) 
    {
        return string.Format("{0}/{1}/{2}/",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesPhotoFolderName);
    }

    /// <summary>
    /// Returns the relative path of all a customer's photos given the customer's ID and album name.
    /// </summary>
    /// <param name="customerID">The customer ID of the photo's owner.</param>
    /// <param name="albumName">The name of the album the photo is found in.</param>
    /// <returns>The relative path of all of the customer's photos in the provided album.</returns>
    public static string GetCustomerPhotoPath(int customerID, string albumName) 
    {
        return string.Format("{0}/{1}/{2}/{3}",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesPhotoFolderName,
            albumName);
    }

    /// <summary>
    /// Returns the relative path of a photo given the customer's ID, the album name and the file name.
    /// </summary>
    /// <param name="customerID">The customer ID of the photo's owner.</param>
    /// <param name="albumName">The name of the album the photo is found in.</param>
    /// <param name="filename">The file name and extension of the photo.</param>
    /// <returns>The relative path of the photo.</returns>
    public static string GetCustomerPhotoPath(int customerID, string albumName, string filename) 
    {
        return string.Format("{0}/{1}/{2}/{3}/{4}",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesPhotoFolderName,
            albumName,
            filename);
    }

    /// <summary>
    /// Returns the absolute Url of a photo given the customer's ID, the album name and the file name.
    /// </summary>
    /// <param name="customerID">The customer ID of the photo's owner.</param>
    /// <param name="albumName">The name of the album the photo is found in.</param>
    /// <param name="filename">The file name and extension of the photo.</param>
    /// <returns>The absolute Url of the photo.</returns>
    public static string GetCustomerPhotoUrl(int customerID, string albumName, string filename) 
    {
        return string.Format("{0}/{1}/{2}/{3}/{4}/{5}",
            GlobalSettings.CustomerImages.BasePath,
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesPhotoFolderName,
            albumName,
            filename);
    }

    /// <summary>
    /// Returns the relative path of all a customer's videos given the customer's ID.
    /// </summary>
    /// <param name="customerID">The customer ID of the video's owner.</param>
    /// <returns>The relative path of all of the customer's videos.</returns>
    public static string GetCustomerVideoPath(int customerID) 
    {
        return string.Format("{0}/{1}/{2}/",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesVideoFolderName);
    }

    /// <summary>
    /// Returns the relative path of all a customer's videos given the customer's ID and album name.
    /// </summary>
    /// <param name="customerID">The customer ID of the video's owner.</param>
    /// <param name="albumName">The name of the album the video is found in.</param>
    /// <returns>The relative path of all of the customer's videos in the provided album.</returns>
    public static string GetCustomerVideoPath(int customerID, string albumName) 
    {
        return string.Format("{0}/{1}/{2}/{3}",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesVideoFolderName,
            albumName);
    }

    /// <summary>
    /// Returns the relative path of a photo given the customer's ID, the album name and the file name.
    /// </summary>
    /// <param name="customerID">The customer ID of the video's owner.</param>
    /// <param name="albumName">The name of the album the video is found in.</param>
    /// <param name="filename">The file name and extension of the video.</param>
    /// <returns>The relative path of the video.</returns>
    public static string GetCustomerVideoPath(int customerID, string albumName, string filename) 
    {
        return string.Format("{0}/{1}/{2}/{3}/{4}",
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesVideoFolderName,
            albumName,
            filename);
    }

    /// <summary>
    /// Returns the absolute Url of a video given the customer's ID, the album name and the file name.
    /// </summary>
    /// <param name="customerID">The customer ID of the video's owner.</param>
    /// <param name="albumName">The name of the album the video is found in.</param>
    /// <param name="filename">The file name and extension of the video.</param>
    /// <returns>The absolute Url of the video.</returns>
    public static string GetCustomerVideoUrl(int customerID, string albumName, string filename) 
    {
        return string.Format("{0}/{1}{2}/{3}/{4}/{5}",
            GlobalSettings.CustomerImages.BasePath,
            GlobalSettings.CustomerImages.CustomerImagesFolderName,
            customerID,
            GlobalSettings.CustomerImages.CustomerImagesVideoFolderName,
            albumName,
            filename);
    }
    #endregion

    #region Photo Manipulation
    public static byte[] ResizeImage(byte[] original, int maxWidth, int maxHeight)
    {
        using (var ms = new MemoryStream(original))
        using (var bmp = new Bitmap(ms))
        {
            ImageFormat format = bmp.RawFormat;
            decimal ratio;
            int newWidth = 0;
            int newHeight = 0;

            if (bmp.Width > maxWidth || bmp.Height > maxHeight)
            {
                if (bmp.Width > bmp.Height)
                {
                    ratio = (decimal)maxWidth / bmp.Width;
                    newWidth = maxWidth;
                    decimal lnTemp = bmp.Height * ratio;
                    newHeight = (int)lnTemp;
                }
                else
                {
                    ratio = (decimal)maxHeight / bmp.Height;
                    newHeight = maxHeight;
                    decimal lnTemp = bmp.Width * ratio;
                    newWidth = (int)lnTemp;
                }
            }

            if (newWidth == 0) newWidth = bmp.Width;
            if (newHeight == 0) newHeight = bmp.Height;

            using (var bmpOut = new Bitmap(newWidth, newHeight))
            using (var msOut = new MemoryStream())
            {
                Graphics g = Graphics.FromImage(bmpOut);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                g.DrawImage(bmp, 0, 0, newWidth, newHeight);

                bmpOut.Save(msOut, ImageFormat.Jpeg);

                return (msOut.ToArray());
            }
        }
    }
    public static byte[] Crop(string imageUrl, int width, int height, int X, int Y)
    {
        // Download the image
        var webClient = new WebClient();
        byte[] imageBytes = webClient.DownloadData(imageUrl);

        return Crop(imageBytes, width, height, X, Y);
    }
    public static byte[] Crop(byte[] imageBytes, int width, int height, int X, int Y)
    {
        // Convert the bytes into an Image
        MemoryStream ms = new MemoryStream(imageBytes);
        System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);

        using (System.Drawing.Image OriginalImage = returnImage)
        {
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height))
            {
                bmp.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);
                using (System.Drawing.Graphics Graphic = System.Drawing.Graphics.FromImage(bmp))
                {
                    Graphic.SmoothingMode = SmoothingMode.AntiAlias;
                    Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    Graphic.DrawImage(OriginalImage, new System.Drawing.Rectangle(0, 0, width, height), X, Y, width, height, System.Drawing.GraphicsUnit.Pixel);
                    MemoryStream nms = new MemoryStream();
                    bmp.Save(nms, OriginalImage.RawFormat);
                    return nms.GetBuffer();
                }
            }
        }
    }
    #endregion
}

public enum CloneType
{
    Serialization,
    Reflection
}