using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;
using System.IO;

namespace Medtrix.MailService
{
    public class EmailService
    {
        public static object SendMail(String recepients, String subject, String body, Attachment attachment)
        {
            String status = "";
            try
            {

                MailMessage message = new MailMessage();
  
                SmtpClient client = new SmtpClient();
                message.From = new MailAddress("support@yogaarogyam.co","Yoga Arogyam");
                message.Subject = subject;
                message.Body = body;
                message.To.Add(recepients);

                message.IsBodyHtml = true;
                if (attachment != null)
                {
                    message.Attachments.Add(attachment);
                }
                client.EnableSsl = false;
                client.UseDefaultCredentials = false;
                client.Port = 25;
                client.Host = "relay-hosting.secureserver.net";
                client.Send(message);


                /* if (String.IsNullOrEmpty(recepients))
                 {
                     return status;
                 }

                 String hostAddress = System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
                 int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]);

                 if ((String.IsNullOrEmpty(hostAddress)) || (-1 == port))
                 {
                     return status;
                 }

                 NetworkCredential smtpCredentials = null;

                 String userName = System.Configuration.ConfigurationManager.AppSettings["SmtpUserName"];
                 String paswword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"];

                 if ((!String.IsNullOrEmpty(userName)) && (!String.IsNullOrEmpty(paswword)))
                 {
                     smtpCredentials = new NetworkCredential(userName, paswword);
                 }

                 SmtpClient mailClient = new SmtpClient(hostAddress, port);

                 if (smtpCredentials != null)
                 {
                     mailClient.Credentials = smtpCredentials;
                 }

                 mailClient.EnableSsl = true;

                 MailMessage msg = new MailMessage(userName, recepients);
                 msg.IsBodyHtml = true;
                 msg.Body = body;
                 msg.Subject = subject;

                 if(attachment != null)
                 {
                     msg.Attachments.Add(attachment);
                 }

                 mailClient.Send(msg);

                 status = true;*/
                status = recepients + subject + body + attachment;
            }
            catch (Exception exp)
            {
                Medtrix.Trace.Logger.Log("Email Error : " + exp.Message);
                status = "false";
                return exp;
            }
            return status;
        }
    }
}
