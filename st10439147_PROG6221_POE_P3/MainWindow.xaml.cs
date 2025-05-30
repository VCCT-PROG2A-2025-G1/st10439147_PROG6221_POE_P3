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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userText = UserInput.Text.Trim();
            if (!string.IsNullOrEmpty(userText))
            {
                AddChatBubble(userText, true);
                string botReply = GetBotResponse(userText);
                AddChatBubble(botReply, false);
                UserInput.Clear();
                ChatScrollViewer.ScrollToEnd();
            }
        }

        private void AddChatBubble(string message, bool isUser)
        {
            var bubble = new Border
            {
                Background = isUser ? new SolidColorBrush(Color.FromRgb(0, 255, 247)) : new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 300,
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = isUser ? Brushes.Black : Brushes.LimeGreen,
                    TextWrapping = TextWrapping.Wrap
                }
            };

            ChatPanel.Children.Add(bubble);
        }

        private string GetBotResponse(string userInput)
        {
            // Replace this with your actual logic or API call
            return $"Security tip: Always verify email sources before clicking.";
        }
    }
}

