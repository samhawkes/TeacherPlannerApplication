﻿using System;
using System.IO;
using System.Windows.Input;
using TeacherPlanner.Helpers;
using TeacherPlanner.Planner.Models;

namespace TeacherPlanner.Planner.ViewModels
{
    public class DefineTimetableWeeksViewModel : ObservableObject
    {
        public Action CloseAction { get; }
        public ICommand SaveTimeTableWeeksCommand { get; }
        private string _path = FileHandlingHelper.LoggedInUserDataPath;
        private string _filename = "TimetableWeeks.csv";
        private string _filepath;
        private DateRowModel[] _rows;

        public DefineTimetableWeeksViewModel(Action closeWindowAction)
        {
            CloseAction = closeWindowAction;
            _filepath = Path.Combine(_path, _filename);
            SaveTimeTableWeeksCommand = new SimpleCommand(_ => OnSaveTimeTableWeeks());

            if (TryGetTimeTableWeeks(out var dateRows))
                Rows = dateRows;
            else
                Rows = CreateTimeTableWeeks();
        }

        public DateRowModel[] Rows 
        {
            get => _rows;
            set => RaiseAndSetIfChanged(ref _rows, value);
        }

        private bool TryGetTimeTableWeeks(out DateRowModel[] dateRows)
        {
            
            dateRows = new DateRowModel[0];
            if (!File.Exists(_filepath))
                return false;
            // TODO: Replace the new List with the loaded data
            dateRows = LoadDateRows();
            return true;
        }
        private DateRowModel[] LoadDateRows()
        {
            string[][] weeks = FileHandlingHelper.ReadDataFromCSVFile(_filepath);
            DateRowModel[] dateRows = new DateRowModel[weeks.Length];
            for (int i = 0; i < weeks.Length; i++)
            {
                string[] week = weeks[i];
                string[] dateString = week[0].Split("/");
                bool week1 = week[1].ToLower() == "true";
                bool week2 = week[2].ToLower() == "true";
                bool holiday = week[3].ToLower() == "true";
                
                DateTime date = new DateTime(Int32.Parse(dateString[0]), Int32.Parse(dateString[1]), Int32.Parse(dateString[2]));
                dateRows[i] = new DateRowModel(date, week1, week2, holiday);
            }
            return dateRows;
        }
        private DateRowModel[] CreateTimeTableWeeks()
        {
            var schoolYear = GetSchoolYear();
            DateTime date = GetFirstMonday(schoolYear);
            int weeks = 50;
            var rows = new DateRowModel[weeks];

            for (int i = 0; i < weeks; i++)
            {
                rows[i] = new DateRowModel(date.AddDays(i * 7));
            }

            return rows;
        }
  
        private DateTime GetFirstMonday(int schoolYear)
        {
            DateTime date = new DateTime(schoolYear, 9, 1);
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(1);
            }
            return date.AddDays(-7);
        }

        private int GetSchoolYear()
        {
            int month = Int32.Parse(DateTime.Today.ToString("MM"));
            int year = Int32.Parse(DateTime.Today.ToString("yyyy"));
            return month < 8 ? year - 1 : year;
        }

        private void OnSaveTimeTableWeeks()
        {
            SaveTimeTableWeeks();
            // LoadNewDays - From Planner View Model
            CloseAction();
        }
        private void SaveTimeTableWeeks()
        {
            int length = Rows.Length;
            string[][] weeks = new string[length][];

            // TODO: Write Rows property to file
            for (int i = 0; i < length; i++)
            {
                string[] rowData = Rows[i].Package();
                weeks[i] = rowData;
            }
            FileHandlingHelper.TryWriteDataToCSVFile(_path, _filename, weeks);
        }
    }
}
