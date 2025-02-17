﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Database;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.Login.Models;
using TeacherPlanner.Planner.Models;
using TeacherPlanner.PlannerYear.Models;
using TeacherPlanner.Timetable.Models;
using TeacherPlanner.Timetable.ViewModels;

namespace TeacherPlanner.LessonSequence.ViewModels
{
    public class LessonSequenceViewModel : ObservableObject
    {
        private readonly AcademicYearModel _academicYear;
        private readonly TimetableModel _timetable;
        private readonly CalendarManager _calendarManager;
        private string _selectedClassCode;
        private DateTime _selectedDate;
        private ObservableCollection<DayModel> _lessonSequence;
        private TimetableViewModel _selectedClassCodeTimetable;

        public ICommand ChangeDateCommand { get; }
        public ICommand ChangeDateToTodayCommand { get; }
        public ICommand UpdatePeriodsCommand { get; }
        public event EventHandler PeriodsUpdatedEvent;
        public LessonSequenceViewModel(AcademicYearModel academicYear, TimetableModel timetable, CalendarManager calendarManager, UserModel userModel)
        {
            _academicYear = academicYear;
            _timetable = timetable;
            _calendarManager = calendarManager;

            // Need to use backing field here to avoid hitting the setter and
            // getting into an annoying chicken and egg situation
            _selectedDate = DateTime.Today;
            LessonSequence = new ObservableCollection<DayModel>();


            SelectedClassCodeTimetable = new TimetableViewModel(userModel, calendarManager, academicYear, TimetableDisplayModes.Lite);

            Update();

            ChangeDateCommand = new SimpleCommand(daysToAdd => IncrementDate(Int32.Parse((string)daysToAdd)));
            UpdatePeriodsCommand = new SimpleCommand(_ => OnUpdatePeriods());
        }



        public ObservableCollection<DayModel> LessonSequence 
        {
            get => _lessonSequence;
            set => RaiseAndSetIfChanged(ref _lessonSequence, value);
        }

        public ObservableCollection<string> ClassCodes { get; set; }

        public TimetableViewModel SelectedClassCodeTimetable 
        {
            get => _selectedClassCodeTimetable;
            set => RaiseAndSetIfChanged(ref _selectedClassCodeTimetable, value);
        }

        public DateTime SelectedDate 
        {
            get => _selectedDate;
            set
            {
                if (RaiseAndSetIfChanged(ref _selectedDate, value))
                {
                    UpdateLessonSequence();
                }
            }

        }

        public string SelectedClassCode 
        {
            get => _selectedClassCode;
            set
            {
                if (RaiseAndSetIfChanged(ref _selectedClassCode, value))
                {
                    UpdateLessonSequence();
                    SelectedClassCodeTimetable.UpdateAndRefreshCurrentlyDisplayedTimetableWeek(value);
                }
            }
        }

        private bool CanUpdate 
        {
            get 
            {
                return _timetable.Week1 != null && _timetable.Week2 != null && DatabaseModelManager.WeekModelsExist(_academicYear.ID);
            }
        }

        public void Update()
        {
            if (CanUpdate)
            {
                ClassCodes = new ObservableCollection<string>(_timetable.GetUniqueClassCodes());
                SelectedClassCode = ClassCodes.Any() ? ClassCodes[0] : string.Empty;
            }
        }

        public ObservableCollection<DayModel> GetLessonSequence(DateTime date, string classcode)
        {
            if (CanUpdate)
            {
                // Identify all the lessons needed
                // 1 in the past, 5 in the future
                var dates = GetLessonSequenceDates(date, classcode).Distinct().ToList();

                // Read any periods from database already stored there
                var days = DatabaseModelManager.GetLessonSequenceDayModels(dates, classcode, _academicYear.ID, _timetable, _calendarManager);



                // Some periods will not already be stored, so they will need creating.
                return days;
            }
            else 
            {
                return new ObservableCollection<DayModel>();
            }
        }

        public void UpdateLessonSequence()
        {
            if (CanUpdate)
            {
                if (ClassCodes.Any())
                    LessonSequence = GetLessonSequence(SelectedDate, SelectedClassCode);
            }
        }

        public void OnUpdatePeriods(bool notify = true)
        {
            if (LessonSequence.Any())
            {
                foreach (DayModel day in LessonSequence)
                {
                    // Add periods to Database
                    day.Periods = DatabaseModelManager.TryUpdatePeriods(day.Periods);

                    // Add Day to Database
                    day.ID = DatabaseModelManager.TryUpdateDay(day);
                }
                PeriodsUpdatedEvent.Invoke(null, EventArgs.Empty);
                if (notify)
                    MessageBox.Show("Saved to Database", "Lesson Sequencer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

       

        private void IncrementDate(int increment)
        {
            if (LessonSequence.Any())
            {
                switch (increment)
                {
                    case -1:
                        SelectedDate = LessonSequence[0].Date;
                        break;
                    case 1:
                        SelectedDate = LessonSequence[2].Date;
                        break;
                    case 7:
                        SelectedDate = LessonSequence.Last().Date;
                        break;
                    case -7:
                        var date = SelectedDate;
                        for (int i = 0; i < 5; i++)
                        {
                            var dates = FindDateOfNextLessons(date, SelectedClassCode, out _, -1);
                            if (dates != null)
                            {
                                date = dates[0].Date;
                            }
                            else
                            {
                                break;
                            }
                        }
                        SelectedDate = date;
                        break;
                    default:
                        SelectedDate = DateTime.Today;
                        break;
                }
            }
        }

        

        private List<DateTime> GetLessonSequenceDates(DateTime date, string classcode)
        {
            // Note: Some days might have more than one lesson of the same classcode
            // In this instance, we want to record both lessons as individual spots in the dates list.

            // Find the date of the most recent lesson(s) that has already happened
            var dates = FindDateOfNextLessons(date, classcode, out _, -1);

            // If there were no lessons previously, find the date of the next upcoming future lesson(s)
            if (dates == null || !dates.Any())
                dates = FindDateOfNextLessons(date.AddDays(-1), classcode, out _);

            // Keep searching for lessons until 6 have been found in total.
            while (dates.Count < 7)
            {
                var nextLessons = FindDateOfNextLessons(dates.Last(), classcode, out var atEndOfYearLimit);

                if (nextLessons == null)
                    break;

                foreach(var d in nextLessons)
                {
                    dates.Add(d);
                }

                if (atEndOfYearLimit)
                    break;
            }
            return dates;
        }

        private List<DateTime> FindDateOfNextLessons(DateTime date, string classcode, out bool atEndOfYearLimit, int advanceAmount = 1)
        {
            atEndOfYearLimit = false;
            var dates = new List<DateTime>();
            //var timetablePeriods = DatabaseManager.GetTimetablePeriodsFromClasscode(classcode);
            while (dates.Count < 7)
            { 
                date = AdvanceDate(date, advanceAmount);
                int timetableWeek = _calendarManager.GetWeek(date);
                if (timetableWeek == 0)
                    return null;
                if (timetableWeek == 1 || timetableWeek == 2)
                {
                    if (date < _calendarManager.StartOfYearDateLimit)
                    {
                        if (date > _calendarManager.EndOfYearDateLimit)
                            atEndOfYearLimit = true;
                        return dates;
                    }

                    var day = (int)date.DayOfWeek;
                    for (int j = 1; j < 6; j++)
                    {
                        var period = _timetable.GetPeriod(timetableWeek, day, (PeriodCodes)j);
                        if (period.ClassCode == classcode)
                        {
                            dates.Add(date);
                        }
                    }
                    if (dates.Count > 0)
                        return dates;
                }
            }
            return dates;
        }

        private DateTime AdvanceDate(DateTime date, int amount)
        {
            date = date.AddDays(amount);
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(amount);
            }
            return date;
        }

    }
}
