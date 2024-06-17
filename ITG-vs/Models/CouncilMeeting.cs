namespace ITG_vs.Models
{
    public class CouncilMeeting
    {
        public string lblCouncilMeeting { get; set; }
        public string lblSummaryStatementsComplete { get; set; }
        public string lblPeerReviewMeeting { get; set; }
        public string lblApplicationReceiptDate { get; set; }
        public string lblLettersOfIntent { get; set; }
        public string lblNIHGuidePublication { get; set; }
        public string NumberOfDaysPub { get; set; }
   
        public bool d3Timeline { get; set; }
        public DateTime dtDate { get; set; }
        public DateTime dtTemp1 { get; set; }
        public DateTime dtTemp2 { get; set; }
        public DateTime dtTemp3 { get; set; }
        public DateTime dtTemp4 { get; set; }
        public string xmlDoc { get; set; }
        public string lblCouncilRound { get; set; }
        public string lblIdeaForum { get; set; }
        public string lblBeeMeeting { get; internal set; }
        public string lblNHLBICouncilConceptClearance { get; internal set; }
        public string lblROSubmitsToNIHGuide { get; internal set; }
        public string lblMemo3 { get; internal set; }
        public string lblSubmitDERADraft { get; internal set; }
        public string lblConsultWithReviewBranch { get; internal set; }
        public string lblMemo2 { get; internal set; }
        public DateTime dtDate2 { get; set; }
        public DateTime roundDate { get; internal set; }
        public DateTime roundDate2 { get; internal set; }
        public DateTime roundDate3 { get; internal set; }
        public DateTime dtTemp5 { get; internal set; }
        public DateTime dtTemp6 { get; internal set; }
        public DateTime dtTemp7 { get; internal set; }
        public DateTime dtTemp8 { get; internal set; }
        public DateTime dtTemp9 { get; internal set; }

        ///RFP
        public string lblRFPReleaseRFP { get;  set; }
        public string lblRFPFBON { get;  set; }
        public string lblRFPAPA { get;  set; }
        public string lblCWOSR { get;  set; }
        public string lblDRNI { get;  set; }
        public string lblNHLBICCA { get;  set; }
        public string lblRFPCouncilMeeting { get; internal set; }
        public string lblRFPTERD { get; internal set; }
        public string lblRFPFirstReivew { get; internal set; }
        public string lblRFPPRD { get; internal set; }
        public string lblRFPLettersofIntent { get; internal set; }
        public string NumberOfDaysRFP { get; internal set; }
        public DateTime dtTemp10 { get; internal set; }
    }
}
