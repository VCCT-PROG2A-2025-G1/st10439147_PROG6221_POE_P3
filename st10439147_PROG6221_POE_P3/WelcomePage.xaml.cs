using ST10439147_PROG6221_POE.MyClasses;
using ST10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace st10439147_PROG6221_POE_P3
{
    /// <summary>
    /// Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomePage : Window
    {
        private Greeting _greeting;
        private bool _greetingCompleted = false;
        private UserMemory _userMemory;

        public WelcomePage()
        {
            InitializeComponent();

            // Initialize UserMemory
            _userMemory = new UserMemory();

            try
            {
                _greeting = new Greeting();
            }
            catch (Exception ex)
            {
                // Handle case where Greeting class might not be available
                System.Diagnostics.Debug.WriteLine($"Warning: Could not initialize Greeting: {ex.Message}");
                _greeting = null;
            }

            // Make window draggable
            this.MouseLeftButtonDown += WelcomeWindow_MouseLeftButtonDown;

            // Initially hide the buttons until greeting is complete
            HideButtons();

            // Play greeting sound when window loads
            this.Loaded += WelcomeWindow_Loaded;

            // Start entrance animations (but buttons remain hidden)
            StartEntranceAnimations();
        }

        private void WelcomeWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private async void WelcomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Play the greeting sound and wait for it to complete
            try
            {
                if (_greeting != null)
                {
                    await Task.Run(() => _greeting.PlayGreeting());
                }
                else
                {
                    // If greeting is not available, just wait a short time for visual effect
                    await Task.Delay(1000);
                }

                // Greeting completed - show the buttons with animation
                _greetingCompleted = true;
                ShowButtonsWithAnimation();
            }
            catch (Exception ex)
            {
                // Handle any errors - still show buttons even if greeting fails
                System.Diagnostics.Debug.WriteLine($"Error playing greeting: {ex.Message}");
                _greetingCompleted = true;
                ShowButtonsWithAnimation();
            }
        }

        private void HideButtons()
        {
            // Hide Continue and Exit buttons initially
            var continueButton = FindName("ContinueButton") as FrameworkElement;
            var exitButton = FindName("CloseButton") as FrameworkElement;

            if (continueButton != null)
                continueButton.Visibility = Visibility.Hidden;

            if (exitButton != null)
                exitButton.Visibility = Visibility.Hidden;
        }

        private void ShowButtonsWithAnimation()
        {
            // Show and animate the Continue and Exit buttons
            var continueButton = FindName("ContinueButton") as FrameworkElement;
            var exitButton = FindName("CloseButton") as FrameworkElement;

            if (continueButton != null)
            {
                continueButton.Visibility = Visibility.Visible;
                AnimateButtonAppearance(continueButton, 0.2);
            }

            if (exitButton != null)
            {
                exitButton.Visibility = Visibility.Visible;
                AnimateButtonAppearance(exitButton, 0.4);
            }
        }

        private void AnimateButtonAppearance(FrameworkElement element, double delay)
        {
            // Ensure element exists
            if (element == null) return;

            // Start with invisible and small scale
            element.Opacity = 0;
            element.RenderTransform = new System.Windows.Media.ScaleTransform(0.8, 0.8);

            // Create fade in animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                BeginTime = TimeSpan.FromSeconds(delay)
            };

            // Create scale animation
            var scaleXAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                BeginTime = TimeSpan.FromSeconds(delay)
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.6),
                BeginTime = TimeSpan.FromSeconds(delay)
            };

            // Apply easing function for smooth animation
            var easingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            fadeAnimation.EasingFunction = easingFunction;
            scaleXAnimation.EasingFunction = easingFunction;
            scaleYAnimation.EasingFunction = easingFunction;

            // Apply animations safely
            try
            {
                element.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

                if (element.RenderTransform is System.Windows.Media.ScaleTransform scaleTransform)
                {
                    scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleXAnimation);
                    scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleYAnimation);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Animation error: {ex.Message}");
                // Fallback - just make element visible
                element.Opacity = 1;
                element.RenderTransform = new System.Windows.Media.ScaleTransform(1, 1);
            }
        }

        private void StartEntranceAnimations()
        {
            // Animate only the title and subtitle, not the buttons
            var titleElement = FindName("TitleText") as FrameworkElement;
            var subtitleElement = FindName("SubtitleText") as FrameworkElement;

            if (titleElement != null)
                AnimateTextElementSlideIn(titleElement, 0.1);

            if (subtitleElement != null)
                AnimateTextElementSlideIn(subtitleElement, 0.3);
        }

        private void AnimateTextElementSlideIn(FrameworkElement element, double delay)
        {
            // Ensure element exists
            if (element == null) return;

            // Start with invisible
            element.Opacity = 0;
            element.RenderTransform = new System.Windows.Media.TranslateTransform(0, 20);

            // Create slide-in animation
            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delay)
            };

            var slideAnimation = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delay)
            };

            // Apply easing function
            var easingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            fadeAnimation.EasingFunction = easingFunction;
            slideAnimation.EasingFunction = easingFunction;

            // Apply animations safely
            try
            {
                element.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

                if (element.RenderTransform is System.Windows.Media.TranslateTransform translateTransform)
                {
                    translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideAnimation);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Animation error: {ex.Message}");
                // Fallback - just make element visible
                element.Opacity = 1;
                element.RenderTransform = new System.Windows.Media.TranslateTransform(0, 0);
            }
        }

        // SIMPLIFIED: Only Continue button functionality
        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            // Only allow continue if greeting has completed
            if (!_greetingCompleted)
            {
                MessageBox.Show(
                    "Please wait for the welcome greeting to complete.",
                    "Please Wait",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            NavigateToNextPage();
        }

        private void NavigateToNextPage()
        {
            // Create exit animation before opening main window
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3)
            };

            fadeOut.Completed += (s, args) =>
            {
                try
                {
                    // Pass the UserMemory instance to NextPage
                    NextPage nextWindow = new NextPage(_userMemory);
                    this.Hide();

                    var result = nextWindow.ShowDialog();

                    // Set DialogResult based on what happened in NextPage
                    this.DialogResult = result;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error navigating to next page: {ex.Message}",
                        "Navigation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    // Show this window again
                    this.Show();
                    this.Opacity = 1.0;
                }
            };

            this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        // Only Close/Exit button functionality  
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit CyberGuard AI?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = false; // Set DialogResult first
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.2)
                };

                fadeOut.Completed += (s, args) => this.Close();
                this.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }

        /// <summary>
        /// Static method to show the welcome window and wait for greeting completion
        /// </summary>
        /// <returns>True if user clicked Continue, False if user chose to exit</returns>
        public static bool ShowWelcome()
        {
            var welcomeWindow = new WelcomePage();
            var result = welcomeWindow.ShowDialog();
            return result == true;
        }

        /// <summary>
        /// Method to show welcome window as dialog and return result
        /// </summary>
        /// <returns>True if user clicked Continue, False if closed/exit</returns>
        public bool? ShowWelcomeDialog()
        {
            return this.ShowDialog();
        }

        /// <summary>
        /// Method to get the UserMemory instance
        /// </summary>
        public UserMemory GetUserMemory()
        {
            return _userMemory;
        }
    }
}