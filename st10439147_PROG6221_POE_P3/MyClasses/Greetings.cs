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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;// Importing System.Threading namespace for thread management
using System.Media;// Importing System.Media namespace for sound playback
using System.IO;// Importing System.IO namespace for file handling

namespace ST10439147_PROG6221_POE_P3.MyClasses
{
    internal class Greeting
    {
        public Greeting() { }

        public void PlayGreeting()// Method to play the greeting sound
        {
            try
            {
                // Get the absolute path of the sound file
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Welcome.wav");

                // Check if the file exists
                if (!File.Exists(filePath))// If the file does not exist, error message will be displayed
                {
                    Console.WriteLine("Error: The file 'Welcome.wav' does not exist in the application directory.");
                    return;
                }

                // Used additional debugging information
                //Console.WriteLine("File found: " + filePath);

                using (SoundPlayer player = new SoundPlayer(filePath))
                {
                    player.Load();  // Ensure the file is loaded before playing
                    player.PlaySync();  // Play and wait for completion because PlaySync is used due to the fact that the sound file is not too long
                                        // Console.WriteLine("Playing sound...");
                }
            }
            catch (FileNotFoundException fnfEx)// Catch specific exceptions, FileNotFoundException
            {
                Console.WriteLine("File not found: " + fnfEx.Message);// Display error message
            }
            catch (DirectoryNotFoundException dnfEx)// Catch specific exceptions, DirectoryNotFoundException
            {
                Console.WriteLine("Directory not found: " + dnfEx.Message);// Display error message
            }
            catch (InvalidOperationException ioEx)// Catch specific exceptions, InvalidOperationException. This means that the sound file is not in the correct format
            {
                Console.WriteLine("Invalid operation: " + ioEx.Message);// Display error message
            }
            catch (System.IO.IOException ioEx)// Catch specific exceptions, IOException. This means that the file is not accessible
            {
                Console.WriteLine("Invalid operation: " + ioEx.Message);// Display error message
            }
            catch (UnauthorizedAccessException uaEx)// Catch specific exceptions, UnauthorizedAccessException. This means that the file is not accessible
            {
                Console.WriteLine("Access denied: " + uaEx.Message);// Display error message
            }
            catch (Exception ex)// Catch all other exceptions
            {
                Console.WriteLine("General error playing greeting: " + ex.Message);// Display error message
            }
        }
    }
}
//----------------------------------------------------------------DDDDDoooooo END OF FILE DDDDDoooooooo----------------------------------------------------------------------------------------------------------//