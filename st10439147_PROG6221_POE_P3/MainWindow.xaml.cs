using ST10439147_PROG6221_POE.MyClasses;
using ST10439147_PROG6221_POE_P2.MyClasses;
using ST10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace st10439147_PROG6221_POE_P3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Communication _communication;
        private EnhancedResponses _responseGenerator;
        private bool _isWaitingForName = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCybersecurityTheme();
            InitializeCommunication();
        }

        private void InitializeCybersecurityTheme()
        {
            // Set window properties for cybersecurity theme
            this.Background = new SolidColorBrush(Color.FromRgb(12, 15, 25)); // Dark navy background
            this.Title = "CyberGuard AI Assistant";

            // Add typing indicator (initially hidden)
            CreateTypingIndicator();
        }

        private void InitializeCommunication()
        {
            UserMemory _memory = new UserMemory();
            // Initialize the response generator (you'll need to create this class)
            _responseGenerator = new EnhancedResponses(_memory);

            // Initialize communication system
            _communication = new Communication(_responseGenerator);

            // Subscribe to communication events
            _communication.OnErrorMessage += HandleErrorMessage;
            _communication.OnBotResponse += HandleBotResponse;
            _communication.OnWelcomeMessage += HandleWelcomeMessage;
            _communication.OnExitMessage += HandleExitMessage;
            _communication.OnHelpMessage += HandleHelpMessage;

            // Show initial welcome message
            AddChatBubble(_communication.GetInitialWelcomeMessage(), false);
        }

        private void CreateTypingIndicator()
        {
            // This method can be used to show when the bot is "thinking"
            // Implementation depends on your XAML structure
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userText = UserInput.Text.Trim();
            if (!string.IsNullOrEmpty(userText))
            {
                AddChatBubble(userText, true);

                // Show typing indicator
                ShowTypingIndicator();

                // Simulate bot thinking delay
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(800);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    HideTypingIndicator();
                    ProcessUserInput(userText);
                };
                timer.Start();

                UserInput.Clear();
                ChatScrollViewer.ScrollToEnd();
            }
        }

        private void ProcessUserInput(string userInput)
        {
            if (_isWaitingForName && !_communication.IsUserNameSet)
            {
                // User is setting their name
                bool nameSet = _communication.SetUserName(userInput);
                if (nameSet)
                {
                    _isWaitingForName = false;
                    // Welcome message will be handled by the event
                }
                // Error message will be handled by the event if name is invalid
            }
            else
            {
                // Process regular chat input
                var response = _communication.ProcessInput(userInput);
                HandleChatResponse(response);
            }
        }

        private void HandleChatResponse(ChatResponse response)
        {
            switch (response.Type)
            {
                case ChatResponseType.Regular:
                    AddChatBubble(response.Message, false);
                    break;
                case ChatResponseType.Help:
                    AddChatBubble($"ℹ️ {response.Message}", false);
                    break;
                case ChatResponseType.Exit:
                    AddChatBubble($"👋 {response.Message}", false);
                    // Optionally close the application or disable input
                    break;
                case ChatResponseType.Error:
                    AddChatBubble($"⚠️ {response.Message}", false);
                    break;
                case ChatResponseType.Welcome:
                    AddChatBubble($"🎉 {response.Message}", false);
                    break;
            }
        }

        // Event handlers for Communication class events
        private void HandleErrorMessage(string message)
        {
            AddChatBubble($"⚠️ {message}", false);
        }

        private void HandleBotResponse(string message)
        {
            AddChatBubble(message, false);
        }

        private void HandleWelcomeMessage(string message)
        {
            AddChatBubble($"🎉 {message}", false);
        }

        private void HandleExitMessage(string message)
        {
            AddChatBubble($"👋 {message}", false);
        }

        private void HandleHelpMessage(string message)
        {
            AddChatBubble($"ℹ️ {message}", false);
        }

        private void AddChatBubble(string message, bool isUser)
        {
            // Create main bubble container
            var bubble = new Border
            {
                Background = isUser ?
                    new LinearGradientBrush(
                        Color.FromRgb(0, 150, 255),   // Bright blue
                        Color.FromRgb(0, 100, 200),   // Darker blue
                        90) :
                    new LinearGradientBrush(
                        Color.FromRgb(30, 35, 45),    // Dark gray
                        Color.FromRgb(20, 25, 35),    // Darker gray
                        90),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 12),
                Margin = new Thickness(isUser ? 50 : 10, 5, isUser ? 10 : 50, 5),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 400,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 3,
                    BlurRadius = 8,
                    Opacity = 0.3
                }
            };

            // Add border glow effect for bot messages
            if (!isUser)
            {
                bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 150)); // Cyan-green
                bubble.BorderThickness = new Thickness(1);

                // Add subtle glow effect
                var glowEffect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Color.FromRgb(0, 255, 150),
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 10,
                    Opacity = 0.4
                };
                bubble.Effect = glowEffect;
            }

            // Create message content
            var messagePanel = new StackPanel();

            // Add sender indicator
            var senderLabel = new TextBlock
            {
                Text = isUser ? "YOU" : "CYBERGUARD AI",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = isUser ?
                    new SolidColorBrush(Color.FromRgb(200, 230, 255)) :
                    new SolidColorBrush(Color.FromRgb(0, 255, 150)),
                Margin = new Thickness(0, 0, 0, 4),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            // Add message text
            var messageText = new TextBlock
                {
                    Text = message,
                Foreground = isUser ?
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)) :
                    new SolidColorBrush(Color.FromRgb(220, 255, 220)),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                FontFamily = new FontFamily("Consolas, Monaco, monospace"), // Monospace font for tech feel
                LineHeight = 18
            };

            // Add timestamp
            var timestamp = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 120)),
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            messagePanel.Children.Add(senderLabel);
            messagePanel.Children.Add(messageText);
            messagePanel.Children.Add(timestamp);

            bubble.Child = messagePanel;

            // Add animation
            bubble.Opacity = 0;
            ChatPanel.Children.Add(bubble);

            // Fade in animation
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            bubble.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            // Auto-scroll to bottom
            ChatScrollViewer.ScrollToEnd();
        }

        private void ShowTypingIndicator()
        {
            var typingBubble = new Border
            {
                Name = "TypingIndicator",
                Background = new SolidColorBrush(Color.FromRgb(30, 35, 45)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 10, 15, 12),
                Margin = new Thickness(10, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 200,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 150)),
                BorderThickness = new Thickness(1)
            };

            var typingText = new TextBlock
            {
                Text = "CyberGuard is analyzing...",
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 150)),
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                FontFamily = new FontFamily("Consolas, Monaco, monospace")
            };

            typingBubble.Child = typingText;
            ChatPanel.Children.Add(typingBubble);

            // Add blinking animation
            var blink = new DoubleAnimation(0.3, 1, TimeSpan.FromMilliseconds(800))
        {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            typingBubble.BeginAnimation(UIElement.OpacityProperty, blink);

            ChatScrollViewer.ScrollToEnd();
        }

        private void HideTypingIndicator()
        {
            // Remove typing indicator
            var typingIndicator = ChatPanel.Children.OfType<Border>()
                .FirstOrDefault(b => b.Name == "TypingIndicator");
            if (typingIndicator != null)
            {
                ChatPanel.Children.Remove(typingIndicator);
    }
}

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
            }
        }
    }
}