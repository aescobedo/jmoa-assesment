using Exigo.OData;
using Exigo.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class MessagesService
{
	public MessagesService()
	{

    }

    #region Email Folders
    public List<EmailFolder> GetEmailFolders()
    {
        var context = ExigoApiContext.CreateODataContext();
        return context.EmailFolders
            .Where(c => c.CustomerID == Identity.Current.CustomerID)
            .Select(c => c)
            .ToList();
    }

    public int CreatePersonalEmailFolder(string description)
    {
        // First, verify that we are not creating a duplicate folder name.
        var existingFolders = GetEmailFolders();
        var duplicateFolders = existingFolders.Where(c => c.Name.ToUpper() == description.Trim().ToUpper());
        if(duplicateFolders.Count() > 0) return duplicateFolders.First().MailFolderID;


        // If we got here, create the new folder
        var response = ExigoApiContext.CreateWebServiceContext().CreateMailFolder(new CreateMailFolderRequest
        {
            CustomerID = Identity.Current.CustomerID,
            MailFolderName = description
        });

        
        // Return the new folder ID
        return ExigoApiContext.CreateODataContext().EmailFolders
            .OrderByDescending(c => c.MailFolderID)
            .First()
            .MailFolderID;
    }
    #endregion

    #region Email
    public int GetUnreadMailCount()
    {
        return ExigoApiContext.CreateODataContext().Emails
            .Where(c => c.MailFolderID == (int)MailForderType.Inbox)
            .Where(c => c.MailStatusTypeID == (int)MailStatusType.New)
            .Count();
    }

    public List<Email> GetEmails(int folderID = 1)
    {
        var context = ExigoApiContext.CreateODataContext();
        return context.Emails
            .Where(c => c.CustomerID == Identity.Current.CustomerID)
            .Where(c => c.MailFolderID == folderID)
            .OrderByDescending(c => c.MailDate)
            .Select(c => c)
            .ToList();
    }
    public Email GetEmail(int mailID)
    {
        return ExigoApiContext.CreateODataContext().Emails.Expand("Attachments")
            .Where(c => c.CustomerID == Identity.Current.CustomerID)
            .Where(c => c.MailID == mailID)
            .Select(c => c)
            .FirstOrDefault();
    }

    public void UpdateEmailsStatus(int[] mailIDs, MailStatusType status)
    {
        foreach(var mailID in mailIDs)
        {
            var response = ExigoApiContext.CreateWebServiceContext().UpdateEmailStatus(new UpdateEmailStatusRequest
            {
                CustomerID = Identity.Current.CustomerID,
                MailID = mailID,
                MailStatusType = status
            });
        }
    }
    public void MoveEmails(int[] mailIDs, int mailFolderID)
    {
        foreach(var mailID in mailIDs)
        {
            var response = ExigoApiContext.CreateWebServiceContext().MoveEmail(new MoveEmailRequest
            {
                CustomerID = Identity.Current.CustomerID,
                MailID = mailID,
                ToMailFolderID = mailFolderID
            });
        }
    }
    public void PermanentlyDeleteEmails(int[] mailIDs)
    {
        foreach(var mailID in mailIDs)
        {
            var response = ExigoApiContext.CreateWebServiceContext().DeleteEmail(new DeleteEmailRequest
            {
                CustomerID = Identity.Current.CustomerID,
                MailID = mailID
            });
        }
    }

    public List<int> GetAllEmailIDs(MailStatusType status, int folderID)
    {
        var query = ExigoApiContext.CreateODataContext().Emails
            .Where(c => c.CustomerID == Identity.Current.CustomerID)
            .Where(c => c.MailStatusTypeID == (int)status)
            .Where(c => c.MailFolderID == folderID)
            .Select(c => new 
            {
                c.MailID
            })
            .ToList();

        var list = new List<int>();
        list = query.Select(c => c.MailID).ToList();

        return list;
    }
    public List<int> GetAllEmailIDs(int folderID)
    {
        var query = ExigoApiContext.CreateODataContext().Emails
            .Where(c => c.CustomerID == Identity.Current.CustomerID)
            .Where(c => c.MailFolderID == folderID)
            .Select(c => new 
            {
                c.MailID
            })
            .ToList();

        var list = new List<int>();
        list = query.Select(c => c.MailID).ToList();

        return list;
    }
    #endregion

}