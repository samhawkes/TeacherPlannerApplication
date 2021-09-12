﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeacherPlanner.Constants;
using TeacherPlanner.Helpers;
using TeacherPlanner.Login.Models;

namespace TeacherPlanner.Login.ViewModels
{
    public class CreateAccountViewModel : ObservableObject
    {
        private string _username;
        private string _feedbackForCreateUsername;
        private string _feedbackForCreatePassword;
        public CreateAccountViewModel()
        {
            CreateAccountButtonClickedCommand = new SimpleCommand(password => OnCreateAccountButtonClicked(password));
            OnPasswordChangeCommand = new SimpleCommand(password => OnPasswordChange(password));
            FeedbackForCreateUsername = string.Empty;
            FeedbackForCreatePassword = string.Empty;
        }
        public string Username 
        { 
            get => _username;
            set 
            {
                RaiseAndSetIfChanged(ref _username, value.Trim().ToLower());
                ParseUsername(_username);
            }
        }
        
        private string PasswordHash { get; set; }
        public bool UsernameIsValidFormat { get; set; } = false;
        public bool PasswordIsValidFormat { get; set; } = false;
        public ICommand CreateAccountButtonClickedCommand { get; }
        public ICommand OnPasswordChangeCommand { get; }
        public string FeedbackForCreateUsername 
        {
            get => _feedbackForCreateUsername;
            set => RaiseAndSetIfChanged(ref _feedbackForCreateUsername, value);
        }
        public string FeedbackForCreatePassword
        {
            get => _feedbackForCreatePassword;
            set => RaiseAndSetIfChanged(ref _feedbackForCreatePassword, value);
        }
            
        
        public void ParseUsername(string username)
        {
            if (username.Length > 3)
            {
                FeedbackForCreateUsername = "Username is Valid";
                UsernameIsValidFormat = true;
            }
            else
            {
                FeedbackForCreateUsername = "Username is too short";
                UsernameIsValidFormat = false;
            }
        }

        public void ParsePassword(string password)
        {
            var minimumSize = 8;
            if (password.Length >= minimumSize)
            {
                FeedbackForCreatePassword = "Password is Valid";
                PasswordIsValidFormat = true;
                PasswordHash = password.Trim();//SecurePasswordHasher.Hash(password.Trim());
            }
            else
            {
                FeedbackForCreatePassword = $"Password is too short - must be at least {minimumSize} characters long";
                PasswordIsValidFormat = false;
            }
        }
        
        public void OnPasswordChange(object password)
        {
            PasswordBox passwordBox = (PasswordBox)password;
            ParsePassword(passwordBox.Password);
        }
        private void StoreCredentials()
        {
            // Combine username and the hash of their entered password and store a hash of that value
            var userHash = SecurePasswordHasher.Hash(Username + PasswordHash);
            FileHandlingHelper.TryWriteDataToFile(FileHandlingHelper.ApplicationConfigPath, FilesAndDirectories.AccountDataFileName, userHash);

            // Store username in separate file using symmetrical encryption to be able to keep track of registered users
            FileHandlingHelper.TryWriteDataToFile(FileHandlingHelper.ApplicationConfigPath, FilesAndDirectories.AccountManagementUsersFileName, Username, "a", true, FileHandlingHelper.Secret);
        }

        private bool CheckUserExists(string username)
        {
            var path = Path.Combine(FileHandlingHelper.ApplicationConfigPath, FilesAndDirectories.AccountManagementUsersFileName);
            if (!File.Exists(path))
            {
                return false;
            }
            var users = File.ReadAllLines(path);
            for (int i = 0; i < users.Length; i++)
            {
                if (username == Cryptography.DecryptString(FileHandlingHelper.Secret, users[i].Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        public void OnCreateAccountButtonClicked(object password)
        {       
            if (UsernameIsValidFormat && PasswordIsValidFormat)
            {
                if (!CheckUserExists(Username))
                {
                    StoreCredentials();
                    PasswordBox passwordBox = (PasswordBox)password;
                    UserModel user = new UserModel(Username, passwordBox.Password.Trim());
                    Directory.CreateDirectory(Path.Combine(FileHandlingHelper.UserDataPath, FileHandlingHelper.EncryptFileOrDirectory(Username)));
                    MessageBox.Show("Account Created Successfully");
                }
                else
                {
                    Username = string.Empty;
                    MessageBox.Show("User already exists");
                }
            }
            else
            {
                MessageBox.Show($"Username:{UsernameIsValidFormat}, Password:{PasswordIsValidFormat}");
            }
        }
    }
}
