public string GetUserFriendlySummary(int count = 5)
{
    var log = GetActivityLog(count);
    if (log == null || log.Count == 0)
        return "No recent activity found.";

    var lines = new List<string> { "Here’s a summary of recent actions:" };
    int i = 1;
    foreach (var entry in log)
    {
        string line = "";
        switch (entry.ActivityType)
        {
            case ActivityType.TaskAdded:
                line = $"Task added: '{entry.Description}'";
                if (entry.Metadata != null && entry.Metadata.ContainsKey("ReminderDays"))
                    line += $" (Reminder set for {entry.Metadata["ReminderDays"]} days from now)";
                break;
            case ActivityType.QuizStarted:
                line = $"Quiz started - {entry.Metadata?["Questions"] ?? "questions"} answered.";
                break;
            case ActivityType.ReminderSet:
                line = $"Reminder set: '{entry.Description}' on {entry.Metadata?["ReminderDate"] ?? "[date]"}";
                break;
            default:
                line = entry.Description;
                break;
        }
        lines.Add($"{i++}. {line}.");
    }
    return string.Join("\n", lines);
}