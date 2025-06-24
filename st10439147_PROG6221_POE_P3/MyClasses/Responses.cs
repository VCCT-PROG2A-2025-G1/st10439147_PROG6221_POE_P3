//Dillon Rinkwest
//Student Number: ST10439147
// Module: PROG6221
// Group: 1
//POE Part 2

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
using System.Threading.Tasks;

namespace ST10439147_PROG6221_POE_P3.MyClasses
{
    /// <summary>
    /// The Response class is responsible for generating responses based on user input.
    /// It contains a dictionary of keywords and their corresponding responses.
    /// The class also includes methods for generating general chat responses and specific responses based on keywords.
    /// The class is designed to be used in a chatbot application, providing users with information about cybersecurity topics.
    /// </summary>
    public class Responses
    {
        private readonly UserMemory userMemory;

        /// <summary>
        /// Stores the current response string
        /// </summary>
        public string response;

        public Responses(UserMemory userMemory)
        {
            this.userMemory = userMemory;
        }
        /// <summary>
        /// Dictionary containing keywords and their corresponding responses
        /// </summary>
        protected readonly Dictionary<string, string> _response;

        /// <summary>
        /// Constructor that initializes the response dictionary with cybersecurity information
        /// </summary>
        public Responses()
        {
            // Define a dictionary to hold the responses
            // Dictionary is chosen for its key-value pair structure, allowing for easy retrieval of responses based on keywords
            _response = new Dictionary<string, string>
            {
                // General cybersecurity terms
                { "pharming", "Pharming is online fraud that involves the use of malicious code to direct victims to spoofed websites in an attempt to steal their credentials and data." },
                { "phishing", "Phishing is a cyber attack where scammers trick you into revealing personal information through fake emails, websites, or messages. Always verify the sender before clicking links or providing information." },
                { "ransomware", "Ransomware is a type of malware that encrypts a victim's files and demands payment in exchange for the decryption key. Regular backups and updated security software are your best defense." },
                { "social engineering", "Social engineering is a tactic used by cybercriminals to manipulate individuals into divulging confidential information through psychological manipulation rather than technical hacking." },
                { "spoofing", "Spoofing is a technique used by cybercriminals to deceive users into thinking they are interacting with a legitimate entity by falsifying data or identities." },
                { "virus", "A virus is a type of malware that is designed to replicate itself and spread to other devices, often causing damage to files and systems." },
                { "worm", "A worm is a type of malware that is designed to spread across networks and infect multiple devices without requiring user interaction." },
                { "trojan", "A trojan is a type of malware that disguises itself as a legitimate file or program to trick users into downloading and executing it." },
                { "botnet", "A botnet is a network of infected devices that are controlled by a cybercriminal to carry out malicious activities like DDoS attacks or cryptocurrency mining." },
                { "ddos", "DDoS (Distributed Denial of Service) is a type of cyber attack that floods a network or server with traffic to disrupt its normal operation." },
                { "firewall", "A firewall is a security system that monitors and controls incoming and outgoing network traffic based on predetermined security rules to protect your network." },
                { "antivirus", "Antivirus is a software program designed to detect, prevent, and remove malware from a computer or network. Keep it updated for best protection." },
                { "vpn", "A VPN (Virtual Private Network) encrypts internet traffic and masks the user's IP address to protect their privacy and security, especially on public networks." },
                { "2fa", "2FA (Two-Factor Authentication) is a security process that requires users to provide two forms of identification to access an account or service, significantly improving security." },
                { "password tips", "Use a strong, unique password for each account, include letters, numbers, and symbols, avoid personal information, and enable two-factor authentication (2FA)." },
                { "zero-day", "A zero-day vulnerability is a security flaw that is exploited by attackers before the software developer has released a patch to fix it." },
                { "mfa", "MFA (Multi-Factor Authentication) is an enhanced security measure that requires multiple forms of verification before granting access to accounts or systems." },
                { "spyware", "Spyware is malicious software designed to gather information about a person or organization without their knowledge, often stealing personal data." },
                { "keylogger", "A keylogger is a type of monitoring software or hardware that records keystrokes made by a user, often used to steal passwords and sensitive data." },
                { "adware", "Adware is software that automatically displays or downloads advertisements when a user is online, sometimes containing malicious components." },
                { "encryption", "Encryption is the process of converting information into a code to prevent unauthorized access, protecting your data even if intercepted." },
                { "sql injection", "SQL injection is an attack that inserts malicious SQL code into a database query, potentially giving attackers access to sensitive data." },
                { "xss", "XSS (Cross-Site Scripting) is a web security vulnerability that allows attackers to inject malicious scripts into websites viewed by other users." },
                { "csrf", "CSRF (Cross-Site Request Forgery) is an attack that forces authenticated users to execute unwanted actions on a web application." },
                { "mitm", "MITM (Man-in-the-Middle) attack occurs when attackers secretly relay and possibly alter communications between two parties." },
                { "brute force", "Brute force is an attack method that uses trial and error to crack passwords, encryption keys, or find hidden web pages through systematic attempts." },
                { "rootkit", "A rootkit is a collection of software tools that enable unauthorized access to areas of a computer while actively hiding its presence from detection." },
                { "malware", "Malware is malicious software designed to disrupt, damage, or gain unauthorized access to computer systems. Keep your antivirus updated to protect against it." },
                { "patch", "A patch is a software update designed to address security vulnerabilities and bugs in a program or operating system. Install patches promptly." },
                { "sandbox", "A sandbox is a security mechanism that provides a separate environment for running untested or untrusted programs safely." },

                // Cybersecurity tips
                { "email tips", "Never click on suspicious links or download attachments from unknown senders. Verify the sender's email address carefully and be wary of urgent requests for personal information." },
                { "browsing tips", "Use HTTPS websites, keep your browser updated, and consider using ad blockers and privacy extensions. Be cautious about what you download and which website permissions you grant." },
                { "device tips", "Keep your operating system and applications updated, use antivirus software, and regularly back up your data to an external drive or secure cloud service." },
                { "wifi tips", "Use WPA3 encryption for your home network, create a strong router password, change the default SSID name, and consider setting up a guest network for visitors." },
                { "social media tips", "Review privacy settings regularly, limit the personal information you share, be selective about friend/connection requests, and be wary of suspicious messages even from known contacts." },
                { "data backup", "Regularly save copies of your important files to a separate storage location to protect against data loss from ransomware or hardware failure." },
                { "update software", "Always install security patches and updates for your operating system and applications as soon as they're available to protect against known vulnerabilities." },
                { "tips", "Watch for poor spelling/grammar, unusual sender addresses, requests for sensitive information, unexpected attachments, and suspicious links that don't match legitimate URLs." },
                { "public wifi safety", "Avoid accessing sensitive accounts or conducting financial transactions on public networks; use a VPN when connecting to public wifi." },

                // Password safety
                { "password safety", "Use a strong, unique password for each account and enable two-factor authentication (2FA). Consider using a password manager for better security." },
                { "password safety tips", "Use a mix of letters, numbers, and symbols; avoid using easily guessable information like birthdays or names; and change your passwords regularly." },
                { "password strength", "A strong password is at least 12 characters long and includes a mix of uppercase letters, lowercase letters, numbers, and special characters." },
                { "password reset", "If you forget your password, use the 'forgot password' feature on the login page to reset it. Always choose a new password that is different from the old one." },
                { "password sharing", "Never share your passwords with anyone. If you must share access, consider using a password manager that allows for secure sharing." },
                { "password expiration", "Some organizations require regular password changes. If prompted, create a new password that meets the security requirements." },
                { "password hints", "Avoid using obvious hints like 'my pet's name.' Instead, use a hint that only you would understand." },
                { "password recovery", "Set up recovery options like security questions or backup email addresses to help regain access if you forget your password." },
                { "password manager", "A password manager securely stores and encrypts your passwords, making it easier to use strong, unique passwords for each account without having to remember them all." },
                { "password reuse", "Avoid reusing passwords across multiple accounts. If one account is compromised, others using the same password are at risk." },
                // Comprehensive cybersecurity guide
                { "cybersecurity guide", @"COMPREHENSIVE CYBERSECURITY GUIDE

                            PASSWORD SECURITY:
    • Use unique, strong passwords (12+ characters with mix of letters, numbers, symbols)
    • Enable two-factor authentication (2FA) on all important accounts
    • Use a reputable password manager to generate and store passwords
    • Never reuse passwords across multiple accounts

                            EMAIL SAFETY:
    • Verify sender addresses carefully before clicking links or attachments
    • Look for spelling errors, urgent language, and suspicious requests
    • Never provide personal information via email
    • Use official websites instead of email links for sensitive transactions

                            BROWSING SAFELY:
    • Only visit HTTPS websites (look for the lock icon)
    • Keep browsers updated with latest security patches
    • Use ad blockers and privacy extensions
    • Be cautious with downloads from unknown sources

                            DEVICE PROTECTION:
    • Install reputable antivirus software and keep it updated
    • Enable automatic security updates for your operating system
    • Use device lock screens with PINs, passwords, or biometrics
    • Regularly backup important data to secure locations

                            NETWORK SECURITY:
    • Use WPA3 encryption on home Wi-Fi networks
    • Avoid public Wi-Fi for sensitive activities
    • Consider using a VPN for additional privacy protection
    • Change default router passwords and settings

                            SOCIAL MEDIA SAFETY:
    • Review and adjust privacy settings regularly
    • Be selective about what personal information you share
    • Verify friend/connection requests from unknown people
    • Be cautious of suspicious messages, even from known contacts

                            RECOGNIZING THREATS:
    • Phishing: Fake emails/websites trying to steal your information
    • Ransomware: Malware that encrypts your files and demands payment
    • Social Engineering: Psychological manipulation to reveal information
    • Scam calls: Fraudsters impersonating legitimate organizations

                            INCIDENT RESPONSE:
    • If compromised, immediately change passwords and enable 2FA
    • Scan devices with updated antivirus software
    • Monitor financial accounts for suspicious activity
    • Report incidents to relevant authorities or organizations

Remember: Cybersecurity is an ongoing process, not a one-time setup. Stay informed about new threats and regularly review your security practices." },

                { "cybersecurity terms", @"CYBERSECURITY TERMS SUMMARY

                        MALWARE TYPES:
    • Virus - Self-replicating malware that spreads to other devices
    • Worm - Network-spreading malware that doesn't require user interaction
    • Trojan - Malware disguised as legitimate software
    • Ransomware - Encrypts files and demands payment for decryption
    • Spyware - Secretly gathers information without user knowledge
    • Keylogger - Records keystrokes to steal passwords and data
    • Adware - Automatically displays unwanted advertisements
    • Rootkit - Hides malicious activity from detection systems

                        ATTACK METHODS:
    • Phishing - Fake communications to steal personal information
    • Pharming - Redirects users to spoofed websites
    • Social Engineering - Psychological manipulation for information theft
    • Spoofing - Falsifying data or identities to deceive users
    • Brute Force - Systematic password cracking attempts
    • SQL Injection - Malicious database queries to access data
    • XSS (Cross-Site Scripting) - Injecting malicious scripts into websites
    • CSRF (Cross-Site Request Forgery) - Forcing unwanted user actions
    • MITM (Man-in-the-Middle) - Intercepting communications between parties
    • DDoS - Overwhelming networks with traffic to disrupt services
    • Zero-day - Exploiting unknown software vulnerabilities

                        SECURITY MEASURES:
    • Firewall - Network traffic monitoring and control system
    • Antivirus - Software to detect and remove malware
    • VPN - Encrypts internet traffic and masks IP addresses
    • 2FA/MFA - Multi-factor authentication for enhanced security
    • Encryption - Converting data into unreadable code
    • Patch - Software updates to fix security vulnerabilities
    • Sandbox - Isolated environment for testing suspicious programs

                         SECURITY TIPS AVAILABLE:
    • Password Safety & Tips - Creating and managing strong passwords
    • Email Tips - Identifying and avoiding email threats
    • Browsing Tips - Safe internet browsing practices
    • Device Tips - Protecting computers and mobile devices
    • WiFi Tips - Securing wireless network connections
    • Social Media Tips - Privacy and safety on social platforms
    • Public WiFi Safety - Staying secure on public networks
    • Data Backup - Protecting against data loss

                          COMPREHENSIVE GUIDES(Please type these words for the following):
    • Cybersecurity Guide - Complete overview of security practices

Ask me about any of these terms for detailed information and practical advice!" },
                // Detailed summary with comprehensive explanations
                { "summary", @"DETAILED CYBERSECURITY TERMS SUMMARY

**MALWARE TYPES - DETAILED EXPLANATIONS:**

• **Virus** - Self-replicating malicious code that attaches to legitimate programs and spreads when those programs are executed. Viruses can corrupt files, slow system performance, and steal data. They require host files to spread and often cause visible damage to infected systems.

• **Worm** - Standalone malware that spreads across networks without requiring user interaction or host files. Worms can consume network bandwidth, install backdoors, and create vulnerabilities for other attacks. They're particularly dangerous in networked environments.

• **Trojan** - Malicious software disguised as legitimate applications to trick users into installation. Unlike viruses, trojans don't replicate themselves but can steal data, provide remote access to attackers, or download additional malware. They rely on social engineering for distribution.

• **Ransomware** - Malware that encrypts victim's files and demands payment (usually cryptocurrency) for the decryption key. Modern ransomware often includes data exfiltration threats and targets critical infrastructure. Regular backups and updated security are essential defenses.

• **Spyware** - Software that secretly monitors user activity, collecting personal information, browsing habits, and keystrokes. It can slow system performance and compromise privacy. Often bundled with legitimate software or installed through deceptive practices.

• **Keylogger** - Monitoring tool that records all keystrokes, potentially capturing passwords, credit card numbers, and sensitive communications. Can be hardware-based (physical devices) or software-based. Particularly dangerous for financial and personal data theft.

• **Adware** - Software that displays unwanted advertisements, often redirecting browsers and collecting user data for targeted marketing. While not always malicious, adware can degrade system performance and privacy. Some variants contain more dangerous payloads.

• **Rootkit** - Sophisticated malware designed to maintain persistent access while hiding its presence from users and security software. Rootkits can modify system files, intercept system calls, and provide backdoor access to attackers.

**ATTACK METHODS - DETAILED EXPLANATIONS:**

• **Phishing** - Social engineering attacks using fraudulent communications (emails, texts, calls) that appear to come from trusted sources. Attackers aim to steal credentials, financial information, or install malware. Success rates are high due to increasingly sophisticated techniques.

• **Pharming** - Attack that redirects users from legitimate websites to fraudulent ones without their knowledge, often through DNS manipulation or malicious code. Unlike phishing, users may type correct URLs but still reach malicious sites.

• **Social Engineering** - Psychological manipulation techniques used to trick people into revealing confidential information or performing actions that compromise security. Exploits human psychology rather than technical vulnerabilities, making it highly effective.

• **Spoofing** - Technique where attackers falsify data to impersonate trusted entities. Includes email spoofing (fake sender addresses), IP spoofing (fake network addresses), and website spoofing (fake legitimate sites). Used to bypass security measures and gain trust.

• **Brute Force** - Attack method that systematically tries all possible combinations to crack passwords, encryption keys, or gain unauthorized access. Automated tools can attempt thousands of combinations per second. Strong passwords and account lockouts are key defenses.

• **SQL Injection** - Web application attack that inserts malicious SQL code into database queries, potentially allowing attackers to view, modify, or delete database contents. Can lead to complete system compromise and data breaches.

• **XSS (Cross-Site Scripting)** - Web vulnerability that allows attackers to inject malicious scripts into websites viewed by other users. Can steal cookies, session tokens, or redirect users to malicious sites. Affects both websites and their visitors.

• **CSRF (Cross-Site Request Forgery)** - Attack that forces authenticated users to execute unwanted actions on web applications where they're currently logged in. Exploits the trust that applications have in users' browsers.

• **MITM (Man-in-the-Middle)** - Attack where attackers secretly intercept and potentially modify communications between two parties who believe they're communicating directly. Common on unsecured networks and can compromise all transmitted data.

• **DDoS (Distributed Denial of Service)** - Attack that overwhelms target systems with traffic from multiple sources, making services unavailable to legitimate users. Often uses botnets of infected devices and can cause significant business disruption.

• **Zero-day** - Attacks that exploit previously unknown software vulnerabilities before developers can create and distribute patches. Particularly dangerous because traditional security measures may not detect them.

**SECURITY MEASURES - DETAILED EXPLANATIONS:**

• **Firewall** - Network security system that monitors and controls incoming and outgoing network traffic based on predetermined security rules. Acts as a barrier between trusted internal networks and untrusted external networks like the internet.

• **Antivirus** - Software designed to detect, prevent, and remove malware. Modern antivirus uses signature-based detection, heuristic analysis, and behavioral monitoring. Requires regular updates to maintain effectiveness against new threats.

• **VPN (Virtual Private Network)** - Creates encrypted tunnel between user's device and VPN server, masking IP address and protecting data transmission. Essential for privacy on public networks and accessing geo-restricted content securely.

• **2FA/MFA (Two/Multi-Factor Authentication)** - Security process requiring multiple forms of verification before granting access. Combines something you know (password), something you have (phone/token), and/or something you are (biometrics). Significantly reduces account compromise risk.

• **Encryption** - Process of converting readable data into coded format that can only be decrypted with the proper key. Protects data both in transit (during transmission) and at rest (when stored). Essential for privacy and regulatory compliance.

• **Patch** - Software update that fixes security vulnerabilities, bugs, or adds new features. Critical for maintaining security as new vulnerabilities are constantly discovered. Should be applied promptly to minimize exposure to attacks.

• **Sandbox** - Isolated environment where suspicious programs can be executed safely without affecting the main system. Used for malware analysis, testing untrusted software, and containing potential threats.

This detailed summary provides comprehensive understanding of each cybersecurity concept. For specific implementation advice or additional questions about any topic, please ask!" }
            };
        }

        /// <summary>
        /// Method to search for keywords in user input and return appropriate responses
        /// </summary>
        public virtual string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "I didn't receive any valid input. Please try again.";
            }

            string lowerInput = input.Trim().ToLower();

            foreach (var key in _response.Keys)
            {
                if (lowerInput.Contains(key))
                {
                    return _response[key]; // Directly return the response string
                }
            }

            return ChatResponse(input);
        }

        /// <summary>
        /// Handles general chat responses when no specific cybersecurity keywords are detected
        /// </summary>
        public string ChatResponse(string input)
        {

            // Basic input validation
            if (string.IsNullOrWhiteSpace(input))
            {
                return "I didn't receive any valid input. Please try again.";
            }

            // Trim and convert input to lowercase for consistent matching
            string lowerInput = input.Trim().ToLower();

            // Check for various general conversation patterns
            if (lowerInput.Contains("how are you"))
            {
                return "I'm doing well thanks, please feel free to ask me anyhting cybersecurity related!";
            }
            else if (lowerInput.Contains("what's your purpose") || lowerInput.Contains("what is your purpose"))
            {
                return "I help educate users on cybersecurity best practices and answer questions about online threats.";
            }
            else if (lowerInput.Contains("what can i ask") || lowerInput.Contains("help") || lowerInput.Contains("commands"))
            {
                return "You can ask about password safety, phishing scams, ransomware, viruses, and other cybersecurity topics. Try asking for a 'cybersecurity guide' for comprehensive information!";
            }
            else if (lowerInput.Contains("hello") || lowerInput.Contains("hi"))
            {
                return "Hello! How can I help with your cybersecurity questions today?";
            }
            else if (lowerInput.Contains("thank you") || lowerInput.Contains("thanks"))
            {
                return "You are welcome! Is there anything else you would like help with?";
            }
            else if (lowerInput.Contains("goodbye") || lowerInput.Contains("bye"))
            {
                return "Goodbye! Stay safe online and remember to keep your security practices up to date.";
            }
            else
            {
                // If no specific response is found, return a default message
                return "I didn't quite understand that. Could you please rephrase that question.";
            }
        }
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//