using System;
using System.Collections.Generic;
using System.Linq;
using AES.Listeners;
using AES.Models;

namespace AES
{
    class AesCipher
    {
        private const string AddRoundKeyCommandName = "Add Round key";
        private const string ShiftRowsCommandName = "Shift rows";
        private const string SubstituteBytesCommandName = "Substitute bytes";
        private const string InitialCommandName = "Initial";
        private const string MixColumnsCommandName = "Mix columns";
        private const int ArrayDimensionLength = 4;

        private readonly SBox sBox;
        private readonly StateTracker stateTracker;
        private readonly AesCipherListener listener;
        private readonly RijndaelOperations operations;

        public AesCipher()
        {
            stateTracker = new StateTracker(ArrayDimensionLength);
            listener = new AesCipherListener(stateTracker);
            operations = new RijndaelOperations();
            sBox = new SBox();
        }

        /// <summary>
        /// The AES cipher program.
        /// </summary>
        /// <param name="in">The byte array signifying the chunk to be ciphered</param>
        /// <param name="roundKeys">All the round keys grouped in groups of 4</param>
        /// <returns>A ByteArray with the cipher text of the original 16 bytes</returns>
        public ByteArray Cipher (byte[] @in, IEnumerable<RoundWords> roundKeys)
        {
            ValidateInputData(@in, roundKeys);

            stateTracker.UpdateState((x) => { return new ByteArray(@in, x.Length); }, 0, InitialCommandName);
            stateTracker.UpdateState(operations.AddRoundKey, 0, AddRoundKeyCommandName, roundKeys.First());
            for (int i = 1; i < 10; i++)
            {
                stateTracker.UpdateState(sBox.Subbyte, i, SubstituteBytesCommandName);
                stateTracker.UpdateState(operations.ShiftRows, i, ShiftRowsCommandName);
                stateTracker.UpdateState(operations.MixColumns, i, MixColumnsCommandName);
                stateTracker.UpdateState(operations.AddRoundKey, i, AddRoundKeyCommandName, roundKeys.ElementAt(i));
            }

            stateTracker.UpdateState(sBox.Subbyte, 10, SubstituteBytesCommandName);
            stateTracker.UpdateState(operations.ShiftRows, 10, ShiftRowsCommandName);
            stateTracker.UpdateState(operations.AddRoundKey, 10, AddRoundKeyCommandName, roundKeys.Last());

            ByteArray finalState = stateTracker.GetFinalStateAndClean();
            return finalState;
        }

        public ByteArray Decipher(byte[] @in, IEnumerable<RoundWords> roundKeys)
        {
            ValidateInputData(@in, roundKeys);

            stateTracker.UpdateState((x) => { return new ByteArray(@in, x.Length); }, 0, InitialCommandName);
            stateTracker.UpdateState(operations.AddRoundKey, 0, AddRoundKeyCommandName, roundKeys.Last());
            stateTracker.UpdateState(operations.InvertedShiftRows, 0, ShiftRowsCommandName);
            stateTracker.UpdateState(sBox.InvertedSubbyte, 0, SubstituteBytesCommandName);

            for (int i = 1; i < 10; i++)
            {
                stateTracker.UpdateState(operations.AddRoundKey, i, AddRoundKeyCommandName, roundKeys.ElementAt(10 - i));
                stateTracker.UpdateState(operations.InvertedMixColumns, i, MixColumnsCommandName);
                stateTracker.UpdateState(operations.InvertedShiftRows, i, ShiftRowsCommandName);
                stateTracker.UpdateState(sBox.InvertedSubbyte, i, SubstituteBytesCommandName);
            }

            stateTracker.UpdateState(operations.AddRoundKey, 10, AddRoundKeyCommandName, roundKeys.First());

            ByteArray finalState = stateTracker.GetFinalStateAndClean();
            return finalState;
        }

        private static void ValidateInputData(byte[] @in, IEnumerable<RoundWords> roundKeys)
        {
            if (@in.Length != 16)
            {
                throw new ArgumentOutOfRangeException("@in", "The input chunk must be 16 bytes long");
            }
            if (roundKeys.Count() != 11)
            {
                throw new ArgumentOutOfRangeException("roundKeys", "Must have 11 rounds");
            }
        }
    }
}
