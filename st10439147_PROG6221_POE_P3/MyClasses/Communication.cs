using ST10439147_PROG6221_POE_P3.MyClasses;
using st10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ST10439147_PROG6221_POE.MyClasses
{
    /// <summary>
    /// Enhanced WPF-adapted Communication class with integrated task management capabilities
    /// </summary>
    public class Communication
    {
        private readonly EnhancedResponses _responseGenerator;
        private readonly TaskManager _taskManager;
        private readonly UserMemory _userMemory;
        private readonly QuizGameController _quizController;
        private readonly ActivityLog _activityLog;


        private int? _pendingReminderTaskId;
        private bool _awaitingReminderResponse;

        // Username is now passed from external source (NextPage)
        private string _currentUserName;
        public string LastUserInput { get; private set; }

        // Events for UI updates
        public event Action<string> OnErrorMessage;
        public event Action<string> OnBotResponse;
        public event Action<string> OnWelcomeMessage;
        public event Action<string> OnExitMessage;
        public event Action<string> OnHelpMessage;
        public event Action<string> OnTaskResponse;

        // Task-related events
        public event Action<Task> OnTaskCreated;
        public event Action<Task> OnTaskUpdated;
        public event Action<List<Task>> OnTasksListed;

        public Communication(EnhancedResponses responseGenerator, TaskManager taskManager, UserMemory userMemory, QuizGameController quizController, ActivityLog activityLog)
        {
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            _userMemory = userMemory ?? throw new ArgumentNullException(nameof(userMemory));
            _quizController = quizController ?? throw new ArgumentNullException(nameof(quizController));
            _activityLog = activityLog ?? throw new ArgumentNullException(nameof(activityLog));

            // Subscribe to task manager events
            _taskManager.TaskAdded += OnTaskManagerTaskAdded;
            _taskManager.TaskUpdated += OnTaskManagerTaskUpdated;
            _taskManager.TaskCompleted += OnTaskManagerTaskCompleted;
            _taskManager.ReminderTriggered += OnTaskManagerReminderTriggered;

            // Subscribe to quiz events
            _quizController.QuizStarted += OnQuizStarted;
            _quizController.QuestionAsked += OnQuizQuestionAsked;
            _quizController.AnswerProvided += OnQuizAnswerProvided;
            _quizController.QuizCompleted += OnQuizCompleted;
        }

        /// <summary>
        /// Sets the current user name (called from external source like NextPage)
        /// </summary>
        public void SetCurrentUser(string userName)
        {
            _currentUserName = userName;
        }

        /// <summary>
        /// Gets the current user name
        /// </summary>
        public string GetCurrentUserName()
        {
            return _currentUserName ?? string.Empty;
        }

        /// <summary>
        /// Validates user input according to the original Communication class rules
        /// </summary>
        public ValidationResult ValidateInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new ValidationResult(false, "Input cannot be empty. Please try again.");
            }

            if (input.Length > 500)
            {
                return new ValidationResult(false, "Input is too long. Please keep it under 500 characters.");
            }

            return new ValidationResult(true, string.Empty);
        }

        private void OnQuizStarted(object sender, EventArgs e)
        {
            OnBotResponse?.Invoke("🎯 The quiz has started! Get ready!");
            _activityLog.LogQuizActivity("started", null, _currentUserName);
        }

        private void OnQuizQuestionAsked(object sender, QuestionAskedEventArgs e)
        {
            var options = string.Join("\n", e.Question.GetFormattedOptions());
            var questionMessage = $"❓ Question {e.QuestionNumber}/{e.TotalQuestions}:\n{e.Question.Question}\n\n{options}";
            OnBotResponse?.Invoke(questionMessage);
            _activityLog.LogQuizActivity("question_asked", $"Question {e.QuestionNumber}: {e.Question.Question}", _currentUserName);
        }

        private void OnQuizAnswerProvided(object sender, AnswerResultEventArgs e)
        {
            var result = e.IsCorrect ? "✅ Correct!" : $"❌ Incorrect.\n{e.Feedback}";
            OnBotResponse?.Invoke($"{result}\n\nScore: {e.CurrentScore}/{e.TotalQuestions}");
            _activityLog.LogQuizActivity("answer_provided", $"Answer: {(e.IsCorrect ? "Correct" : "Incorrect")}", _currentUserName);
        }

        private void OnQuizCompleted(object sender, QuizCompletedEventArgs e)
        {
            var message = $"🎉 Quiz completed!\nFinal Score: {e.FinalScore}/{e.TotalQuestions} ({e.Percentage:0.0}%)\n{e.PerformanceFeedback}";
            OnBotResponse?.Invoke(message);
            _activityLog.LogQuizActivity("completed", $"Score: {e.FinalScore}/{e.TotalQuestions} ({e.Percentage:0.0}%)", _currentUserName);
        }

        /// <summary>
        /// Modified ProcessInput method to handle reminder confirmation
        /// Add this logic to your existing ProcessInput method after input validation
        /// </summary>
        public ChatResponse ProcessInputWithReminderHandling(string input)
        {
            LastUserInput = input?.Trim() ?? string.Empty;

            // Validate input
            var validation = ValidateInput(LastUserInput);
            if (!validation.IsValid)
            {
                _activityLog.LogChatInteraction(LastUserInput, validation.ErrorMessage, ChatResponseType.Error, _currentUserName);
                return new ChatResponse(ChatResponseType.Error, validation.ErrorMessage);
            }

            // CHECK FOR REMINDER CONFIRMATION FIRST (before other processing)
            if (_awaitingReminderResponse)
            {
                var reminderResponse = HandleReminderConfirmation(LastUserInput);
                if (reminderResponse != null)
                {
                    _activityLog.LogChatInteraction(LastUserInput, reminderResponse.Message, reminderResponse.Type, _currentUserName);
                    return reminderResponse;
                }
            }

            // Continue with your existing ProcessInput logic...
            string lowerInput = LastUserInput.ToLower();

            // Handle exit commands
            if (lowerInput == "exit" || lowerInput == "goodbye" || lowerInput == "bye")
            {
                // Reset any pending reminder state
                _awaitingReminderResponse = true;
                _pendingReminderTaskId = null;

                string exitMessage = !string.IsNullOrEmpty(_currentUserName)
                    ? $"Remember to ask me anything as I am always available.\nGoodbye, have a great day {_currentUserName}!"
                    : "Goodbye, have a great day!";

                _activityLog.LogChatInteraction(LastUserInput, exitMessage, ChatResponseType.Exit, _currentUserName);
                return new ChatResponse(ChatResponseType.Exit, exitMessage);
            }

            // Handle quiz commands
            if (lowerInput.StartsWith("start quiz"))
            {
                try
                {
                    _quizController.StartQuiz();
                    var response = "Starting general quiz...";
                    _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, _currentUserName);
                    return new ChatResponse(ChatResponseType.Quiz, response);
                }
                catch (Exception ex)
                {
                    var errorResponse = $"Failed to start quiz: {ex.Message}";
                    _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                    return new ChatResponse(ChatResponseType.Error, errorResponse);
                }
            }
            else if (lowerInput.StartsWith("quiz on "))
            {
                try
                {
                    var topic = input.Substring("quiz on ".Length).Trim();
                    _quizController.StartTopicQuiz(topic);
                    var response = $"Starting quiz on {topic}...";
                    _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, _currentUserName);
                    return new ChatResponse(ChatResponseType.Quiz, response);
                }
                catch (Exception ex)
                {
                    var errorResponse = $"Failed to start topic quiz: {ex.Message}";
                    _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                    return new ChatResponse(ChatResponseType.Error, errorResponse);
                }
            }

            // Check if user input is a quiz answer like "A", "B", "C"
            if (_quizController.IsQuizInProgress && input.Length == 1)
            {
                int? selectedIndex = GetAnswerIndexFromLetter(input);
                if (selectedIndex.HasValue)
                {
                    try
                    {
                        _quizController.SubmitAnswer(selectedIndex.Value);
                        var response = "✅ Answer submitted.";
                        _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, _currentUserName);
                        return new ChatResponse(ChatResponseType.Quiz, response);
                    }
                    catch (Exception ex)
                    {
                        var errorResponse = $"Failed to submit answer: {ex.Message}";
                        _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                        return new ChatResponse(ChatResponseType.Error, errorResponse);
                    }
                }
            }

            // Quit quiz
            if (lowerInput == "quit quiz" || lowerInput == "stop quiz")
            {
                if (_quizController.IsQuizInProgress)
                {
                    // 🔹 Fetch quiz results before resetting
                    int correctAnswers = _quizController.CurrentScore;
                    int totalQuestions = _quizController.TotalQuestions;
                    double percentage = totalQuestions > 0 ? (correctAnswers / (double)totalQuestions) * 100 : 0;

                    // 🧠 Record in user memory if needed
                    _userMemory.RecordQuizResult(correctAnswers, totalQuestions, percentage);

                    // 📝 Format score message
                    string response = $"🛑 Quiz has been stopped.\n" +
                                      $"📊 Score: {correctAnswers}/{totalQuestions} ({percentage:0.0}%)";

                    // 📓 Log the score as a quiz activity
                    _activityLog.LogQuizActivity("stopped", $"Score: {correctAnswers}/{totalQuestions} ({percentage:0.0}%)", this.GetCurrentUserName());
                    _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, this.GetCurrentUserName());

                    // ❌ Reset the quiz after saving data
                    _quizController.ResetQuiz();
                    return new ChatResponse(ChatResponseType.Quiz, response);
                }
                else
                {
                    var response = "No quiz is currently in progress.";
                    _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Regular, this.GetCurrentUserName());
                    return new ChatResponse(ChatResponseType.Regular, response);
                }
            }

            // Restart quiz
            if (lowerInput == "restart quiz")
            {
                try
                {
                    if (_quizController.IsQuizInProgress)
                    {
                        _quizController.ResetQuiz();
                        _quizController.StartQuiz();
                        var response = "🔄 Quiz restarted.";
                        _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, _currentUserName);
                        return new ChatResponse(ChatResponseType.Quiz, response);
                    }
                    else
                    {
                        _quizController.StartQuiz();
                        var response = "🆕 Starting a new quiz.";
                        _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Quiz, _currentUserName);
                        return new ChatResponse(ChatResponseType.Quiz, response);
                    }
                }
                catch (Exception ex)
                {
                    var errorResponse = $"Failed to restart quiz: {ex.Message}";
                    _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                    return new ChatResponse(ChatResponseType.Error, errorResponse);
                }
            }

            // Handle quiz history command
            /* if (lowerInput == "quiz history")
             {
                 try
                 {
                     var stats = _userMemory.GetQuizStats();
                     string response = $"📚 QUIZ HISTORY:\n" +
                                       $"• Quizzes Taken: {stats["quizzes_taken"]}\n" +
                                       $"• Questions Answered: {stats["questions_answered"]}\n" +
                                       $"• Correct Answers: {stats["correct_answers"]}\n" +
                                       $"• Average Score: {stats["average_percentage"]:0.0}%";

                     _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Regular, _currentUserName);
                     return new ChatResponse(ChatResponseType.Regular, response);
                 }
                 catch (Exception ex)
                 {
                     var errorResponse = $"Failed to retrieve quiz history: {ex.Message}";
                     _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                     return new ChatResponse(ChatResponseType.Error, errorResponse);
                 }
             }*/

            // Handle activity log commands
            if (lowerInput == "show activity" || lowerInput == "activity log" || lowerInput == "What have you done for me?”")
            {
                var response = _activityLog.GetConciseActivitySummary(5); // Updated method call
                _activityLog.LogChatInteraction(LastUserInput, "User-friendly activity summary displayed", ChatResponseType.Regular, _currentUserName);
                return new ChatResponse(ChatResponseType.Regular, response);
            }

            if (lowerInput == "show chat history" || lowerInput == "chat history")
            {
                var response = _activityLog.GetFormattedChatHistory();
                _activityLog.LogChatInteraction(LastUserInput, "Chat history displayed", ChatResponseType.Regular, _currentUserName);
                return new ChatResponse(ChatResponseType.Regular, response);
            }

            // Handle help command
            if (lowerInput == "help")
            {
                string helpMessage = GetHelpMessage();
                _activityLog.LogChatInteraction(LastUserInput, helpMessage, ChatResponseType.Help, _currentUserName);
                return new ChatResponse(ChatResponseType.Help, helpMessage);
            }

            // Check if input is task-related
            var taskResponse = ProcessTaskCommand(LastUserInput);
            if (taskResponse != null)
            {
                _activityLog.LogChatInteraction(LastUserInput, taskResponse.Message, ChatResponseType.Task, _currentUserName);
                return taskResponse;
            }

            // Generate regular response using the EnhancedResponse class
            try
            {
                string response = _responseGenerator.GetResponse(LastUserInput);
                _activityLog.LogChatInteraction(LastUserInput, response, ChatResponseType.Regular, _currentUserName);
                return new ChatResponse(ChatResponseType.Regular, response);
            }
            catch (Exception ex)
            {
                string errorResponse = $"An error occurred while processing your request: {ex.Message}";
                _activityLog.LogChatInteraction(LastUserInput, errorResponse, ChatResponseType.Error, _currentUserName);
                return new ChatResponse(ChatResponseType.Error, errorResponse);
            }
        }

        /// <summary>
        /// Process task-related commands
        /// </summary>
        private ChatResponse ProcessTaskCommand(string input)
        {
            string lowerInput = input.ToLower();

            try
            {
                // Add task commands
                if (ContainsTaskCreationKeywords(lowerInput))
                {
                    return HandleTaskCreation(input);
                }

                // List tasks commands
                if (ContainsTaskListKeywords(lowerInput))
                {
                    return HandleTaskListing(lowerInput);
                }

                // Update task status commands
                if (ContainsTaskUpdateKeywords(lowerInput))
                {
                    return HandleTaskStatusUpdate(input);
                }

                // Delete task commands
                if (ContainsTaskDeleteKeywords(lowerInput))
                {
                    return HandleTaskDeletion(input);
                }

                // Cybersecurity task shortcuts
                if (ContainsCybersecurityTaskKeywords(lowerInput))
                {
                    return HandleCybersecurityTaskCreation(lowerInput);
                }

                return null; // Not a task command
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Task command error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle task creation from natural language input with interactive reminder confirmation
        /// </summary>
        private ChatResponse HandleTaskCreation(string input)
        {
            try
            {
                var taskInfo = ParseTaskFromInput(input);

                if (string.IsNullOrEmpty(taskInfo.Title))
                {
                    return new ChatResponse(ChatResponseType.Task,
                        "I couldn't identify the task title. Please specify what task you'd like to create. " +
                        "Example: 'Add task - Review privacy settings'");
                }

                // Create the task first
                _taskManager.AddTask(
                    taskInfo.Title,
                    taskInfo.Description,
                    taskInfo.DueDate,
                    taskInfo.Priority,
                    taskInfo.Category,
                    null // Don't set reminder yet
                );

                // Format the initial response like the example
                string response = $"Task added with the description \"{taskInfo.Description}\" Would you like a reminder?";

                // Track in user memory
                _userMemory.AddDiscussedTopic("task_management");
                _userMemory.RecordPositiveTopicInteraction("task_creation");

                // Store the task ID for potential reminder setup
                _pendingReminderTaskId = int.TryParse(_taskManager.GetAllTasks().LastOrDefault()?.Id, out var taskId) ? taskId : (int?)null;
                _awaitingReminderResponse = true;

                return new ChatResponse(ChatResponseType.Task, response);
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to create task: {ex.Message}");
            }
        }


        /// <summary>
        /// Handle reminder confirmation responses
        /// </summary>
        private ChatResponse HandleReminderConfirmation(string input)
        {
            if (!_awaitingReminderResponse || !_pendingReminderTaskId.HasValue)
            {
                return null; // Not waiting for reminder response
            }

            string lowerInput = input.ToLower().Trim();

            // Check if user wants a reminder
            if (lowerInput.StartsWith("yes") || lowerInput.Contains("remind me") || lowerInput.Contains("in ") || lowerInput.Contains("reminder"))
            {
                // Parse the reminder time from input
                TimeSpan? reminderTime = ParseReminderTimeFromConfirmation(input);

                if (reminderTime.HasValue)
                {
                    // Update the task with reminder
                    var task = _taskManager.GetAllTasks().FirstOrDefault(t => t.Id == _pendingReminderTaskId.Value.ToString());
                    if (task != null)
                    {
                        _taskManager.UpdateTaskReminder(_pendingReminderTaskId.Value.ToString(), reminderTime.Value);

                        // Reset pending state
                        _awaitingReminderResponse = false;
                        _pendingReminderTaskId = null;

                        string reminderText = FormatReminderTime(reminderTime.Value);
                        return new ChatResponse(ChatResponseType.Task, $"Got it! I'll remind you {reminderText}.");
                    }
                }
                else
                {
                    return new ChatResponse(ChatResponseType.Task,
                        "Please specify when you'd like to be reminded. For example: 'Yes, remind me in 3 days' or 'Yes, remind me tomorrow'");
                }
            }
            else if (lowerInput == "no" || lowerInput == "no thanks" || lowerInput.Contains("don't need"))
            {
                // User doesn't want a reminder
                _awaitingReminderResponse = false;
                _pendingReminderTaskId = null;

                return new ChatResponse(ChatResponseType.Task, "No problem! Task created without a reminder.");
            }

            return null; // Didn't match reminder confirmation pattern
        }


        /// <summary>
        /// Parse reminder time from user's confirmation response
        /// </summary>
        private TimeSpan? ParseReminderTimeFromConfirmation(string input)
        {
            var lowerInput = input.ToLower();

            // Handle "in X days" pattern
            var dayMatch = Regex.Match(lowerInput, @"in\s+(\d+(?:\.\d+)?)\s+days?");
            if (dayMatch.Success)
            {
                double days = double.Parse(dayMatch.Groups[1].Value);
                return TimeSpan.FromDays(days);
            }

            // Handle "in X hours"
            var hourMatch = Regex.Match(lowerInput, @"in\s+(\d+(?:\.\d+)?)\s+hours?");
            if (hourMatch.Success)
            {
                double hours = double.Parse(hourMatch.Groups[1].Value);
                return TimeSpan.FromHours(hours);
            }

            // Handle "tomorrow", "today"
            if (lowerInput.Contains("tomorrow"))
                return TimeSpan.FromDays(1);
            if (lowerInput.Contains("today"))
                return TimeSpan.FromHours(1);

            return null;
        }

        /// <summary>
        /// Format reminder time for display
        /// </summary>
        private string FormatReminderTime(TimeSpan reminderTime)
        {
            if (reminderTime.TotalDays >= 1)
            {
                int days = (int)reminderTime.TotalDays;
                return $"in {days} day{(days > 1 ? "s" : "")}";
            }
            else if (reminderTime.TotalHours >= 1)
            {
                int hours = (int)reminderTime.TotalHours;
                return $"in {hours} hour{(hours > 1 ? "s" : "")}";
            }
            else
            {
                return "shortly";
            }
        }

        /// <summary>
        /// Enhanced task title extraction to handle "Add task - Title" format
        /// </summary>
        private string ExtractTaskTitleFromInput(string input)
        {
            // Look for "Add task - Title" pattern first
            var dashPattern = @"(?:add|create|new)\s+task\s*-\s*(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)";
            var match = Regex.Match(input, dashPattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value.Trim();
            }

            // Original patterns as fallback
            var patterns = new[]
            {
                @"(?:create|add|new)\s+task:?\s*(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)",
                @"task:?\s*(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)",
                @"(?:remind me to|need to|should)\s+(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)"
            };

            foreach (var pattern in patterns)
            {
                match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Handle task listing requests
        /// </summary>
        private ChatResponse HandleTaskListing(string input)
        {
            try
            {
                List<Task> tasks;
                string listType = "all";

                if (input.Contains("overdue"))
                {
                    tasks = _taskManager.GetOverdueTasks();
                    listType = "overdue";
                }
                else if (input.Contains("upcoming"))
                {
                    tasks = _taskManager.GetUpcomingTasks();
                    listType = "upcoming";
                }
                else if (input.Contains("completed"))
                {
                    tasks = _taskManager.GetTasksByStatus(TaskStatus.Completed);
                    listType = "completed";
                }
                else if (input.Contains("pending"))
                {
                    tasks = _taskManager.GetTasksByStatus(TaskStatus.Pending);
                    listType = "pending";
                }
                else if (input.Contains("high priority") || input.Contains("urgent"))
                {
                    tasks = _taskManager.GetTasksByPriority(TaskPriority.High);
                    listType = "high priority";
                }
                else if (input.Contains("critical"))
                {
                    tasks = _taskManager.GetTasksByPriority(TaskPriority.Critical);
                    listType = "critical";
                }
                else
                {
                    tasks = _taskManager.GetAllTasks();
                }

                OnTasksListed?.Invoke(tasks);

                string response = FormatTaskList(tasks, listType);

                // Track in user memory
                _userMemory.AddDiscussedTopic("task_viewing");
                _userMemory.RecordPositiveTopicInteraction("task_management");

                return new ChatResponse(ChatResponseType.Task, response);
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to list tasks: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle task status updates
        /// </summary>
        private ChatResponse HandleTaskStatusUpdate(string input)
        {
            try
            {
                // Try to identify the task by title in the input
                var allTasks = _taskManager.GetAllTasks();
                var taskToUpdate = allTasks
                    .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Title) && input.ToLower().Contains(t.Title.ToLower()));

                if (taskToUpdate == null)
                {
                    // Fallback: update first pending task if no title match
                    var pendingTasks = _taskManager.GetTasksByStatus(TaskStatus.Pending);
                    if (pendingTasks.Count == 0)
                    {
                        return new ChatResponse(ChatResponseType.Task,
                            "No pending tasks found to update. Use 'list tasks' to see all tasks.");
                    }
                    taskToUpdate = pendingTasks.First();
                }

                TaskStatus newStatus = TaskStatus.InProgress;
                if (input.Contains("complete") || input.Contains("done") || input.Contains("finish"))
                {
                    newStatus = TaskStatus.Completed;
                }
                else if (input.Contains("cancel"))
                {
                    newStatus = TaskStatus.Cancelled;
                }

                _taskManager.UpdateTaskStatus(taskToUpdate.Id, newStatus);

                string response = $"✅ Task updated successfully!\n" +
                                $"📋 Task: {taskToUpdate.Title}\n" +
                                $"🔄 Status: {newStatus}";

                return new ChatResponse(ChatResponseType.Task, response);
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to update task: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle cybersecurity task creation shortcuts
        /// </summary>
        private ChatResponse HandleCybersecurityTaskCreation(string input)
        {
            try
            {
                var dueDate = DateTime.Now.AddDays(7); // Default to 1 week
                CybersecurityTaskType taskType = CybersecurityTaskType.SecurityAudit;

                if (input.Contains("password"))
                    taskType = CybersecurityTaskType.PasswordUpdate;
                else if (input.Contains("software") || input.Contains("update"))
                    taskType = CybersecurityTaskType.SoftwareUpdate;
                else if (input.Contains("backup"))
                    taskType = CybersecurityTaskType.BackupVerification;
                else if (input.Contains("two factor") || input.Contains("2fa"))
                    taskType = CybersecurityTaskType.TwoFactorSetup;

                // Try to parse due date from input
                var parsedDate = ParseDateFromInput(input);
                if (parsedDate.HasValue)
                    dueDate = parsedDate.Value;

                _taskManager.AddCybersecurityTask(taskType, dueDate);

                string response = $"🔒 Cybersecurity task created!\n" +
                                $"📋 Type: {taskType}\n" +
                                $"📅 Due: {dueDate:MMM dd, yyyy}\n" +
                                $"⚠️ This is a security-critical task with automatic reminders.";

                // Track in user memory
                _userMemory.AddDiscussedTopic("cybersecurity_tasks");
                _userMemory.RecordPositiveTopicInteraction("security_planning");

                return new ChatResponse(ChatResponseType.Task, response);
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to create cybersecurity task: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle task deletion
        /// </summary>
        private ChatResponse HandleTaskDeletion(string input)
        {
            try
            {
                // Try to identify the task by title in the input
                var allTasks = _taskManager.GetAllTasks();
                var taskToDelete = allTasks
                    .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Title) && input.ToLower().Contains(t.Title.ToLower()));

                if (taskToDelete == null)
                {
                    return new ChatResponse(ChatResponseType.Task,
                        "Could not identify the task to delete. Please specify the exact task title. For example: 'Delete the password update task'");
                }

                bool deleted = _taskManager.DeleteTask(taskToDelete.Id);
                if (deleted)
                {
                    return new ChatResponse(ChatResponseType.Task, $"🗑️ Task '{taskToDelete.Title}' deleted successfully.");
                }
                else
                {
                    return new ChatResponse(ChatResponseType.Error, $"Failed to delete task '{taskToDelete.Title}'.");
                }
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to delete task: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse task information from natural language input
        /// </summary>
        private TaskInfo ParseTaskFromInput(string input)
        {
            var taskInfo = new TaskInfo
            {
                Title = ExtractTaskTitle(input),
                Description = ExtractTaskDescription(input),
                DueDate = ParseDateFromInput(input) ?? DateTime.Now.AddDays(1),
                Priority = ParsePriorityFromInput(input),
                Category = ParseCategoryFromInput(input),
                ReminderTime = ParseReminderFromInput(input)
            };

            return taskInfo;
        }

        /// <summary>
        /// Extract task title from input
        /// </summary>
        private string ExtractTaskTitle(string input)
        {
            // Look for patterns like "create task:", "add task:", "task:", etc.
            var patterns = new[]
            {
                @"(?:create|add|new)\s+task:?\s*(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)",
                @"task:?\s*(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)",
                @"(?:remind me to|need to|should)\s+(.+?)(?:\s+by\s+|\s+due\s+|\s+priority\s+|$)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Enhanced task description to create a more natural description
        /// </summary>
        private string ExtractTaskDescription(string input)
        {
            var title = ExtractTaskTitle(input);
            if (string.IsNullOrWhiteSpace(title))
                return $"Task created from: {input}";

            string lowerTitle = title.ToLower();

            if (lowerTitle.Contains("encryption"))
                return "Ensure sensitive data is encrypted both at rest and in transit.";

            if (lowerTitle.Contains("malware"))
                return "Run a malware scan and update your anti-malware definitions.";

            if (lowerTitle.Contains("patch"))
                return "Apply the latest security patches to all systems and applications.";

            if (lowerTitle.Contains("vpn"))
                return "Use a VPN when accessing public Wi-Fi to protect your data.";

            if (lowerTitle.Contains("breach"))
                return "Investigate potential data breaches and update credentials if necessary.";

            if (lowerTitle.Contains("compliance"))
                return "Review compliance requirements and ensure all policies are up to date.";

            if (lowerTitle.Contains("training"))
                return "Schedule cybersecurity awareness training for all staff members.";

            if (lowerTitle.Contains("incident"))
                return "Prepare an incident response plan to handle security events effectively.";
            // Use keyword-based templates
            if (lowerTitle.Contains("privacy"))
                return "Review account privacy settings to ensure your data is protected.";

            if (lowerTitle.Contains("password"))
                return "Update passwords to maintain strong security across accounts.";

            if (lowerTitle.Contains("backup"))
                return "Verify backups to ensure important data is safely stored.";

            if (lowerTitle.Contains("software") || lowerTitle.Contains("update"))
                return "Check for and install any critical software updates.";

            if (lowerTitle.Contains("2fa") || lowerTitle.Contains("two-factor"))
                return "Enable two-factor authentication for stronger account security.";

            if (lowerTitle.Contains("audit"))
                return "Conduct a security audit to evaluate and improve system defenses.";

            if (lowerTitle.Contains("scan") || lowerTitle.Contains("virus"))
                return "Perform a full antivirus scan to detect and remove threats.";

            // Default fallback
            return $"Task: {title.Trim()}. Be sure to complete it on time.";
        }



        /// <summary>
        /// Parse due date from input
        /// </summary>
        private DateTime? ParseDateFromInput(string input)
        {
            var lowerInput = input.ToLower();

            // Handle relative dates
            if (lowerInput.Contains("today"))
                return DateTime.Today;
            if (lowerInput.Contains("tomorrow"))
                return DateTime.Today.AddDays(1);
            if (lowerInput.Contains("next week"))
                return DateTime.Today.AddDays(7);
            if (lowerInput.Contains("next month"))
                return DateTime.Today.AddMonths(1);

            // Handle "in X days/weeks" patterns
            var match = Regex.Match(lowerInput, @"in\s+(\d+)\s+(day|week|month)s?");
            if (match.Success)
            {
                int amount = int.Parse(match.Groups[1].Value);
                string unit = match.Groups[2].Value;

                switch (unit)
                {
                    case "day":
                        return DateTime.Today.AddDays(amount);
                    case "week":
                        return DateTime.Today.AddDays(amount * 7);
                    case "month":
                        return DateTime.Today.AddMonths(amount);
                }
            }

            // Try to parse specific dates
            var datePatterns = new[]
            {
                @"by\s+(\d{1,2}/\d{1,2}/\d{4})",
                @"due\s+(\d{1,2}/\d{1,2}/\d{4})",
                @"on\s+(\d{1,2}/\d{1,2}/\d{4})"
            };

            foreach (var pattern in datePatterns)
            {
                var dateMatch = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out DateTime parsedDate))
                {
                    return parsedDate;
                }
            }

            return null;
        }

        /// <summary>
        /// Parse priority from input
        /// </summary>
        private TaskPriority ParsePriorityFromInput(string input)
        {
            var lowerInput = input.ToLower();

            if (lowerInput.Contains("critical") || lowerInput.Contains("urgent"))
                return TaskPriority.Critical;
            if (lowerInput.Contains("high priority") || lowerInput.Contains("important"))
                return TaskPriority.High;
            if (lowerInput.Contains("low priority"))
                return TaskPriority.Low;

            return TaskPriority.Medium; // Default
        }

        /// <summary>
        /// Parse category from input
        /// </summary>
        private string ParseCategoryFromInput(string input)
        {
            var lowerInput = input.ToLower();

            if (lowerInput.Contains("security") || lowerInput.Contains("cyber"))
                return "Cybersecurity";
            if (lowerInput.Contains("password"))
                return "Security";
            if (lowerInput.Contains("backup"))
                return "Data Management";
            if (lowerInput.Contains("update"))
                return "Maintenance";

            return "General";
        }

        /// <summary>
        /// Parse reminder time from input
        /// </summary>
        private TimeSpan? ParseReminderFromInput(string input)
        {
            var lowerInput = input.ToLower();

            if (lowerInput.Contains("remind me"))
            {
                var match = Regex.Match(lowerInput, @"(\d+)\s+(hour|day)s?\s+before");
                if (match.Success)
                {
                    int amount = int.Parse(match.Groups[1].Value);
                    string unit = match.Groups[2].Value;

                    return unit == "hour" ? TimeSpan.FromHours(amount) : TimeSpan.FromDays(amount);
                }

                return TimeSpan.FromHours(1); // Default reminder
            }

            return null;
        }

        /// <summary>
        /// Format task list for display
        /// </summary>
        private string FormatTaskList(List<Task> tasks, string listType)
        {
            if (tasks.Count == 0)
            {
                return $"📋 No {listType} tasks found.";
            }

            var response = $"📋 {listType.ToUpper()} TASKS ({tasks.Count}):\n\n";

            for (int i = 0; i < Math.Min(tasks.Count, 10); i++)
            {
                var task = tasks[i];
                string status = GetTaskStatusEmoji(task);
                string priority = GetPriorityEmoji(task.Priority);
                string dueDateInfo = GetDueDateInfo(task);

                response += $"{status} {priority} {task.Title}\n";
                response += $"   📅 {dueDateInfo}\n";
                response += $"   📂 {task.Category}\n\n";
            }

            if (tasks.Count > 10)
            {
                response += $"... and {tasks.Count - 10} more tasks.";
            }

            return response;
        }

        /// <summary>
        /// Get status emoji for task
        /// </summary>
        private string GetTaskStatusEmoji(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Completed:
                    return "✅";
                case TaskStatus.InProgress:
                    return "🔄";
                case TaskStatus.Cancelled:
                    return "❌";
                default:
                    return task.IsOverdue ? "⚠️" : "⭕";
            }
        }

        /// <summary>
        /// Get priority emoji
        /// </summary>
        private string GetPriorityEmoji(TaskPriority priority)
        {
            switch (priority)
            {
                case TaskPriority.Critical:
                    return "🔴";
                case TaskPriority.High:
                    return "🟠";
                case TaskPriority.Medium:
                    return "🟡";
                case TaskPriority.Low:
                    return "🟢";
                default:
                    return "⚪";
            }
        }

        /// <summary>
        /// Get formatted due date information
        /// </summary>
        private string GetDueDateInfo(Task task)
        {
            var timeUntilDue = task.DueDate - DateTime.Now;

            if (task.IsOverdue)
            {
                var overdueDays = (int)Math.Abs(timeUntilDue.TotalDays);
                return $"Due: {task.DueDate:MMM dd} (Overdue by {overdueDays} days)";
            }
            else if (task.IsUpcoming)
            {
                var daysUntilDue = (int)timeUntilDue.TotalDays;
                return $"Due: {task.DueDate:MMM dd} (In {daysUntilDue} days)";
            }
            else
            {
                return $"Due: {task.DueDate:MMM dd, yyyy}";
            }
        }

        #region Keyword Detection Methods


        private int? GetAnswerIndexFromLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            char letter = char.ToUpper(input.Trim()[0]);
            if (letter >= 'A' && letter <= 'Z')
            {
                return letter - 'A';
            }

            return null;
        }

        private bool ContainsTaskCreationKeywords(string input)
        {
            var keywords = new[] { "create task", "add task", "new task", "task:", "remind me to", "need to", "should", "add a task", "reminder" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsTaskListKeywords(string input)
        {
            var keywords = new[] { "list tasks", "show tasks", "show task", "my tasks", "view tasks", "what tasks", "task list", "What have you done for me" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsTaskUpdateKeywords(string input)
        {
            var keywords = new[] { "complete task", "finish task", "done with", "mark as complete", "update task", "task done" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsTaskDeleteKeywords(string input)
        {
            var keywords = new[] { "delete task", "remove task", "cancel task" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsCybersecurityTaskKeywords(string input)
        {
            var keywords = new[] { "security task", "password update", "backup check", "security audit", "2fa setup", "cyber task" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        #endregion

        #region Event Handlers

        private void OnTaskManagerTaskAdded(object sender, TaskEventArgs e)
        {
            OnTaskCreated?.Invoke(e.Task);
        }

        private void OnTaskManagerTaskUpdated(object sender, TaskEventArgs e)
        {
            OnTaskUpdated?.Invoke(e.Task);
        }

        private void OnTaskManagerTaskCompleted(object sender, TaskEventArgs e)
        {
            // Handle task completion - could show celebration message
            OnBotResponse?.Invoke($"🎉 Congratulations! You completed the task: {e.Task.Title}");
        }

        private void OnTaskManagerReminderTriggered(object sender, TaskReminderEventArgs e)
        {
            // Handle reminder - show reminder message
            OnBotResponse?.Invoke($"⏰ REMINDER: {e.Message}");
        }

        #endregion

        /// <summary>
        /// Gets an enhanced help message including task commands
        /// </summary>
        private string GetHelpMessage()
        {
            return "🤖 CyberGuard AI - Available Commands:\n\n" +
                   "💬 CHAT COMMANDS:\n" +
                   "• Ask about cybersecurity topics\n" +
                   "• Request 'cybersecurity guide' or 'cybersecurity terms'\n" +
                   "• Type 'exit' or 'goodbye' to quit\n\n" +
                   "📋 TASK COMMANDS:\n" +
                   "• 'Create task: [task name] by [date]' - Create new task\n" +
                   "• 'List tasks' - Show all tasks\n" +
                   "• 'Show overdue tasks' - Display overdue items\n" +
                   "• 'Show upcoming tasks' - Display upcoming items\n" +
                   "• 'Security task: password update' - Quick security tasks\n" +
                   "• 'Complete task: [task name]' - Mark task as done\n\n" +
                   "⚡ QUICK SECURITY TASKS:\n" +
                   "• Password update, Software update, Backup check\n" +
                   "• Security audit, 2FA setup\n\n" +
                   "📅 DATE FORMATS:\n" +
                   "• 'today', 'tomorrow', 'next week'\n" +
                   "• 'in 3 days', 'by 12/25/2024'\n" +
                   "• 'high priority', 'critical', 'remind me 2 hours before'";
        }

        /// <summary>
        /// Gets a welcome message for the current user
        /// </summary>
        public string GetWelcomeMessage()
        {
            if (!string.IsNullOrEmpty(_currentUserName))
            {
                return $"Hello {_currentUserName}, how may I help you today? " +
                       "I can help with cybersecurity questions and manage your security tasks. " +
                       "Type 'help' to see all available commands or 'exit' to quit.";
            }
            else
            {
                return "Welcome to CyberGuard AI! I can help with cybersecurity guidance and task management. " +
                       "Type 'help' to see what I can do or 'exit' to quit.";
            }
        }

        /// <summary>
        /// Get task statistics for user
        /// </summary>
        public string GetTaskStatistics()
        {
            try
            {
                var allTasks = _taskManager.GetAllTasks();
                var pendingTasks = _taskManager.GetTasksByStatus(TaskStatus.Pending);
                var completedTasks = _taskManager.GetTasksByStatus(TaskStatus.Completed);
                var overdueTasks = _taskManager.GetOverdueTasks();

                return $"📊 TASK STATISTICS:\n" +
                       $"📝 Total Tasks: {allTasks.Count}\n" +
                       $"⏳ Pending: {pendingTasks.Count}\n" +
                       $"✅ Completed: {completedTasks.Count}\n" +
                       $"⚠️ Overdue: {overdueTasks.Count}";
            }
            catch (Exception ex)
            {
                return $"Error retrieving task statistics: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Helper class for parsing task information
    /// </summary>
    public class TaskInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public string Category { get; set; } = "General";
        public TimeSpan? ReminderTime { get; set; }
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


    /// <summary>
    /// Represents a chat response with type information
    /// </summary>
    public class ChatResponse
    {
        public ChatResponseType Type { get; }
        public string Message { get; }

        public ChatResponse(ChatResponseType type, string message)
        {
            Type = type;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Enum for different types of chat responses
    /// </summary>
    public enum ChatResponseType
    {
        Regular,
        Help,
        Exit,
        Error,
        Welcome,
        Task,
        Quiz
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//