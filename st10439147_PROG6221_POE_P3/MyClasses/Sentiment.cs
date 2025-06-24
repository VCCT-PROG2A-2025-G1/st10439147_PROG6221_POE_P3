//Dillon Rinkwest
//Student Number: ST10439147
// Module: PROG6221
// Group: 1
// POE Final part

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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ST10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// The Sentiment class is responsible for analyzing the sentiment of user input.
    /// It uses a keyword-based approach with improved word boundary detection to determine 
    /// if the input is positive, negative, neutral, anxious, confused, or frustrated.
    /// </summary>
    public class Sentiment
    {
        /// List of words associated with positive sentiment
        private readonly List<string> _positiveWords;

        /// List of words associated with negative sentiment   
        private readonly List<string> _negativeWords;

        /// List of words associated with anxious sentiment 
        private readonly List<string> _anxiousWords;

        /// List of words associated with confused sentiment
        private readonly List<string> _confusedWords;

        /// List of words associated with frustrated sentiment
        private readonly List<string> _frustratedWords;

        /// Dictionary for faster word lookups
        private readonly Dictionary<string, string> _wordToSentiment;

        /// Constructor that initializes sentiment word lists
        /// This constructor sets up the lists of words that will be used to determine sentiment.
        /// 
        public Sentiment()
        {
            // Initialize lists with sentiment words
            // These words are used to detect the sentiment of the user input
            // The words are categorized into positive, negative, anxious, confused, and frustrated
            // This allows for more accurate sentiment analysis by considering the context of the words
            // and their common usage in user input.
            // The lists are initialized with common words that are associated with each sentiment category
            _positiveWords = new List<string>
            {
                "thank", "thanks", "appreciate", "good", "great", "excellent", "wonderful", "happy",
                "helpful", "nice", "love", "like", "fantastic", "awesome", "brilliant", "perfect",
                "amazing", "outstanding", "superb", "marvelous", "delighted", "pleased", "satisfied"
            };

            _negativeWords = new List<string>
            {
                "bad", "terrible", "awful", "hate", "dislike", "poor", "wrong", "horrible",
                "annoying", "disappointed", "upset", "sad", "angry", "frustrating", "useless",
                "worst", "pathetic", "disgusting", "dreadful", "appalling", "miserable"
            };

            _anxiousWords = new List<string>
            {
                "worried", "nervous", "anxious", "concerned", "afraid", "scared", "fear", "worry",
                "paranoid", "uneasy", "stressed", "panic", "danger", "threat", "unsafe",
                "terrified", "frightened", "alarmed", "distressed", "troubled", "apprehensive"
            };

            _confusedWords = new List<string>
            {
                "confused", "unclear", "explain", "complicated",
                "lost", "unsure", "perplexed", "puzzled", "bewildered", "confusing",
                "baffled", "mystified", "uncertain", "vague", "ambiguous"
            };

            _frustratedWords = new List<string>
            {
                "frustrated", "annoyed", "irritated", "stupid", "broken", "buggy",
                "infuriating", "exasperated", "aggravated", "impatient", "ridiculous"
            };

            // Create dictionary for faster lookups
            // This dictionary maps each word to its sentiment category
            // This allows for quick access to the sentiment of a word without needing to search through lists
            // The dictionary is initialized with all the words from the sentiment lists
            // This improves performance when analyzing user input by reducing the time complexity of lookups
            // The dictionary is used to determine the sentiment of each word in the user input
            // The keys are the words and the values are the sentiment categories
            _wordToSentiment = new Dictionary<string, string>();
            foreach (var word in _positiveWords) _wordToSentiment[word] = "positive";
            foreach (var word in _negativeWords) _wordToSentiment[word] = "negative";
            foreach (var word in _anxiousWords) _wordToSentiment[word] = "anxious";
            foreach (var word in _confusedWords) _wordToSentiment[word] = "confused";
            foreach (var word in _frustratedWords) _wordToSentiment[word] = "frustrated";
        }

        /// <summary>
        /// Analyze the sentiment of the user input
        /// This method takes a string input and analyzes its sentiment based on predefined lists of words.
        /// 
        /// </summary>
        public string AnalyzeSentiment(string input)
        {
            // Handle null or empty input
            if (string.IsNullOrWhiteSpace(input))
            {
                return "neutral";
            }

            try
            {
                // Convert to lowercase for case-insensitive comparison
                string lowerInput = input.ToLower();

                // Check for specific multi-word phrases first
                if (ContainsPhrase(lowerInput, "don't understand") ||
                    ContainsPhrase(lowerInput, "I'm confused") ||
                    ContainsPhrase(lowerInput, "fed up") ||
                    ContainsPhrase(lowerInput, "tired of") ||
                    ContainsPhrase(lowerInput, "sick of") ||
                    ContainsPhrase(lowerInput, "doesn't work") ||
                    ContainsPhrase(lowerInput, "not working"))
                {
                    if (ContainsPhrase(lowerInput, "don't understand") ||
                        ContainsPhrase(lowerInput, "I'm confused"))
                        return "confused";
                    else
                        return "frustrated";
                }

                // Count sentiment words using improved word boundary detection
                // This method counts how many words from the sentiment lists appear in the input string.
                // It uses regular expressions to split the input into words and checks each word against the sentiment dictionary.
                var sentimentCounts = new Dictionary<string, int>
                {
                    {"positive", 0},
                    {"negative", 0},
                    {"anxious", 0},
                    {"confused", 0},
                    {"frustrated", 0}
                };

                // Split input into words and check each one
                // This uses Regex to split the input into words based on non-word characters
                // This ensures that words are correctly identified even if they are separated by punctuation or whitespace
                // The Where clause filters out any empty or whitespace-only entries
                // The ToArray method converts the result to an array of words
                // This allows for accurate counting of sentiment words in the input
                // The words are split using a regular expression that matches non-word characters, ensuring that punctuation does not affect word boundaries
                string[] words = Regex.Split(lowerInput, @"\W+")
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .ToArray();

                // Iterate through each word and count sentiment occurrences
                // This loop checks each word against the sentiment dictionary
                // If the word is found in the dictionary, it increments the count for that sentiment
                // This allows for quick access to the sentiment of a word without needing to search through lists
                // The sentiment counts are stored in a dictionary for easy retrieval
                // The dictionary is initialized with all the sentiment categories and their counts set to 0
                // This improves performance when analyzing user input by reducing the time complexity of lookups   
                // The keys are the sentiment categories and the values are the counts of words found in the input
                // The sentiment counts are used to determine the overall sentiment of the input
                foreach (string word in words)
                {
                    if (_wordToSentiment.ContainsKey(word))
                    {
                        sentimentCounts[_wordToSentiment[word]]++;
                    }
                }

                // Find the sentiment with the highest count
                var maxSentiment = sentimentCounts.OrderByDescending(x => x.Value).First();

                // Return neutral if no sentiment words are found
                if (maxSentiment.Value == 0)
                {
                    return "neutral";
                }

                // Handle ties by priority: negative emotions first, then positive
                var tiedSentiments = sentimentCounts.Where(x => x.Value == maxSentiment.Value).ToList();

                if (tiedSentiments.Count == 1)// If there's only one sentiment with the highest count
                {
                    return maxSentiment.Key;// Return the single sentiment if there's no tie
                }

                // Priority order for ties: frustrated -> anxious -> confused -> negative -> positive
                string[] priorityOrder = { "frustrated", "anxious", "confused", "negative", "positive" };

                // Check tied sentiments against priority order
                // This loop iterates through the priority order and checks if any of the tied sentiments match
                // If a match is found, it returns that sentiment
                // This ensures that the most important sentiments are prioritized in case of ties
                // The priority order is defined to ensure that more critical sentiments are returned first
                // The priority order is based on the emotional impact of the sentiments
                // and their relevance to the user's input
                // The order is defined as follows: frustrated > anxious > confused > negative > positive
                // This means that if there is a tie between frustrated and anxious, frustrated will be returned
                foreach (string sentiment in priorityOrder)
                {
                    // Check if the sentiment is in the tied sentiments
                    // This checks if the sentiment is one of the tied sentiments
                    // If it is, it returns that sentiment
                    if (tiedSentiments.Any(x => x.Key == sentiment))
                    {
                        return sentiment;
                    }
                }

                return "neutral"; // Fallback case
            }
            catch (Exception)
            {
                // Return neutral sentiment if any errors occur during analysis
                return "neutral";
            }
        }

        /// <summary>
        /// Check if input contains a specific phrase
        /// This method checks if the input string contains a specific phrase.
        /// </summary>
        private bool ContainsPhrase(string input, string phrase)
        {
            return input.Contains(phrase);
        }

        /// <summary>
        /// Count the occurrences of words from a list in the input using word boundaries
        /// This method counts how many words from a given list appear in the input string.
        /// </summary>
        private int CountWordOccurrences(string input, List<string> wordList)
        {
            if (string.IsNullOrWhiteSpace(input) || wordList == null || wordList.Count == 0)// Check if input or word list is null or empty
            {
                return 0;// Return 0 if input is null or empty, or if the word list is empty
            }

            int count = 0;// Initialize count to 0

            try// Use a try-catch block to handle any exceptions that may occur during processing
            {
                // Split input into words
                // Use Regex to split the input into words based on non-word characters
                // This ensures that words are correctly identified even if they are separated by punctuation or whitespace
                // The Regex.Split method is used to split the input into words based on non-word characters
                // The Where clause filters out any empty or whitespace-only entries
                string[] words = Regex.Split(input.ToLower(), @"\W+")
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .ToArray();

                foreach (string word in wordList)// Iterate through each word in the word list
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        string lowerWord = word.ToLower();
                        if (words.Contains(lowerWord))
                        {
                            count++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return current count if any error occurs
                return count;
            }

            return count;
        }

        /// <summary>
        /// Get a description of the analyzed sentiment
        /// This method provides a human-readable description of the sentiment detected in the input.
        /// It returns a string that describes the sentiment based on the analysis performed by the AnalyzeSentiment method.
        /// </summary>

        public string GetSentimentDescription(string sentiment)
        {
            if (string.IsNullOrWhiteSpace(sentiment))// Check if sentiment is null or empty
            {
                return "No sentiment detected";// Return a default message if sentiment is not provided
            }

            // Convert sentiment to lowercase for case-insensitive comparison
            // This ensures that the method can handle different cases of the sentiment string
            // It allows for more flexibility in user input and ensures consistent matching
            // This is important for accurately determining the sentiment description
            // The ToLower method is used to convert the sentiment string to lowercase
            switch (sentiment.ToLower())
            {
                case "positive":
                    return "The user appears to have a positive attitude";
                case "negative":
                    return "The user appears to have a negative attitude";
                case "anxious":
                    return "The user appears to be worried or anxious";
                case "confused":
                    return "The user appears to be confused or need clarification";
                case "frustrated":
                    return "The user appears to be frustrated or irritated";
                case "neutral":
                    return "The user has a neutral tone";
                default:
                    return "Sentiment could not be determined";
            }
        }

        /// <summary>
        /// Get confidence score for the sentiment analysis (0-1)
        /// This method calculates a confidence score based on the number of sentiment words found in the input.
        /// This method returns a confidence score between 0 and 1 based on the number of sentiment words found in the input.
        /// It uses a simple formula to calculate the score based on the percentage of sentiment words in the input.
        /// A higher score indicates a stronger sentiment presence.
        /// If no sentiment words are found, the score is 0.
        /// If the input is empty or null, the score is also 0.
        /// The score is scaled to a maximum of 1.0 to ensure it remains within the expected range. 
        /// The method uses regular expressions to split the input into words and checks each word against the sentiment dictionary.
        /// </summary>

        public double GetConfidenceScore(string input)
        {
            // Handle null or empty input
            // This method calculates a confidence score based on the number of sentiment words found in the input.
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0.0;// Return 0.0 if input is null or empty, indicating no confidence in sentiment analysis
            }

            // Use a try-catch block to handle any exceptions that may occur during processing
            try
            {
                // Convert input to lowercase for case-insensitive comparison
                string lowerInput = input.ToLower();

                // Split input into words using regular expressions to handle word boundaries
                // This ensures that words are correctly identified even if they are separated by punctuation or whitespace
                // The Regex.Split method is used to split the input into words based on non-word characters
                // The Where clause filters out any empty or whitespace-only entries
                // The ToArray method converts the result to an array of words
                // This allows for accurate counting of sentiment words in the input
                // The words are split using a regular expression that matches non-word characters, ensuring that punctuation does not affect word boundaries
                // The words are then filtered to remove any empty or whitespace-only entries
                string[] words = Regex.Split(lowerInput, @"\W+")
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .ToArray();

                // Count total words and sentiment words
                int totalWords = words.Length;

                // Count the number of sentiment words in the input
                // This is done by checking each word against the sentiment dictionary
                // The sentiment dictionary contains words categorized into positive, negative, anxious, confused, and frustrated sentiments
                int sentimentWords = words.Count(word => _wordToSentiment.ContainsKey(word));

                if (totalWords == 0) return 0.0;// Avoid division by zero if there are no words

                // Base confidence on percentage of sentiment words
                double baseConfidence = (double)sentimentWords / totalWords;

                // Apply scaling: higher confidence for more sentiment words
                return Math.Min(1.0, baseConfidence * 2.0);
            }
            catch (Exception)// Catch any exceptions that may occur during processing
            {
                return 0.0;// Return 0.0 if an error occurs, indicating no confidence in sentiment analysis
            }
        }
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//