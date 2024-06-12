using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;

namespace ITG_vs.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWebHostEnvironment env;

        public IndexModel(ILogger<IndexModel> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this.env = env;
        }
        public XmlDocument xmlTimeline { get; set; }
        public XmlDocument xmlTimelineRFP { get; set; }

        public void OnGet()
        {

            // Checking to see session expired.        
            //xmlTimeline = (XmlDocument)Session["xmldoc"];
            //xmlTimelineRFP = (XmlDocument)Session["xmldocRFP"];

            if (xmlTimeline == null)
            {
                xmlTimeline = new XmlDocument();
                var filePath = Path.Combine(env.WebRootPath, "xml", "RFA", "Timeline.xml");
                xmlTimeline.Load(filePath);

                //Session["xmldoc"] = xmlTimeline;
            }
            if (xmlTimelineRFP == null)
            {
                xmlTimelineRFP = new XmlDocument();
                var filePath = Path.Combine(env.WebRootPath, "xml", "RFP", "Timeline.xml");

                xmlTimelineRFP.Load(filePath);
                //Session["xmldocRFP"] = xmlTimelineRFP;
            }
            //FillContractType();

            //if (!ispostback)
            //{
            //    FillContractType();
            //    resetvalues();
            //}
            //if (dropdownlist1.selectedvalue == "")
            //{
            //    dropdownlist2.enabled = false;
            //}

            //if (ddlContractType.SelectedValue.Equals("RFA"))
            //{
            //    pnlBox2.Visible = true;
            //    lblStep3.Text = "Select Anticipated Council";
            //    Session["contractType"] = "RFA";

            //    pnlBox3.Visible = true;
            //    pnlBox5.Visible = false;

            //    if (!string.IsNullOrEmpty(DropDownList1.SelectedValue)
            //        && !string.IsNullOrEmpty(DropDownList2.SelectedValue))
            //    {
            //        PrintRFA.Visible = true;
            //        PrintRFAInactive.Visible = false;
            //    }
            //    else
            //    {
            //        PrintRFA.Visible = false;
            //        PrintRFAInactive.Visible = true;
            //    }
            //}
            //else if (ddlContractType.SelectedValue.Equals("RFP"))
            //{
            //    pnlBox2.Visible = true;
            //    lblStep3.Text = "Select Anticipated Quarter for Secondary Review";

            //    Session["contractType"] = "RFP";

            //    pnlBox3.Visible = false;
            //    pnlBox5.Visible = true;

            //    if (!string.IsNullOrEmpty(DropDownList1.SelectedValue)
            //        && !string.IsNullOrEmpty(DropDownList2.SelectedValue))
            //    {
            //        PrintRFP.Visible = true;
            //        PrintRFPInactive.Visible = false;
            //    }
            //    else
            //    {
            //        PrintRFP.Visible = false;
            //        PrintRFPInactive.Visible = true;
            //    }
            //}
            //else
            //{
            //    pnlBox2.Visible = false;
            //    pnlBox3.Visible = false;
            //    pnlBox5.Visible = false;
            //    Session["contractType"] = "";
            //}
        }
        public void OnPost()
        {

        }
        private void FillContractType()
        {
            //string strPath = Server.MapPath("~/App_Data/contractType.xml");
            var strPath = Path.Combine(env.WebRootPath, "xml", "contractType.xml");

            try
            {
                XmlTextReader contractType = new XmlTextReader(strPath);
                contractType.Read();

                while (contractType.Read())
                {
                    if (contractType.Name.Equals("contract"))
                    {
                        contractType.MoveToFirstAttribute();
                        string contractTypeText = contractType.Value;
                        contractType.MoveToNextAttribute();
                        string contractTypeValue = contractType.Value;

                       // ddlContractType.Items.Add(new ListItem(contractTypeText, contractTypeValue));
                    }
                }
                contractType.Close();
            }
            catch
            {
                //DropDownList1.Items.Add(new ListItem("No data", ""));
            }
        }
    }
}
