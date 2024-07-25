using ITG_vs.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.interfaces;
using Services.utils;
using System.Text;
using System.Xml;

namespace ITG_vs.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TimelineController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly ITimelineService timelineService;
        private XmlDocument xmlTimeline { get; set; }
        private XmlDocument xmlTimelineRFP { get; set; }
        private CouncilMeeting councilMeeting { get; set; }

        public TimelineController(IWebHostEnvironment env, ITimelineService timelineService)
        {
            this.env = env;
            this.timelineService = timelineService;
        }
        [Route("fillcontracttype")]
        [HttpGet]

        public IActionResult FillContractType()
        {
            //string strPath = Server.MapPath("~/App_Data/contractType.xml");
            var strPath = Path.Combine(env.WebRootPath, "xml", "contractType.xml");

            XmlTextReader contractType = new XmlTextReader(strPath);
            contractType.Read();
            var contractTyeDropdown = new List<object>();

            while (contractType.Read())
            {

                if (contractType.Name.Equals("contract"))
                {
                    contractType.MoveToFirstAttribute();
                    string contractTypeText = contractType.Value;
                    contractType.MoveToNextAttribute();
                    string contractTypeValue = contractType.Value;
                    contractTyeDropdown.Add(new { value = contractTypeValue, text = contractTypeText });
                    //contractTyeDropdown += "<option value='" + contractTypeText + "'>" + contractTypeValue + "</option>";
                    // ddlContractType.Items.Add(new ListItem(contractTypeText, contractTypeValue));
                }
            }
            contractType.Close();
            return Ok(contractTyeDropdown);
        }

        [Route("fillfiscalyears/{selectedContractType}")]
        [HttpGet]
        public IActionResult FillFiscalYears(string selectedContractType)
        {
            string strPath = String.Empty;
            var fiscalYearDropDown = new List<YearOption>();
            //fiscalYearDropDown.Add(new YearOption { Text = "Please select a year", Value = "" });

            if (selectedContractType.Equals("RFA"))
            {
                strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "Initiatives.xml");
                HttpContext.Session.SetString("contractType", "RFA");

                //strPath = Server.MapPath("~/App_Data/RFA/Initiatives.xml");
            }
            else
            {
                strPath = Path.Combine(env.WebRootPath, "xml", "RFP", "Initiatives.xml");
                HttpContext.Session.SetString("contractType", "RFP");
                //strPath = Server.MapPath("~/App_Data/RFP/Initiatives.xml");
            }
            try
            {
                XmlTextReader initiatiaves = new XmlTextReader(strPath);
                initiatiaves.Read();

                while (initiatiaves.Read())
                {
                    if (initiatiaves.Name.Equals("fiscalyear"))
                    {
                        initiatiaves.MoveToFirstAttribute();

                        if (selectedContractType.Equals("RFA"))
                        {
                            if (CheckDate.FisicalYearAvailable(initiatiaves.Value, strPath))
                            {
                                fiscalYearDropDown.Add(new YearOption { Value = initiatiaves.Value, Text = initiatiaves.Value });
                            }
                        }
                        else
                        {
                            try
                            {
                                if (DateTime.Now.Year < Convert.ToInt32(initiatiaves.Value))
                                {
                                    fiscalYearDropDown.Add(new YearOption { Value = initiatiaves.Value, Text = initiatiaves.Value });

                                    //fiscalYearDropDown.Items.Add(new ListItem(initiatiaves.Value, initiatiaves.Value));
                                }
                            }
                            catch (Exception ex)
                            {
                                //
                            }
                        }
                    }
                }
                initiatiaves.Close();

                //Adding future years to RFA or RFP.
                if (selectedContractType.Equals("RFA") || selectedContractType.Equals("RFP"))
                {
                    int[] RFAFutureYears = getRFAFutureYears();
                    foreach (int year in RFAFutureYears)
                    {
                        bool yearPresent = false;

                        foreach (var li in fiscalYearDropDown)
                        {
                            if (year.ToString() == li.Text)
                            {
                                yearPresent = true;
                                break;
                            }
                        }
                        if (!yearPresent)
                            fiscalYearDropDown.Add(new YearOption { Text = year.ToString(), Value = year.ToString() });
                    }
                }
            }
            catch
            {
                fiscalYearDropDown.Add(new YearOption { Text = "No data", Value = "" });
            }
            return Ok(fiscalYearDropDown);
        }

        [Route("filtercouncil")]
        [HttpGet]
        public IActionResult FilterCouncil([FromQuery] FetchCouncilRequest fetchCouncilRequest)
        {
            string strFiscalYear = fetchCouncilRequest.DropDownList1;
            string strMeetingDate = fetchCouncilRequest.DropDownList2;
            //string strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "Timeline.xml");
            //xmlTimeline = new XmlDocument();
            ////string xmlDoc = String.Empty;
            //xmlTimeline.Load(strPath);
            xmlTimeline = GetXmlDocument(fetchCouncilRequest.ContractType);
            xmlTimelineRFP = GetXmlDocument(fetchCouncilRequest.ContractType);

            councilMeeting = new CouncilMeeting();

            if (!string.IsNullOrEmpty(strFiscalYear) && !string.IsNullOrEmpty(strMeetingDate))
            {
                try
                {
                    DateTime dtDate = Convert.ToDateTime(strMeetingDate);

                    if (fetchCouncilRequest.ContractType.Equals("RFA"))
                    {
                        DateTime dtTemp = dtDate;
                        councilMeeting.lblCouncilMeeting = dtDate.ToShortDateString();
                        //lblCouncilMeeting.Text = dtDate.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "Council Meeting", dtDate.ToShortDateString());
                        //CheckDateForColorChange(lblCouncilMeeting, dtDate);

                        dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                        dtTemp = CheckDate.AdjustRFADate(dtTemp);
                        //lblSummaryStatementsComplete.Text = dtTemp.ToShortDateString();
                        councilMeeting.lblSummaryStatementsComplete = dtTemp.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "Summary Statements Complete", dtTemp.ToShortDateString());
                        //CheckDateForColorChange(lblSummaryStatementsComplete, dtDate);
                        councilMeeting.dtDate = dtDate;

                        dtTemp = CheckDate.SubtractMonth(dtDate, 3);
                        dtTemp = CheckDate.AdjustRFADate(dtTemp);
                        // Output Month, year per user request
                        councilMeeting.lblPeerReviewMeeting = dtTemp.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "Peer Review Meeting", dtTemp.ToShortDateString());
                        //CheckDateForColorChange(lblPeerReviewMeeting, dtTemp);
                        councilMeeting.dtTemp1 = dtTemp;

                        dtTemp = CheckDate.SubtractMonth(dtDate, 8);
                        dtTemp = CheckDate.AdjustRFADate(dtTemp);
                        councilMeeting.lblApplicationReceiptDate = dtTemp.ToShortDateString();
                        //lblApplicationReceiptDate.Text = dtTemp.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "Application Due Date", dtTemp.ToShortDateString());
                        //CheckDateForColorChange(lblApplicationReceiptDate, dtTemp);
                        councilMeeting.dtTemp2 = dtTemp;

                        dtTemp = CheckDate.SubtractMonth(dtDate, 9);
                        dtTemp = CheckDate.AdjustRFADate(dtTemp);
                        councilMeeting.lblLettersOfIntent = dtTemp.ToShortDateString();
                        //lblLettersOfIntent.Text = dtTemp.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "Letters of Intent", dtTemp.ToShortDateString());
                        //CheckDateForColorChange(lblLettersOfIntent, dtTemp);
                        councilMeeting.dtTemp3 = dtTemp;

                        dtTemp = CheckDate.SubtractMonth(dtDate, 10);
                        dtTemp = CheckDate.AdjustRFADate(dtTemp);
                        councilMeeting.lblNIHGuidePublication = dtTemp.ToShortDateString();
                        //lblNIHGuidePublication.Text = dtTemp.ToShortDateString();
                        timelineService.EditDate(xmlTimeline, "NIH Guide Publication", dtTemp.ToShortDateString());
                        //CheckDateForColorChange(lblNIHGuidePublication, dtTemp);
                        councilMeeting.dtTemp4 = dtTemp;

                        councilMeeting.NumberOfDaysPub = "60";
                        //NumberOfDaysPub.Text = "60";
                        ChangeDatesByNumberOfDays(Convert.ToInt32(councilMeeting.NumberOfDaysPub), fetchCouncilRequest);
                        councilMeeting.xmlDoc = BuildInitParamsLocal(fetchCouncilRequest, xmlTimeline);
                        HttpContext.Session.SetString("xmlTimeline", JsonConvert.SerializeObject(xmlTimeline));

                        //ChangeDatesByNumberOfDays(Convert.ToInt32(NumberOfDaysPub.Text));
                        //Initialize timeline data, make timeline visible
                        //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                        //d3Timeline.Visible = true;                        //DateTime dtTemp = dtDate;

                    }
                    else if (fetchCouncilRequest.ContractType.Equals("RFP"))
                    {
                        DateTime dtTemp = dtDate;
                        councilMeeting.lblRFPCouncilMeeting = dtDate.ToShortDateString();
                        //CheckDateForColorChange(lblRFPCouncilMeeting, dtDate);
                        councilMeeting.dtDate = dtDate;
                        timelineService.EditDate(xmlTimelineRFP, "Secondary Review", dtDate.ToShortDateString());

                        dtTemp = CheckDate.SubtractDays(dtDate, 15);
                        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        councilMeeting.lblRFPTERD = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPTERD, dtTemp);
                        councilMeeting.dtTemp1 = dtTemp;
                        timelineService.EditDate(xmlTimelineRFP, "Technical Evaluation Reports Due", dtTemp.ToShortDateString());

                        dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                        dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        // Output Month, year per user request
                        councilMeeting.lblRFPFirstReivew = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPFirstReivew, dtTemp);
                        councilMeeting.dtTemp2 = dtTemp;
                        timelineService.EditDate(xmlTimelineRFP, "Primary Technical Review", dtTemp.ToShortDateString());

                        dtTemp = CheckDate.SubtractMonth(dtDate, 4);
                        dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        councilMeeting.lblRFPPRD = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPPRD, dtTemp);
                        councilMeeting.dtTemp3 = dtTemp;
                        timelineService.EditDate(xmlTimelineRFP, "Proposal Receipt Date", dtTemp.ToShortDateString());

                        dtTemp = CheckDate.SubtractMonth(dtDate, 5);
                        dtTemp = CheckDate.SubtractDays(dtTemp, 14);
                        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        councilMeeting.lblRFPLettersofIntent = dtTemp.ToShortDateString();
                        //lblRFPLettersofIntent.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPLettersofIntent, dtTemp);
                        councilMeeting.dtTemp4 = dtTemp;
                        timelineService.EditDate(xmlTimelineRFP, "Letters of Intent", dtTemp.ToShortDateString());

                        dtTemp = CheckDate.SubtractMonth(dtDate, 5);
                        dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        councilMeeting.lblRFPReleaseRFP = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPReleaseRFP, dtTemp);
                        councilMeeting.dtTemp5 = dtTemp;
                        timelineService.EditDate(xmlTimelineRFP, "Release RFP", dtTemp.ToShortDateString());

                        councilMeeting.NumberOfDaysRFP = "30";
                        ChangeDatesByNumberOfDays(Convert.ToInt32(councilMeeting.NumberOfDaysRFP), fetchCouncilRequest);
                        //Initialize timeline data, make timeline visible
                        //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                        //d3Timeline.Visible = true;
                        councilMeeting.xmlDoc = BuildInitParamsLocal(fetchCouncilRequest, xmlTimelineRFP);
                        HttpContext.Session.SetString("xmlTimelineRFP", JsonConvert.SerializeObject(xmlTimelineRFP));

                    }
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
            return Ok(councilMeeting);
        }

        [Route("handlepublicationchangeofdate/{publicationDays}")]
        [HttpPost]
        public IActionResult HandlePublicationChangeofDate([FromBody] FetchCouncilRequest fetchCouncilRequest, string publicationDays)
        {
            string days = string.Empty;
            FilterCouncil(fetchCouncilRequest);

            //xmlTimeline =new XmlDocument().LoadXml( xmlDoc);
            //xmlTimelineRFP = GetXmlDocument(fetchCouncilRequest.ContractType);
            //xmlTimelineRFP = GetXmlDocument(fetchCouncilRequest.ContractType);
            if (fetchCouncilRequest.DropDownList2.Length != 0)
            {
                int result;
                if (string.IsNullOrEmpty(publicationDays) || !int.TryParse(publicationDays, out result)
                    || result < 60)
                {
                    publicationDays = "60";
                }
                else if (result > 120)
                {
                    publicationDays = "120";
                }
                ChangeDatesByNumberOfDays(Convert.ToInt32(publicationDays), fetchCouncilRequest);
            }
            string xml = BuildInitParamsLocal(fetchCouncilRequest, xmlTimeline);
            return Ok(new { xmlDoc = xml, publicationDays = publicationDays });
        }
        private string BuildInitParamsLocal(FetchCouncilRequest fetchCouncilRequest, XmlDocument xmlDocument)
        {
            string strFiscalYear = fetchCouncilRequest.DropDownList1;
            string strMeetingDate = fetchCouncilRequest.DropDownList2;

            if (strFiscalYear.Length == 0 || strFiscalYear.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                if (fetchCouncilRequest.ContractType.Equals("RFA"))
                {

                    foreach (XmlElement element in xmlDocument.GetElementsByTagName("event"))
                    {
                        if (element.Attributes[1].Value != "")
                        {
                            sb.Append(element.Attributes[0].Value + "=" + element.Attributes[1].Value
                                + "=" + element.Attributes[2].Value + ",");
                        }
                    }
                }
                else if (fetchCouncilRequest.ContractType.Equals("RFP"))
                {
                    foreach (XmlElement element in xmlDocument.GetElementsByTagName("event"))
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
        private XmlDocument GetXmlDocument(string contractType = "RFA")
        {
            XmlDocument xmlDocument = new XmlDocument();
            if (contractType == "RFA")
            {
                string strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "Timeline.xml");
                xmlDocument.Load(strPath);
            }
            else
            {
                string strPath = Path.Combine(env.WebRootPath, "xml", "RFP", "Timeline.xml");
                xmlDocument.Load(strPath);
            }

            return xmlDocument;
        }
        [Route("fillmeetingdates")]
        [HttpGet]
        public IActionResult FillMeetingDates([FromQuery] FetchCouncilRequest fetchCouncilRequest)
        {
            string strYear = fetchCouncilRequest.DropDownList1;

            var DropDownList2 = new List<YearOption>();
            //DropDownList2.Add(new YearOption { Text="Please select a date", Value=""});
            try
            {
                XmlTextReader initiatiaves = null;

                if (fetchCouncilRequest.ContractType.Equals("RFA"))
                {


                    string path = Path.Combine(env.WebRootPath, "xml", "RFA", "Initiatives.xml"); ;
                    initiatiaves = new XmlTextReader(path);
                    //initiatiaves = new XmlTextReader(Server.MapPath("~/App_Data/RFA/Initiatives.xml"));
                }
                else if (fetchCouncilRequest.ContractType.Equals("RFP"))
                {
                    string path = Path.Combine(env.WebRootPath, "xml", "RFP", "Initiatives.xml"); ;

                    initiatiaves = new XmlTextReader(path);
                    //initiatiaves = new XmlTextReader(Server.MapPath("~/App_Data/RFP/Initiatives.xml"));
                }
                if (initiatiaves != null)
                {
                    initiatiaves.Read();
                    int count = 1;

                    while (initiatiaves.Read())
                    {
                        if (initiatiaves.Name.Equals("fiscalyear"))
                        {
                            initiatiaves.MoveToFirstAttribute();
                            if (initiatiaves.Value == strYear)
                            {
                                while (initiatiaves.Read())
                                {
                                    if (initiatiaves.Name.Equals("fiscalyear"))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (initiatiaves.Name.Equals("meetingdate"))
                                        {
                                            initiatiaves.MoveToFirstAttribute();

                                            if (fetchCouncilRequest.ContractType.Equals("RFA"))
                                            {
                                                if (CheckDate.CouncilDateAvailableRFA(initiatiaves.Value))
                                                {
                                                    //ListItem liDate = new ListItem();
                                                    //liDate.Text = initiatiaves.Value;
                                                    initiatiaves.MoveToNextAttribute();
                                                    //liDate.Value = initiatiaves.Value;
                                                    DropDownList2.Add(new YearOption { Text = initiatiaves.Value, Value = initiatiaves.Value });
                                                }
                                            }
                                            else
                                            {
                                                //ListItem liDate = new ListItem();
                                                //liDate.Text = initiatiaves.Value;

                                                initiatiaves.MoveToNextAttribute();
                                                //liDate.Value = initiatiaves.Value;
                                                //DropDownList2.Items.Add(liDate);
                                                DropDownList2.Add(new YearOption { Text = initiatiaves.Value, Value = initiatiaves.Value });

                                            }
                                            count++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    initiatiaves.Close();
                    //Adding future Council meetingdates for RFA if there were no meeting dates in XML file.
                    if (fetchCouncilRequest.ContractType.Equals("RFA"))
                    {
                        if (fetchCouncilRequest.DropDownList2Count == 1)
                        {
                            string strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "defaultCouncilDates.xml");
                            try
                            {
                                XmlTextReader councilDates = new XmlTextReader(strPath);
                                councilDates.Read();

                                while (councilDates.Read())
                                {
                                    if (councilDates.Name.Equals("council"))
                                    {
                                        councilDates.MoveToFirstAttribute();

                                        string councilDateMonth = councilDates.Value;
                                        councilDates.MoveToNextAttribute();
                                        string coucilDate = councilDates.Value;
                                        string tempStrYear = strYear;

                                        if (councilDateMonth.Equals("October"))
                                        {
                                            if (!string.IsNullOrEmpty(strYear))
                                                tempStrYear = (Convert.ToInt32(strYear) - 1).ToString();
                                        }

                                        if (CheckDate.CouncilDateAvailableRFA(coucilDate + tempStrYear))
                                        {
                                            //DropDownList2.Add(new ListItem(councilDateMonth + ", " + tempStrYear, coucilDate + tempStrYear));
                                            DropDownList2.Add(new YearOption { Text = councilDateMonth + ", " + tempStrYear, Value = coucilDate + tempStrYear });

                                        }
                                    }
                                }
                                councilDates.Close();
                            }
                            catch
                            {
                                DropDownList2.Add(new YearOption { Text = "No data", Value = "" });
                            }
                        }
                    }
                    //Adding secondary meetingdates for RFP if there were no meeting dates in XML file.
                    if (fetchCouncilRequest.ContractType.Equals("RFP"))
                    {
                        if (fetchCouncilRequest.DropDownList2Count == 1)
                        {
                            int rfpCount = 1;
                            //string strPath = Server.MapPath("~/App_Data/RFP/defaultSecondaryReivewDates.xml");
                            string strPath = Path.Combine(env.WebRootPath, "xml", "RFP", "defaultSecondaryReivewDates.xml");
                            try
                            {
                                XmlTextReader councilDates = new XmlTextReader(strPath);
                                councilDates.Read();

                                while (councilDates.Read())
                                {
                                    if (councilDates.Name.Equals("secondaryReview"))
                                    {
                                        councilDates.MoveToFirstAttribute();
                                        string councilDateMonth = councilDates.Value;
                                        councilDates.MoveToNextAttribute();
                                        string coucilDate = councilDates.Value;

                                        YearOption liDate = new YearOption();

                                        if (Convert.ToDateTime(coucilDate + strYear).Month == 3)
                                        {
                                            liDate.Text = councilDateMonth + strYear;
                                            liDate.Value = coucilDate + strYear;
                                        }
                                        else
                                        {
                                            liDate.Text = councilDateMonth + (Convert.ToInt32(strYear) - 1).ToString();
                                            liDate.Value = coucilDate + (Convert.ToInt32(strYear) - 1).ToString();
                                        }


                                        DropDownList2.Add(liDate);
                                    }
                                }
                                councilDates.Close();
                            }
                            catch (Exception ex)
                            {
                                DropDownList2.Add(new YearOption { Text = "No data", Value = "" });
                            }
                        }
                    }
                }
            }
            catch
            {
                //
            }
            return Ok(DropDownList2);
        }
        //get RFA future years.
        private int[] getRFAFutureYears()
        {
            int[] yearArray = new int[5];

            try
            {
                int currentYear = DateTime.Today.Year;
                for (int i = 0; i < 5; i++)
                {
                    yearArray[i] = currentYear++;
                }
                return yearArray;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ChangeDatesByNumberOfDays(int intDaysAdded, FetchCouncilRequest fetchCouncilRequest)
        {
            try
            {
                if (fetchCouncilRequest.ContractType.Equals("RFA"))
                {
                    DateTime dtDate = Convert.ToDateTime(timelineService.Date(xmlTimeline, "Application Due Date"));
                    dtDate = dtDate.AddDays(-intDaysAdded);

                    dtDate = CheckDate.AdjustRFADate(dtDate);

                    councilMeeting.lblNIHGuidePublication = dtDate.ToShortDateString();
                    //lblNIHGuidePublication.Text = dtDate.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "NIH Guide Publication", dtDate.ToShortDateString());
                    councilMeeting.dtDate2 = dtDate;
                    //CheckDateForColorChange(lblNIHGuidePublication, dtDate);

                    DateTime dtTemp = CheckDate.SubtractMonth(dtDate, 4);
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);

                    //if (lblPubError != null)
                    //    lblPubError.Visible = false;

                    //Initialize timeline data, make timeline visible
                    //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                    //d3Timeline.Visible = true;

                    Tuple<string, List<DateTime>> roundData = timelineService.GetConceptClearanceDates(dtTemp);
                    List<DateTime> roundDates = roundData.Item2;
                    string ideaForumStartText =
                        timelineService.GetPartOfMonth(roundDates[0]) + roundDates[0].ToString("MMMM") + " " + roundDates[0].Year;
                    string ideaForumEndText =
                        timelineService.GetPartOfMonth(roundDates[1]) + roundDates[1].ToString("MMMM") + " " + roundDates[1].Year;
                    string ideaForumText = ideaForumStartText + " - " + ideaForumEndText;
                    string beeMeetingText = roundDates[2].ToString("MMMM") + " " + roundDates[2].Year;
                    string conceptClearanceText = roundDates[3].ToString("MMMM") + " " + roundDates[3].Year;
                    councilMeeting.lblCouncilRound = " - " + roundData.Item1;

                    councilMeeting.lblIdeaForum = ideaForumText;
                    timelineService.EditDate(xmlTimeline, "Idea Forum", roundDates[1].ToShortDateString(), ideaForumText);
                    councilMeeting.roundDate = roundDates[1];
                    //CheckDateForColorChange(lblIdeaForum, roundDates[1]);

                    councilMeeting.lblBeeMeeting = beeMeetingText;
                    timelineService.EditDate(xmlTimeline, "BEE Meeting", roundDates[2].ToShortDateString(), beeMeetingText);
                    councilMeeting.roundDate2 = roundDates[2];
                    //CheckDateForColorChange(lblBeeMeeting, roundDates[2]);

                    councilMeeting.lblNHLBICouncilConceptClearance = conceptClearanceText;
                    timelineService.EditDate(xmlTimeline, "NHLBI Council Concept Clearance", roundDates[3].ToShortDateString(), conceptClearanceText);
                    councilMeeting.roundDate3 = roundDates[3];
                    //CheckDateForColorChange(lblNHLBICouncilConceptClearance, roundDates[3]);

                    dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);
                    councilMeeting.lblROSubmitsToNIHGuide = dtTemp.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "OFDR Submits FOA Draft to NIH Guide", dtTemp.ToShortDateString());
                    councilMeeting.dtTemp5 = dtTemp;
                    //CheckDateForColorChange(lblROSubmitsToNIHGuide, dtTemp);

                    dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                    dtTemp = dtTemp.AddDays(-2);// need to ask Pablo about the number of days added
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);
                    councilMeeting.lblMemo3 = dtTemp.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "DERA Director’s Approval to Publish the FOA in the NIH Guide", dtTemp.ToShortDateString());
                    councilMeeting.dtTemp6 = dtTemp;
                    //CheckDateForColorChange(lblMemo3, dtTemp);

                    dtTemp = CheckDate.SubtractMonth(dtDate, 2);
                    dtTemp = dtTemp.AddDays(-15);
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);
                    councilMeeting.lblSubmitDERADraft = dtTemp.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "Submit FOA Draft for DERA Review", dtTemp.ToShortDateString());
                    councilMeeting.dtTemp7 = dtTemp;
                    //CheckDateForColorChange(lblSubmitDERADraft, dtTemp);

                    dtTemp = CheckDate.SubtractMonth(dtDate, 3);
                    dtTemp = dtTemp.AddDays(2);// need to ask Pablo about the number of days added
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);
                    councilMeeting.lblConsultWithReviewBranch = dtTemp.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "Consult with Office of Grants Management", dtTemp.ToShortDateString());
                    councilMeeting.dtTemp8 = dtTemp;
                    //CheckDateForColorChange(lblConsultWithReviewBranch, dtTemp);

                    //subtracting 10 businessdays from Consult with review branch.
                    dtTemp = CheckDate.AddBusinessDays(dtTemp, -10);
                    dtTemp = CheckDate.AdjustRFADate(dtTemp);
                    councilMeeting.lblMemo2 = dtTemp.ToShortDateString();
                    timelineService.EditDate(xmlTimeline, "Schedule FOA Development Meeting", dtTemp.ToShortDateString());
                    councilMeeting.dtTemp9 = dtTemp;
                    //CheckDateForColorChange(lblMemo2, dtTemp);
                }
                else if (fetchCouncilRequest.ContractType.Equals("RFP"))
                {
                    DateTime dtDate = Convert.ToDateTime(timelineService.Date(xmlTimelineRFP, "Letters of Intent"));
                    dtDate = CheckDate.SubtractDays(dtDate, 1);
                    dtDate = dtDate.AddDays(-intDaysAdded);
                    dtDate = CheckDate.AdjustRFPDate(dtDate);

                    //lblRFPReleaseRFP.Text = dtDate.ToShortDateString();
                    councilMeeting.lblRFPReleaseRFP = dtDate.ToShortDateString();
                    councilMeeting.dtDate2 = dtDate;
                    //CheckDateForColorChange(lblRFPReleaseRFP, dtDate);
                    timelineService.EditDate(xmlTimelineRFP, "Release RFP", dtDate.ToShortDateString());

                    DateTime dtTemp = CheckDate.SubtractDays(dtDate, 15);
                    dtTemp = CheckDate.AdjustRFPDate(dtTemp);

                    //if (lblRFPError != null)
                    //    lblRFPError.Visible = false;

                    //Initialize timeline data, make timeline visible
                    //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                    //d3Timeline.Visible = true;

                    councilMeeting.lblRFPFBON = dtTemp.ToShortDateString();
                    //lblRFPFBON.Text = dtTemp.ToShortDateString();
                    //CheckDateForColorChange(lblRFPFBON, dtTemp);
                    councilMeeting.dtTemp6 = dtTemp;
                    timelineService.EditDate(xmlTimelineRFP, "FedBizOpps Notice", dtTemp.ToShortDateString());

                    dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                    dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                    dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                    councilMeeting.lblRFPAPA = dtTemp.ToShortDateString();
                    //lblRFPAPA.Text = dtTemp.ToShortDateString();
                    //CheckDateForColorChange(lblRFPAPA, dtTemp);
                    councilMeeting.dtTemp7 = dtTemp;
                    timelineService.EditDate(xmlTimelineRFP, "Acquisition Plan Approval", dtTemp.ToShortDateString());

                    dtTemp = CheckDate.SubtractMonth(dtDate, 2);
                    dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                    councilMeeting.lblCWOSR = dtTemp.ToShortDateString();
                    councilMeeting.dtTemp8 = dtTemp;
                    //lblCWOSR.Text = dtTemp.ToShortDateString();
                    //CheckDateForColorChange(lblCWOSR, dtTemp);
                    timelineService.EditDate(xmlTimelineRFP, "Consult with Director, Office of Grants Management", dtTemp.ToShortDateString());

                    dtTemp = CheckDate.SubtractMonth(dtDate, 3);
                    dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                    //lblDRNI.Text = dtTemp.ToShortDateString();
                    councilMeeting.lblDRNI = dtTemp.ToShortDateString();
                    //CheckDateForColorChange(lblDRNI, dtTemp);
                    councilMeeting.dtTemp9 = dtTemp;
                    timelineService.EditDate(xmlTimelineRFP, "Schedule CIDP Meeting", dtTemp.ToShortDateString());

                    //Subtracting 5 businessdays from dtTemp.
                    dtTemp = CheckDate.SubtractMonth(dtDate, 3);
                    dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                    dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                    councilMeeting.lblNHLBICCA = dtTemp.ToShortDateString();
                    //lblNHLBICCA.Text = dtTemp.ToShortDateString();
                    //CheckDateForColorChange(lblNHLBICCA, dtTemp);
                    councilMeeting.dtTemp10 = dtTemp;
                    timelineService.EditDate(xmlTimelineRFP, "NHLBI Concept Approval", dtTemp.ToShortDateString());
                }
            }
            catch
            {
            }
        }
    }
}
