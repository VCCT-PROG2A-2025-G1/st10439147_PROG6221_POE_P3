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


using ST10439147_PROG6221_POE_P3.MyClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ST10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// Enhanced version with comprehensive sentiment-aware response generation
    /// Improved for better user experience and accessibility
    /// </summary>
    public class EnhancedResponses : Responses
    {
        // Dictionary for follow-up questions for each topic
        private readonly Dictionary<string, List<string>> _followUpQuestions;

        // Dictionary for topic transitions to make conversation flow more naturally
        private readonly Dictionary<string, string> _topicTransitions;

        // Dictionary for greeting variations to create more natural conversations
        private readonly Dictionary<string, List<string>> _greetingResponses;

        // Enhanced sentiment-specific response templates
        private readonly Dictionary<string, Dictionary<string, List<string>>> _sentimentResponses;

        // Use the separate UserMemory and Sentiment classes
        private readonly UserMemory _userMemory;
        private readonly Sentiment _sentimentAnalyzer;

        // Track the current conversational context
        private string _currentTopic = "";
        private bool _followUpAsked = false;
        private DateTime _lastResponseTime;
        private string _lastSentiment = "neutral";

        // Random number generator for selecting variations
        private readonly Random _random;

        private string _previousUserInput = "";
        private int _consecutiveQuestionsCount = 0;
        private List<string> _recentTopics = new List<string>();

        private readonly Dictionary<string, List<string>> _conversationConnectors;
        private readonly Dictionary<string, List<string>> _personalizedGreetings;
        private readonly List<string> _empathyPhrases;
        private readonly List<string> _encouragementPhrases;

        // NEW: User-friendly additions
        private readonly Dictionary<string, List<string>> _quickHelp;
        private readonly Dictionary<string, string> _topicDefinitions;
        private readonly List<string> _supportivePhrases;
        private readonly Dictionary<string, List<string>> _simplifiedExplanations;
        private int _conversationLength = 0;
        private bool _userNeedsBreak = false;
        private string _lastFollowUpQuestion = "";



        /// <summary>
        /// Enhanced constructor with comprehensive sentiment response templates
        /// </summary>
        // This constructor initializes the EnhancedResponse class with all necessary components
        // and sets up the sentiment-aware response generation system.
        // base() calls the base class constructor
        public EnhancedResponses(UserMemory userMemory) : base()
        {
            _random = new Random();
            _lastResponseTime = DateTime.Now;

            // Initialize the separate classes
            _userMemory = userMemory;
            _sentimentAnalyzer = new Sentiment();

            // Initialize collections
            _sentimentResponses = new Dictionary<string, Dictionary<string, List<string>>>();
            _greetingResponses = new Dictionary<string, List<string>>();
            _topicTransitions = new Dictionary<string, string>();
            _followUpQuestions = new Dictionary<string, List<string>>();
            _conversationConnectors = new Dictionary<string, List<string>>();
            _personalizedGreetings = new Dictionary<string, List<string>>();
            _empathyPhrases = new List<string>();
            _encouragementPhrases = new List<string>();

            // NEW: Initialize user-friendly features
            _quickHelp = new Dictionary<string, List<string>>();
            _topicDefinitions = new Dictionary<string, string>();
            _supportivePhrases = new List<string>();
            _simplifiedExplanations = new Dictionary<string, List<string>>();

            // Initialize all collections
            InitializeSentimentResponses();
            InitializeGreetingResponses();
            InitializeTopicTransitions();
            InitializeFollowUpQuestions();
            InitializeConversationConnectors();
            InitializePersonalizedGreetings();
            InitializeEmpathyPhrases();
            InitializeEncouragementPhrases();


            InitializeQuickHelp();
            InitializeTopicDefinitions();
            InitializeSupportivePhrases();
            InitializeSimplifiedExplanations();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeQuickHelp()
        {
            _quickHelp.Add("help", new List<string>
            {
                "I can help you with: passwords, phishing, malware, privacy, and ransomware. Just ask me about any of these!",
                "Need help? Try asking: 'How do I create a strong password?' or 'What is phishing?'",
                "I'm here to make cybersecurity simple! Ask me about any security topic you're curious about."
            });

            _quickHelp.Add("topics", new List<string>
            {
                "Popular topics I can explain: Password Security, Phishing Protection, Malware Defense, Privacy Settings, Ransomware Prevention",
                "I specialize in: Strong passwords, Email security, Safe browsing, Data protection, and Backup strategies",
                "What interests you most? Password tips, spotting scams, or protecting your privacy?"
            });

            _quickHelp.Add("examples", new List<string>
            {
                "Try asking: 'How do I spot a phishing email?' or 'What makes a password strong?'",
                "Example questions: 'Is my password secure?' or 'How do I protect my privacy online?'",
                "Sample topics: 'Explain malware' or 'How does ransomware work?'"
            });
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeTopicDefinitions()
        {
            _topicDefinitions.Add("phishing", "Phishing is when criminals send fake emails or messages to trick you into giving away personal information like passwords or credit card numbers.");
            _topicDefinitions.Add("malware", "Malware is malicious software - basically bad programs that can harm your computer, steal your information, or cause other problems.");
            _topicDefinitions.Add("ransomware", "Ransomware is a type of malware that locks your files and demands payment to unlock them - like digital kidnapping of your data.");
            _topicDefinitions.Add("vpn", "A VPN (Virtual Private Network) creates a secure, private connection when you're online - like a secure tunnel for your internet traffic.");
            _topicDefinitions.Add("2fa", "Two-factor authentication (2FA) adds an extra security step when logging in - like needing both your password AND a code from your phone.");
            _topicDefinitions.Add("firewall", "A firewall is like a security guard for your computer - it monitors and controls what information can come in and go out.");
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeSupportivePhrases()
        {
            _supportivePhrases.AddRange(new List<string>
            {
                "You're asking great questions - that's how you stay secure!",
                "Don't worry, everyone starts somewhere with cybersecurity.",
                "It's totally normal to feel overwhelmed by security topics at first.",
                "You're being smart by learning about this stuff!",
                "Every small step you take makes you more secure online.",
                "There's no such thing as a dumb security question!",
                "You're doing exactly the right thing by asking about this."
            });
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeSimplifiedExplanations()
        {
            _simplifiedExplanations.Add("password", new List<string>
            {
                "Think of a password like a key to your house - you want it to be unique and hard for others to guess.",
                "A strong password is like a really complicated lock - it should be long, mix different types of characters, and be unique for each account.",
                "Password tips made simple: Make it long (12+ characters), mix letters/numbers/symbols, and don't reuse it anywhere else."
            });

            _simplifiedExplanations.Add("phishing", new List<string>
            {
                "Phishing is like someone pretending to be your friend to get your house key - they pretend to be a trusted company to steal your login info.",
                "Think of phishing emails like fake letters from your bank - they look real but are trying to trick you into giving away your personal info.",
                "Phishing simplified: If an email asks for passwords or personal info, it's probably fake - real companies don't ask for this via email."
            });

            _simplifiedExplanations.Add("malware", new List<string>
            {
                "Malware is like germs for your computer - it's bad software that can make your device 'sick' or steal your information.",
                "Think of malware as digital pests - like viruses, they can spread, cause damage, and are hard to get rid of without the right tools.",
                "Malware simplified: It's bad software that sneaks onto your device to cause problems or steal your stuff."
            });
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Initialize conversation connectors to make transitions smoother
        // this method sets up various phrases that can be used to connect different parts of the conversation
        private void InitializeConversationConnectors()
        {
            _conversationConnectors.Add("topic_switch", new List<string>
            {
                "Speaking of that,", "That reminds me,", "On a related note,",
                "Building on that,", "Similarly,", "Along those lines,"
            });

            _conversationConnectors.Add("clarification", new List<string>
            {
                "Let me explain that better,", "To put it another way,",
                "What I mean is,", "In simpler terms,", "To clarify,"
            });

            _conversationConnectors.Add("continuation", new List<string>
            {
                "Also,", "Additionally,", "Furthermore,", "Another thing to consider is,",
                "It's also worth noting that,"
            });

            _conversationConnectors.Add("agreement", new List<string>
            {
                "You're absolutely right,", "That's a great point,", "Exactly,",
                "I agree,", "That's very true,"
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializePersonalizedGreetings()
        {
            _personalizedGreetings.Add("returning_user", new List<string>
            {
                "Welcome back! I remember we talked about {topic} before.",
                "Good to see you again! How are things going with {topic}?",
                "Hello again! Ready to continue learning about cybersecurity?",
                "Nice to have you back! Any new security questions today?"
            });

            _personalizedGreetings.Add("topic_expert", new List<string>
            {
                "Hi there! I see you're really interested in {topic}.",
                "Hello! You've been asking great questions about {topic}.",
                "Welcome! Ready for more {topic} insights?",
                "Hi! I love your enthusiasm for {topic}."
            });

            _personalizedGreetings.Add("first_time", new List<string>
            {
                "Hello! I'm excited to help you learn about cybersecurity.",
                "Welcome! I'm here to make cybersecurity easy to understand.",
                "Hi there! Let's explore cybersecurity together.",
                "Hello! I'm your friendly cybersecurity guide."
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeEmpathyPhrases()
        {
            _empathyPhrases.AddRange(new List<string>
            {
                "I completely understand how you feel about this.",
                "Your concerns are totally valid.",
                "Many people feel the same way about this topic.",
                "It's natural to feel that way.",
                "I can see why this would be important to you.",
                "Your experience sounds really frustrating.",
                "That must be concerning for you.",
                "I hear what you're saying.",
                "Your feelings about this make perfect sense."
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private void InitializeEncouragementPhrases()
        {
            _encouragementPhrases.AddRange(new List<string>
            {
                "You're asking all the right questions!",
                "That's exactly the kind of thinking that keeps you secure!",
                "You're really getting the hang of this!",
                "Your security awareness is improving!",
                "Keep up that careful approach!",
                "You're becoming quite the security expert!",
                "That's a smart way to think about it!",
                "You're on the right track!",
                "Great question - that shows you're thinking like a security pro!"
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Initialize comprehensive sentiment-specific response templates
        /// </summary>
        private void InitializeSentimentResponses()
        {
            // Positive sentiment responses
            var positiveResponses = new Dictionary<string, List<string>>();
            positiveResponses.Add("acknowledgment", new List<string> { "I'm glad you're enthusiastic about this!", "It's great to see your positive attitude!", "Your enthusiasm is wonderful!" });
            positiveResponses.Add("encouragement", new List<string> { "Keep up that positive energy!", "You're on the right track!", "That's the spirit!" });
            positiveResponses.Add("transition", new List<string> { "Since you're feeling confident, let's explore more!", "With that positive attitude, you'll master this topic quickly!", "Great! Let's dive deeper into this." });
            _sentimentResponses.Add("positive", positiveResponses);

            // Negative sentiment responses
            var negativeResponses = new Dictionary<string, List<string>>();
            negativeResponses.Add("acknowledgment", new List<string> { "I understand this might be concerning.", "I can see why this would be troubling.", "These issues can indeed be worrying." });
            negativeResponses.Add("reassurance", new List<string> { "Don't worry, we can work through this together.", "These problems have solutions.", "You're taking the right steps by learning about this." });
            negativeResponses.Add("transition", new List<string> { "Let me help ease your concerns with some practical advice.", "Here's what you can do to address these issues.", "I'll guide you through solving this step by step." });
            _sentimentResponses.Add("negative", negativeResponses);

            // Anxious sentiment responses
            var anxiousResponses = new Dictionary<string, List<string>>();
            anxiousResponses.Add("acknowledgment", new List<string> { "I understand you're worried about this.", "Your concerns are completely valid.", "It's natural to feel anxious about cybersecurity." });
            anxiousResponses.Add("reassurance", new List<string> { "Take a deep breath - we'll tackle this together.", "Knowledge is power, and you're gaining that power now.", "Once you understand the basics, you'll feel much more confident." });
            anxiousResponses.Add("transition", new List<string> { "Let me start with simple, actionable steps to help you feel more secure.", "I'll break this down into manageable pieces for you.", "Here are some immediate steps you can take to feel safer." });
            _sentimentResponses.Add("anxious", anxiousResponses);

            // Confused sentiment responses
            var confusedResponses = new Dictionary<string, List<string>>();
            confusedResponses.Add("acknowledgment", new List<string> { "I can see this might be confusing.", "Let me try to clarify this for you.", "That's a complex topic - let me break it down." });
            confusedResponses.Add("clarification", new List<string> { "Let me explain this in simpler terms.", "I'll walk you through this step by step.", "Think of it this way..." });
            confusedResponses.Add("transition", new List<string> { "Here's a clearer explanation:", "Let me use a simple example to illustrate this:", "I'll break this down into easy-to-understand parts:" });
            _sentimentResponses.Add("confused", confusedResponses);

            // Frustrated sentiment responses
            var frustratedResponses = new Dictionary<string, List<string>>();
            frustratedResponses.Add("acknowledgment", new List<string> { "I can sense your frustration.", "I understand this is irritating.", "These technical issues can be really annoying." });
            frustratedResponses.Add("empathy", new List<string> { "You're not alone in feeling this way.", "Many people struggle with these same issues.", "Your frustration is completely understandable." });
            frustratedResponses.Add("solution", new List<string> { "Let me help you find a solution that works.", "Here's a straightforward approach that should help.", "I'll give you a simple fix for this problem." });
            _sentimentResponses.Add("frustrated", frustratedResponses);

            // Neutral sentiment responses
            var neutralResponses = new Dictionary<string, List<string>>();
            neutralResponses.Add("standard", new List<string> { "Here's what you need to know:", "Let me explain this:", "This is an important topic:" });
            neutralResponses.Add("engagement", new List<string> { "Would you like me to elaborate on any part?", "Does this make sense so far?", "What specific aspect interests you most?" });
            _sentimentResponses.Add("neutral", neutralResponses);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Initialize greeting responses with sentiment awareness
        /// </summary>
        private void InitializeGreetingResponses()
        {
            var helloResponses = new List<string>
            {
                "Hi there! How can I help with your cybersecurity questions today?",
                "Hello! What cybersecurity topic would you like to discuss?",
                "Greetings! I'm here to help with any cybersecurity concerns you might have.",
                "Hi! What can I help you with regarding online security today?"
            };
            _greetingResponses.Add("hello", helloResponses);

            var goodbyeResponses = new List<string>
            {
                "Take care and stay secure online!",
                "Goodbye! Remember to keep your digital life safe.",
                "Until next time! Don't forget to update your passwords regularly.",
                "Bye for now! Feel free to return if you have more cybersecurity questions."
            };
            _greetingResponses.Add("goodbye", goodbyeResponses);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Initialize topic transitions
        /// </summary>
        private void InitializeTopicTransitions()
        {
            _topicTransitions.Add("phishing", "Speaking of phishing, having strong passwords is another important defense against unauthorized access to your accounts.");
            _topicTransitions.Add("pharming", "Pharming is a cyberattack that redirects users from legitimate websites to fraudulent ones to steal sensitive information. Using secure DNS services and keeping your devices updated can help protect you from pharming.");
            _topicTransitions.Add("password", "Beyond strong passwords, enabling two-factor authentication adds an extra layer of security to your accounts.");
            _topicTransitions.Add("malware", "To protect against malware effectively, keeping your software and operating system updated is essential.");
            _topicTransitions.Add("ransomware", "The best defense against ransomware is having regular, secure backups of your important data.");
            _topicTransitions.Add("privacy", "For enhanced privacy, especially on public networks, a VPN can encrypt your connection and protect your data.");
            _topicTransitions.Add("firewall", "While firewalls are important, they work best as part of a comprehensive security strategy including strong passwords and updated software.");
            _topicTransitions.Add("antivirus", "Antivirus software is crucial, but remember to keep it updated regularly to protect against the latest threats.");
            _topicTransitions.Add("vpn", "VPNs provide excellent privacy protection, but you should still be cautious about the websites you visit and the information you share.");
            _topicTransitions.Add("2fa", "Two-factor authentication significantly improves account security, especially when combined with strong, unique passwords.");
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Initialize follow-up questions with sentiment variations
        /// </summary>
        private void InitializeFollowUpQuestions()
        {
            var phishingQuestions = new List<string>
            {
                "Have you encountered suspicious emails recently that might be phishing attempts?",
                "Would you like to know specific warning signs to identify phishing emails?",
                "Are you familiar with how to verify if an email sender is legitimate?"
            };
            _followUpQuestions.Add("phishing", phishingQuestions);

            var passwordQuestions = new List<string>
            {
                "Do you currently use different passwords for each of your important accounts?",
                "Have you considered using a password manager to help maintain secure passwords?",
                "Would you like specific tips on creating strong yet memorable passwords?"
            };
            _followUpQuestions.Add("password", passwordQuestions);

            var malwareQuestions = new List<string>
            {
                "Do you have reliable antivirus software installed on all your devices?",
                "Are you keeping your operating system and applications updated regularly?",
                "Would you like to learn about common warning signs that your device might be infected?"
            };
            _followUpQuestions.Add("malware", malwareQuestions);

            var privacyQuestions = new List<string>
            {
                "Have you reviewed your social media privacy settings recently?",
                "Would you like to learn about tools that can enhance your online privacy?",
                "Are there specific privacy concerns you're worried about online?"
            };
            _followUpQuestions.Add("privacy", privacyQuestions);

            var ransomwareQuestions = new List<string>
            {
                "Do you currently have a backup strategy for your important files?",
                "Would you like to know the immediate steps to take if you suspect a ransomware infection?",
                "Are you familiar with how ransomware typically infiltrates systems?"
            };
            _followUpQuestions.Add("ransomware", ransomwareQuestions);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method generates a response based on the user's input and sentiment
        private bool CheckIfUserNeedsHelp(string input)
        {
            string lowerInput = input.ToLower();
            string[] helpIndicators = { "help", "confused", "don't understand", "explain", "what do you mean",
                                      "i don't get it", "huh", "what", "??", "unclear", "lost" };

            return helpIndicators.Any(indicator => lowerInput.Contains(indicator));
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // This method generates a response based on the user's input and sentiment
        private string ProvideContextualHelp(string currentTopic, string sentiment)
        {
            if (string.IsNullOrEmpty(currentTopic))
            {
                return GetRandomItem(_quickHelp["help"]) + " " + GetRandomItem(_quickHelp["examples"]);
            }

            // Provide topic definition if available
            if (_topicDefinitions.ContainsKey(currentTopic))
            {
                string definition = _topicDefinitions[currentTopic];
                string supportivePhrase = GetRandomItem(_supportivePhrases);

                return $"{supportivePhrase} Let me explain {currentTopic} simply: {definition}";
            }

            return $"I understand {currentTopic} can be confusing. Let me break it down in simpler terms for you.";
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private string CheckConversationFlow()
        {
            _conversationLength++;

            if (_conversationLength > 10 && !_userNeedsBreak)
            {
                _userNeedsBreak = true;
                return "We've covered a lot! Would you like to take a break, or shall we continue with another topic? ";
            }

            if (_consecutiveQuestionsCount > 5)
            {
                return "You're asking lots of great questions! Would you like me to summarize what we've discussed so far? ";
            }

            return "";
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        protected string ProvideLearningProgress()
        {
            var discussedTopics = _userMemory.GetDiscussedTopics();

            if (discussedTopics.Count == 0)
            {
                return "We're just getting started! What cybersecurity topic interests you most?";
            }

            string progress = "Great job! We've covered: " + string.Join(", ", discussedTopics) + ". ";
            progress += GetRandomItem(_encouragementPhrases);

            return progress;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private string AdjustResponseForSentiment(string baseResponse, string sentiment)
        {
            // Only apply adjustments if we haven't just applied them
            if (_lastSentiment == sentiment && _previousUserInput.ToLower().Contains("yes"))
            {
                // User just said "yes" - don't repeat the same sentiment adjustments
                return baseResponse;
            }

            string userName = _userMemory.GetUserInfo("name");
            string favoriteTopic = _userMemory.GetFavoriteTopic();

            bool isReturningUser = _userMemory.GetDiscussedTopics().Count > 3;
            bool hasStrongInterest = !string.IsNullOrEmpty(favoriteTopic) &&
                                   _userMemory.HasStrongInterestIn(favoriteTopic);

            string conversationContext = GetConversationContext();

            // Check if user needs extra support
            string supportiveResponse = "";
            if (sentiment == "confused" || sentiment == "frustrated")
            {
                supportiveResponse = GetRandomItem(_supportivePhrases) + " ";
            }

            // Build personalized prefix - but avoid repetition
            string personalizedPrefix = "";
            if (!_previousUserInput.ToLower().Contains("yes")) // Don't repeat if user just answered yes
            {
                personalizedPrefix = BuildPersonalizedPrefix(userName, sentiment, isReturningUser, hasStrongInterest, favoriteTopic);
            }

            string sentimentAdjustedResponse = supportiveResponse + ApplySentimentAdjustments(baseResponse, sentiment, personalizedPrefix, conversationContext);

            // Check conversation flow
            string flowCheck = CheckConversationFlow();
            if (!string.IsNullOrEmpty(flowCheck))
            {
                sentimentAdjustedResponse += flowCheck;
            }

            return sentimentAdjustedResponse;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Build a personalized prefix based on user information and sentiment
        // This method creates a personalized prefix for the response based on user information and sentiment
        private string BuildPersonalizedPrefix(string userName, string sentiment, bool isReturningUser,
                                             bool hasStrongInterest, string favoriteTopic)
        {
            List<string> prefixParts = new List<string>();

            // Add name if available
            if (!string.IsNullOrEmpty(userName))
            {
                prefixParts.Add(userName);
            }

            // Add sentiment-appropriate greeting or acknowledgment
            switch (sentiment)
            {
                case "positive":
                    if (hasStrongInterest)
                        prefixParts.Add($"I love your enthusiasm for {favoriteTopic}!");
                    else if (isReturningUser)
                        prefixParts.Add("Great to see your continued interest in cybersecurity!");
                    break;

                case "frustrated":
                    if (_consecutiveQuestionsCount > 2)
                        prefixParts.Add("I can see you're working through a lot of security concerns.");
                    else
                        prefixParts.Add(GetRandomItem(_empathyPhrases));
                    break;

                case "anxious":
                    prefixParts.Add("Take a deep breath - we'll work through this together.");
                    break;

                case "confused":
                    if (isReturningUser)
                        prefixParts.Add("Let me try a different approach to explain this.");
                    else
                        prefixParts.Add("No worries - let me break this down step by step.");
                    break;
            }

            return prefixParts.Count > 0 ? string.Join(", ", prefixParts) + " " : "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private string ApplySentimentAdjustments(string baseResponse, string sentiment,
                                               string personalizedPrefix, string conversationContext)
        {
            StringBuilder naturalResponse = new StringBuilder();

            // Add personalized prefix
            if (!string.IsNullOrEmpty(personalizedPrefix))
                naturalResponse.Append(personalizedPrefix);

            // Add conversation context connector if appropriate
            if (!string.IsNullOrEmpty(conversationContext))
                naturalResponse.Append(conversationContext + " ");

            // Apply sentiment-specific modifications to the base response
            string modifiedResponse = ModifyResponseForSentiment(baseResponse, sentiment);
            naturalResponse.Append(modifiedResponse);

            // Add sentiment-appropriate encouragement or support
            string encouragement = GetSentimentAppropriateEncouragement(sentiment);
            if (!string.IsNullOrEmpty(encouragement))
                naturalResponse.Append(" " + encouragement);

            return naturalResponse.ToString();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method modifies the response based on the user's sentiment
        // It provides different responses based on the sentiment of the user
        private string ModifyResponseForSentiment(string response, string sentiment)
        {
            switch (sentiment)
            {
                case "confused":
                    // Provide simplified explanation if available
                    string simplifiedVersion = GetSimplifiedExplanation(_currentTopic);
                    if (!string.IsNullOrEmpty(simplifiedVersion))
                    {
                        return simplifiedVersion;
                    }

                    // Make explanations simpler and more step-by-step
                    if (response.Contains("It's important") || response.Contains("You should"))
                    {
                        return response.Replace("It's important", "The key thing to remember is")
                                     .Replace("You should", "I'd recommend");
                    }
                    break;

                case "frustrated":
                    // Make responses more direct and solution-focused
                    if (response.Contains("Here's what you need to know"))
                    {
                        return response.Replace("Here's what you need to know", "Here's a quick solution");
                    }
                    break;

                case "anxious":
                    // Make responses more reassuring
                    if (response.Contains("This is a serious threat"))
                    {
                        return response.Replace("This is a serious threat", "While this is something to be aware of");
                    }
                    break;

                case "positive":
                    // Add more detailed information for engaged users
                    if (response.Length < 200)
                    {
                        string additionalInfo = GetAdditionalInfoForTopic(_currentTopic);
                        if (!string.IsNullOrEmpty(additionalInfo))
                            return response + " " + additionalInfo;
                    }
                    break;
            }

            return response;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private string GetSimplifiedExplanation(string topic)
        {
            if (!string.IsNullOrEmpty(topic) && _simplifiedExplanations.ContainsKey(topic))
            {
                return GetRandomItem(_simplifiedExplanations[topic]);
            }
            return "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Get conversation context for smoother transitions
        private string GetConversationContext()
        {
            // Check if we're continuing a topic
            if (!string.IsNullOrEmpty(_currentTopic) && _recentTopics.Contains(_currentTopic))
            {
                return GetRandomItem(_conversationConnectors["continuation"]);
            }

            // Check if we're switching topics
            if (_recentTopics.Count > 0 && !string.IsNullOrEmpty(_currentTopic) &&
                !_recentTopics.Contains(_currentTopic))
            {
                return GetRandomItem(_conversationConnectors["topic_switch"]);
            }

            return "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Get sentiment-appropriate encouragement
        private string GetSentimentAppropriateEncouragement(string sentiment)
        {
            switch (sentiment)
            {
                case "positive":
                    return GetRandomItem(_encouragementPhrases);

                case "confused":
                    return "Feel free to ask if you need me to explain any part differently!";

                case "frustrated":
                    return "Hopefully this helps clear things up for you.";

                case "anxious":
                    return "Remember, you're taking the right steps by learning about this.";

                default:
                    return "";
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        //  this method retrieves additional information based on the topic     
        // It uses a dictionary to store additional information for each topic
        private string GetAdditionalInfoForTopic(string topic)
        {
            var additionalInfo = new Dictionary<string, string>
            {
                ["password"] = "Pro tip: Consider using a passphrase with a mix of unrelated words - it's both secure and memorable!",
                ["phishing"] = "Here's an insider trick: legitimate companies will never ask for passwords via email, even in 'urgent' situations.",
                ["malware"] = "Advanced tip: Enable real-time protection in your antivirus and keep your browser updated for the best defense.",
                ["privacy"] = "Did you know? Using different email addresses for different types of accounts can really boost your privacy game!",
                ["ransomware"] = "Expert advice: The 3-2-1 backup rule (3 copies, 2 different media, 1 offsite) is your best ransomware insurance."
            };

            return additionalInfo.ContainsKey(topic) ? additionalInfo[topic] : "";// Return empty string if no additional info available for the topic
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Helper method to get random template from sentiment responses
        /// </summary>
        private string GetRandomTemplate(Dictionary<string, List<string>> templates, string category)
        {
            if (templates.ContainsKey(category) && templates[category].Count > 0)
            {
                return templates[category][_random.Next(templates[category].Count)];
            }
            return "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Enhanced sentiment-aware follow-up question generation
        /// </summary>
        private string GetSentimentAwareFollowUp(string topic, string sentiment)
        {
            if (string.IsNullOrEmpty(topic))
            {
                return GetNaturalGenericFollowUp(sentiment);
            }

            // Get base follow-up questions
            if (!_followUpQuestions.ContainsKey(topic))
            {
                return GetNaturalGenericFollowUp(sentiment);
            }

            var baseQuestions = _followUpQuestions[topic];
            string baseQuestion = baseQuestions[_random.Next(baseQuestions.Count)];

            // Make follow-ups more conversational based on sentiment and history
            string followUp;
            do
            {
                followUp = MakeFollowUpMoreNatural(baseQuestion, topic, sentiment);
            } while (followUp == _lastFollowUpQuestion && baseQuestions.Count > 1);

            _lastFollowUpQuestion = followUp;
            return followUp;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Make follow-up questions more natural and personalized
        // this method generates a more natural follow-up question based on the user's sentiment and previous discussions
        // It uses the user's name and previous discussions to make the question more conversational
        // It also uses a random number to determine whether to use the user's name or not
        // It returns a string that is a more natural follow-up question
        private string MakeFollowUpMoreNatural(string baseQuestion, string topic, string sentiment)
        {
            string userName = _userMemory.GetUserInfo("name");
            bool hasDiscussedBefore = _userMemory.HasDiscussedTopic(topic);

            // Natural conversation starters
            List<string> naturalStarters = new List<string>();

            switch (sentiment)
            {
                case "positive":
                    naturalStarters.AddRange(new[] { "Since you're interested, ", "I'm curious - ", "You might also wonder: " });
                    break;
                case "anxious":
                    naturalStarters.AddRange(new[] { "To help ease your mind, ", "Something that might help: ", "Let me ask - " });
                    break;
                case "confused":
                    naturalStarters.AddRange(new[] { "To make this clearer, ", "Let me check - ", "This might help: " });
                    break;
                default:
                    naturalStarters.AddRange(new[] { "By the way, ", "I'm wondering - ", "Quick question: " });
                    break;
            }

            string starter = GetRandomItem(naturalStarters);

            // Make the question more conversational
            string naturalQuestion = baseQuestion.ToLower();

            // Add personal touch if we have the user's name and this fits naturally
            if (!string.IsNullOrEmpty(userName) && _random.Next(3) == 0) // 1/3 chance to use name
            {
                return $"{starter}{userName}, {naturalQuestion}";
            }

            return $"{starter}{naturalQuestion}";
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method generates a natural follow-up question based on the user's sentiment
        // It uses a dictionary to store different follow-up questions based on the user's sentiment
        private string GetNaturalGenericFollowUp(string sentiment)
        {
            var naturalFollowUps = new Dictionary<string, List<string>>
            {
                ["positive"] = new List<string>
                {
                    "What else are you curious about in cybersecurity?",
                    "Is there another security topic you'd like to dive into?",
                    "Any other cybersecurity questions on your mind?"
                },
                ["anxious"] = new List<string>
                {
                    "What's your biggest security worry right now?",
                    "Is there a specific threat that's been concerning you?",
                    "What would help you feel more secure online?"
                },
                ["confused"] = new List<string>
                {
                    "What part would be most helpful for me to explain differently?",
                    "Which aspect of this is trickiest to understand?",
                    "What would make this clearer for you?"
                },
                ["frustrated"] = new List<string>
                {
                    "What specific problem can I help you solve right now?",
                    "What's the most urgent security issue you're facing?",
                    "How can I make this easier for you?"
                }
            };

            if (naturalFollowUps.ContainsKey(sentiment))
                return GetRandomItem(naturalFollowUps[sentiment]);

            return "What would you like to know more about?";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Generate generic follow-up questions based on sentiment
        /// </summary>
        private string GetGenericFollowUp(string sentiment)
        {
            switch (sentiment)
            {
                case "positive":
                    return "What other cybersecurity topics would you like to explore?";

                case "anxious":
                    return "Is there a specific security concern that's worrying you most?";

                case "confused":
                    return "What part would you like me to explain more clearly?";

                case "frustrated":
                    return "What specific issue can I help you solve right now?";

                case "negative":
                    return "What cybersecurity challenge can I help you address?";

                default:
                    return "Would you like to know more about this topic?";
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Enhanced main response method with comprehensive sentiment integration
        /// </summary>
        public override string GetResponse(string input)
        {
            // Track conversation patterns for more natural responses
            TrackConversationPatterns(input);
            UpdateRecentTopics();

            // Check for conversation gap and reset if needed
            TimeSpan timeSinceLastResponse = DateTime.Now - _lastResponseTime;
            bool isNewConversation = timeSinceLastResponse.TotalMinutes > 5;
            _lastResponseTime = DateTime.Now;

            if (isNewConversation)
            {
                _currentTopic = "";
                _followUpAsked = false;
                _lastSentiment = "neutral";
                _consecutiveQuestionsCount = 0;
                _recentTopics.Clear();
            }

            // Check for greetings
            string greeting = CheckForPersonalizedGreeting(input);
            if (greeting != null)
            {
                return greeting;
            }

            // Analyze sentiment
            string sentiment = _sentimentAnalyzer.AnalyzeSentiment(input);
            _userMemory.RecordSentiment(sentiment);
            _lastSentiment = sentiment;

            ExtractUserInfo(input);

            // Handle quick responses like "yes" or "no"
            string trimmedInput = input.ToLower().Trim();
            if ((trimmedInput == "yes" || trimmedInput == "no") && !string.IsNullOrEmpty(_currentTopic))
            {
                string yesNoResponse = GetNextStepResponse(_currentTopic, trimmedInput);
                _followUpAsked = false;
                _previousUserInput = input;
                return yesNoResponse;
            }

            // Get base response from parent class
            string baseResponse = base.GetResponse(input);

            if (baseResponse != "I didn't quite understand that. Could you please rephrase that question.")
            {
                string detectedTopic = DetectTopic(input);
                if (!string.IsNullOrEmpty(detectedTopic))
                {
                    if (_recentTopics.Count > 0 && _recentTopics[0] == detectedTopic)
                    {
                        // Prevent repetitive topic explanation
                        return $"We just talked about {detectedTopic}. Is there something specific you'd like to dive deeper into?";
                    }

                    _currentTopic = detectedTopic;
                    _userMemory.AddDiscussedTopic(detectedTopic);

                    if (IsQuestion(input))
                    {
                        _userMemory.RecordTopicQuestion(detectedTopic);
                        _consecutiveQuestionsCount++;
                    }
                    else
                    {
                        _consecutiveQuestionsCount = 0;
                    }

                    if (sentiment == "positive")
                    {
                        _userMemory.RecordPositiveTopicInteraction(detectedTopic);
                    }

                    // Add topic transition if the topic changed
                    if (_recentTopics.Count > 0 && _recentTopics[0] != _currentTopic)
                    {
                        string transition = _topicTransitions.ContainsKey(_currentTopic)
                            ? _topicTransitions[_currentTopic]
                            : $"Let’s talk a bit more about {_currentTopic}.";
                        baseResponse = transition + " " + baseResponse;
                    }
                }

                // Apply sentiment adjustments
                string sentimentAdjustedResponse = AdjustResponseForSentiment(baseResponse, sentiment);

                // Handle natural follow-up
                string finalResponse = EnhanceWithNaturalFollowUp(sentimentAdjustedResponse, _currentTopic, sentiment);

                _previousUserInput = input;
                return finalResponse;
            }
            else
            {
                // Handle unrecognized input
                return HandleUnrecognizedInputNaturally(input, sentiment);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method tracks conversation patterns to improve response quality
        // It counts consecutive questions and updates recent topics for context
        // This helps the AI understand user engagement and adapt responses accordingly
        private void TrackConversationPatterns(string input)
        {
            // Track if user is asking questions consecutively
            if (IsQuestion(input))
            {
                _consecutiveQuestionsCount++;
            }
            else
            {
                _consecutiveQuestionsCount = 0;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method updates recent topics for better context in responses
        // It maintains a list of the last 3 topics discussed to provide relevant follow-up information
        // This helps the AI maintain context and continuity in conversations
        private void UpdateRecentTopics()
        {
            if (!string.IsNullOrEmpty(_currentTopic) && !_recentTopics.Contains(_currentTopic))
            {
                _recentTopics.Insert(0, _currentTopic);

                // Keep only last 3 topics for context
                if (_recentTopics.Count > 3)
                {
                    _recentTopics.RemoveAt(_recentTopics.Count - 1);
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Handle unrecognized input with sentiment-aware responses
        /// </summary>
        private string HandleUnrecognizedInputWithSentiment(string input, string sentiment)
        {
            string userName = _userMemory.GetUserInfo("name");
            string personalizedPrefix = string.IsNullOrEmpty(userName) ? "" : userName + ", ";

            switch (sentiment)
            {
                case "frustrated":
                    return personalizedPrefix + "I can sense your frustration. Let me try to help in a different way. Could you tell me what specific cybersecurity issue you're dealing with?";

                case "confused":
                    return personalizedPrefix + "I can see you might be confused. Let me simplify this - what cybersecurity topic would you like me to explain step by step?";

                case "anxious":
                    return personalizedPrefix + "I understand you might be worried. Don't worry - I'm here to help. What specific security concern can I address for you?";

                case "negative":
                    return personalizedPrefix + "I understand this might be concerning. Let me help you with a specific cybersecurity topic. What would you like to know about?";

                case "positive":
                    return personalizedPrefix + "I appreciate your enthusiasm! While I didn't catch that exactly, I'd love to help with any cybersecurity questions you have. What interests you most?";

                default:
                    return GenerateContextualResponse(input);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Enhanced follow-up with sentiment awareness
        /// </summary>
        private string EnhanceWithSentimentAwareFollowUp(string baseResponse, string topic, string sentiment)
        {
            if (string.IsNullOrEmpty(topic))
            {
                // Add generic sentiment-aware follow-up
                string genericFollowUp = GetGenericFollowUp(sentiment);
                return baseResponse + " " + genericFollowUp;
            }

            string followUp = GetSentimentAwareFollowUp(topic, sentiment);
            _followUpAsked = true;

            return baseResponse + " " + followUp;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Check if the input contains a greeting and return an appropriate response
        /// </summary>
        private string CheckForPersonalizedGreeting(string input)
        {
            string lowerInput = input.ToLower();

            if (lowerInput.Contains("hello") || lowerInput.Contains("hi ") || lowerInput == "hi" ||
                lowerInput.Contains("hey") || lowerInput.Contains("greetings"))
            {
                return GetPersonalizedGreeting();
            }

            if (lowerInput.Contains("goodbye") || lowerInput.Contains("bye") ||
                lowerInput.Contains("see you") || lowerInput.Contains("farewell"))
            {
                return GetPersonalizedFarewell();
            }

            return null;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        //  this method generates a personalized greeting based on user information and interaction history
        // It uses the user's name, favorite topic, and whether they are a returning user to create a tailored greeting
        // It also checks if the user has a strong interest in a topic to provide a more engaging welcome
        // This helps create a more personalized and engaging user experience right from the start
        private string GetPersonalizedGreeting()
        {
            string userName = _userMemory.GetUserInfo("name");
            string favoriteTopic = _userMemory.GetFavoriteTopic();
            bool isReturningUser = _userMemory.GetDiscussedTopics().Count > 2;
            bool hasStrongInterest = !string.IsNullOrEmpty(favoriteTopic) &&
                                   _userMemory.HasStrongInterestIn(favoriteTopic);

            List<string> greetingOptions = new List<string>();

            if (isReturningUser && hasStrongInterest)
            {
                var personalizedGreetings = _personalizedGreetings["topic_expert"];
                foreach (var greeting in personalizedGreetings)
                {
                    greetingOptions.Add(greeting.Replace("{topic}", favoriteTopic));
                }
            }
            else if (isReturningUser)
            {
                var returningGreetings = _personalizedGreetings["returning_user"];
                foreach (var greeting in returningGreetings)
                {
                    if (greeting.Contains("{topic}") && !string.IsNullOrEmpty(favoriteTopic))
                        greetingOptions.Add(greeting.Replace("{topic}", favoriteTopic));
                    else if (!greeting.Contains("{topic}"))
                        greetingOptions.Add(greeting);
                }
            }
            else
            {
                greetingOptions.AddRange(_personalizedGreetings["first_time"]);
            }

            string selectedGreeting = GetRandomItem(greetingOptions);

            if (!string.IsNullOrEmpty(userName))
            {
                return $"Hi {userName}! {selectedGreeting.Substring(selectedGreeting.IndexOf(' ') + 1)}";
            }

            return selectedGreeting;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        private string GetPersonalizedFarewell()
        {
            string userName = _userMemory.GetUserInfo("name");
            string favoriteTopic = _userMemory.GetFavoriteTopic();

            List<string> farewellOptions = new List<string>
            {
                "Take care and stay secure online!",
                "Goodbye! Keep practicing good cybersecurity habits.",
                "Until next time! Remember what we discussed about staying safe.",
                "Bye for now! Feel free to come back anytime with security questions."
            };

            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                farewellOptions.Add($"Goodbye! Keep exploring {favoriteTopic} - you're really getting good at it!");
            }

            string selectedFarewell = GetRandomItem(farewellOptions);

            if (!string.IsNullOrEmpty(userName))
            {
                return $"{selectedFarewell.Replace("!", $", {userName}!")}";
            }

            return selectedFarewell;

        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method enhances the base response with a natural follow-up question
        // It checks if the user just answered a question and avoids adding a follow-up in that case
        // It also ensures that follow-ups are not repeated too quickly
        // This helps maintain a natural flow in the conversation and avoids overwhelming the user with follow-up questions
        private string EnhanceWithNaturalFollowUp(string baseResponse, string topic, string sentiment)
        {
            // FIX: Don't add follow-up if user just answered a question
            if (_previousUserInput.ToLower().Trim() == "yes" || _previousUserInput.ToLower().Trim() == "no")
            {
                return baseResponse + " " + GetNextStepResponse(topic, _previousUserInput.ToLower().Trim());
            }

            // Don't ask follow-up if we just asked one
            if (_followUpAsked && DateTime.Now.Subtract(_lastResponseTime).TotalSeconds < 30)
            {
                return baseResponse;
            }

            string followUp = GetSentimentAwareFollowUp(topic, sentiment);
            _followUpAsked = true;

            List<string> transitions = new List<string> { " ", " Also, ", " By the way, " };
            string transition = GetRandomItem(transitions);

            return baseResponse + transition + followUp;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method checks if the input is a question
        // It looks for common question words and punctuation to determine if the input is a question
        private string GetNextStepResponse(string topic, string userAnswer)
        {
            if (userAnswer == "yes")
            {
                switch (topic)
                {
                    case "phishing":
                        return "Great! Here are key warning signs to watch for: unexpected urgent requests, suspicious sender addresses, and links that don't match the claimed destination.";
                    case "password":
                        return "Excellent! I recommend using a password manager and creating passwords with at least 12 characters, mixing letters, numbers, and symbols.";
                    case "malware":
                        return "Perfect! Make sure to keep your antivirus updated, avoid suspicious downloads, and regularly scan your system.";
                    case "privacy":
                        return "Awesome! Consider using a VPN, adjusting your social media privacy settings, and being cautious about sharing personal information online.";
                    case "ransomware":
                        return "Fantastic! Regularly back up your data, keep your software updated, and be cautious with email attachments and links.";
                    case "social engineering":
                        return "Great! Be aware of tactics like pretexting, baiting, and tailgating. Always verify identities before sharing sensitive information.";
                    case "network security":
                        return "Excellent! Ensure your Wi-Fi is secured with WPA3 encryption, change default router passwords, and regularly update firmware.";
                    case "pharming":
                        return "Awesome! Always check URLs carefully, use HTTPS, and consider using browser extensions that warn about suspicious sites.";
                    case "firewall":
                        return "Great! Make sure your firewall is enabled, configure it to block unwanted traffic, and regularly review its settings.";
                    case "antivirus":
                        return "Excellent! Keep your antivirus software updated, run regular scans, and be cautious about downloading files from untrusted sources.";
                    case "vpn":
                        return "Awesome! A VPN can help protect your privacy online. Make sure to choose a reputable provider and connect whenever using public Wi-Fi.";
                    case "2fa":
                        return "Great! Enabling two-factor authentication adds an extra layer of security. Use an authenticator app or SMS codes for verification.";
                    default:
                        return "Wonderful! Feel free to ask me any specific questions about " + topic + ".";
                }
            }
            else if (userAnswer == "no")
            {
                return "No problem! What other cybersecurity topic would you like to explore?";
            }

            return "";
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // this method handles unrecognized input in a more natural way
        // It uses the user's sentiment to generate a more personalized response
        // It also incorporates the user's name and favorite topic if available
        // This helps create a more engaging and personalized user experience, even when the input is not recognized
        // It also uses the user's sentiment to generate a more personalized response
        private string HandleUnrecognizedInputNaturally(string input, string sentiment)
        {
            string userName = _userMemory.GetUserInfo("name");
            string favoriteTopic = _userMemory.GetFavoriteTopic();

            // Build a more natural response based on context
            List<string> responseOptions = new List<string>();

            switch (sentiment)
            {
                case "frustrated":
                    responseOptions.Add("I can tell you're frustrated. Let me try to help in a different way.");
                    responseOptions.Add("I understand this is annoying. What specific cybersecurity issue can I help you tackle?");
                    break;

                case "confused":
                    responseOptions.Add("I can see you might be confused. Let me try a simpler approach.");
                    responseOptions.Add("No worries if that didn't come across clearly. What cybersecurity topic interests you?");
                    break;

                case "anxious":
                    responseOptions.Add("I understand you might be worried. I'm here to help ease those security concerns.");
                    responseOptions.Add("Don't worry - we can work through any cybersecurity questions you have.");
                    break;

                default:
                    responseOptions.Add("I'm not quite sure I caught that, but I'm here to help with cybersecurity questions.");
                    responseOptions.Add("Could you tell me what cybersecurity topic you'd like to explore?");
                    break;
            }

            string baseResponse = GetRandomItem(responseOptions);

            // Add personalization if we have context
            if (!string.IsNullOrEmpty(userName))
            {
                baseResponse = $"{userName}, {baseResponse.ToLower()}";
            }

            // Suggest their favorite topic if we know it
            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                baseResponse += $" Would you like to continue learning about {favoriteTopic}?";
            }
            else
            {
                baseResponse += " What would you like to know about?";
            }

            return baseResponse;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        // Helper method to get random item from list
        private string GetRandomItem(List<string> items)
        {
            if (items == null || items.Count == 0)// If the list is null or empty, return an empty string
                return "";

            return items[_random.Next(items.Count)];// Get a random item from the list using the Random instance
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Extract and store user information from input
        /// </summary>
        private void ExtractUserInfo(string input)
        {
            string lowerInput = input.ToLower();

            // Extract name
            if (lowerInput.Contains("my name is") || lowerInput.Contains("i'm ") || lowerInput.Contains("i am "))
            {
                string pattern = @"(?:my name is|i'm|i am)\s+(\w+)";// Regex pattern to match "my name is [name]", "I'm [name]", or "I am [name]"
                Match match = Regex.Match(lowerInput, pattern);// Match the input against the pattern
                if (match.Success && match.Groups.Count > 1)// If the match is successful and there is a group captured
                {
                    string name = match.Groups[1].Value;// Get the captured name from the match
                    _userMemory.StoreUserInfo("name", name);// Store the name in user memory
                }
            }

            // Extract interests
            if (lowerInput.Contains("interested in") || lowerInput.Contains("like to know about"))
            {
                string pattern = @"(?:interested in|like to know about)\s+(\w+)";
                Match match = Regex.Match(lowerInput, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    string interest = match.Groups[1].Value;
                    _userMemory.StoreUserInfo("interest", interest);
                }
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Detect the current topic of conversation from user input
        /// </summary>
        private string DetectTopic(string input)
        {
            string lowerInput = input.ToLower();

            // Define topic keywords that match your base Response class topics
            var topicKeywords = new Dictionary<string, List<string>>();

            // Expanded phishing-related terms
            topicKeywords.Add("phishing", new List<string> {
                "phishing", "scam", "fake email", "suspicious email", "email scam",
                "click link", "verify account", "urgent action", "pharming",
                "spoofed", "fraudulent", "smishing", "vishing", "fake website",
                "deceptive", "impersonation", "credential harvesting", "account locked",
                "security alert", "bank scam", "lottery scam", "prize scam", "social engineering"
            });

            // Expanded password-related terms
            topicKeywords.Add("password", new List<string> {
                "password", "strong password", "password manager", "authentication",
                "credentials", "login", "secure password", "password security",
                "password tips", "password strength", "weak password", "passphrase",
                "password reuse", "password reset", "password policy", "password breach",
                "password leak", "password vault", "password generator", "password protection"
            });

            // Expanded malware-related terms
            topicKeywords.Add("malware", new List<string> {
                "malware", "virus", "trojan", "spyware", "antivirus", "infection",
                "malicious software", "computer virus", "worm", "adware",
                "rootkit", "keylogger", "botnet", "payload", "exploit", "backdoor",
                "malicious attachment", "malicious link", "malicious code", "malicious file"
            });

            // Expanded privacy-related terms
            topicKeywords.Add("privacy", new List<string> {
                "privacy", "personal data", "tracking", "surveillance", "data collection",
                "personal information", "data protection", "online privacy",
                "data breach", "data leak", "privacy settings", "incognito", "private browsing",
                "cookie", "tracking cookie", "browser fingerprinting", "anonymity", "gdpr", "ccpa"
            });

            // Expanded ransomware-related terms
            topicKeywords.Add("ransomware", new List<string> {
                "ransomware", "ransom", "encrypt", "locked files", "bitcoin demand",
                "files encrypted", "pay ransom", "crypto locker", "decrypt", "decryption key",
                "ransom note", "extortion", "data hostage", "double extortion", "crypto virus"
            });

            // Expanded network security-related terms
            topicKeywords.Add("network security", new List<string> {
                "network security", "wifi", "router", "firewall", "vpn", "encryption",
                "ssid", "wpa2", "wpa3", "network breach", "packet sniffing", "man in the middle",
                "mitm", "public wifi", "secure connection", "network monitoring", "intrusion detection"
            });

            // Expanded social engineering-related terms
            topicKeywords.Add("social engineering", new List<string> {
                "social engineering", "pretexting", "baiting", "tailgating", "impersonation",
                "manipulation", "psychological attack", "con artist", "confidence trick",
                "shoulder surfing", "dumpster diving", "reverse social engineering"
            });

            // Expanded pharming-related terms
            topicKeywords.Add("pharming", new List<string> {
                "pharming", "dns hijacking", "dns poisoning", "redirect", "fake site",
                "spoofed website", "malicious redirect", "dns attack", "dns spoofing"
            });

            // Expanded firewall-related terms
            topicKeywords.Add("firewall", new List<string> {
                "firewall", "network filter", "packet filter", "port blocking", "application firewall",
                "windows firewall", "mac firewall", "linux firewall", "block traffic", "allow traffic"
            });

            // Expanded antivirus-related terms
            topicKeywords.Add("antivirus", new List<string> {
                "antivirus", "virus scanner", "malware scanner", "real-time protection",
                "virus definition", "malware removal", "security suite", "threat detection"
            });

            // Expanded VPN-related terms
            topicKeywords.Add("vpn", new List<string> {
                "vpn", "virtual private network", "secure tunnel", "encrypted connection",
                "hide ip", "anonymize", "remote access", "vpn service", "vpn provider"
            });

            // Expanded 2FA-related terms
            topicKeywords.Add("2fa", new List<string> {
                "2fa", "two-factor authentication", "multi-factor authentication", "mfa",
                "authenticator app", "sms code", "security code", "verification code", "one-time password", "otp"
            });

            // Check for topic keywords with better matching
            foreach (var topic in topicKeywords)
            {
                foreach (string keyword in topic.Value)
                {
                    if (lowerInput.Contains(keyword))
                    {
                        return topic.Key;
                    }
                }
            }

            return "";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Check if the input appears to be a question
        /// </summary>
        private bool IsQuestion(string input)
        {
            string lowerInput = input.ToLower().Trim();

            // Check for question words
            string[] questionWords = { "what", "how", "why", "when", "where", "who", "which", "can", "could", "would", "should", "is", "are", "do", "does", "did" };

            // Check if starts with question word or ends with question mark
            return lowerInput.EndsWith("?") ||
                   questionWords.Any(word => lowerInput.StartsWith(word + " "));
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// Enhanced contextual response that considers sentiment
        /// </summary>
        private string GenerateContextualResponse(string input)
        {
            // Get user's favorite topic for better contextualization
            string favoriteTopic = _userMemory.GetFavoriteTopic();

            if (!string.IsNullOrEmpty(favoriteTopic))
            {
                return "I notice you're particularly interested in " + favoriteTopic + ". Would you like me to explain something specific about " + favoriteTopic + ", or would you prefer to explore a different cybersecurity topic?";
            }

            // If all else fails, prompt for a specific cybersecurity topic
            string userName = _userMemory.GetUserInfo("name");
            string defaultResponse = "I specialize in cybersecurity topics like phishing, malware, passwords, and privacy. What specific aspect would you like to learn about?";

            if (!string.IsNullOrEmpty(userName))
            {
                return userName + ", " + defaultResponse;
            }

            return defaultResponse;
        }
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//