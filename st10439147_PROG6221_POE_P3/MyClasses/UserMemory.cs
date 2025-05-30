//Dillon Rinkwest
//Student Number: ST10439147
// Module: PROG6221
// Group: 1
// POE Part 2

//## References
//-ClaudAI from Anthropic https://claude.ai/
//-ChatGPT from OpenAI https://chatgpt.com/
//-Deepseek AI Model from High-Flyer https://www.deepseek.com/
//-Stack Overflow https://stackoverflow.com/
//-Pro C# 10 with .NET 6, Foundational Principles and Practices in Programming, Eleventh Edition by Andrew Troelsen and Phil Japiske

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// Enhanced UserMemory class with favorite topic tracking capabilities
    /// </summary>
    public class UserMemory
    {
        // Existing fields
        private readonly Dictionary<string, string> _userInfo;
        private readonly List<string> _discussedTopics;
        private readonly Queue<string> _sentimentHistory;

        // New field to track topic frequency and engagement
        private readonly Dictionary<string, TopicData> _topicEngagement;

        private const int MaxSentimentHistoryLength = 5;

        /// <summary>
        /// Inner class to track topic engagement data
        /// </summary>
        private class TopicData
        {
            public int DiscussionCount { get; set; } = 0;
            public int PositiveInteractions { get; set; } = 0;
            public int QuestionsAsked { get; set; } = 0;
            public DateTime LastDiscussed { get; set; } = DateTime.Now;
            public TimeSpan TotalTimeSpent { get; set; } = TimeSpan.Zero;
            public DateTime TopicStartTime { get; set; } = DateTime.Now;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // constructor to initialize the UserMemory
        public UserMemory()
        {
            _userInfo = new Dictionary<string, string>();
            _discussedTopics = new List<string>();
            _sentimentHistory = new Queue<string>();
            _topicEngagement = new Dictionary<string, TopicData>();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public void StoreUserInfo(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                string normalizedKey = key.Trim().ToLower();
                string normalizedValue = value.Trim();

                if (_userInfo.ContainsKey(normalizedKey))
                    _userInfo[normalizedKey] = normalizedValue;
                else
                    _userInfo.Add(normalizedKey, normalizedValue);
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public string GetUserInfo(string key)
        {
            if (string.IsNullOrWhiteSpace(key))// Check if the key is null or empty
                return null;

            try // Try to retrieve the user info
            {
                string normalizedKey = key.Trim().ToLower();// Normalize the key to ensure case-insensitivity
                return _userInfo.ContainsKey(normalizedKey) ? _userInfo[normalizedKey] : null;// Return the value if the key exists, otherwise return null
            }
            catch (Exception)
            {
                return null;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // This method adds a topic to the discussed topics list and tracks engagement  
        public void AddDiscussedTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) // Check if the topic is null or empty
                return;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();// Normalize the topic to ensure case-insensitivity

                // Add to discussed topics list
                if (!_discussedTopics.Contains(normalizedTopic))// Only add if not already discussed
                {
                    _discussedTopics.Insert(0, normalizedTopic);// Insert at the beginning to keep most recent first
                    const int maxTopics = 20;   // Limit the number of discussed topics to 20
                    if (_discussedTopics.Count > maxTopics)     // If more than 20 topics, remove the oldest one
                        _discussedTopics.RemoveAt(_discussedTopics.Count - 1); // Remove the last topic to maintain the limit
                }

                // Track engagement data
                TrackTopicEngagement(normalizedTopic);// Track detailed engagement with the topic
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Track detailed engagement with a topic
        /// </summary>
        private void TrackTopicEngagement(string topic)
        {
            if (!_topicEngagement.ContainsKey(topic))
            {
                _topicEngagement[topic] = new TopicData();
            }

            var topicData = _topicEngagement[topic];
            topicData.DiscussionCount++;
            topicData.LastDiscussed = DateTime.Now;
            topicData.TopicStartTime = DateTime.Now; // Reset for new discussion session
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Record when user asks a question about a specific topic
        /// </summary>
        public void RecordTopicQuestion(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();

                if (!_topicEngagement.ContainsKey(normalizedTopic))
                {
                    _topicEngagement[normalizedTopic] = new TopicData();
                }

                _topicEngagement[normalizedTopic].QuestionsAsked++;
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Record positive interaction with a topic (user shows interest, asks follow-ups, etc.)
        /// </summary>
        public void RecordPositiveTopicInteraction(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();

                if (!_topicEngagement.ContainsKey(normalizedTopic))
                {
                    _topicEngagement[normalizedTopic] = new TopicData();
                }

                _topicEngagement[normalizedTopic].PositiveInteractions++;
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Record time spent on a topic (call when switching topics)
        /// </summary>
        public void RecordTopicTimeSpent(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();

                if (_topicEngagement.ContainsKey(normalizedTopic))
                {
                    var topicData = _topicEngagement[normalizedTopic];
                    var timeSpent = DateTime.Now - topicData.TopicStartTime;
                    topicData.TotalTimeSpent = topicData.TotalTimeSpent.Add(timeSpent);
                }
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get the user's favorite topic based on multiple engagement factors
        /// </summary>
        public string GetFavoriteTopic()
        {
            if (_topicEngagement.Count == 0)
                return null;

            try
            {
                // Calculate engagement scores for each topic
                var topicScores = new Dictionary<string, double>();

                foreach (var kvp in _topicEngagement)
                {
                    string topic = kvp.Key;
                    TopicData data = kvp.Value;

                    // Calculate engagement score based on multiple factors
                    double score = 0;

                    // Discussion frequency (40% weight)
                    score += data.DiscussionCount * 0.4;

                    // Positive interactions (30% weight)
                    score += data.PositiveInteractions * 0.3;

                    // Questions asked (20% weight) - shows curiosity
                    score += data.QuestionsAsked * 0.2;

                    // Recency bonus (10% weight) - more recent topics get slight preference
                    var daysSinceLastDiscussion = (DateTime.Now - data.LastDiscussed).TotalDays;
                    var recencyBonus = Math.Max(0, (7 - daysSinceLastDiscussion) / 7) * 0.1;
                    score += recencyBonus;

                    // Time spent bonus (small influence)
                    var minutesSpent = data.TotalTimeSpent.TotalMinutes;
                    score += Math.Min(minutesSpent / 60, 2) * 0.05; // Max 2 hour bonus, small weight

                    topicScores[topic] = score;
                }

                // Return the topic with the highest score
                return topicScores.OrderByDescending(x => x.Value).First().Key;
            }
            catch (Exception)
            {
                // Fallback to most discussed topic
                return GetMostDiscussedTopic();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get the most frequently discussed topic (simpler approach)
        /// </summary>
        public string GetMostDiscussedTopic()
        {
            if (_topicEngagement.Count == 0)
                return null;

            try
            {
                return _topicEngagement
                    .OrderByDescending(x => x.Value.DiscussionCount)
                    .First().Key;
            }
            catch (Exception)
            {
                return null;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Get top N favorite topics
        /// </summary>
        /// <param name="count">Number of topics to return</param>
        /// <returns>List of favorite topics in order of preference</returns>
        public List<string> GetTopFavoriteTopics(int count = 3)
        {
            if (_topicEngagement.Count == 0)
                return new List<string>();

            try
            {
                var topicScores = new Dictionary<string, double>();

                foreach (var kvp in _topicEngagement)
                {
                    string topic = kvp.Key;
                    TopicData data = kvp.Value;

                    double score = (data.DiscussionCount * 0.4) +
                                  (data.PositiveInteractions * 0.3) +
                                  (data.QuestionsAsked * 0.2);

                    var daysSinceLastDiscussion = (DateTime.Now - data.LastDiscussed).TotalDays;
                    var recencyBonus = Math.Max(0, (7 - daysSinceLastDiscussion) / 7) * 0.1;
                    score += recencyBonus;

                    topicScores[topic] = score;
                }

                return topicScores
                    .OrderByDescending(x => x.Value)
                    .Take(count)
                    .Select(x => x.Key)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Check if user has shown strong interest in a specific topic
        /// </summary>
        /// <param name="topic">Topic to check</param>
        /// <returns>True if user has shown strong interest</returns>
        public bool HasStrongInterestIn(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return false;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();

                if (!_topicEngagement.ContainsKey(normalizedTopic))
                    return false;

                var data = _topicEngagement[normalizedTopic];

                // Consider strong interest if:
                // - Discussed 3+ times, OR
                // - Asked 2+ questions about it, OR
                // - Had 3+ positive interactions
                return data.DiscussionCount >= 3 ||
                       data.QuestionsAsked >= 2 ||
                       data.PositiveInteractions >= 3;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        //  this method checks if the user has discussed a specific topic
        public bool HasDiscussedTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return false;

            try
            {
                string normalizedTopic = topic.Trim().ToLower();
                return _discussedTopics.Contains(normalizedTopic);
            }
            catch (Exception)
            {
                return false;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public List<string> GetDiscussedTopics()
        {
            try
            {
                return new List<string>(_discussedTopics);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public void RecordSentiment(string sentiment)
        {
            if (string.IsNullOrWhiteSpace(sentiment))
                return;

            try
            {
                string normalizedSentiment = sentiment.Trim().ToLower();
                _sentimentHistory.Enqueue(normalizedSentiment);

                while (_sentimentHistory.Count > MaxSentimentHistoryLength)
                {
                    _sentimentHistory.Dequeue();
                }
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // This method retrieves the dominant sentiment from the sentiment history
        public string GetDominantSentiment()
        {
            if (_sentimentHistory.Count == 0)// If there are no sentiments recorded, return "neutral"
                return "neutral";

            // This method calculates the dominant sentiment based on the sentiment history
            try
            {
                // Count occurrences of each sentiment type
                // Initialize sentiment counts for all possible sentiments
                // This is done to ensure that even if a sentiment is not present, it will still be counted as 0
                // This allows for easy retrieval of the dominant sentiment
                var sentimentCounts = new Dictionary<string, int>
                {
                    { "positive", 0 }, { "negative", 0 }, { "neutral", 0 },
                    { "anxious", 0 }, { "confused", 0 }, { "frustrated", 0 }
                };

                // Iterate through the sentiment history and count occurrences of each sentiment
                // This is done to ensure that the sentiment counts are accurate
                // This allows for easy retrieval of the dominant sentiment
                foreach (var sentiment in _sentimentHistory)
                {
                    if (sentimentCounts.ContainsKey(sentiment))// If the sentiment is one of the known sentiments, increment its count
                        sentimentCounts[sentiment]++;
                    else
                        sentimentCounts["neutral"]++;// If the sentiment is not one of the known sentiments, increment the neutral count
                }

                return sentimentCounts.OrderByDescending(x => x.Value).First().Key;// Return the sentiment with the highest count
            }
            catch (Exception)
            {
                return "neutral";       // If an error occurs, return "neutral" as a fallback
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public string GetRecentSentiment()
        {
            try
            {
                // Return the most recent sentiment from the history
                return _sentimentHistory.Count > 0 ? _sentimentHistory.Last() : "neutral";
            }
            catch (Exception)
            {
                return "neutral";// If an error occurs, return "neutral" as a fallback
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        public void ClearUserData()
        {
            try
            {
                _userInfo.Clear();
                _discussedTopics.Clear();
                _sentimentHistory.Clear();
                _topicEngagement.Clear();
            }
            catch (Exception) { }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method retrieves a summary of the user's memory, including user info, discussed topics, and sentiment analysis
        public Dictionary<string, string> GetUserSummary()
        {
            try
            {
                var summary = new Dictionary<string, string>(_userInfo);// Start with user info

                summary["topics_discussed_count"] = _discussedTopics.Count.ToString();
                summary["dominant_sentiment"] = GetDominantSentiment();
                summary["recent_sentiment"] = GetRecentSentiment();

                if (_discussedTopics.Count > 0)
                    summary["most_recent_topic"] = _discussedTopics[0];

                // Add favorite topic information
                var favoriteTopic = GetFavoriteTopic();
                if (!string.IsNullOrEmpty(favoriteTopic))
                {
                    summary["favorite_topic"] = favoriteTopic;
                    summary["favorite_topic_discussions"] = _topicEngagement[favoriteTopic].DiscussionCount.ToString();
                }

                return summary;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}