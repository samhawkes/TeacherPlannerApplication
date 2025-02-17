﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.LessonSequence.ViewModels;
using TeacherPlanner.Login.Models;
using TeacherPlanner.Planner.ViewModels;
using TeacherPlanner.PlannerYear.Models;
using TeacherPlanner.PlannerYear.Views;
using TeacherPlanner.Timetable.ViewModels;
using TeacherPlanner.ToDo.ViewModels;

namespace TeacherPlanner.PlannerYear.ViewModels
{
    public class PlannerYearViewModel : ObservableObject
    {
        // Fields
        private KeyDatesWindowViewModel _keyDatesWindowViewModel;
        private Window _keyDatesWindow = null;
        private TimetableViewModel _timetableViewModel;
        private ObservableCollection<KeyDateItemViewModel> _keyDates;
        private readonly AcademicYearModel _academicYear;

        // Commands / Actions / Events
        public ICommand DefineTimetableWeeksCommand { get; }
        public ICommand SwitchViewCommand { get; }
        public ICommand KeyDatesClickedCommand { get; }
        public ICommand ImportTimetableCommand { get; }

        public event EventHandler<string> SwitchViewEvent;
        public PlannerYearViewModel(UserModel userModel, AcademicYearModel year)
        {
            _academicYear = year;
            UserModel = userModel;
            //FileHandlingHelper.SetDirectories(UserModel, yearString);
            KeyDates = new ObservableCollection<KeyDateItemViewModel>();

            CalendarManager = new CalendarManager(year);

            TimetableViewModel = new TimetableViewModel(UserModel, CalendarManager, _academicYear, TimetableDisplayModes.Normal);
            PlannerViewModel = new PlannerViewModel(UserModel, TimetableViewModel.CurrentTimetable, CalendarManager, KeyDates, _academicYear);
            LessonSequenceViewModel = new LessonSequenceViewModel(_academicYear, TimetableViewModel.CurrentTimetable, CalendarManager, UserModel);
            TimetableViewModel.TimetableChangedEvent += (_, timetableModel) => PlannerViewModel.UpdateCurrentTimetable(timetableModel);

            ToDoViewModel = new TodoPageViewModel(UserModel, year);

            // keyDates view Model new up and event subscription
            _keyDatesWindowViewModel = new KeyDatesWindowViewModel(KeyDates, _academicYear);
            _keyDatesWindowViewModel.KeyDatesListUpdatedEvent += (_, __) => UpdateKeyDatesList();
            _keyDatesWindowViewModel.CloseWindowEvent += (_, __) => OnKeyDatesWindowClosed();

            LessonSequenceViewModel.PeriodsUpdatedEvent += (_, __) => PlannerViewModel.LoadNewDays();
            PlannerViewModel.PlannerUpdatedEvent += (_, __) => LessonSequenceViewModel.UpdateLessonSequence();

            DefineTimetableWeeksCommand = new SimpleCommand(_ => OnDefineTimetableWeeks());
            SwitchViewCommand = new SimpleCommand(_ => OnSwitchView(_));
            KeyDatesClickedCommand = new SimpleCommand(_ => OnKeyDatesClicked());
            ImportTimetableCommand = new SimpleCommand(_ => OnImportTimetable());

            //Method Calls
            UpdateKeyDatesList();


        }

        // Properties

        public CalendarManager CalendarManager { get; private set; }
        public UserModel UserModel { get; }
        public PlannerViewModel PlannerViewModel { get; }
        public LessonSequenceViewModel LessonSequenceViewModel { get; }
        public TodoPageViewModel ToDoViewModel { get; }
        public TimetableViewModel TimetableViewModel
        {
            get => _timetableViewModel;
            set => RaiseAndSetIfChanged(ref _timetableViewModel, value);
        }
        public ObservableCollection<KeyDateItemViewModel> KeyDates
        {
            get => _keyDates;
            set => RaiseAndSetIfChanged(ref _keyDates, value);
        }


        // Methods
        public void OnDefineTimetableWeeks()
        {
            var defineTimetableWeeksWindow = new DefineTimetableWeeksWindow();
            var viewModel = new DefineTimetableWeeksViewModel(UserModel, _academicYear);
            defineTimetableWeeksWindow.DataContext = viewModel;

            if (defineTimetableWeeksWindow.ShowDialog() ?? true)
            {
                //TimetableViewModel.TryGetImportedTimetable();
                PlannerViewModel.LoadNewDays();
                LessonSequenceViewModel.Update();
            }
        }

        public void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("Tab Changed");
            var tabControl = sender as TabControl;
            var header = ControlExtensions.GetHeaderOfPreviouslySelectedTab(e);
            if (header == "Planner")
            {
                PlannerViewModel.OnSave(false);
                LessonSequenceViewModel.UpdateLessonSequence();
            }
            else if (header == "Lesson Sequencer")
            {
                LessonSequenceViewModel.OnUpdatePeriods(false);
                PlannerViewModel.LoadNewDays();
            }
            e.Handled = true;
        }

        private void UpdateKeyDatesList()
        {
            PlannerViewModel.LeftDay.UpdateTodaysKeyDates();
            PlannerViewModel.RightDay.UpdateTodaysKeyDates();
        }
        private void OnSwitchView(object v)
        {
            SwitchViewEvent.Invoke(null, (string)v);
        }


        private void OnKeyDatesClicked()
        {
            // Check to see whether the window is already open
            if (_keyDatesWindow == null)
            {
                _keyDatesWindow = new KeyDatesWindow
                {
                    DataContext = _keyDatesWindowViewModel
                };
                _keyDatesWindow.Show();
            }
            // If window is already open, then just give focus to it
            else
                _keyDatesWindow.Focus();
        }

        private void OnKeyDatesWindowClosed()
        {
            _keyDatesWindow = null;
        }

        private void OnImportTimetable()
        {
            var importWindow = new ImportTimetableWindow();
            var importTimetableViewModel = new ImportTimetableWindowViewModel(_academicYear);
            importWindow.DataContext = importTimetableViewModel;

            if (importWindow.ShowDialog() ?? true)
            {
                TimetableViewModel.TryGetImportedTimetable();
                LessonSequenceViewModel.Update();
            }
        }
    }
}
