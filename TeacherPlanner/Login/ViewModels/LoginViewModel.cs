﻿ using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Database;
using Database.DatabaseModels;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.Login.Models;
using TeacherPlanner.Planner.Models;

namespace TeacherPlanner.Login.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private string _username;

        public LoginViewModel()
        {
            LoginButtonClickedCommand = new SimpleCommand(password => OnLoginButtonClicked(password));
            LoggedIn = false;
        }
        public bool LoggedIn { get; set; }
        public string Username 
        {
            get => _username;
            set => RaiseAndSetIfChanged(ref _username, value.Trim().ToLower());
        }
        public User User { get; set; }
        public ICommand LoginButtonClickedCommand { get; }
        
        private void OnLoginButtonClicked(object parameter)
        {
            var values = (object[])parameter;
            var window = (Window)values[0];
            var passwordBox = (PasswordBox)values[1];

            User = Authenticate(Username, passwordBox.Password.Trim());
            if (LoggedIn)
                window.Close();
            else
            {
                Username = "";
                passwordBox.Password = "";
                var usernameBox = (TextBox)values[2];
                usernameBox.Focus();
                MessageBox.Show("Invalid Credentials");
            }
        }

        private User Authenticate(string username, string password)
        {
            var user = DatabaseManager.GetUserAccount(username);
            if (user != null && SecurePasswordHasher.Verify(password, user.Password))
            {
                LoggedIn = true;
                return user;
            }
            LoggedIn = false;
            return null;
        }
    }
}
