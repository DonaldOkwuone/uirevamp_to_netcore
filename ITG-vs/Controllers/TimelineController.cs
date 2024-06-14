using ITG_vs.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.interfaces;
using Services.utils;
using System.IO;
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
                //strPath = Server.MapPath("~/App_Data/RFA/Initiatives.xml");
            }
            else
            {
                strPath = Path.Combine(env.WebRootPath, "xml", "RFP", "Initiatives.xml");

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
            string strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "Timeline.xml");
            XmlDocument xmlTimeline = new XmlDocument();
            string xmlDoc = String.Empty;
            xmlTimeline.Load(strPath);

            CouncilMeeting councilMeeting = new CouncilMeeting();

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
                        ChangeDatesByNumberOfDays(Convert.ToInt32(councilMeeting.NumberOfDaysPub));
                        councilMeeting.xmlDoc = BuildInitParamsLocal(fetchCouncilRequest, xmlTimeline);
                        //ChangeDatesByNumberOfDays(Convert.ToInt32(NumberOfDaysPub.Text));
                        //Initialize timeline data, make timeline visible
                        //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                        //d3Timeline.Visible = true;                        //DateTime dtTemp = dtDate;

                    }
                    else if (fetchCouncilRequest.ContractType.Equals("RFP"))
                    {
                        //DateTime dtTemp = dtDate;
                        //lblRFPCouncilMeeting.Text = dtDate.ToShortDateString();
                        //CheckDateForColorChange(lblRFPCouncilMeeting, dtDate);
                        //Timeline.EditDate(xmlTimelineRFP, "Secondary Review", dtDate.ToShortDateString());

                        //dtTemp = CheckDate.SubtractDays(dtDate, 15);
                        //dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        //lblRFPTERD.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPTERD, dtTemp);
                        //Timeline.EditDate(xmlTimelineRFP, "Technical Evaluation Reports Due", dtTemp.ToShortDateString());

                        //dtTemp = CheckDate.SubtractMonth(dtDate, 1);
                        //dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        //dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        //// Output Month, year per user request
                        //lblRFPFirstReivew.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPFirstReivew, dtTemp);
                        //Timeline.EditDate(xmlTimelineRFP, "Primary Technical Review", dtTemp.ToShortDateString());

                        //dtTemp = CheckDate.SubtractMonth(dtDate, 4);
                        //dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        //dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        //lblRFPPRD.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPPRD, dtTemp);
                        //Timeline.EditDate(xmlTimelineRFP, "Proposal Receipt Date", dtTemp.ToShortDateString());

                        //dtTemp = CheckDate.SubtractMonth(dtDate, 5);
                        //dtTemp = CheckDate.SubtractDays(dtTemp, 14);
                        //dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        //lblRFPLettersofIntent.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPLettersofIntent, dtTemp);
                        //Timeline.EditDate(xmlTimelineRFP, "Letters of Intent", dtTemp.ToShortDateString());

                        //dtTemp = CheckDate.SubtractMonth(dtDate, 5);
                        //dtTemp = CheckDate.SubtractDays(dtTemp, 15);
                        //dtTemp = CheckDate.AdjustRFPDate(dtTemp);
                        //lblRFPReleaseRFP.Text = dtTemp.ToShortDateString();
                        //CheckDateForColorChange(lblRFPReleaseRFP, dtTemp);
                        //Timeline.EditDate(xmlTimelineRFP, "Release RFP", dtTemp.ToShortDateString());

                        //NumberOfDaysRFP.Text = "30";
                        //ChangeDatesByNumberOfDays(Convert.ToInt32(NumberOfDaysRFP.Text));
                        ////Initialize timeline data, make timeline visible
                        //ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
                        //d3Timeline.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
            return Ok(councilMeeting);
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
            XmlDocument xmlTimeline = new XmlDocument();

            if (contractType == "RFA")
            {
                string strPath = Path.Combine(env.WebRootPath, "xml", "RFA", "Timeline.xml");
                xmlTimeline.Load(strPath);
            }
            else
            {
                string strPath = Path.Combine(env.WebRootPath, "xml", "RPA", "Timeline.xml");
                xmlTimeline.Load(strPath);
            }

            return xmlTimeline;
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
                        if (DropDownList2.Count == 1)
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

        private void ChangeDatesByNumberOfDays(int intDaysAdded)
        {
            //try
            //{
            //    if (ddlContractType.SelectedItem.Value.Equals("RFA"))
            //    {
            //        DateTime dtDate = Convert.ToDateTime(Timeline.Date(xmlTimeline, "Application Due Date"));
            //        dtDate = dtDate.AddDays(-intDaysAdded);

            //        dtDate = CheckDate.AdjustRFADate(dtDate);

            //        lblNIHGuidePublication.Text = dtDate.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "NIH Guide Publication", dtDate.ToShortDateString());
            //        CheckDateForColorChange(lblNIHGuidePublication, dtDate);

            //        DateTime dtTemp = CheckDate.SubtractMonth(dtDate, 4);
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);

            //        if (lblPubError != null)
            //            lblPubError.Visible = false;

            //        //Initialize timeline data, make timeline visible
            //        ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
            //        d3Timeline.Visible = true;

            //        Tuple<string, List<DateTime>> roundData = GetConceptClearanceDates(dtTemp);
            //        List<DateTime> roundDates = roundData.Item2;
            //        string ideaForumStartText =
            //            GetPartOfMonth(roundDates[0]) + roundDates[0].ToString("MMMM") + " " + roundDates[0].Year;
            //        string ideaForumEndText =
            //            GetPartOfMonth(roundDates[1]) + roundDates[1].ToString("MMMM") + " " + roundDates[1].Year;
            //        string ideaForumText = ideaForumStartText + " - " + ideaForumEndText;
            //        string beeMeetingText = roundDates[2].ToString("MMMM") + " " + roundDates[2].Year;
            //        string conceptClearanceText = roundDates[3].ToString("MMMM") + " " + roundDates[3].Year;
            //        lblCouncilRound.Text = " - " + roundData.Item1;

            //        lblIdeaForum.Text = ideaForumText;
            //        Timeline.EditDate(xmlTimeline, "Idea Forum", roundDates[1].ToShortDateString(), ideaForumText);
            //        CheckDateForColorChange(lblIdeaForum, roundDates[1]);

            //        lblBeeMeeting.Text = beeMeetingText;
            //        Timeline.EditDate(xmlTimeline, "BEE Meeting", roundDates[2].ToShortDateString(), beeMeetingText);
            //        CheckDateForColorChange(lblBeeMeeting, roundDates[2]);

            //        lblNHLBICouncilConceptClearance.Text = conceptClearanceText;
            //        Timeline.EditDate(xmlTimeline, "NHLBI Council Concept Clearance", roundDates[3].ToShortDateString(), conceptClearanceText);
            //        CheckDateForColorChange(lblNHLBICouncilConceptClearance, roundDates[3]);

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 1);
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);
            //        lblROSubmitsToNIHGuide.Text = dtTemp.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "OFDR Submits FOA Draft to NIH Guide", dtTemp.ToShortDateString());
            //        CheckDateForColorChange(lblROSubmitsToNIHGuide, dtTemp);

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 1);
            //        dtTemp = dtTemp.AddDays(-2);// need to ask Pablo about the number of days added
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);
            //        lblMemo3.Text = dtTemp.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "DERA Director’s Approval to Publish the FOA in the NIH Guide", dtTemp.ToShortDateString());
            //        CheckDateForColorChange(lblMemo3, dtTemp);

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 2);
            //        dtTemp = dtTemp.AddDays(-15);
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);
            //        lblSubmitDERADraft.Text = dtTemp.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "Submit FOA Draft for DERA Review", dtTemp.ToShortDateString());
            //        CheckDateForColorChange(lblSubmitDERADraft, dtTemp);

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 3);
            //        dtTemp = dtTemp.AddDays(2);// need to ask Pablo about the number of days added
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);
            //        lblConsultWithReviewBranch.Text = dtTemp.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "Consult with Office of Grants Management", dtTemp.ToShortDateString());
            //        CheckDateForColorChange(lblConsultWithReviewBranch, dtTemp);

            //        //subtracting 10 businessdays from Consult with review branch.
            //        dtTemp = CheckDate.AddBusinessDays(dtTemp, -10);
            //        dtTemp = CheckDate.AdjustRFADate(dtTemp);
            //        lblMemo2.Text = dtTemp.ToShortDateString();
            //        Timeline.EditDate(xmlTimeline, "Schedule FOA Development Meeting", dtTemp.ToShortDateString());
            //        CheckDateForColorChange(lblMemo2, dtTemp);
            //    }
            //    else if (ddlContractType.SelectedItem.Value.Equals("RFP"))
            //    {
            //        DateTime dtDate = Convert.ToDateTime(Timeline.Date(xmlTimelineRFP, "Letters of Intent"));
            //        dtDate = CheckDate.SubtractDays(dtDate, 1);
            //        dtDate = dtDate.AddDays(-intDaysAdded);
            //        dtDate = CheckDate.AdjustRFPDate(dtDate);

            //        lblRFPReleaseRFP.Text = dtDate.ToShortDateString();
            //        CheckDateForColorChange(lblRFPReleaseRFP, dtDate);
            //        Timeline.EditDate(xmlTimelineRFP, "Release RFP", dtDate.ToShortDateString());

            //        DateTime dtTemp = CheckDate.SubtractDays(dtDate, 15);
            //        dtTemp = CheckDate.AdjustRFPDate(dtTemp);

            //        if (lblRFPError != null)
            //            lblRFPError.Visible = false;

            //        //Initialize timeline data, make timeline visible
            //        ClientScript.RegisterStartupScript(GetType(), "hwa", "initTimeline();", true);
            //        d3Timeline.Visible = true;

            //        lblRFPFBON.Text = dtTemp.ToShortDateString();
            //        CheckDateForColorChange(lblRFPFBON, dtTemp);
            //        Timeline.EditDate(xmlTimelineRFP, "FedBizOpps Notice", dtTemp.ToShortDateString());

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 1);
            //        dtTemp = CheckDate.SubtractDays(dtTemp, 15);
            //        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
            //        lblRFPAPA.Text = dtTemp.ToShortDateString();
            //        CheckDateForColorChange(lblRFPAPA, dtTemp);
            //        Timeline.EditDate(xmlTimelineRFP, "Acquisition Plan Approval", dtTemp.ToShortDateString());

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 2);
            //        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
            //        lblCWOSR.Text = dtTemp.ToShortDateString();
            //        CheckDateForColorChange(lblCWOSR, dtTemp);
            //        Timeline.EditDate(xmlTimelineRFP, "Consult with Director, Office of Grants Management", dtTemp.ToShortDateString());

            //        dtTemp = CheckDate.SubtractMonth(dtDate, 3);
            //        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
            //        lblDRNI.Text = dtTemp.ToShortDateString();
            //        CheckDateForColorChange(lblDRNI, dtTemp);
            //        timelineService.EditDate(xmlTimelineRFP, "Schedule CIDP Meeting", dtTemp.ToShortDateString());

            //        //Subtracting 5 businessdays from dtTemp.
            //        dtTemp = CheckDate.SubtractMonth(dtDate, 3);
            //        dtTemp = CheckDate.SubtractDays(dtTemp, 15);
            //        dtTemp = CheckDate.AdjustRFPDate(dtTemp);
            //        lblNHLBICCA.Text = dtTemp.ToShortDateString();
            //        CheckDateForColorChange(lblNHLBICCA, dtTemp);
            //        Timeline.EditDate(xmlTimelineRFP, "NHLBI Concept Approval", dtTemp.ToShortDateString());
            //    }
            //}
            //catch
            //{
            //}
        }
    }
}
