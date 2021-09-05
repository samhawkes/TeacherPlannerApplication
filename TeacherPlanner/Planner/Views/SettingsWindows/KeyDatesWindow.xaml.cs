﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeacherPlanner.Planner.Views.SettingsWindows
{
    /// <summary>
    /// Interaction logic for KeyDatesWindow.xaml
    /// </summary>
    public partial class KeyDatesWindow : Window
    {
        public KeyDatesWindow()
        {
            InitializeComponent();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
