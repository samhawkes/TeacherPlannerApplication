﻿using System;

namespace TeacherPlanner.Planner.Models
{
    public class DayModel
    {
        public DayModel(DateTime date)
        {
            Periods = new PeriodModel[6];
            NoteSectionModel = new NoteSectionModel();
            Date = date;
        }

        // Properties

        public DateTime Date { get; }
        public PeriodModel[] Periods;
        public PeriodModel Period1
        {
            get { return Periods[0]; }
            set => Periods[0] = value;
        }
        public PeriodModel Period2
        {
            get { return Periods[1]; }
            set => Periods[1] = value;
        }
        public PeriodModel Period3
        {
            get { return Periods[2]; }
            set => Periods[2] = value;
        }
        public PeriodModel Period4
        {
            get { return Periods[3]; }
            set => Periods[3] = value;
        }
        public PeriodModel Period5
        {
            get { return Periods[4]; }
            set => Periods[4] = value;
        }
        public PeriodModel Period6
        {
            get { return Periods[5]; }
            set => Periods[5] = value;
        }

        public NoteSectionModel NoteSectionModel { get; set; }
        
        // Public Methods

        public void LoadPeriodDataIntoNewPeriod(int periodNumber, string classCode, string[] periodData)
        {
            PeriodModel newPeriodModel = new PeriodModel(periodNumber, classCode);
            newPeriodModel.LoadData(periodData);
            Periods[periodNumber - 1] = newPeriodModel;
        }
        
        public void LoadEmptyPeriods()
        {
            for (int i = 0; i < Periods.Length; i++)
            {
                Periods[i] = LoadEmptyIntoNewPeriod(i + 1);
            }
        }
        public void LoadEmptyIntoNewNoteSection()
        {
            NoteSectionModel.Load(new string[] { " ", " ", " ", " ", " ", " " });
        }
        public string PackageSaveData()
        {
            string saveData = string.Empty;

            for (int i = 0; i < Periods.Length; i++)
            {
                saveData += Periods[i].PackageSaveData();
                if (i != Periods.Length - 1) saveData += "\n";
            }
            string notes = NoteSectionModel.PackageSaveData();
            saveData += notes;

            return saveData;
        }

        // Private methods

        private PeriodModel LoadEmptyIntoNewPeriod(int periodNumber)
        {
            PeriodModel newEmptyPeriodModel = new PeriodModel(periodNumber, string.Empty);
            newEmptyPeriodModel.LoadData(new string[] { " ` ` ", " ` ` ", " ` ` ", " ` ` ", " ` ` ", " ` ` ", " ` ` " });
            return newEmptyPeriodModel;
        }
    }
}
