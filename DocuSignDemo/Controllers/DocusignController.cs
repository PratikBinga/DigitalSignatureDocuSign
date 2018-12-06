using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Document = DocuSign.eSign.Model.Document;
namespace DocusignDemo.Controllers
{
    public class DocusignController : Controller
    {
        MyCredential credential = new MyCredential();
        private string INTEGRATOR_KEY = "b43a7a81-bd0a-4894-9d8d-b5eebff80d66";
        public ActionResult SendDocumentforSign()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendDocumentforSign(DocusignDemo.Models.Recipient recipient, HttpPostedFileBase UploadDocument)
        {
            Models.Recipient recipientModel = new Models.Recipient();
            string directorypath = Server.MapPath("~/App_Data/" + "Files/");
            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
            }
            byte[] data;
            using (Stream inputStream = UploadDocument.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }
            var serverpath = directorypath + recipient.Name.Trim() + ".pdf";
            System.IO.File.WriteAllBytes(serverpath, data);
            docusign(serverpath, recipient.Name, recipient.Email);
            return View();
        }
        public string loginApi(string usr, string pwd)
        {
            usr = "Pratikswvk@gmail.com";
            pwd = "Cns@12345";
            // we set the api client in global config when we configured the client  
            ApiClient apiClient = Configuration.Default.ApiClient;
            string authHeader = "{\"Username\":\"" + usr + "\", \"Password\":\"" + pwd + "\", \"IntegratorKey\":\"" + INTEGRATOR_KEY + "\"}";
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);
            // we will retrieve this from the login() results  
            string accountId = null;
            // the authentication api uses the apiClient (and X-DocuSign-Authentication header) that are set in Configuration object  
            AuthenticationApi authApi = new AuthenticationApi();
            LoginInformation loginInfo = authApi.Login();
            // find the default account for this user  
            foreach (DocuSign.eSign.Model.LoginAccount loginAcct in loginInfo.LoginAccounts)
            {
                if (loginAcct.IsDefault == "true")
                {
                    accountId = loginAcct.AccountId;
                    break;
                }
            }
            if (accountId == null)
            { // if no default found set to first account  
                accountId = loginInfo.LoginAccounts[0].AccountId;
            }
            return accountId;
        }
        public void docusign(string path, string recipientName, string recipientEmail)
        {
            try { 
            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;
            //Verify Account Details  
            string accountId = loginApi(credential.UserName, credential.Password);
            // Read a file from disk to use as a document.  
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Please sign this doc";
            // Add a document to the envelope  
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = Path.GetFileName(path);
            doc.DocumentId = "1";
            envDef.Documents = new List<Document>();
            envDef.Documents.Add(doc);
            // Add a recipient to sign the documeent  
            DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
            signer.Email = recipientEmail;
            signer.Name = recipientName;
            signer.RecipientId = "1";
            envDef.Recipients = new DocuSign.eSign.Model.Recipients();
            envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
            envDef.Recipients.Signers.Add(signer);
            //set envelope status to "sent" to immediately send the signature request  
            envDef.Status = "sent";
            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)  
            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
            // print the JSON response  
            var result = JsonConvert.SerializeObject(envelopeSummary);
                if(result != null)
                {
                    Console.WriteLine("Document sent Successfully!");
                }
                else
                {
                    Console.WriteLine("Something went wrong!");

                }
            }
            catch(Exception ex)
            {
                
            }

           
        }
    }
    public class MyCredential
    {
        public string UserName
        {
            get;
            set;
        } = "Enter UserName";
        public string Password
        {
            get;
            set;
        } = "Enter Password";
    }
}