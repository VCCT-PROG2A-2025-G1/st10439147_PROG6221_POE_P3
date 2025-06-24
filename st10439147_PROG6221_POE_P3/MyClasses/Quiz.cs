using ST10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace st10439147_PROG6221_POE_P3.MyClasses
{
    // Event args for quiz events
    public class QuestionAskedEventArgs : EventArgs
    {
        public QuizQuestion Question { get; set; }
        public int QuestionNumber { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class AnswerResultEventArgs : EventArgs
    {
        public QuizQuestion Question { get; set; }
        public bool IsCorrect { get; set; }
        public int CurrentScore { get; set; }
        public int TotalQuestions { get; set; }
        public string Feedback { get; set; }
    }

    public class QuizCompletedEventArgs : EventArgs
    {
        public int FinalScore { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public string PerformanceFeedback { get; set; }
        public Dictionary<string, object> UserSummary { get; set; }
    }

    /// <summary>
    /// GUI-compatible Quiz Game Controller
    /// </summary>
    public class QuizGameController
    {
        private readonly UserMemory _userMemory;
        private readonly QuestionBank _questionBank;
        private readonly QuizScoreManager _scoreManager;

        private List<QuizQuestion> _currentQuestions;
        private int _currentQuestionIndex;
        private bool _quizInProgress;

        private const int MinimumQuestions = 10;


        // Events for GUI to subscribe to
        public event EventHandler<QuestionAskedEventArgs> QuestionAsked;
        public event EventHandler<AnswerResultEventArgs> AnswerProvided;
        public event EventHandler<QuizCompletedEventArgs> QuizCompleted;
        public event EventHandler QuizStarted;
        public event EventHandler QuizReset;

        public QuizGameController()
        {
            _userMemory = new UserMemory();
            _questionBank = new QuestionBank();
            _scoreManager = new QuizScoreManager();
            _currentQuestions = new List<QuizQuestion>();
            _currentQuestionIndex = 0;
            _quizInProgress = false;
        }

        // Properties for GUI access
        public int CurrentScore => _scoreManager.CurrentScore;
        public int TotalQuestions => _scoreManager.TotalQuestions;
        public double CurrentPercentage => _scoreManager.CurrentPercentage;
        public bool IsQuizInProgress => _quizInProgress;
        public int CurrentQuestionNumber => _currentQuestionIndex + 1;
        public QuizQuestion CurrentQuestion => _currentQuestionIndex < _currentQuestions.Count ?
            _currentQuestions[_currentQuestionIndex] : null;

        /// <summary>
        /// Start a new quiz
        /// </summary>
        public void StartQuiz(int? questionCount = null)
        {
            if (_quizInProgress)
                throw new InvalidOperationException("Quiz is already in progress. Call ResetQuiz() first.");

            _currentQuestions = _questionBank.GetRandomQuestions(questionCount ?? MinimumQuestions);
            _currentQuestionIndex = 0;
            _quizInProgress = true;

            QuizStarted?.Invoke(this, EventArgs.Empty);

            // Ask the first question
            AskCurrentQuestion();
        }

        /// <summary>
        /// Start quiz with specific topic
        /// </summary>
        public void StartTopicQuiz(string topic, int? questionCount = null)
        {
            if (_quizInProgress)
                throw new InvalidOperationException("Quiz is already in progress. Call ResetQuiz() first.");

            var topicQuestions = _questionBank.GetQuestionsByTopic(topic);

            if (topicQuestions.Count == 0)
                throw new ArgumentException($"No questions found for topic: {topic}");

            _currentQuestions = topicQuestions
                .OrderBy(x => Guid.NewGuid())
                .Take(questionCount ?? Math.Min(MinimumQuestions, topicQuestions.Count))
                .ToList();

            _currentQuestionIndex = 0;
            _quizInProgress = true;

            QuizStarted?.Invoke(this, EventArgs.Empty);
            AskCurrentQuestion();
        }

        /// <summary>
        /// Submit an answer for the current question
        /// </summary>
        public void SubmitAnswer(int selectedOptionIndex)
        {
            if (!_quizInProgress || CurrentQuestion == null)
                throw new InvalidOperationException("No quiz in progress or no current question.");

            if (selectedOptionIndex < 0 || selectedOptionIndex >= CurrentQuestion.Options.Count)
                throw new ArgumentOutOfRangeException(nameof(selectedOptionIndex), "Invalid option index.");

            var question = CurrentQuestion;
            bool isCorrect = selectedOptionIndex == question.CorrectAnswerIndex;

            // Record the answer
            _scoreManager.RecordAnswer(question, isCorrect);
            TrackUserEngagement(question, isCorrect);

            // Raise answer provided event
            AnswerProvided?.Invoke(this, new AnswerResultEventArgs
            {
                Question = question,
                IsCorrect = isCorrect,
                CurrentScore = _scoreManager.CurrentScore,
                TotalQuestions = _scoreManager.TotalQuestions,
                Feedback = isCorrect ? question.Feedback :
                    $"Incorrect. The correct answer is {question.GetCorrectAnswerLetter()}) {question.GetCorrectAnswer()}"
            });

            // Move to next question or complete quiz
            _currentQuestionIndex++;

            if (_currentQuestionIndex >= _currentQuestions.Count)
            {
                CompleteQuiz();
            }
            else
            {
                AskCurrentQuestion();
            }
        }

        /// <summary>
        /// Get available topics
        /// </summary>
        public List<string> GetAvailableTopics()
        {
            return _questionBank.GetAllTopics();
        }

        /// <summary>
        /// Get total question count for a topic
        /// </summary>
        public int GetTopicQuestionCount(string topic)
        {
            return _questionBank.GetQuestionsByTopic(topic).Count;
        }

        /// <summary>
        /// Reset the current quiz
        /// </summary>
        public void ResetQuiz()
        {
            _scoreManager.ResetScore();
            _currentQuestions.Clear();
            _currentQuestionIndex = 0;
            _quizInProgress = false;

            QuizReset?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Get user summary for display
        /// </summary>
        public Dictionary<string, object> GetUserSummary()
        {
            var summary = _userMemory.GetUserSummary();
            var favoriteTopic = _userMemory.GetFavoriteTopic();
            var topTopics = _userMemory.GetTopFavoriteTopics(3);
            var weakTopics = _scoreManager.GetTopicsNeedingImprovement();

            var result = new Dictionary<string, object>(summary.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value))
            {
                ["favorite_topic"] = favoriteTopic ?? "",
                ["top_topics"] = topTopics,
                ["weak_topics"] = weakTopics,
                ["performance_feedback"] = _scoreManager.GetPerformanceFeedback(),
                ["final_score"] = _scoreManager.CurrentScore,
                ["total_questions"] = _scoreManager.TotalQuestions,
                ["percentage"] = _scoreManager.CurrentPercentage
            };

            return result;
        }

        /// <summary>
        /// Get personalized recommendations
        /// </summary>
        public List<string> GetPersonalizedRecommendations()
        {
            var recommendations = new List<string>();
            var favoriteTopic = _userMemory.GetFavoriteTopic();
            var recentSentiment = _userMemory.GetRecentSentiment();
            var weakTopics = _scoreManager.GetTopicsNeedingImprovement();

            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                recommendations.Add($"Since you're interested in {favoriteTopic}, consider taking advanced courses in this area.");
            }

            if (weakTopics.Count > 0)
            {
                recommendations.Add($"Focus on improving your knowledge in: {string.Join(", ", weakTopics)}");
            }

            if (recentSentiment == "frustrated" || recentSentiment == "anxious")
            {
                recommendations.Add("Don't worry if some questions were challenging - cybersecurity is complex!");
                recommendations.Add("Practice makes perfect. Try taking the quiz again later.");
            }
            else if (recentSentiment == "positive")
            {
                recommendations.Add("Great job! You're building strong cybersecurity habits.");
                recommendations.Add("Consider sharing your knowledge with friends and family.");
            }

            recommendations.Add("Remember: Cybersecurity is everyone's responsibility!");
            recommendations.Add("Keep learning and stay updated on the latest security threats.");

            return recommendations;
        }

        private void AskCurrentQuestion()
        {
            if (CurrentQuestion != null)
            {
                QuestionAsked?.Invoke(this, new QuestionAskedEventArgs
                {
                    Question = CurrentQuestion,
                    QuestionNumber = _currentQuestionIndex + 1,
                    TotalQuestions = _currentQuestions.Count
                });
            }
        }

        private void CompleteQuiz()
        {
            _quizInProgress = false;

            _userMemory.RecordQuizResult(
                _scoreManager.CurrentScore,
                _scoreManager.TotalQuestions,
                _scoreManager.CurrentPercentage
            );

            QuizCompleted?.Invoke(this, new QuizCompletedEventArgs
            {
                FinalScore = _scoreManager.CurrentScore,
                TotalQuestions = _scoreManager.TotalQuestions,
                Percentage = _scoreManager.CurrentPercentage,
                PerformanceFeedback = _scoreManager.GetPerformanceFeedback(),
                UserSummary = GetUserSummary()
            });
        }

        private void TrackUserEngagement(QuizQuestion question, bool isCorrect)
        {
            _userMemory.AddDiscussedTopic(question.Topic);

            if (isCorrect)
            {
                _userMemory.RecordPositiveTopicInteraction(question.Topic);
                _userMemory.RecordSentiment("positive");
            }
            else
            {
                _userMemory.RecordSentiment("frustrated");
            }

            _userMemory.RecordTopicQuestion(question.Topic);
        }
    }

    /// <summary>
    /// Enhanced QuizQuestion class with GUI-specific methods
    /// </summary>
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Topic { get; set; }
        public string Explanation { get; set; }
        public string Feedback { get; set; }

        public QuizQuestion()
        {
            Options = new List<string>();
        }

        /// <summary>
        /// Validates if the question is properly configured
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Question) &&
                   Options.Count >= 2 &&
                   CorrectAnswerIndex >= 0 &&
                   CorrectAnswerIndex < Options.Count &&
                   !string.IsNullOrEmpty(Topic);
        }

        /// <summary>
        /// Gets the correct answer text
        /// </summary>
        public string GetCorrectAnswer()
        {
            return CorrectAnswerIndex >= 0 && CorrectAnswerIndex < Options.Count
                ? Options[CorrectAnswerIndex]
                : "";
        }

        /// <summary>
        /// Gets the option letter (A, B, C, D) for the correct answer
        /// </summary>
        public char GetCorrectAnswerLetter()
        {
            return (char)('A' + CorrectAnswerIndex);
        }

        /// <summary>
        /// Format question for GUI display with HTML-like formatting
        /// </summary>
        public string GetFormattedQuestion()
        {
            return Question?.Replace("\n", Environment.NewLine) ?? "";
        }

        /// <summary>
        /// Get options with letter prefixes for GUI
        /// </summary>
        public List<string> GetFormattedOptions()
        {
            var formattedOptions = new List<string>();
            for (int i = 0; i < Options.Count; i++)
            {
                formattedOptions.Add($"{(char)('A' + i)}) {Options[i]}");
            }
            return formattedOptions;
        }
    }

    /// <summary>
    /// Enhanced QuestionBank class (keeping original functionality)
    /// </summary>
    public class QuestionBank
    {
        private readonly List<QuizQuestion> _questions;

        public QuestionBank()
        {
            _questions = new List<QuizQuestion>();
            InitializeQuestions();
        }

        /// <summary>
        /// Gets a random selection of questions for the quiz
        /// </summary>
        public List<QuizQuestion> GetRandomQuestions(int count)
        {
            return _questions.OrderBy(x => Guid.NewGuid()).Take(count).ToList();
        }

        /// <summary>
        /// Gets questions by specific topic
        /// </summary>
        public List<QuizQuestion> GetQuestionsByTopic(string topic)
        {
            return _questions.Where(q => q.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Gets all available topics
        /// </summary>
        public List<string> GetAllTopics()
        {
            return _questions.Select(q => q.Topic).Distinct().OrderBy(t => t).ToList();
        }

        /// <summary>
        /// Gets total number of questions available
        /// </summary>
        public int GetTotalQuestionCount()
        {
            return _questions.Count;
        }

        /// <summary>
        /// Get questions count by topic
        /// </summary>
        public Dictionary<string, int> GetTopicQuestionCounts()
        {
            return _questions.GroupBy(q => q.Topic)
                           .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Initialize all cybersecurity questions
        /// </summary>
        private void InitializeQuestions()
        {
            // Phishing Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                CorrectAnswerIndex = 2,
                Topic = "phishing",
                Explanation = "Reporting phishing emails helps prevent scams and protects others.",
                Feedback = "Correct! Reporting phishing emails helps prevent scams."
            });

            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following is a common sign of a phishing email?",
                Options = new List<string> { "Professional formatting", "Urgent language and threats", "Correct spelling", "Valid sender address" },
                CorrectAnswerIndex = 1,
                Topic = "phishing",
                Explanation = "Phishing emails often use urgency and fear tactics to pressure victims.",
                Feedback = "Great job! You're learning to stay safe online!"
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What is a common characteristic of phishing websites?",
                Options = new List<string> { "They have HTTPS certificates", "They mimic legitimate websites", "They load very slowly", "They have no images" },
                CorrectAnswerIndex = 1,
                Topic = "phishing",
                Explanation = "Phishing websites often look like legitimate sites to trick users into entering credentials.",
                Feedback = "Excellent! Always verify website authenticity."
            });

            // Password Security Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What makes a strong password?",
                Options = new List<string> { "Your birthday", "At least 12 characters with mixed case, numbers, and symbols", "Your pet's name", "123456789" },
                CorrectAnswerIndex = 1,
                Topic = "password security",
                Explanation = "Strong passwords are long and complex, making them harder to crack.",
                Feedback = "Excellent! You understand password security principles."
            });

            _questions.Add(new QuizQuestion
            {
                Question = "How often should you change your passwords?",
                Options = new List<string> { "Never", "Every day", "When there's a security breach or concern", "Every hour" },
                CorrectAnswerIndex = 2,
                Topic = "password security",
                Explanation = "Regular password changes aren't always necessary, but should be done when security is compromised.",
                Feedback = "Keep learning to stay safe online!"
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What is the best practice for managing multiple passwords?",
                Options = new List<string> { "Use the same password everywhere", "Write them on paper", "Use a password manager", "Memorize them all" },
                CorrectAnswerIndex = 2,
                Topic = "password security",
                Explanation = "Password managers securely store and generate unique passwords for each account.",
                Feedback = "Smart choice! Password managers are essential security tools."
            });

            // Social Engineering Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What is social engineering in cybersecurity?",
                Options = new List<string> { "Building social networks", "Manipulating people to reveal confidential information", "Engineering social media", "Creating user profiles" },
                CorrectAnswerIndex = 1,
                Topic = "social engineering",
                Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities.",
                Feedback = "Great job! You're becoming a cybersecurity pro!"
            });

            _questions.Add(new QuizQuestion
            {
                Question = "A stranger calls claiming to be from IT support and asks for your login credentials. What should you do?",
                Options = new List<string> { "Provide the information immediately", "Hang up and verify through official channels", "Ask for their employee ID", "Give them a fake password" },
                CorrectAnswerIndex = 1,
                Topic = "social engineering",
                Explanation = "Always verify the identity of anyone requesting sensitive information through official channels.",
                Feedback = "Perfect! Never trust unsolicited requests for credentials."
            });

            // Safe Browsing Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What should you look for to verify a website is secure?",
                Options = new List<string> { "Colorful design", "HTTPS in the URL and a padlock icon", "Lots of advertisements", "Pop-up windows" },
                CorrectAnswerIndex = 1,
                Topic = "safe browsing",
                Explanation = "HTTPS encrypts data between your browser and the website.",
                Feedback = "Excellent! You know how to browse safely."
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What should you do if you accidentally visit a suspicious website?",
                Options = new List<string> { "Continue browsing", "Close the browser immediately", "Enter your personal information", "Download files from the site" },
                CorrectAnswerIndex = 1,
                Topic = "safe browsing",
                Explanation = "Closing suspicious websites immediately prevents potential malware downloads.",
                Feedback = "Smart thinking! Quick action prevents security risks."
            });

            // Malware Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What is malware?",
                Options = new List<string> { "Good software", "Malicious software designed to harm computers", "Email attachments", "Web browsers" },
                CorrectAnswerIndex = 1,
                Topic = "malware",
                Explanation = "Malware includes viruses, trojans, ransomware, and other harmful software.",
                Feedback = "Correct! Understanding malware helps you stay protected."
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What is ransomware?",
                Options = new List<string> { "Free software", "Malware that encrypts files and demands payment", "A type of antivirus", "A web browser" },
                CorrectAnswerIndex = 1,
                Topic = "malware",
                Explanation = "Ransomware locks your files and demands payment for the decryption key.",
                Feedback = "Correct! Ransomware is a serious threat to be aware of."
            });

            // WiFi Security Questions
            _questions.Add(new QuizQuestion
            {
                Question = "Is it safe to use public WiFi for online banking?",
                Options = new List<string> { "Yes, always", "No, never without a VPN", "Only on weekends", "Only during business hours" },
                CorrectAnswerIndex = 1,
                Topic = "wifi security",
                Explanation = "Public WiFi can be monitored by attackers, making sensitive activities risky.",
                Feedback = "Great! You understand WiFi security risks."
            });

            _questions.Add(new QuizQuestion
            {
                Question = "What is the most secure type of WiFi encryption?",
                Options = new List<string> { "WEP", "WPA", "WPA2", "WPA3" },
                CorrectAnswerIndex = 3,
                Topic = "wifi security",
                Explanation = "WPA3 is the latest and most secure WiFi encryption standard.",
                Feedback = "Excellent! Always use the strongest encryption available."
            });

            // Data Protection Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What is the best way to protect sensitive data?",
                Options = new List<string> { "Share it with everyone", "Store it in plain text", "Encrypt it and use strong access controls", "Post it online" },
                CorrectAnswerIndex = 2,
                Topic = "data protection",
                Explanation = "Encryption and access controls are fundamental data protection measures.",
                Feedback = "Excellent! You're a cybersecurity champion!"
            });

            // Two-Factor Authentication Questions
            _questions.Add(new QuizQuestion
            {
                Question = "What is two-factor authentication (2FA)?",
                Options = new List<string> { "Using two passwords", "An additional security layer requiring a second form of verification", "Two different browsers", "Two email accounts" },
                CorrectAnswerIndex = 1,
                Topic = "two-factor authentication",
                Explanation = "2FA adds an extra security layer beyond just passwords.",
                Feedback = "Perfect! 2FA significantly improves account security."
            });

            // Software Updates Questions
            _questions.Add(new QuizQuestion
            {
                Question = "Why are software updates important for security?",
                Options = new List<string> { "They make software slower", "They fix security vulnerabilities", "They add advertisements", "They use more storage" },
                CorrectAnswerIndex = 1,
                Topic = "software updates",
                Explanation = "Updates often include critical security patches that fix known vulnerabilities.",
                Feedback = "Great understanding! Keep your software updated."
            });

            // --- Additional Questions ---

            // Phishing
            _questions.Add(new QuizQuestion
            {
                Question = "If a link in an email looks suspicious, what should you do before clicking?",
                Options = new List<string> { "Click it to see where it goes", "Hover over the link to check the URL", "Forward it to friends", "Ignore the email" },
                CorrectAnswerIndex = 1,
                Topic = "phishing",
                Explanation = "Hovering over links lets you see the real destination before clicking.",
                Feedback = "Good job! Always check links before clicking."
            });

            // Password Security
            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following is NOT a good password practice?",
                Options = new List<string> { "Using unique passwords for each account", "Sharing your password with a trusted friend", "Enabling two-factor authentication", "Using a password manager" },
                CorrectAnswerIndex = 1,
                Topic = "password security",
                Explanation = "You should never share your password, even with trusted friends.",
                Feedback = "Correct! Passwords should always be kept private."
            });

            // Social Engineering
            _questions.Add(new QuizQuestion
            {
                Question = "Which is an example of social engineering?",
                Options = new List<string> { "A hacker guessing your password", "A fake IT person calling to ask for your credentials", "A virus infecting your computer", "A firewall blocking traffic" },
                CorrectAnswerIndex = 1,
                Topic = "social engineering",
                Explanation = "Social engineering manipulates people, not technology.",
                Feedback = "Well done! Recognizing social engineering is key."
            });

            // Safe Browsing
            _questions.Add(new QuizQuestion
            {
                Question = "What should you do before entering personal information on a website?",
                Options = new List<string> { "Check for HTTPS and a padlock icon", "Check if the site looks colorful", "See if there are pop-ups", "Nothing, just enter it" },
                CorrectAnswerIndex = 0,
                Topic = "safe browsing",
                Explanation = "HTTPS and a padlock icon indicate a secure connection.",
                Feedback = "Great! Always check for secure connections."
            });

            // Malware
            _questions.Add(new QuizQuestion
            {
                Question = "Which action can help prevent malware infections?",
                Options = new List<string> { "Opening email attachments from unknown senders", "Keeping your software updated", "Clicking on pop-up ads", "Disabling your antivirus" },
                CorrectAnswerIndex = 1,
                Topic = "malware",
                Explanation = "Software updates often patch vulnerabilities that malware exploits.",
                Feedback = "Correct! Updates are important for security."
            });

            // Data Protection
            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following is a safe way to dispose of sensitive documents?",
                Options = new List<string> { "Throw them in the trash", "Shred them", "Leave them on your desk", "Give them to a friend" },
                CorrectAnswerIndex = 1,
                Topic = "data protection",
                Explanation = "Shredding ensures sensitive information cannot be reconstructed.",
                Feedback = "Excellent! Shredding protects your data."
            });

            // --- New Questions ---

            // Phishing
            _questions.Add(new QuizQuestion
            {
                Question = "What is 'spear phishing'?",
                Options = new List<string> { "A phishing attack targeting a specific individual or organization", "A phishing attack using phone calls", "A phishing attack using social media", "A phishing attack with malware attachments" },
                CorrectAnswerIndex = 0,
                Topic = "phishing",
                Explanation = "Spear phishing is a targeted phishing attack aimed at a specific person or organization.",
                Feedback = "Correct! Spear phishing is highly targeted and dangerous."
            });

            // Password Security
            _questions.Add(new QuizQuestion
            {
                Question = "Why should you avoid using personal information in your passwords?",
                Options = new List<string> { "It's hard to remember", "It makes passwords easier to guess", "Websites don't allow it", "It takes too long to type" },
                CorrectAnswerIndex = 1,
                Topic = "password security",
                Explanation = "Personal information can be easily found or guessed by attackers.",
                Feedback = "Good! Avoiding personal info makes your passwords stronger."
            });

            // Social Engineering
            _questions.Add(new QuizQuestion
            {
                Question = "Which of the following is a common social engineering tactic?",
                Options = new List<string> { "Offering free gifts", "Using technical jargon", "Sending encrypted emails", "Ignoring security policies" },
                CorrectAnswerIndex = 0,
                Topic = "social engineering",
                Explanation = "Social engineers often offer free gifts to lure victims.",
                Feedback = "Correct! Be wary of offers that seem too good to be true."
            });

            // Safe Browsing
            _questions.Add(new QuizQuestion
            {
                Question = "What is a browser extension?",
                Options = new List<string> { "A type of malware", "A small software add-on that adds features to your browser", "A new browser window", "A pop-up ad" },
                CorrectAnswerIndex = 1,
                Topic = "safe browsing",
                Explanation = "Browser extensions add functionality but can also pose security risks if not trusted.",
                Feedback = "Great! Only install extensions from trusted sources."
            });

            // Malware
            _questions.Add(new QuizQuestion
            {
                Question = "What should you do if your computer is infected with malware?",
                Options = new List<string> { "Ignore it", "Run a reputable antivirus scan", "Unplug your monitor", "Delete random files" },
                CorrectAnswerIndex = 1,
                Topic = "malware",
                Explanation = "Running a reputable antivirus scan is the best first step.",
                Feedback = "Correct! Antivirus software can help remove malware."
            });

            // Data Protection
            _questions.Add(new QuizQuestion
            {
                Question = "What is data encryption?",
                Options = new List<string> { "Deleting data", "Converting data into a coded form to prevent unauthorized access", "Backing up data", "Sharing data online" },
                CorrectAnswerIndex = 1,
                Topic = "data protection",
                Explanation = "Encryption protects data by making it unreadable without the correct key.",
                Feedback = "Excellent! Encryption is vital for data security."
            });
        }
    }

    /// <summary>
    /// Enhanced QuizScoreManager (keeping original functionality)
    /// </summary>
    public class QuizScoreManager
    {
        private int _currentScore;
        private int _totalQuestions;
        private readonly List<QuizResult> _results;

        public int CurrentScore => _currentScore;
        public int TotalQuestions => _totalQuestions;
        public double CurrentPercentage => _totalQuestions > 0 ? (_currentScore / (double)_totalQuestions) * 100 : 0;

        public QuizScoreManager()
        {
            _results = new List<QuizResult>();
            ResetScore();
        }

        /// <summary>
        /// Record a question result
        /// </summary>
        public void RecordAnswer(QuizQuestion question, bool isCorrect)
        {
            _results.Add(new QuizResult
            {
                Question = question,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.Now
            });

            if (isCorrect)
            {
                _currentScore++;
            }

            _totalQuestions++;
        }

        /// <summary>
        /// Reset the score for a new quiz
        /// </summary>
        public void ResetScore()
        {
            _currentScore = 0;
            _totalQuestions = 0;
            _results.Clear();
        }

        /// <summary>
        /// Get feedback message based on current performance
        /// </summary>
        public string GetPerformanceFeedback()
        {
            double percentage = CurrentPercentage;

            switch (percentage)
            {
                case double p when p >= 90:
                    return "Outstanding! You're a cybersecurity expert! 🏆";
                case double p when p >= 80:
                    return "Excellent work! You have strong cybersecurity knowledge. 🌟";
                case double p when p >= 70:
                    return "Good job! You're on the right track to staying safe online. 👍";
                case double p when p >= 60:
                    return "Not bad! Keep learning to improve your cybersecurity awareness. 📚";
                case double p when p >= 50:
                    return "You're getting there! Review the topics and try again. 💪";
                default:
                    return "Keep practicing! Cybersecurity knowledge is crucial for staying safe online. 🔒";
            }
        }

        /// <summary>
        /// Get detailed performance analysis
        /// </summary>
        public Dictionary<string, int> GetTopicPerformance()
        {
            var topicScores = new Dictionary<string, int>();
            var topicTotals = new Dictionary<string, int>();

            foreach (var result in _results)
            {
                string topic = result.Question.Topic;

                if (!topicScores.ContainsKey(topic))
                {
                    topicScores[topic] = 0;
                    topicTotals[topic] = 0;
                }

                if (result.IsCorrect)
                {
                    topicScores[topic]++;
                }
                topicTotals[topic]++;
            }

            return topicScores;
        }

        /// <summary>
        /// Get topics that need improvement
        /// </summary>
        public List<string> GetTopicsNeedingImprovement()
        {
            var topicPerformance = GetTopicPerformance();
            var topicTotals = new Dictionary<string, int>();

            foreach (var result in _results)
            {
                string topic = result.Question.Topic;
                if (!topicTotals.ContainsKey(topic))
                    topicTotals[topic] = 0;
                topicTotals[topic]++;
            }

            var weakTopics = new List<string>();
            foreach (var kvp in topicPerformance)
            {
                double percentage = (kvp.Value / (double)topicTotals[kvp.Key]) * 100;
                if (percentage < 70) // Less than 70% correct
                {
                    weakTopics.Add(kvp.Key);
                }
            }

            return weakTopics;
        }

        /// <summary>
        /// Inner class to track individual question results
        /// </summary>
        private class QuizResult
        {
            public QuizQuestion Question { get; set; }
            public bool IsCorrect { get; set; }
            public DateTime AnsweredAt { get; set; }
        }
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//