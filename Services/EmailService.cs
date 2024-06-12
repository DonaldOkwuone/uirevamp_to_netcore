using Services.interfaces;
using Services.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Services
{
    public class EmailService : IEmailService
    {
        private XmlDocument xmlTimeline;
        private readonly ITimelineService timelineService;

        public EmailService(ITimelineService timelineService)
        {
            this.timelineService = timelineService;
        }
        public string GetEmailBody(string emailTitle, string emailSender)
        {
            StringBuilder sbBody = new StringBuilder();

            sbBody.Append(@"<html>");
            sbBody.Append(@"<head><title>ITG</title></head>");
            sbBody.Append(@"<body style='padding-top: 80px; padding-left: 40px;'>");
            sbBody.Append(@"<table summary='Layout table to output email content.' style='width: 680px; border-width: 0px;' cellpadding='0' cellspacing='0'>");
            sbBody.Append(@"<tr><td style='border-width: thin; border-style: solid; border-color: #cccccc'>");
            sbBody.Append(this.GetEmailHeader());
            sbBody.Append(@"</td></tr>");
            sbBody.Append(@"<tr><td style='border-width: thin; border-style: solid; border-color: #cccccc; border-top-width: 0px; padding: 20px 10px 10px 10px'>");
            sbBody.Append(@"<div style='font-weight: bold;'>INITIATIVE TIMELINE GENERATOR</div>");
            sbBody.Append(@"<div><a href='https://internal.nhlbi.nih.gov/itg/'>https://internal.nhlbi.nih.gov/itg/</a></div>");
            sbBody.Append(@"<p>");
            sbBody.Append(Settings.Get("emailNote"));
            //sbBody.Append(ConfigurationManager.AppSettings["emailNote"]);
            sbBody.Append(@"</p>");

            sbBody.Append(@"<p style='font-weight: bold;'>RFA, PA/RFP Title: ");
            sbBody.Append(emailTitle);
            //sbBody.Append(txtTitle.Text);
            sbBody.Append(@"</p>");

            sbBody.Append(timelineService.OutputHtmlTable(this.xmlTimeline));
            sbBody.Append(@"</td></tr>");
            sbBody.Append(@"<tr><td style='border-width: 0px; text-align: right; color: #999999; font-size: 10px; padding-right: 10px;'>");
            sbBody.Append(@"Sent by ");
            sbBody.Append(emailSender);
            //sbBody.Append(txtName.Text);
            sbBody.Append(@" using ITG Web application");
            sbBody.Append(@"</td></tr></table>");
            sbBody.Append(@"</body></html>");
            return sbBody.ToString();
        }

        public string GetEmailHeader()
        {

            StringBuilder sbBody = new StringBuilder();
            sbBody.Append(@"<table style='width: 670px; border-width: 0px;' cellpadding='0' cellspacing='0'>");
            sbBody.Append(@"<tr><td style='width: 430px; text-align: left;'>");
            sbBody.Append(@"<img src='https://www.nhlbi.nih.gov/sites/default/files/inline-images/NHLBI%20Logo.png' alt='NHLBI logo; National Heart Lung and Blood Institute: People Science Health' width='350' height='63' border='0' />");
            sbBody.Append(@"</td>");
            sbBody.Append(@"<td style='text-align: left; width: 240px; vetical-align: top; font-size: 10px; font-family: verdana;'>");
            sbBody.Append(@"<div style='font-weight: bold;'>QUESTIONS?</div>");
            sbBody.Append(@"<div>Contact Director, Office of Grants Management</div>");
            sbBody.Append(@"<div>Email: <a href='mailto:prengerv@nhlbi.nih.gov'>prengerv@nhlbi.nih.gov</a></div>");
            sbBody.Append(@"<div>Phone: 301-435-0270</div>");
            sbBody.Append(@"</td></tr></table>");
            return sbBody.ToString();
        }

        public void SendEmail(string email, string emailTitle)
        {
           
                MailAddress from = new MailAddress("itg@nhlbi.nih.gov", "ITG (DO NOT REPLY)");
                //MailAddress to = new MailAddress(ConfigurationManager.AppSettings["email"], "NHLBI Chief Review Branch");
                MailAddress to = new MailAddress(Settings.Get("email"), "NHLBI Chief Review Branch");
                MailAddress cc = new MailAddress(email);
                //MailAddress cc = new MailAddress(txtEmail.Text);
                //MailMessage message = new MailMessage(from, to);
                MailMessage message = new MailMessage(from, cc);
                //message.CC.Add(cc);
                message.IsBodyHtml = true;
                message.Subject = "Initiative Timeline: " + emailTitle;
                //message.Body = this.EmailBody();
                message.Body = this.GetEmailBody(emailTitle, emailTitle);
                SmtpClient client = new SmtpClient(Settings.Get("mail_server"));
                client.Send(message);
           
                //Session["msg"] = ex.Message.ToString();
                //Session["url"] = "Default.aspx";
                //Response.Redirect("Message.aspx");
            
        }
    }
}
