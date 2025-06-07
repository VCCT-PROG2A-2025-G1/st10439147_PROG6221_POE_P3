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

        public Communication(EnhancedResponses responseGenerator, TaskManager taskManager, UserMemory userMemory)
        {
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            _taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            _userMemory = userMemory ?? throw new ArgumentNullException(nameof(userMemory));

            // Subscribe to task manager events
            _taskManager.TaskAdded += OnTaskManagerTaskAdded;
            _taskManager.TaskUpdated += OnTaskManagerTaskUpdated;
            _taskManager.TaskCompleted += OnTaskManagerTaskCompleted;
            _taskManager.ReminderTriggered += OnTaskManagerReminderTriggered;
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

        /// <summary>
        /// Processes user input and returns the appropriate response type
        /// </summary>
        public ChatResponse ProcessInput(string input)
        {
            LastUserInput = input?.Trim() ?? string.Empty;

            // Validate input
            var validation = ValidateInput(LastUserInput);
            if (!validation.IsValid)
            {
                return new ChatResponse(ChatResponseType.Error, validation.ErrorMessage);
            }

            string lowerInput = LastUserInput.ToLower();

            // Handle exit commands
            if (lowerInput == "exit" || lowerInput == "goodbye" || lowerInput == "bye")
            {
                string exitMessage = !string.IsNullOrEmpty(_currentUserName)
                    ? $"Remember to ask me anything as I am always available.\nGoodbye, have a great day {_currentUserName}!"
                    : "Goodbye, have a great day!";

                return new ChatResponse(ChatResponseType.Exit, exitMessage);
            }

            // Handle help command
            if (lowerInput == "help")
            {
                string helpMessage = GetHelpMessage();
                return new ChatResponse(ChatResponseType.Help, helpMessage);
            }

            // Check if input is task-related
            var taskResponse = ProcessTaskCommand(LastUserInput);
            if (taskResponse != null)
            {
                return taskResponse;
            }

            // Generate regular response using the EnhancedResponse class
            try
            {
                string response = _responseGenerator.GetResponse(LastUserInput);
                return new ChatResponse(ChatResponseType.Regular, response);
            }
            catch (Exception ex)
            {
                string errorResponse = $"An error occurred while processing your request: {ex.Message}";
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
        /// Handle task creation from natural language input
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
                        "Example: 'Create task: Update firewall settings by tomorrow'");
                }

                _taskManager.AddTask(
                    taskInfo.Title,
                    taskInfo.Description,
                    taskInfo.DueDate,
                    taskInfo.Priority,
                    taskInfo.Category,
                    taskInfo.ReminderTime
                );

                string response = $"✅ Task created successfully!\n" +
                                $"📋 Title: {taskInfo.Title}\n" +
                                $"📅 Due: {taskInfo.DueDate:MMM dd, yyyy HH:mm}\n" +
                                $"🔥 Priority: {taskInfo.Priority}\n" +
                                $"📂 Category: {taskInfo.Category}";

                if (taskInfo.ReminderTime.HasValue)
                {
                    response += $"\n⏰ Reminder: {taskInfo.ReminderTime.Value.TotalHours} hours before due date";
                }

                // Track in user memory
                _userMemory.AddDiscussedTopic("task_management");
                _userMemory.RecordPositiveTopicInteraction("task_creation");

                return new ChatResponse(ChatResponseType.Task, response);
            }
            catch (Exception ex)
            {
                return new ChatResponse(ChatResponseType.Error, $"Failed to create task: {ex.Message}");
            }
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
                // This is a simplified version - in a real implementation, you'd need
                // a way to identify which specific task to update (by ID, title match, etc.)
                var tasks = _taskManager.GetTasksByStatus(TaskStatus.Pending);

                if (tasks.Count == 0)
                {
                    return new ChatResponse(ChatResponseType.Task,
                        "No pending tasks found to update. Use 'list tasks' to see all tasks.");
                }

                // For demo purposes, let's assume updating the first pending task
                // In a real implementation, you'd parse the input to identify the specific task
                var taskToUpdate = tasks.First();

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
            // This would need more sophisticated parsing to identify which task to delete
            // For now, return a guidance message
            return new ChatResponse(ChatResponseType.Task,
                "To delete a task, please use the task management interface or specify the exact task title. " +
                "For example: 'Delete the password update task'");
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
        /// Extract task description from input
        /// </summary>
        private string ExtractTaskDescription(string input)
        {
            // For now, use the same as title - could be enhanced to look for description keywords
            return $"Task created from: {input}";
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

        private bool ContainsTaskCreationKeywords(string input)
        {
            var keywords = new[] { "create task", "add task", "new task", "task:", "remind me to", "need to", "should" };
            return keywords.Any(keyword => input.Contains(keyword));
        }

        private bool ContainsTaskListKeywords(string input)
        {
            var keywords = new[] { "list tasks", "show tasks", "my tasks", "view tasks", "what tasks", "task list" };
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
        Task
    }
}