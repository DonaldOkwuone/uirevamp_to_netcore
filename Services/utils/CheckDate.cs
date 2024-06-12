using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Services.utils
{
    public static class CheckDate
    {
        public static bool FisicalYearAvailable(string strYear, string strPath)
        {
            bool blnYes = false;
            try
            {
                XmlReader initiatiaves =  XmlReader.Create(strPath);
                //XmlTextReader initiatiaves = new XmlTextReader(strPath);
                initiatiaves.Read();

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
                                        initiatiaves.MoveToAttribute(1);
                                        try
                                        {
                                            DateTime dtDate = Convert.ToDateTime(initiatiaves.Value);
                                            blnYes = CouncilDateAvailableRFA(dtDate);
                                        }
                                        catch
                                        {
                                            //do nothing
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                initiatiaves.Close();
            }
            catch
            {
                //
            }
            return blnYes;
        }
        public static bool CouncilDateAvailableOld(DateTime dtCouncilDate)
        {
            bool blnYes = false;
            //DateTime dtTemp = dtCouncilDate;
            //previously 14 months.
            DateTime dtTemp = Adjust(SubtractMonth(dtCouncilDate, 13));
            //if DateTime.Now is earlear than dtTemp:
            if (DateTime.Compare(DateTime.Now, dtTemp) < 0)
            {
                blnYes = true;
            }
            return blnYes;
        }
        public static bool CouncilDateAvailable(DateTime dtCouncilDate)
        {
            bool blnYes = false;

            //DateTime dtTemp = NIH Guide Publication Date
            //changes to 10 insted of 11 on 02/09/2011.
            DateTime dtTemp = Adjust(SubtractMonth(dtCouncilDate, 10));

            //NHLBI Council Concept Clearance Date
            dtTemp = Adjust(SubtractMonth(dtTemp, 4));

            //if DateTime.Now is earlear than dtTemp:
            if (DateTime.Compare(DateTime.Now, dtTemp) < 0)
            {
                blnYes = true;
            }
            return blnYes;
        }
        public static bool CouncilDateAvailable(string strCouncilDate)
        {
            return CouncilDateAvailable(Convert.ToDateTime(strCouncilDate));
        }
        public static bool CouncilDateAvailableRFA(DateTime dtCouncilDate)
        {
            bool blnYes = false;

            if (DateTime.Compare(DateTime.Now, dtCouncilDate) < 0)
            {
                blnYes = true;
            }
            return blnYes;
        }
        public static bool CouncilDateAvailableRFA(string strCouncilDate)
        {
            return CouncilDateAvailableRFA(Convert.ToDateTime(strCouncilDate));
        }
        public static bool CouncilDateAvailableRFP(DateTime dtCouncilDate)
        {
            bool blnYes = false;

            if (DateTime.Compare(DateTime.Now, dtCouncilDate) < 0)
            {
                blnYes = true;
            }
            return blnYes;
        }
        public static bool CouncilDateAvailableRFP(string strCouncilDate)
        {
            return CouncilDateAvailableRFP(Convert.ToDateTime(strCouncilDate));
        }
        public static DateTime SubtractMonth(DateTime dtDate, int intNumberOfMonth)
        {
            return dtDate.AddMonths(-intNumberOfMonth);
        }
        public static DateTime SubtractDays(DateTime dtDate, int numberOfDays)
        {
            return dtDate.AddDays(-numberOfDays);
        }
        public static DateTime Adjust(DateTime dtDate)
        {
            return skipFedralHoliday(SkipWeekend(SkipBlackoutDate(dtDate)));
        }
        public static DateTime AdjustRFADate(DateTime dtDate)
        {
            return skipFedralHoliday(SkipWeekend(SkipBlackoutDateRFA(dtDate)));
        }
        public static DateTime AdjustRFPDate(DateTime dtDate)
        {
            return skipFedralHoliday(SkipWeekend(SkipBlackoutDateRFP(dtDate)));
        }

        public static DateTime SkipBlackoutDate(DateTime dtDate)
        {

            DateTime dtTemp = dtDate;
            DateTime dtDate1 = dtDate;
            DateTime dtDate2 = dtDate;
            string strDate1, strDate2;
            string strDays = Settings.Get(AbbreviatedMonth(dtDate));


            if (strDays.IndexOf('-') > 0)
            {
                char[] spacer = { '-' };
                string[] arrDays = strDays.Split(spacer);
                dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[0] + "/" + dtDate.Year);
                dtDate2 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[1] + "/" + dtDate.Year);

                if (dtDate.CompareTo(dtDate1) >= 0 && dtDate.CompareTo(dtDate2) <= 0)
                {
                    dtTemp = Convert.ToDateTime(dtDate2.Month + "/" + (dtDate2.Day + 1) + "/" + dtDate2.Year);
                }
                else
                {
                    //do nothing
                }
            }
            else
            {
                dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + strDays + "/" + dtDate.Year);
                if (dtDate.CompareTo(dtDate1) == 0)
                {
                    dtTemp = Convert.ToDateTime(dtDate1.Month + "/" + (dtDate1.Day + 1) + "/" + dtDate1.Year);
                }
                else
                {
                    //do nothing
                }
            }
            return dtTemp;
        }
        public static DateTime SkipBlackoutDateRFA(DateTime dtDate)
        {

            DateTime dtTemp = dtDate;
            DateTime dtDate1 = dtDate;
            DateTime dtDate2 = dtDate;
            string strDate1, strDate2;
            string strDays = Settings.Get(AbbreviatedMonth(dtDate) + "RFA");

            if (!String.IsNullOrEmpty(strDays))
            {
                if (strDays.IndexOf('-') > 0)
                {
                    char[] spacer = { '-' };
                    string[] arrDays = strDays.Split(spacer);
                    dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[0] + "/" + dtDate.Year);
                    dtDate2 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[1] + "/" + dtDate.Year);

                    if (dtDate.CompareTo(dtDate1) >= 0 && dtDate.CompareTo(dtDate2) <= 0)
                    {
                        dtTemp = Convert.ToDateTime(dtDate2.Month + "/" + (dtDate2.Day + 1) + "/" + dtDate2.Year);
                    }
                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + strDays + "/" + dtDate.Year);
                    if (dtDate.CompareTo(dtDate1) == 0)
                    {
                        dtTemp = Convert.ToDateTime(dtDate1.Month + "/" + (dtDate1.Day + 1) + "/" + dtDate1.Year);
                    }
                    else
                    {
                        //do nothing
                    }
                }
            }
            return dtTemp;
        }
        public static DateTime SkipBlackoutDateRFP(DateTime dtDate)
        {

            DateTime dtTemp = dtDate;
            DateTime dtDate1 = dtDate;
            DateTime dtDate2 = dtDate;
            string strDate1, strDate2;
            string strDays = Settings.Get(AbbreviatedMonth(dtDate) + "RFP");

            if (!String.IsNullOrEmpty(strDays))
            {
                if (strDays.IndexOf('-') > 0)
                {
                    char[] spacer = { '-' };
                    string[] arrDays = strDays.Split(spacer);
                    dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[0] + "/" + dtDate.Year);
                    dtDate2 = Convert.ToDateTime(dtDate.Month + "/" + arrDays[1] + "/" + dtDate.Year);

                    if (dtDate.CompareTo(dtDate1) >= 0 && dtDate.CompareTo(dtDate2) <= 0)
                    {
                        dtTemp = Convert.ToDateTime(dtDate2.Month + "/" + (dtDate2.Day + 1) + "/" + dtDate2.Year);
                    }
                    else
                    {
                        //do nothing
                    }
                }
                else
                {
                    dtDate1 = Convert.ToDateTime(dtDate.Month + "/" + strDays + "/" + dtDate.Year);
                    if (dtDate.CompareTo(dtDate1) == 0)
                    {
                        dtTemp = Convert.ToDateTime(dtDate1.Month + "/" + (dtDate1.Day + 1) + "/" + dtDate1.Year);
                    }
                    else
                    {
                        //do nothing
                    }
                }
            }
            return dtTemp;
        }

        public static DateTime SkipWeekend(DateTime dtDate)
        {
            DateTime dtTemp = dtDate;
            if (IsSunday(dtDate))
            {
                dtTemp = dtDate.AddDays(1);
            }
            else if (IsSaturday(dtDate))
            {
                dtTemp = dtDate.AddDays(2);
            }
            return dtTemp;
        }
        public static DateTime skipFedralHoliday(DateTime dtDate)
        {
            DateTime dtTemp = dtDate;

            if (dtDate.Month.Equals("1") && dtDate.Day.Equals("20"))
            {
                //skip Martin Luther King Jr. birthday
                dtTemp = dtDate.AddDays(1);
            }
            else if (dtDate.Month.Equals("2") && dtDate.DayOfWeek.Equals("Monday"))
            {
                //skip Washington's birthday
                DateTime dtTemp1 = dtDate.AddDays(-21);
                DateTime dtTemp2 = dtDate.AddDays(-28);
                if (dtTemp1.Month.Equals("2") && dtTemp2.Month.Equals("1"))
                {
                    dtTemp = dtDate.AddDays(1);
                }
            }
            else if (dtDate.Month.Equals("5") && dtDate.DayOfWeek.Equals("Monday"))
            {
                //skip Memorial Day
                dtTemp = dtDate.AddDays(7);
                if (dtTemp.Month.Equals("6"))
                {
                    dtTemp = dtDate.AddDays(1);
                }
            }
            else if (dtDate.Month.Equals("9") && dtDate.DayOfWeek.Equals("Monday"))
            {
                //skip Labor Day
                dtTemp = dtDate.AddDays(-7);
                if (dtTemp.Month.Equals("8"))
                {
                    dtTemp = dtDate.AddDays(1);
                }
            }
            else if (dtDate.Month.Equals("10") && dtDate.Day.Equals("12"))
            {
                //skip Columbus Day
                dtTemp = dtDate.AddDays(1);
            }
            else if (dtDate.Month.Equals("11") && dtDate.Day.Equals("11"))
            {
                //skip Veterans Day
                dtTemp = dtDate.AddDays(1);
            }
            else if (dtDate.Month.Equals("11") && dtDate.DayOfWeek.Equals("Thursday"))
            {
                //skip Labor Day
                dtTemp = dtDate.AddDays(7);
                if (dtTemp.Month.Equals("12"))
                {
                    dtTemp = dtDate.AddDays(1);
                }
            }
            else if (dtDate.Month.Equals("12") && dtDate.Day.Equals("25"))
            {
                //skip Christmas  Day
                dtTemp = dtDate.AddDays(1);
            }
            return dtTemp;
        }
        public static string AbbreviatedMonth(string strDate)
        {
            DateTimeFormatInfo dtf = new DateTimeFormatInfo();
            return dtf.GetAbbreviatedMonthName(DateTime.Parse(strDate).Month);
        }
        public static string AbbreviatedMonth(DateTime dtDate)
        {
            DateTimeFormatInfo dtf = new DateTimeFormatInfo();
            return dtf.GetAbbreviatedMonthName(dtDate.Month);
        }
        public static string Month(DateTime dtDate)
        {
            DateTimeFormatInfo dtf = new DateTimeFormatInfo();
            return dtf.GetMonthName(dtDate.Month);
        }
        public static string MonthYear(DateTime dtDate)
        {
            return Month(dtDate) + ", " + dtDate.Year;
        }
        public static bool IsWeekend(string strDate)
        {
            string strDay = Convert.ToDateTime(strDate).DayOfWeek.ToString();
            return (strDay == "Saturday" || strDay == "Sunday");
        }
        public static bool IsWeekend(DateTime dtDate)
        {
            string strDay = dtDate.DayOfWeek.ToString();
            return (strDay == "Saturday" || strDay == "Sunday");
        }
        public static bool IsSaturday(string strDate)
        {
            return (Convert.ToDateTime(strDate).DayOfWeek.ToString() == "Saturday");
        }
        public static bool IsSaturday(DateTime dtDate)
        {
            return (dtDate.DayOfWeek.ToString() == "Saturday");
        }
        public static bool IsSunday(string strDate)
        {
            return (Convert.ToDateTime(strDate).DayOfWeek.ToString() == "Sunday");
        }
        public static bool IsSunday(DateTime dtDate)
        {
            return (dtDate.DayOfWeek.ToString() == "Sunday");
        }
        public static DateTime AddBusinessDays(DateTime date, int days)
        {
            if (days == 0) return date;

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
                date = skipFedralHoliday(SkipBlackoutDate(date));
                days -= 1;
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
                date = skipFedralHoliday(SkipBlackoutDate(date));
                days -= 1;
            }

            date = date.AddDays(days / 5 * 7);
            int extraDays = days % 5;

            if ((int)date.DayOfWeek + extraDays > 5)
            {
                extraDays += 2;
            }
            return date.AddDays(extraDays);

        }

    }
}
