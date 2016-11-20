using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using AES.UI;
using AES.Enums;
using AES.Listeners;
using AES.Models;

namespace AES
{
    class Program
    {
        private const string SurpriseExceptionMessage = "How did You even get here?";

        static void Main(string[] args)
        {
            ProgramMenu menu = new ProgramMenu();

            Tuple<string[], string[]> lastCipherText = null;
            while (true)
            {
                ProgramAction direction = menu.ChooseCipherAction();

                AesCipher cipherProgram = new AesCipher();
                Tuple<string[], string[]> valuesToUse = null;
                while (valuesToUse == null)
                {
                    switch (direction)
                    {
                        case ProgramAction.Cipher:
                            {
                                CipherInputAction menuStartingChoice = menu.ChooseCipherInputType();
                                valuesToUse = GetCipherProgramValuesToUse(menu, menuStartingChoice);
                                IEnumerable<RoundWords> roundKeys = GetRoundKeys(valuesToUse);
                                menu.PromptForContinue();

                                byte[] inputBytes = valuesToUse.Item2.Select(x => CreateByteFromHexadecimal(x)).ToArray();
                                ByteArray result = cipherProgram.Cipher(inputBytes, roundKeys);
                                lastCipherText = new Tuple<string[], string[]>(valuesToUse.Item1, result.Bytes1dArray.Select(x => CreateHexadecimalFromByte(x)).ToArray());
                                break;
                            }
                        case ProgramAction.Decipher:
                            {
                                DecipherInputAction menuStartingChoice = menu.ChooseDecipherInputType();
                                valuesToUse = GetDecipherProgramValuesToUse(menu, menuStartingChoice, lastCipherText);
                                IEnumerable<RoundWords> roundKeys = GetRoundKeys(valuesToUse);
                                menu.PromptForContinue();

                                byte[] inputBytes = valuesToUse.Item2.Select(x => CreateByteFromHexadecimal(x)).ToArray();
                                cipherProgram.Decipher(inputBytes, roundKeys);
                                break;
                            }
                        default:
                            {
                                throw new Exception(SurpriseExceptionMessage);
                            }
                    }
                }

                menu.PromptForContinue();
            }
        }

        private static IEnumerable<RoundWords> GetRoundKeys(Tuple<string[], string[]> valuesToUse)
        {
            IAesKeyExpanderListener keyListener = new AesKeyExpanderListener();
            AesKeyExpander keyExpander = new AesKeyExpander(keyListener);
            byte[] keyBytes = valuesToUse.Item1.Select(x => CreateByteFromHexadecimal(x)).ToArray();
            IEnumerable<byte[]> allKeys = keyExpander.ExpandKey(keyBytes);
            IEnumerable<RoundWords> roundKeys = ChunkTogetherRoundKeys(allKeys);
            return roundKeys;
        }

        private static Tuple<string[], string[]> GetDecipherProgramValuesToUse(ProgramMenu menu, DecipherInputAction menuStartingChoice, Tuple<string[], string[]> lastCipherText)
        {
            Tuple<string[], string[]> valuesToUse = null;
            switch (menuStartingChoice)
            {
                case DecipherInputAction.LastGeneratedCipherText:
                    {
                        if (lastCipherText != null)
                        {
                            valuesToUse = lastCipherText;
                        }
                        else
                        {
                            menu.NoLastCipherTextWarning();
                        }

                        break;
                    }
                case DecipherInputAction.UserInput:
                    {
                        valuesToUse = menu.InputValues();
                        break;
                    }
                default:
                    {
                        throw new Exception(SurpriseExceptionMessage);
                    }
            }

            return valuesToUse;
        }

        private static Tuple<string[], string[]> GetCipherProgramValuesToUse(ProgramMenu menu, CipherInputAction menuStartingChoice)
        {
            Tuple<string[], string[]> valuesToUse;
            switch (menuStartingChoice)
            {
                case CipherInputAction.Predefined:
                    {
                        valuesToUse = menu.DisplayAndChoosePredefinedValue();
                        break;
                    }
                case CipherInputAction.UserInput:
                    {
                        valuesToUse = menu.InputValues();
                        break;
                    }
                default:
                    {
                        throw new Exception(SurpriseExceptionMessage);
                    }
            }

            return valuesToUse;
        }

        private static byte CreateByteFromHexadecimal(string x)
        {
            byte hexaToByteResult = byte.Parse(x, NumberStyles.HexNumber);
            return hexaToByteResult;
        }

        private static string CreateHexadecimalFromByte(byte x)
        {
            string byteToHexaResult = BitConverter.ToString(new byte[] { x });
            return byteToHexaResult;
        }

        private static IEnumerable<RoundWords> ChunkTogetherRoundKeys(IEnumerable<byte[]> allKeys)
        {
            IList<RoundWords> roundWords = new List<RoundWords>();
            for (int i = 0; i < allKeys.Count() / 4; i++)
            {
                RoundWords wordWrap = new RoundWords();
                IEnumerable<byte[]> wordsAfterSkip = allKeys.Skip(i * 4);
                IEnumerable<byte[]> words = wordsAfterSkip.Take(4);
                wordWrap.Words = words;
                roundWords.Add(wordWrap);
            }

            return roundWords;
        }
    }
}