using Services.interfaces;
using Services.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services
{
    public class TimelineService : ITimelineService
    {
        public string Date(XmlDocument xmlDoc, string strEvent)
        {
            string strDate = "";
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("event"))
            {
                if (node.Attributes[0].Value.Equals(strEvent))
                {
                    strDate = node.Attributes[1].Value;
                }
            }
            return strDate;
        }

        public void EditDate(XmlDocument xmlDoc, string strEvent, string strDate, string strLabel)
        {
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("event"))
            {
                if (node.Attributes[0].Value.Equals(strEvent))
                {
                    node.SetAttribute("date", strDate);
                    node.SetAttribute("label", strLabel);
                }
            }
        }

        public void EditDate(XmlDocument xmlDoc, string strEvent, string strDate)
        {
            EditDate(xmlDoc, strEvent, strDate, strDate);
        }

        public string EmailBody(XmlDocument xmlDoc)
        {
            StringBuilder sbBody = new StringBuilder();
            sbBody.Append("<table class='table' border='1' cellpadding='5' cellspacing='0' style='font-family: verdana; font-size: 12px; width: 600px;'>");
            foreach (XmlElement node in xmlDoc.GetElementsByTagName("event"))
            {
                sbBody.Append("<tr>");
                sbBody.Append("<td style='font-weight: bold; vertical-align: middle;'>");
                if (node.Attributes[0].Value.StartsWith("Memo"))
                {
                    sbBody.Append(Settings.Get(node.Attributes[0].Value) + " (" + node.Attributes[0].Value + ")");
                    //sbBody.Append(ConfigurationManager.AppSettings[node.Attributes[0].Value] + " (" + node.Attributes[0].Value + ")");
                }
                else
                {
                    sbBody.Append(node.Attributes[0].Value);
                }
                sbBody.Append("</td>");
                sbBody.Append("<td style='vertical-align: middle;'>");
                try
                {
                    if (node.Attributes[0].Value.Equals("Peer Review Meeting"))
                    {
                        sbBody.Append(CheckDate.MonthYear(Convert.ToDateTime(node.Attributes[1].Value)));
                    }
                    else
                    {
                        sbBody.Append(Convert.ToDateTime(node.Attributes[1].Value).ToShortDateString());
                    }
                }
                catch
                {
                    sbBody.Append("nbsp;");
                }
                sbBody.Append("</td>");
                sbBody.Append("</tr>");
            }
            sbBody.Append("</table>");
            return sbBody.ToString();
        }

        public string OutputHtmlTable(XmlDocument xmlDoc)
        {
            throw new NotImplementedException();
        }

        public void Print(XmlDocument xmlDoc, string tblPrint)
        {
            string tr = "<tr class='label'>";
            string tdName = String.Empty;
            string tdDate = String.Empty;

            foreach (XmlElement node in xmlDoc.GetElementsByTagName("event"))
            {

                //TableCell tdName = new TableCell();
                //tdName.CssClass = "label";

                if (node.Attributes[0].Value.StartsWith("Memo"))
                {
                    tdName = "<td class='label'>" + Settings.Get(node.Attributes[0].Value) + " (" + node.Attributes[0].Value + ")" + "</td>";
                    //tdName.Text = Settings.Get(node.Attributes[0].Value) + " (" + node.Attributes[0].Value + ")";
                }
                else
                {
                    tdName = "<td class='label'>" + node.Attributes[0].Value + "</td>";
                    //tdName.Text = node.Attributes[0].Value;
                }
                //tdDate.CssClass = "value";
                try
                {
                    if (node.Attributes[0].Value.Equals("Peer Review Meeting"))
                    {
                        tdDate = "<td class='value'>" + CheckDate.MonthYear(Convert.ToDateTime(node.Attributes[1].Value)) + "</td>";
                        //tdDate.Text = CheckDate.MonthYear(Convert.ToDateTime(node.Attributes[1].Value));
                    }
                    else
                    {
                        //tdDate.Text = Convert.ToDateTime(node.Attributes[1].Value).ToShortDateString();
                        tdDate = "<td class='value'>" + Convert.ToDateTime(node.Attributes[1].Value).ToShortDateString() + "</td>";

                    }
                }
                catch
                {
                    //tdDate.Text = "";
                }

                tr += tdName + tdDate + "</tr>";

                //tr.Cells.Add(tdName);
                //tr.Cells.Add(tdDate);
                //tblPrint.Rows.Add(tr);
            }
        }

    }
}
