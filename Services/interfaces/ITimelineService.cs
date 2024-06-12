using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services.interfaces
{
    public interface ITimelineService
    {
        string Date(XmlDocument xmlDoc, string strEvent);
        void EditDate(XmlDocument xmlDoc, string strEvent, string strDate, string strLabel);
        void EditDate(XmlDocument xmlDoc, string strEvent, string strDate);
        string EmailBody(XmlDocument xmlDoc);
        string OutputHtmlTable(XmlDocument xmlDoc);
        void Print(XmlDocument xmlDoc, string tblPrint);


    }
}
