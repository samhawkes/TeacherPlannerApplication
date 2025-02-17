﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace TeacherPlanner.Login.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginAccountView : UserControl
    {
        public LoginAccountView()
        {
            InitializeComponent();
        }
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = Brushes.Black;
            button.Background = Brushes.LightBlue;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = Brushes.Transparent;
            button.Background = Brushes.LightGray;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Control box = sender as Control;
            box.Background = Brushes.White;
            //box.Background.Opacity = 0.5;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Control box = sender as Control;
            box.Background = Brushes.LightGray;
        }
    }
}
