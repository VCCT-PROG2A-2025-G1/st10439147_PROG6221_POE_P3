# st10439147_PROG6221_POE_P3
## Cybersecurity-Awareness Chatbot (CyberGuard AI)

### Overview

CyberGuard AI is a WPF-based chatbot application designed to help users learn about cybersecurity topics interactively. It features advanced sentiment analysis, personalized responses, task management with reminders, and a quiz game to reinforce learning. The chatbot adapts to user interests and sentiment, providing a friendly and educational experience.

### Features

- **Sentiment-Aware Chatbot:** Responds to user input with context-aware, empathetic, and educational messages.
- **Task Management:** Create, update, delete, and list tasks using natural language, with support for reminders.
- **Quiz Game:** Start quizzes on general or specific cybersecurity topics, answer questions, and track your score.
- **Activity Log:** View recent activities and chat history directly from the chatbot.
- **Personalization:** Remembers user name, interests, and adapts responses accordingly.
- **Animated UI:** Smooth entrance, button, and text animations for a modern user experience.
- **Robust Error Handling:** Graceful handling of invalid input and application errors.

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio 2019 or later
- .NET Framework 4.8

### Installation

1. Clone or download the repository.
2. Open the solution file (`.sln`) in Visual Studio.
3. Restore NuGet packages if prompted.
4. Build the solution.

### Running the Application

1. Start the application from Visual Studio (`F5` or __Debug > Start Debugging__).
2. The animated welcome screen will appear.
3. Click **Continue** after the greeting to access the main chatbot window.
4. Interact with the chatbot using commands like:
   - `Create task: Update password by tomorrow`
   - `Start quiz`
   - `Show activity`
   - `Help`

### Usage Tips

- Use natural language to interact with the chatbot.
- Type `help` to see available commands and usage instructions.
- View your activity log or chat history with `show activity` or `chat history`.
- The chatbot remembers your name and interests for a personalized experience.

### Known Issues

- User and task data are not persisted between sessions.
- Multi-user support is not available.
- Some advanced analytics are limited.

### Project Structure

- `MyClasses/` - Core logic for chatbot, sentiment analysis, user memory, task management, and quizzes.
- `MainWindow.xaml` / `MainWindow.xaml.cs` - Main chat interface.
- `WelcomePage.xaml` / `WelcomePage.xaml.cs` - Animated welcome screen.
- `NextPage.xaml` / `NextPage.xaml.cs` - Handles navigation after welcome.
- `App.xaml` - Application entry point and resources.

### Youtube Link of code explanation
   https://youtu.be/9v0owElDcAY

### Credits

Developed by: Dillon Rinkwest (st10439147)  
References:  
- ClaudAI from Anthropic https://claude.ai/
- ChatGPT from OpenAI https://chatgpt.com/
- Deepseek AI Model from High-Flyer https://www.deepseek.com/
- Stack Overflow https://stackoverflow.com/
- Pro C# 10 with .NET 6, Foundational Principles and Practices in Programming, Eleventh Edition by Andrew Troelsen and Phil Japiske
