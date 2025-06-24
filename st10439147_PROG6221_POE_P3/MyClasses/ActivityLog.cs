using System;
using System.Collections.Generic;
using System.Linq;
using ST10439147_PROG6221_POE_P3.MyClasses;
using st10439147_PROG6221_POE_P3.MyClasses;

namespace ST10439147_PROG6221_POE.MyClasses
{
    /// <summary>
    /// Handles activity logging, chat history, and user interaction tracking
    /// </summary>
    public class ActivityLog
    {
        private readonly List<ActivityEntry> _activities;
        private readonly List<ChatEntry> _chatHistory;
        private readonly object _lockObject = new object();

        // Constants for activity tracking
        private const int DEFAULT_DISPLAY_COUNT = 10;
        private const int MAX_ACTIVITY_ENTRIES = 1000;
        private const int MAX_CHAT_ENTRIES = 500;

        // Events for real-time updates
        public event Action<ActivityEntry> OnActivityAdded;
        public event Action<ChatEntry> OnChatEntryAdded;
        public event Action<List<ActivityEntry>> OnActivitiesUpdated;

        public ActivityLog()
        {
            _activities = new List<ActivityEntry>();
            _chatHistory = new List<ChatEntry>();
        }

        #region Chat History Management

        /// <summary>
        /// Logs a chat interaction (user input and bot response)
        /// </summary>
        public void LogChatInteraction(string userInput, string botResponse, ChatResponseType responseType, string userName = null)
        {
            lock (_lockObject)
            {
                var chatEntry = new ChatEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    UserName = userName ?? "Anonymous",
                    UserInput = userInput?.Trim() ?? string.Empty,
                    BotResponse = botResponse?.Trim() ?? string.Empty,
                    ResponseType = responseType,
                    Keywords = ExtractKeywords(userInput)
                };

                _chatHistory.Add(chatEntry);

                // Maintain maximum entries
                if (_chatHistory.Count > MAX_CHAT_ENTRIES)
                {
                    _chatHistory.RemoveAt(0);
                }

                // Also log as activity
                LogActivity(ActivityType.ChatInteraction,
                    $"Chat: {GetTruncatedText(userInput, 50)}",
                    userName);

                OnChatEntryAdded?.Invoke(chatEntry);
            }
        }

        /// <summary>
        /// Gets chat history with optional filtering
        /// </summary>
        public List<ChatEntry> GetChatHistory(int count = DEFAULT_DISPLAY_COUNT, string userName = null, DateTime? fromDate = null)
        {
            lock (_lockObject)
            {
                var query = _chatHistory.AsQueryable();

                if (!string.IsNullOrEmpty(userName))
                {
                    query = query.Where(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.Timestamp >= fromDate.Value);
                }

                return query
                    .OrderByDescending(c => c.Timestamp)
                    .Take(count)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets formatted chat history for display
        /// </summary>
        public string GetFormattedChatHistory(int count = DEFAULT_DISPLAY_COUNT, bool showMore = false)
        {
            var entries = GetChatHistory(count);

            if (entries.Count == 0)
            {
                return "📝 No chat history available.";
            }

            var result = $"💬 CHAT HISTORY (Last {entries.Count} interactions):\n\n";

            foreach (var entry in entries.OrderBy(e => e.Timestamp))
            {
                var timeInfo = GetRelativeTimeString(entry.Timestamp);
                var responseIcon = GetResponseTypeIcon(entry.ResponseType);

                result += $"🕒 {timeInfo}\n";
                result += $"👤 You: {GetTruncatedText(entry.UserInput, 80)}\n";
                result += $"{responseIcon} Bot: {GetTruncatedText(entry.BotResponse, 80)}\n";

                if (entry.Keywords.Any())
                {
                    result += $"🏷️ Keywords: {string.Join(", ", entry.Keywords.Take(3))}\n";
                }

                result += "\n";
            }

            if (showMore && _chatHistory.Count > count)
            {
                result += $"💡 Showing {count} of {_chatHistory.Count} total interactions.\n";
                result += "Use 'show more chat history' to see additional entries.";
            }

            return result;
        }

        #endregion

        #region Activity Tracking

        /// <summary>
        /// Logs an activity entry
        /// </summary>
        public void LogActivity(ActivityType activityType, string description, string userName = null, Dictionary<string, object> metadata = null)
        {
            lock (_lockObject)
            {
                var activity = new ActivityEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    ActivityType = activityType,
                    Description = description,
                    UserName = userName ?? "Anonymous",
                    Metadata = metadata ?? new Dictionary<string, object>()
                };

                _activities.Add(activity);

                // Maintain maximum entries
                if (_activities.Count > MAX_ACTIVITY_ENTRIES)
                {
                    _activities.RemoveAt(0);
                }

                OnActivityAdded?.Invoke(activity);
            }
        }

        /// <summary>
        /// Logs task-related activities
        /// </summary>
        public void LogTaskActivity(Task task, string action, string userName = null)
        {
            var metadata = new Dictionary<string, object>
            {
                ["TaskId"] = task.Id,
                ["TaskTitle"] = task.Title,
                ["TaskPriority"] = task.Priority.ToString(),
                ["TaskCategory"] = task.Category,
                ["Action"] = action
            };

            LogActivity(ActivityType.TaskManagement,
                $"Task {action}: {task.Title}",
                userName,
                metadata);
        }

        /// <summary>
        /// Logs quiz-related activities
        /// </summary>
        public void LogQuizActivity(string action, string details = null, string userName = null)
        {
            var metadata = new Dictionary<string, object>
            {
                ["Action"] = action,
                ["Details"] = details ?? string.Empty
            };

            LogActivity(ActivityType.QuizActivity,
                $"Quiz {action}" + (string.IsNullOrEmpty(details) ? string.Empty : $": {details}"),
                userName,
                metadata);
        }

        /// <summary>
        /// Gets activity log with filtering options
        /// </summary>
        public List<ActivityEntry> GetActivityLog(int count = DEFAULT_DISPLAY_COUNT, ActivityType? activityType = null, string userName = null, DateTime? fromDate = null)
        {
            lock (_lockObject)
            {
                var query = _activities.AsQueryable();

                if (activityType.HasValue)
                {
                    query = query.Where(a => a.ActivityType == activityType.Value);
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    query = query.Where(a => a.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= fromDate.Value);
                }

                return query
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------//

        public string GetConciseActivitySummary(int count = 10)
        {
            var activities = GetActivityLog(count).OrderByDescending(a => a.Timestamp).ToList();
            if (activities.Count == 0)
                return "No recent activities found.";

            var summary = "Here’s a summary of recent actions:\n";

            int index = 1;
            foreach (var activity in activities)
            {
                string line = $"{index}. ";

                switch (activity.ActivityType)
                {
                    case ActivityType.TaskManagement:
                        var title = activity.Metadata.ContainsKey("TaskTitle") ? activity.Metadata["TaskTitle"]?.ToString() : "Unnamed task";
                        var reminder = activity.Metadata.ContainsKey("ReminderTime") ? activity.Metadata["ReminderTime"]?.ToString() : null;
                        line += $"Task added: '{title}'";
                        if (!string.IsNullOrEmpty(reminder))
                            line += $" (Reminder set for {reminder})";
                        break;

                    case ActivityType.QuizActivity:
                        var action = activity.Metadata.ContainsKey("Action") ? activity.Metadata["Action"].ToString() : "Quiz";
                        var details = activity.Metadata.ContainsKey("Details") ? activity.Metadata["Details"].ToString() : null;
                        line += $"Quiz {action.ToLower()}";
                        if (!string.IsNullOrEmpty(details))
                            line += $" - {details}";
                        break;

                    case ActivityType.SystemAction:
                        line += $"System event: {activity.Description}";
                        break;

                    default:
                        line += activity.Description;
                        break;
                }

                summary += line + "\n";
                index++;
            }

            return summary;
        }

        /// <summary>
        /// Gets formatted activity log for display
        /// </summary>
        public string GetFormattedActivityLog(int count = DEFAULT_DISPLAY_COUNT, bool showMore = false)
        {
            var activities = GetActivityLog(count);

            if (activities.Count == 0)
            {
                return "📊 No activity recorded.";
            }

            var result = $"📊 ACTIVITY LOG (Last {activities.Count} activities):\n\n";

            foreach (var activity in activities.OrderBy(a => a.Timestamp))
            {
                var timeInfo = GetRelativeTimeString(activity.Timestamp);
                var icon = GetActivityTypeIcon(activity.ActivityType);

                result += $"{icon} {timeInfo} - {activity.Description}\n";

                // Add specific metadata for different activity types
                if (activity.ActivityType == ActivityType.TaskManagement && activity.Metadata.ContainsKey("TaskPriority"))
                {
                    result += $"   🔥 Priority: {activity.Metadata["TaskPriority"]}\n";
                }
                else if (activity.ActivityType == ActivityType.QuizActivity && activity.Metadata.ContainsKey("Details"))
                {
                    var details = activity.Metadata["Details"]?.ToString();
                    if (!string.IsNullOrEmpty(details))
                    {
                        result += $"   📋 {details}\n";
                    }
                }

                result += "\n";
            }

            if (showMore && _activities.Count > count)
            {
                result += $"💡 Showing {count} of {_activities.Count} total activities.\n";
                result += "Use 'show more activity' to see additional entries.";
            }

            return result;
        }

        #endregion

        #region Statistics and Analysis

        /// <summary>
        /// Gets comprehensive user activity statistics
        /// </summary>
        public ActivityStatistics GetActivityStatistics(string userName = null, DateTime? fromDate = null)
        {
            lock (_lockObject)
            {
                var activities = string.IsNullOrEmpty(userName)
                    ? _activities
                    : _activities.Where(a => a.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

                var chats = string.IsNullOrEmpty(userName)
                    ? _chatHistory
                    : _chatHistory.Where(c => c.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

                if (fromDate.HasValue)
                {
                    activities = activities.Where(a => a.Timestamp >= fromDate.Value);
                    chats = chats.Where(c => c.Timestamp >= fromDate.Value);
                }

                var activitiesList = activities.ToList();
                var chatsList = chats.ToList();

                return new ActivityStatistics
                {
                    TotalActivities = activitiesList.Count,
                    TotalChatInteractions = chatsList.Count,
                    TasksCreated = activitiesList.Count(a => a.ActivityType == ActivityType.TaskManagement &&
                                                            a.Metadata.ContainsKey("Action") &&
                                                            a.Metadata["Action"].ToString().Contains("created")),
                    QuizzesTaken = activitiesList.Count(a => a.ActivityType == ActivityType.QuizActivity &&
                                                            a.Metadata.ContainsKey("Action") &&
                                                            a.Metadata["Action"].ToString().Contains("started")),
                    MostActiveDay = GetMostActiveDay(activitiesList),
                    LastActivity = activitiesList.OrderByDescending(a => a.Timestamp).FirstOrDefault()?.Timestamp,
                    TopKeywords = GetTopKeywords(chatsList, 5),
                    ActivityByType = activitiesList.GroupBy(a => a.ActivityType)
                                                 .ToDictionary(g => g.Key, g => g.Count())
                };
            }
        }

        /// <summary>
        /// Gets conversation topics and keywords identified from chat history
        /// </summary>
        public List<string> GetConversationTopics(int count = 10)
        {
            lock (_lockObject)
            {
                return _chatHistory
                    .SelectMany(c => c.Keywords)
                    .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .Take(count)
                    .Select(g => g.Key)
                    .ToList();
            }
        }

        /// <summary>
        /// Searches chat history by keyword or content
        /// </summary>
        public List<ChatEntry> SearchChatHistory(string searchTerm, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ChatEntry>();

            lock (_lockObject)
            {
                var lowerSearchTerm = searchTerm.ToLower();

                return _chatHistory
                    .Where(c => c.UserInput.ToLower().Contains(lowerSearchTerm) ||
                               c.BotResponse.ToLower().Contains(lowerSearchTerm) ||
                               c.Keywords.Any(k => k.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(c => c.Timestamp)
                    .Take(maxResults)
                    .ToList();
            }
        }

        #endregion

        #region Helper Methods

        private List<string> ExtractKeywords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            // Define important cybersecurity and task-related keywords
            var importantKeywords = new[]
            {
                "password", "security", "firewall", "backup", "malware", "virus", "phishing",
                "encryption", "2fa", "two-factor", "vpn", "antivirus", "audit", "compliance",
                "task", "reminder", "due", "priority", "urgent", "complete", "deadline",
                "quiz", "question", "answer", "score", "test", "learning"
            };

            var words = input.ToLower()
                            .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(w => w.Length > 2)
                            .ToList();

            return words.Where(w => importantKeywords.Contains(w))
                       .Distinct()
                       .ToList();
        }

        private string GetTruncatedText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text ?? string.Empty;

            return text.Substring(0, maxLength - 3) + "...";
        }

        private string GetRelativeTimeString(DateTime timestamp)
        {
            var timeDiff = DateTime.Now - timestamp;

            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            else if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes}m ago";
            else if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours}h ago";
            else if (timeDiff.TotalDays < 7)
                return $"{(int)timeDiff.TotalDays}d ago";
            else
                return timestamp.ToString("MMM dd, HH:mm");
        }

        private string GetResponseTypeIcon(ChatResponseType responseType)
        {

            switch (responseType)
            {
                case ChatResponseType.Task:
                    return "📋";
                case ChatResponseType.Quiz:
                    return "🎯";
                case ChatResponseType.Help:
                    return "❓";
                case ChatResponseType.Error:
                    return "❌";
                case ChatResponseType.Exit:
                    return "👋";
                default:
                    return "🤖";
            }
        }

        private string GetActivityTypeIcon(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.TaskManagement:
                    return "📋";
                case ActivityType.QuizActivity:
                    return "🎯";
                case ActivityType.ChatInteraction:
                    return "💬";
                case ActivityType.SystemAction:
                    return "⚙️";
                default:
                    return "📊";
            }
        }

        private DayOfWeek? GetMostActiveDay(List<ActivityEntry> activities)
        {
            if (!activities.Any())
                return null;

            return activities
                .GroupBy(a => a.Timestamp.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        private List<string> GetTopKeywords(List<ChatEntry> chats, int count)
        {
            return chats
                .SelectMany(c => c.Keywords)
                .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToList();
        }

        #endregion

        #region Public Interface Methods

        /// <summary>
        /// Clears all activity and chat history
        /// </summary>
        public void ClearHistory()
        {
            lock (_lockObject)
            {
                _activities.Clear();
                _chatHistory.Clear();
            }
        }

        /// <summary>
        /// Exports activity data (could be enhanced to save to file)
        /// </summary>
        public string ExportActivityData(DateTime? fromDate = null, DateTime? toDate = null)
        {
            lock (_lockObject)
            {
                var activities = _activities.AsQueryable();
                var chats = _chatHistory.AsQueryable();

                if (fromDate.HasValue)
                {
                    activities = activities.Where(a => a.Timestamp >= fromDate.Value);
                    chats = chats.Where(c => c.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    activities = activities.Where(a => a.Timestamp <= toDate.Value);
                    chats = chats.Where(c => c.Timestamp <= toDate.Value);
                }

                var result = "=== ACTIVITY EXPORT ===\n\n";
                result += $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                result += $"Activities: {activities.Count()}\n";
                result += $"Chat Interactions: {chats.Count()}\n\n";

                result += "=== ACTIVITIES ===\n";
                foreach (var activity in activities.OrderBy(a => a.Timestamp))
                {
                    result += $"{activity.Timestamp:yyyy-MM-dd HH:mm:ss} | {activity.ActivityType} | {activity.Description}\n";
                }

                result += "\n=== CHAT HISTORY ===\n";
                foreach (var chat in chats.OrderBy(c => c.Timestamp))
                {
                    result += $"{chat.Timestamp:yyyy-MM-dd HH:mm:ss} | User: {chat.UserInput}\n";
                    result += $"                     | Bot: {chat.BotResponse}\n\n";
                }

                return result;
            }
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// Represents a single activity entry
    /// </summary>
    public class ActivityEntry
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public ActivityType ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a chat history entry
    /// </summary>
    public class ChatEntry
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserInput { get; set; } = string.Empty;
        public string BotResponse { get; set; } = string.Empty;
        public ChatResponseType ResponseType { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    /// <summary>
    /// Activity statistics summary
    /// </summary>
    public class ActivityStatistics
    {
        public int TotalActivities { get; set; }
        public int TotalChatInteractions { get; set; }
        public int TasksCreated { get; set; }
        public int QuizzesTaken { get; set; }
        public DayOfWeek? MostActiveDay { get; set; }
        public DateTime? LastActivity { get; set; }
        public List<string> TopKeywords { get; set; } = new List<string>();
        public Dictionary<ActivityType, int> ActivityByType { get; set; } = new Dictionary<ActivityType, int>();
    }

    /// <summary>
    /// Types of activities that can be logged
    /// </summary>
    public enum ActivityType
    {
        ChatInteraction,
        TaskManagement,
        QuizActivity,
        SystemAction
    }

    #endregion

}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//