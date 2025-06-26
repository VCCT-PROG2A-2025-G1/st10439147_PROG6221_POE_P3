//Dillon Rinkwest
//Student Number: ST10439147
// Module: PROG6221
// Group: 1

//References
//-ClaudAI from Anthropic https://claude.ai/
//-ChatGPT from OpenAI https://chatgpt.com/
//-Deepseek AI Model from High-Flyer https://www.deepseek.com/
//-Stack Overflow https://stackoverflow.com/
//-Pro C# 10 with .NET 6, Foundational Principles and Practices in Programming, Eleventh Edition by Andrew Troelsen and Phil Japiske
using ST10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace st10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// Manages tasks and reminders for cybersecurity activities
    /// </summary>
    public class TaskManager
    {
        private readonly List<Task> _tasks;
        private readonly Timer _reminderTimer;
        private readonly UserMemory _userMemory;

        public event EventHandler<TaskReminderEventArgs> ReminderTriggered;
        public event EventHandler<TaskEventArgs> TaskAdded;
        public event EventHandler<TaskEventArgs> TaskUpdated;
        public event EventHandler<TaskEventArgs> TaskCompleted;
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        public TaskManager(UserMemory userMemory)
        {
            _tasks = new List<Task>();
            _userMemory = userMemory;

            // Set up reminder timer to check every minute
            _reminderTimer = new Timer(60000); // 1 minute
            _reminderTimer.Elapsed += CheckReminders;
            _reminderTimer.Start();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Add a new task
        /// </summary>
        public void AddTask(string title, string description, DateTime dueDate,
                           TaskPriority priority = TaskPriority.Medium,
                           string category = "General", TimeSpan? reminderTime = null)
        {
            try
            {
                var task = new Task
                {
                    Title = title,
                    Description = description,
                    DueDate = dueDate,
                    Priority = priority,
                    Status = TaskStatus.Pending,
                    Category = category,
                    ReminderTime = reminderTime,
                    IsReminder = reminderTime.HasValue
                };

                _tasks.Add(task);

                // Track in user memory
                _userMemory?.AddDiscussedTopic($"task_{category.ToLower()}");
                _userMemory?.RecordPositiveTopicInteraction("task_management");

                TaskAdded?.Invoke(this, new TaskEventArgs(task));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add task: {ex.Message}", ex);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Update task status
        /// </summary>
        public void UpdateTaskStatus(string taskId, TaskStatus status)
        {
            try
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    task.Status = status;

                    if (status == TaskStatus.Completed)
                    {
                        _userMemory?.RecordPositiveTopicInteraction("task_completion");
                        TaskCompleted?.Invoke(this, new TaskEventArgs(task));
                    }

                    TaskUpdated?.Invoke(this, new TaskEventArgs(task));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update task: {ex.Message}", ex);
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Update task reminder
        /// </summary>
        public void UpdateTaskReminder(string taskId, TimeSpan reminderTime)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                throw new ArgumentException($"Task with ID {taskId} not found.");

            task.ReminderTime = reminderTime;
            task.IsReminder = true;

            // No need to trigger reminder here; it will be caught by timer when due
            TaskUpdated?.Invoke(this, new TaskEventArgs(task));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get all tasks
        /// </summary>
        public List<Task> GetAllTasks()
        {
            return new List<Task>(_tasks);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get tasks by status
        /// </summary>
        public List<Task> GetTasksByStatus(TaskStatus status)
        {
            return _tasks.Where(t => t.Status == status).ToList();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get overdue tasks
        /// </summary>
        public List<Task> GetOverdueTasks()
        {
            return _tasks.Where(t => t.IsOverdue).ToList();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get upcoming tasks (next 3 days)
        /// </summary>
        public List<Task> GetUpcomingTasks()
        {
            return _tasks.Where(t => t.IsUpcoming).ToList();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get tasks by priority
        /// </summary>
        public List<Task> GetTasksByPriority(TaskPriority priority)
        {
            return _tasks.Where(t => t.Priority == priority && t.Status != TaskStatus.Completed)
                        .OrderBy(t => t.DueDate).ToList();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Delete a task
        /// </summary>
        public bool DeleteTask(string taskId)
        {
            try
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    _tasks.Remove(task);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Add cybersecurity-specific tasks
        /// </summary>
        public void AddCybersecurityTask(CybersecurityTaskType taskType, DateTime dueDate,
                                       string additionalNotes = "")
        {
            var taskInfo = GetCybersecurityTaskInfo(taskType);
            AddTask(taskInfo.Title, $"{taskInfo.Description}\n{additionalNotes}",
                   dueDate, taskInfo.Priority, "Cybersecurity", TimeSpan.FromHours(1));
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Check for reminders
        /// </summary>
        private void CheckReminders(object sender, ElapsedEventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var tasksWithReminders = _tasks.Where(t => t.IsReminder &&
                                                          t.Status != TaskStatus.Completed &&
                                                          t.ReminderTime.HasValue).ToList();

                foreach (var task in tasksWithReminders)
                {
                    var reminderTime = task.DueDate.Subtract(task.ReminderTime.Value);

                    // Trigger if within the same minute
                    if (now >= reminderTime && now < reminderTime.AddMinutes(1))
                    {
                        ReminderTriggered?.Invoke(this, new TaskReminderEventArgs(task));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reminder check error: {ex.Message}");
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get predefined cybersecurity task information
        /// </summary>
        private CybersecurityTaskInfo GetCybersecurityTaskInfo(CybersecurityTaskType taskType)
        {
            switch (taskType)
            {
                case CybersecurityTaskType.PasswordUpdate:
                    return new CybersecurityTaskInfo
                    {
                        Title = "Update Passwords",
                        Description = "Review and update all critical passwords. Use strong, unique passwords for each account.",
                        Priority = TaskPriority.High
                    };
                case CybersecurityTaskType.SoftwareUpdate:
                    return new CybersecurityTaskInfo
                    {
                        Title = "Software Security Update",
                        Description = "Check for and install critical security updates for all systems and applications.",
                        Priority = TaskPriority.High
                    };
                case CybersecurityTaskType.BackupVerification:
                    return new CybersecurityTaskInfo
                    {
                        Title = "Backup Verification",
                        Description = "Verify that all critical data backups are working correctly and test restore procedures.",
                        Priority = TaskPriority.Medium
                    };
                case CybersecurityTaskType.SecurityAudit:
                    return new CybersecurityTaskInfo
                    {
                        Title = "Security Audit",
                        Description = "Conduct comprehensive security audit of systems, networks, and access controls.",
                        Priority = TaskPriority.Critical
                    };
                case CybersecurityTaskType.TwoFactorSetup:
                    return new CybersecurityTaskInfo
                    {
                        Title = "Two-Factor Authentication Setup",
                        Description = "Enable two-factor authentication on all critical accounts and services.",
                        Priority = TaskPriority.High
                    };
                default:
                    return new CybersecurityTaskInfo
                    {
                        Title = "General Security Task",
                        Description = "Complete cybersecurity-related task as specified.",
                        Priority = TaskPriority.Medium
                    };
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        public void Dispose()
        {
            _reminderTimer?.Stop();
            _reminderTimer?.Dispose();
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    }

    // Supporting classes and enums
    public class TaskEventArgs : EventArgs
    {
        public Task Task { get; }
        public TaskEventArgs(Task task) => Task = task;
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    public class TaskReminderEventArgs : EventArgs
    {
        public Task Task { get; }
        public string Message { get; }

        public TaskReminderEventArgs(Task task)
        {
            Task = task;
            Message = $"Reminder: {task.Title} is due {task.DueDate:MMM dd, yyyy HH:mm}";
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        public TaskReminderEventArgs(Task task, string message)
        {
            Task = task;
            Message = message;
        }
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    public enum CybersecurityTaskType
    {
        PasswordUpdate,
        SoftwareUpdate,
        BackupVerification,
        SecurityAudit,
        TwoFactorSetup
    }
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
    public class CybersecurityTaskInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
    }


}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//