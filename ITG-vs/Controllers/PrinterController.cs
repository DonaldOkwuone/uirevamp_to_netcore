using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.interfaces;
using System.Text;
using System.Xml;

namespace ITG_vs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrinterController : ControllerBase
    {
        private readonly ITimelineService timelineService;
        private XmlDocument? xmlTimeline { get; set; }
        public PrinterController(ITimelineService timelineService)
        {
            this.timelineService = timelineService;
        }
        [Route("init-printer")]
        [HttpPost]
        public IActionResult initPrinter([FromBody] string xmlDoc)
        {


            var contractType = HttpContext.Session.GetString("contractType");

            if (contractType == null)
            {
                return BadRequest("Timeline not found!");
            }
            string xml = null;
            if (contractType == "RFA")
            {
                xml = HttpContext.Session.GetString("xmlTimeline") ?? HttpContext.Session.GetString("xmlTimelineRFP");

            }
            else if (contractType == "RFP")
            {
                xml = HttpContext.Session.GetString("xmlTimelineRFP") ?? HttpContext.Session.GetString("xmlTimelineRFP");

            }

            if (xml == null)
            {
                return BadRequest("Timeline not found!");
            }
            xmlTimeline = JsonConvert.DeserializeObject<XmlDocument>(xml);
            var tableText = this.PrintBody();
            string xmlDocText = BuildInitParams();
            return Ok(new { result = tableText, xml = xmlDocText });
        }

        private string PrintBody()
        {
            StringBuilder sbBody = new StringBuilder();
            sbBody.Append(@"<table style='width: 680px; border-width: 0px;' cellpadding='0' cellspacing='0'>");
            sbBody.Append(@"<thead><tr><th>");//<td style='border-width: thin; border-style: solid; border-color: #cccccc'>");
            sbBody.Append(this.PrintHeader());
            sbBody.Append(@"</th></tr></thead>");
            //sbBody.Append(@"</td></tr>");
            sbBody.Append(@"<tr><td style='border-width: thin; border-style: solid; border-color: #cccccc; border-top-width: 0px; padding: 20px 10px 10px 10px'>");
            sbBody.Append(@"<div style='font-weight: bold;'>INITIATIVE TIMELINE GENERATOR</div>");
            sbBody.Append(@"<div><a href='https://internal.nhlbi.nih.gov/itg/'>https://internal.nhlbi.nih.gov/itg/</a></div>");
            sbBody.Append(@"<p>The generated timeline for your initiative is for discussion and planning purposes only. Please contact the <a href = 'mailto:nhlbi_referraloffice@mail.nih.gov'> Office of FOA Development and Referral(OFDR) </a> for more details. It will be helpful to include the timeline when contacting OFDR with inquiries.</p>");
            sbBody.Append(timelineService.OutputHtmlTable(this.xmlTimeline));
            sbBody.Append(@"</td></tr></table>");
            return sbBody.ToString();
        }
        private string PrintHeader()
        {
            StringBuilder sbBody = new StringBuilder();
            sbBody.Append(@"<table style='width: 670px; border-width: 0px;' cellpadding='0' cellspacing='0'>");
            sbBody.Append(@"<tr><td style='width: 430px; text-align: left;'>");
            sbBody.Append(@"<img src='https://www.nhlbi.nih.gov/sites/default/files/inline-images/NHLBI%20Logo.png' alt='NHLBI logo; National Heart Lung and Blood Institute: People Science Health'/>");
            sbBody.Append(@"</td>");
            sbBody.Append(@"<td style='text-align: left; width: 240px; vetical-align: top; font-size: 14px; font-family: verdana;'>");
            sbBody.Append(@"<div style='font-weight: bold;'>QUESTIONS?</div>");
            sbBody.Append(@"<div>Contact the Office of FOA Development and Referral (OFDR)</div>");
            sbBody.Append(@"<div>Email:&nbsp;<a href='mailto:nhlbi_referraloffice@mail.nih.gov'>nhlbi_referraloffice@mail.nih.gov</a></div>");
            sbBody.Append(@"<div>Phone: 301-435-0270</div>");
            sbBody.Append(@"</td></tr></table>");
            return sbBody.ToString();
        }

        private string BuildInitParams()
        {
            StringBuilder sb = new StringBuilder();
            if (xmlTimeline != null)
            {
                foreach (XmlElement element in xmlTimeline.GetElementsByTagName("event"))
                {
                    if (element.Attributes[1].Value != "")
                    {
                        sb.Append(element.Attributes[0].Value.Replace(",", "") + "=" + element.Attributes[1].Value
                            + "=" + element.Attributes[2].Value + ",");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
