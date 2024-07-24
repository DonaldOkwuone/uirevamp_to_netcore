using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services.interfaces
{
    public interface IEmailService
    {
        void SendEmail(string email, string emailTitle, XmlDocument xmlTimeline);
        string GetEmailBody(string emailTitle, string emailSender);
        string GetEmailHeader();
    }
}
