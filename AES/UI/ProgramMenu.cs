using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AES.Enums;
using AES.ExtensionFunctions;

namespace AES.UI
{
    // Way too lazy to decouple the prints from the logic now.
    public class ProgramMenu
    {
        private const string PressAnyKey = "Press any key to continue";
        private readonly IList<Tuple<string[], string[]>> predefinedInput;

        public ProgramMenu()
        {
            predefinedInput = new List<Tuple<string[], string[]>>
            {
                new Tuple<string[], string[]>(
                    new string[] { "2b", "7e", "15", "16", "28", "ae", "d2", "a6", "ab", "f7", "15", "88", "09", "cf", "4f", "3c" },
                    new string[] { "32", "43", "f6", "a8", "88", "5a", "30", "8d", "31", "31", "98", "a2", "e0", "37", "07", "34" }),
                new Tuple<string[], string[]>(
                    new string[] { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0a", "0b", "0c", "0d", "0e", "0f" },
                    new string[] { "00", "11", "22", "33", "44", "55", "66", "77", "88", "99", "aa", "bb", "cc", "dd", "ee", "ff" })
            };
        }

        internal DecipherInputAction ChooseDecipherInputType()
        {
            int selectedCommand = PromptAndValidateUserForANumber(
                WriteDecipherActionPrompt,
                Enum.GetValues(typeof(DecipherInputAction)),
                (x, y) => { return (int)y.GetValue(x); });

            return (DecipherInputAction)selectedCommand;
        }

        public ProgramAction ChooseCipherAction()
        {
            int selectedCommand = PromptAndValidateUserForANumber(
                WriteCipherActionPrompt,
                Enum.GetValues(typeof(ProgramAction)),
                (x, y) => { return (int)y.GetValue(x); });

            return (ProgramAction)selectedCommand;
        }

        public CipherInputAction ChooseCipherInputType()
        {
            int selectedCommand = PromptAndValidateUserForANumber(
                WriteInitialInstructions,
                Enum.GetValues(typeof(CipherInputAction)),
                (x, y) => { return (int)y.GetValue(x); });

            return (CipherInputAction)selectedCommand;
        }

        public Tuple<string[], string[]> DisplayAndChoosePredefinedValue()
        {
            int selectedValue = PromptAndValidateUserForANumber(
                DisplayPredefinedValues,
                predefinedInput.ToArray(),
                (x, y) => { return x + 1; });

            return predefinedInput.ElementAt(selectedValue - 1);
        }

        public Tuple<string[], string[]> InputValues()
        {
            string[] userDefinedKey = PromptForBytes("Specify the AES key as hex byte representations (Without whitespace):");
            string[] userDefinedInputBytes = PromptForBytes("Specify the input bytes as hex representations (Without whitespace):");
            ForeverHeader.Clear();

            return new Tuple<string[], string[]>(userDefinedKey, userDefinedInputBytes);
        }

        public void PromptForContinue()
        {
            Console.WriteLine(PressAnyKey);
            Console.ReadKey(true);
            ForeverHeader.Clear();
        }

        internal void NoLastCipherTextWarning()
        {
            Console.WriteLine("No cipher text generated during this run of the program.");
            Console.WriteLine("Please choose to input Your own values");
            PromptForContinue();
        }

        private int PromptAndValidateUserForANumber(Action WriteInstructions, Array choiceEnumeration, Func<int, Array, int> enumerationIndexRetrieval)
        {
            ForeverHeader.Clear();
            WriteInstructions();
            int selectedCommand = 0;
            while (true)
            {
                ConsoleKeyInfo keyRead = Console.ReadKey(true);
                if (!int.TryParse(keyRead.KeyChar.ToString(), out selectedCommand))
                {
                    ForeverHeader.Clear();
                    WriteInstructions();
                    Console.WriteLine("You should try pressing a number");
                    continue;
                }

                IEnumerable<int> allowedCommands = (choiceEnumeration.Cast<object>()).Select((x, index) => enumerationIndexRetrieval(index, choiceEnumeration));
                if (!allowedCommands.Contains(selectedCommand))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("You can only choose between the numbers ");
                    foreach (int commandIndex in allowedCommands)
                    {
                        builder.Append($"{commandIndex}, ");
                    }

                    ForeverHeader.Clear();
                    WriteInstructions();
                    Console.WriteLine(builder.ToString());
                    continue;
                }
                else
                {
                    break;
                }
            }

            ForeverHeader.Clear();
            return selectedCommand;
        }

        private void DisplayPredefinedValues()
        {
            Console.WriteLine("Choose from one of the predefined values");
            StringBuilder displayBuilder = new StringBuilder();
            for (int i = 0; i < predefinedInput.Count; i++)
            {
                displayBuilder.AppendLine($"{i + 1}: Key  : {ConcatenateStringArray(predefinedInput[i].Item1)}");
                displayBuilder.AppendLine($"   Input: {ConcatenateStringArray(predefinedInput[i].Item2)}");
            }

            Console.WriteLine(displayBuilder.ToString());
        }

        private bool isHexadecimalByteRepresentation(string hex)
        {
            byte temp;
            return byte.TryParse(hex, NumberStyles.HexNumber, null, out temp);
        }

        private string ConcatenateStringArray(string[] stringArray)
        {
            StringBuilder builder = new StringBuilder();
            foreach(string item in stringArray)
            {
                builder.Append(item);
            }

            return builder.ToString();
        }

        private string[] PromptForBytes(string initialMessage)
        {
            while (true)
            {
                ForeverHeader.Clear();
                Console.WriteLine(initialMessage);
                string temp = Console.ReadLine();
                if (temp.Length != 32)
                {
                    Console.WriteLine("The key has to be 16 bytes - aka 32 characters symbolising 16 hexadecimal bytes");
                    Console.WriteLine(PressAnyKey);
                    Console.ReadKey(true);
                    continue;
                }

                string[] tempArray = temp.Chunk(2).Select(x => new string(x.ToArray())).ToArray();
                if (tempArray.Any(x => !isHexadecimalByteRepresentation(x)))
                {
                    Console.WriteLine("One/some of the bytes are not in a correct hexadecimal form - eg. \"FF\"");
                    Console.WriteLine(PressAnyKey);
                    Console.ReadKey(true);
                    continue;
                }

                return tempArray;
            }
        }

        // Some nice loose coupling going on right here.
        private void WriteDecipherActionPrompt()
        {
            Console.WriteLine("You can decipher the last created cipher text or input a new one");
            Console.WriteLine("Press 1 to use the already generated cipher text");
            Console.WriteLine("Press 2 to input new cipher text");
        }

        // Some nice loose coupling going on right here.
        private void WriteCipherActionPrompt()
        {
            Console.WriteLine("You can choose to cipher or decipher a block of bytes");
            Console.WriteLine("Press 1 to start the Cipher program");
            Console.WriteLine("Press 2 to start the Decipher program");
        }

        // Some nice loose coupling going on right here.
        private void WriteInitialInstructions()
        {
            Console.WriteLine("You can choose to run the cipher with a test key/input or use Your own.");
            Console.WriteLine("Press 1 to choose predefined values");
            Console.WriteLine("Press 2 to input Your own values");
        }
    }
}
