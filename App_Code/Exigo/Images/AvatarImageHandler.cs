using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

public class AvatarImageHandler : IHttpHandler
{
	public AvatarImageHandler()
	{
	}

    public bool IsReusable
    {
        get { return true; }
    }

    public void ProcessRequest(HttpContext context)
    {
        var url = context.Request.Url.AbsolutePath;

        int customerID = Convert.ToInt32(url.Split('/')[url.Split('/').Length - 2]);
        var size = url.Split('/')[url.Split('/').Length - 1].Split('.')[0];
        var defaultAvatarAsBase64 = string.Empty;
        var avatarType = GlobalUtilities.CustomerAvatarImageType.Large;
        
        switch(size)
        {
            case "tiny": 
                defaultAvatarAsBase64 = GlobalSettings.CustomerImages.DefaultTinyAvatarAsBase64; 
                avatarType = GlobalUtilities.CustomerAvatarImageType.Tiny;
                break;

            case "large": 
            default: 
            defaultAvatarAsBase64 = GlobalSettings.CustomerImages.DefaultLargeAvatarAsBase64; 
            avatarType = GlobalUtilities.CustomerAvatarImageType.Large;
            break;
        }

        if(customerID == 0)
        {
            return;
        }


        context.Response.Clear();
        context.Response.ContentType = "image/png";
       

        try
        {
            var imageUrl = GlobalUtilities.GetCustomerTinyAvatarUrl(customerID, avatarType);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if(response.StatusCode != HttpStatusCode.OK) throw new Exception("GOTO_CATCH"); 
            var responseStream = response.GetResponseStream();
            
            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();
            context.Response.AddHeader("Content-Length", bytes.Length.ToString());

            context.Response.OutputStream.Write(bytes, 0, bytes.Length);         
   
        }
        catch
        {
            var bytes = Convert.FromBase64String(defaultAvatarAsBase64);
            context.Response.AddHeader("Content-Length", bytes.Length.ToString());
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
        
        context.Response.AddHeader("Content-Disposition","inline;filename=avatar-" + customerID + ".png");
        context.Response.Cache.SetExpires(DateTime.Now.AddDays(1));
        context.Response.Flush();
        context.Response.End();
    }
}