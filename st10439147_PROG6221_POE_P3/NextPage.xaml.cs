using System;
using System.Windows;
using System.Windows.Media.Animation;
using ST10439147_PROG6221_POE_P3.MyClasses; // Add this using statement

namespace st10439147_PROG6221_POE_P3
{
    /// <summary>
    /// Interaction logic for NextPageWindow.xaml
    /// </summary>
    public partial class NextPage : Window
    {
        // Username properties moved from Communication.cs
        public string UserName { get; private set; }
        public bool IsUserNameSet { get; private set; }

        // Add UserMemory instance
        private UserMemory _userMemory;

        // Add this field to your class
        private bool _isModal = false;

        // Constructor that accepts UserMemory instance
        public NextPage(UserMemory userMemory = null)
        {
            InitializeComponent();

            // Initialize UserMemory - create new instance if none provided
            _userMemory = userMemory ?? new UserMemory();

            // Make window draggable
            this.MouseLeftButtonDown += NextPageWindow_MouseLeftButtonDown;

            // Set default values
            InitializeDefaults();

            // Initialize username properties
            IsUserNameSet = false;
        }

        private void NextPageWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Allow the window to be dragged
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch (InvalidOperationException)
            {
                // Handle the case where DragMove is called when mouse is not pressed
                // This can happen in some edge cases
            }
        }

        // Updated static method to accept UserMemory
        public static bool ShowNextPageAsDialog(UserMemory userMemory = null)
        {
            var nextPage = new NextPage(userMemory);
            nextPage._isModal = true;
            var result = nextPage.ShowDialog(); // ShowDialog() - this allows DialogResult
            return result == true;
        }

        public static NextPage ShowNextPageAsWindow(UserMemory userMemory = null)
        {
            var nextPage = new NextPage(userMemory);
            nextPage._isModal = false;
            nextPage.Show(); // Show() - don't set DialogResult for regular windows
            return nextPage;
        }

        // Method to get the UserMemory instance (useful for passing to MainWindow)
        public UserMemory GetUserMemory()
        {
            return _userMemory;
        }

        private void InitializeDefaults()
        {
            // Check if NameTextBox exists before trying to use it
            if (NameTextBox == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: NameTextBox not found in XAML");
                return;
            }

            // Set initial placeholder text
            NameTextBox.Text = "Enter your name...";
            NameTextBox.Foreground = System.Windows.Media.Brushes.LightGray;

            NameTextBox.GotFocus += (s, e) =>
            {
                if (NameTextBox.Text == "Enter your name...")
                {
                    NameTextBox.Text = "";
                    NameTextBox.Foreground = System.Windows.Media.Brushes.White;
                }
            };

            NameTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = "Enter your name...";
                    NameTextBox.Foreground = System.Windows.Media.Brushes.LightGray;
                }
            };

            NameTextBox.TextChanged += (s, e) =>
            {
                // If it's not the placeholder and not empty, make sure the text is visible
                if (!string.IsNullOrWhiteSpace(NameTextBox.Text) && NameTextBox.Text != "Enter your name...")
                {
                    NameTextBox.Foreground = System.Windows.Media.Brushes.White;
                }
            };
        }

        /// <summary>
        /// Validates user name according to the original Communication class rules
        /// </summary>
        private ValidationResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult(false, "Name cannot be empty. Please try again.");
            }

            // Check if name contains only letters and whitespace
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    return new ValidationResult(false, "Name should only contain letters. Please try again.");
                }
            }

            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Sets the user name after validation and stores it in UserMemory
        /// </summary>
        private bool SetUserName(string name)
        {
            var validation = ValidateName(name);
            if (validation.IsValid)
            {
                UserName = name.Trim();
                IsUserNameSet = true;

                // Store the username in UserMemory
                _userMemory.StoreUserInfo("username", UserName);
                _userMemory.StoreUserInfo("name", UserName); // Store under both keys for flexibility

                return true;
            }
            else
            {
                MessageBox.Show(
                    validation.ErrorMessage,
                    "Invalid Name",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (NameTextBox == null)
            {
                MessageBox.Show("Name input field not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string enteredName = NameTextBox.Text == "Enter your name..." ? "" : NameTextBox.Text;

            if (!SetUserName(enteredName))
            {
                NameTextBox.Focus();
                return;
            }

            SaveUserName();

            // Create and show MainWindow immediately
            NavigateToMainWindow();
        }

        private void NavigateToMainWindow()
        {
            try
            {
                // Create MainWindow with the username
                MainWindow mainWindow = new MainWindow(UserName);

                // If this is a modal dialog, set DialogResult and close properly
                if (_isModal)
                {
                    // Show MainWindow before closing this dialog
                    mainWindow.Show();

                    // Set as main application window if needed
                    if (Application.Current.MainWindow == this)
                    {
                        Application.Current.MainWindow = mainWindow;
                    }

                    // Set DialogResult last to close this dialog
                    this.DialogResult = true;
                }
                else
                {
                    // For non-modal windows
                    // Set as main application window if this was the main window
                    if (Application.Current.MainWindow == this)
                    {
                        Application.Current.MainWindow = mainWindow;
                    }

                    // Show the new window
                    mainWindow.Show();

                    // Close this window
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening main window: {ex.Message}",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveUserName()
        {
            try
            {
                // Store additional user information in UserMemory
                _userMemory.StoreUserInfo("registration_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _userMemory.StoreUserInfo("first_login", "true");

                // You can also implement file/database saving logic here
                // Properties.Settings.Default.UserName = UserName;
                // Properties.Settings.Default.Save();

                // Show in debug output
                System.Diagnostics.Debug.WriteLine($"User name saved to UserMemory: {UserName}");

                // Optional: Show summary of stored data
                var summary = _userMemory.GetUserSummary();
                foreach (var item in summary)
                {
                    System.Diagnostics.Debug.WriteLine($"{item.Key}: {item.Value}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving name: {ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_isModal)
                    {
                        this.DialogResult = false;
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
                catch (InvalidOperationException)
                {
                    // If we can't set DialogResult, handle gracefully
                    if (_isModal)
                    {
                        this.Close();
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
        }

        /// <summary>
        /// Public method to get the configured user name
        /// </summary>
        public string GetUserName()
        {
            return IsUserNameSet ? UserName : "";
        }
    }

    /// <summary>
    /// Represents the result of input validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        public ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }
}