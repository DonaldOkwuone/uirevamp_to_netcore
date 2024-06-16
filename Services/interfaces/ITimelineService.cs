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
        Tuple<string, List<DateTime>> GetConceptClearanceDates(DateTime dtDate);
        string GetPartOfMonth(DateTime dtDate);
        List<DateTime> GetRound1Dates(int year);
        List<DateTime> GetRound2Dates(int year);
        string OutputHtmlTable(XmlDocument xmlDoc);
        void Print(XmlDocument xmlDoc, string tblPrint);


    }
}
