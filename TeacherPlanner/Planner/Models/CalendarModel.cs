﻿using System;
using TeacherPlanner.Helpers;

namespace TeacherPlanner.Planner.Models
{
    public class CalendarModel
    {
        private DateTime _date;

        public CalendarModel(DateTime date)
        {
            Date = date;
        }

        public string Name { get { return Date.ToString("dddd"); } }
        public string Month { get { return Date.ToString("MMMM"); } }
        public string Year { get { return Date.ToString("yyyy"); } }
        public string DisplayDate { get { return Date.ToString(CalendarHelper.DateHeadingFormat); } }

        public string DisplayMonthYear { get { return Date.ToString("MMMM yyyy"); } }
        public DateTime Date
        {
            get
            {
                return _date; //.ToString(TimeTable.DateHeadingFormat);
            }
            set
            {
                _date = value;
            }
        }
    }
}
