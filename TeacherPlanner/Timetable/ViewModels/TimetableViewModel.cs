﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.Login.Models;
using TeacherPlanner.Planner.Models;
using TeacherPlanner.Planner.ViewModels;
using TeacherPlanner.Planner.Views.SettingsWindows;
using TeacherPlanner.Timetable.Models;

namespace TeacherPlanner.Timetable.ViewModels
{
    public class TimetableViewModel : ObservableObject
    {
        private Models.TimetableWeekModel _currentlyDisplayedTimetableWeek;
        private TimetableModel _currentTimetable;
        private int _selectedWeek;
        public ICommand SwitchTimetableWeekCommand { get; }
        public ICommand ImportTimetableCommand { get; }
        public ICommand ManageTimetableCommand { get; }
        public ICommand ApplySelectedTimetableCommand { get; }
        public event EventHandler<TimetableModel> TimetableChangedEvent;
        public TimetableViewModel(UserModel userModel, CalendarManager calendarManager)
        {
            ImportTimetableCommand = new SimpleCommand(_ => OnTimetableImportClick());
            ApplySelectedTimetableCommand = new SimpleCommand(timetableName => ApplySelectedTimetable((string) timetableName));
            SwitchTimetableWeekCommand = new SimpleCommand(_ => OnSwitchTimetableWeek());


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

        public void OnTimetableImportClick()
        {
            var importWindow = new ImportTimetableWindow();
            var importTimetableViewModel = new ImportTimetableWindowViewModel(UserModel, importWindow);
            importWindow.DataContext = importTimetableViewModel;

            if (importWindow.ShowDialog() ?? true)
            {
                TryGetImportedTimetable();
                UpdateCurrentlyDisplayedTimetableWeek();
                TimetableChangedEvent.Invoke(null, CurrentTimetable);
            }
        }

        public bool TryGetImportedTimetable()
        {
            var directory = Path.Combine(FileHandlingHelper.LoggedInUserDataPath, FileHandlingHelper.EncryptFileOrDirectory(FilesAndDirectories.TimetableDirectory, UserModel.Key));
            Directory.CreateDirectory(directory);
            var filenames = Directory.GetFiles(directory);

            for (var i = 0; i < filenames.Length; i++)
            {
                var filename = Path.GetFileName(filenames[i]);
                if (filename == FileHandlingHelper.EncryptFileOrDirectory(FilesAndDirectories.SavedTimetableFileName, UserModel.Key))
                {
                    var timetableData = FileHandlingHelper.ReadDataFromCSVFile(filenames[i], true, UserModel.Key);
                    CurrentTimetable = new TimetableModel(timetableData);
                    TimetableIsImported = true;
                    return true;
                }
            }
            return false;
        }


        // Private Methods

        private void UpdateCurrentlyDisplayedTimetableWeek()
        {
            CurrentlyDisplayedTimetableWeek = SelectedWeek == 1 ? CurrentTimetable.Week1 : CurrentTimetable.Week2;
        }
        private void OnSwitchTimetableWeek()
        {
            SelectedWeek = SelectedWeek == 1 ? 2 : 1;
            UpdateCurrentlyDisplayedTimetableWeek();
        }

        private void ApplySelectedTimetable(string filename)
        {
            MessageBox.Show(filename);
        }
    }
}
