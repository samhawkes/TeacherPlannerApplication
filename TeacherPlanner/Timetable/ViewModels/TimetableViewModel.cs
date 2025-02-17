﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Database;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.Login.Models;
using TeacherPlanner.PlannerYear.Models;
using TeacherPlanner.Timetable.Models;

namespace TeacherPlanner.Timetable.ViewModels
{
    public class TimetableViewModel : ObservableObject
    {
        private Models.TimetableWeekModel _currentlyDisplayedTimetableWeek;
        private TimetableModel _currentTimetable;
        private int _selectedWeek;
        private readonly AcademicYearModel _academicYear;
        public ICommand SwitchTimetableWeekCommand { get; }
        public ICommand ManageTimetableCommand { get; }
        public event EventHandler<TimetableModel> TimetableChangedEvent;
        public TimetableViewModel(UserModel userModel, CalendarManager calendarManager, AcademicYearModel academicYear, TimetableDisplayModes timetableDisplayMode)
        {

            SwitchTimetableWeekCommand = new SimpleCommand(_ => OnSwitchTimetableWeek());
            TimetableChangedEvent += (_, __) => CountOccurances();

            CurrentTimetable = new TimetableModel(timetableDisplayMode);
            _academicYear = academicYear;
            UserModel = userModel;
            SelectedWeek = 1;

            TimetableWeeksAreDefined = calendarManager.GetWeek(DateTime.Today) != -1;

            if (TryGetImportedTimetable())
                UpdateCurrentlyDisplayedTimetableWeek();

        }

        // Properties

        public bool TimetableIsImported { get; private set; }
        public bool TimetableWeeksAreDefined { get; private set; }
        public int SelectedWeek
        {
            get => _selectedWeek;
            set => RaiseAndSetIfChanged(ref _selectedWeek, value);
        }
        public UserModel UserModel { get; }
        public TimetableModel CurrentTimetable
        {
            get => _currentTimetable;
            set => RaiseAndSetIfChanged(ref _currentTimetable, value);
        }
        public Models.TimetableWeekModel CurrentlyDisplayedTimetableWeek
        {
            get => _currentlyDisplayedTimetableWeek;
            set => RaiseAndSetIfChanged(ref _currentlyDisplayedTimetableWeek, value);
        }

        // Public Methods



        public bool TryGetImportedTimetable()
        {
            var dbModels = DatabaseManager.GetTimetablePeriods(_academicYear.ID);

            if (!dbModels.Any())
                return false;

            CurrentTimetable.Update(dbModels);
            TimetableIsImported = true;
            TimetableChangedEvent.Invoke(null, CurrentTimetable);
            UpdateCurrentlyDisplayedTimetableWeek();
            return true;
        }

        public void UpdateAndRefreshCurrentlyDisplayedTimetableWeek(string classcode = "")
        {
            UpdateCurrentlyDisplayedTimetable(classcode);
            UpdateCurrentlyDisplayedTimetableWeek();
        }

        private void UpdateCurrentlyDisplayedTimetable(string classcode)
        {
            var dbModels = DatabaseManager.GetTimetablePeriods(_academicYear.ID);
            CurrentTimetable.Update(dbModels);
            CurrentTimetable.Filter(new List<string>() { classcode });
        }   

        private void UpdateCurrentlyDisplayedTimetableWeek()
        {
            CurrentlyDisplayedTimetableWeek = SelectedWeek == 1 ? CurrentTimetable.Week1 : CurrentTimetable.Week2;
        }

        public void OnSwitchTimetableWeek()
        {
            SelectedWeek = SelectedWeek == 1 ? 2 : 1;
            UpdateCurrentlyDisplayedTimetableWeek();
        }

        // Private Methods

        private void CountOccurances()
        {
            Dictionary<string, int> classCodeCounts = new Dictionary<string, int>();
            var timetablePeriods = new List<TimetablePeriodModel>();
            for (int week = 1; week <= 2; week++)
            {
                for (int day = 1; day <= 5; day++)
                {
                    foreach (PeriodCodes period in Enum.GetValues(typeof(PeriodCodes)))
                    {
                        var timetablePeriod = CurrentTimetable.GetPeriod(week, day, period);

                        if (classCodeCounts.ContainsKey(timetablePeriod.ClassCode))
                            classCodeCounts[timetablePeriod.ClassCode] += 1;
                        else
                            classCodeCounts.Add(timetablePeriod.ClassCode, 1);

                        timetablePeriod.Occurance = classCodeCounts[timetablePeriod.ClassCode];
                        timetablePeriods.Add(timetablePeriod);
                    }
                }
            }

            // Count which specific occurance this particular period is
            foreach (var period in timetablePeriods)
            {
                period.Occurances = classCodeCounts[period.ClassCode];
            }
        }
    }
}
