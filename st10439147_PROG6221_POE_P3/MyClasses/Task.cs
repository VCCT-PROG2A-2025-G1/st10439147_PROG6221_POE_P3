using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace st10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// Represents a task with cybersecurity focus
    /// </summary>
    public class Task : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private DateTime _dueDate;
        private TaskPriority _priority;
        private TaskStatus _status;
        private bool _isReminder;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(nameof(DueDate)); }
        }

        public TaskPriority Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        public TaskStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public bool IsReminder
        {
            get => _isReminder;
            set { _isReminder = value; OnPropertyChanged(nameof(IsReminder)); }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Category { get; set; } = "General";
        public TimeSpan? ReminderTime { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsOverdue => Status != TaskStatus.Completed && DueDate < DateTime.Now;
        public bool IsUpcoming => Status != TaskStatus.Completed &&
                                 DueDate > DateTime.Now &&
                                 DueDate <= DateTime.Now.AddDays(3);
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}

