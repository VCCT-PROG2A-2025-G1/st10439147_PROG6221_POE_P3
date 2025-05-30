using ST10439147_PROG6221_POE_P2.MyClasses;
using ST10439147_PROG6221_POE_P3.MyClasses;
using System;

namespace ST10439147_PROG6221_POE.MyClasses
{
    /// <summary>
    /// WPF-adapted version of the Communication class for handling chatbot interactions in a GUI environment
    /// </summary>
    public class Communication
    {
        private readonly EnhancedResponses _responseGenerator;

        public string UserName { get; private set; }
        public bool IsUserNameSet { get; private set; }
        public string LastUserInput { get; private set; }

        // Events for UI updates
        public event Action<string> OnErrorMessage;
        public event Action<string> OnBotResponse;
        public event Action<string> OnWelcomeMessage;
        public event Action<string> OnExitMessage;
        public event Action<string> OnHelpMessage;

        public Communication(EnhancedResponses responseGenerator)
        {
            _responseGenerator = responseGenerator ?? throw new ArgumentNullException(nameof(responseGenerator));
            IsUserNameSet = false;
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
        /// Validates user name according to the original Communication class rules
        /// </summary>
        public ValidationResult ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult(false, "Name cannot be empty. Please try again.");
            }

            // Check if name contains only letters and whitespace
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    return new ValidationResult(false, "Name should only contain letters. Please try again.");
                }
            }

            return new ValidationResult(true, string.Empty);
        }

        /// <summary>
        /// Sets the user name after validation
        /// </summary>
        public bool SetUserName(string name)
        {
            var validation = ValidateName(name);
            if (validation.IsValid)
            {
                UserName = name.Trim();
                IsUserNameSet = true;

                string welcomeMessage = $"Hello {UserName}, how may I help you today? " +
                                      "Type 'help' to see what I can do or 'exit' to quit.";
                OnWelcomeMessage?.Invoke(welcomeMessage);
                return true;
            }
            else
            {
                OnErrorMessage?.Invoke(validation.ErrorMessage);
                return false;
            }
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
                string exitMessage = IsUserNameSet
                    ? $"Remember to ask me anything as I am always available.\nGoodbye, have a great day {UserName}!"
                    : "Goodbye, have a great day!";

                return new ChatResponse(ChatResponseType.Exit, exitMessage);
            }

            // Handle help command
            if (lowerInput == "help")
            {
                string helpMessage = "You can ask about password safety, phishing scams, ransomware, viruses, " +
                                   "and other cybersecurity topics. Try asking for a 'cybersecurity guide' or " +
                                   "'cybersecurity terms' for comprehensive information!";

                return new ChatResponse(ChatResponseType.Help, helpMessage);
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
        /// Gets the initial welcome message for new users
        /// </summary>
        public string GetInitialWelcomeMessage()
        {
            return "Welcome to CyberGuard AI! Please enter your name to get started.";
        }
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
        Welcome
    }
}