using System;
using System.Linq;
using AES.Models;

namespace AES
{
    class RijndaelOperations
    {
        public ByteArray InvertedMixColumns(ByteArray state)
        {
            ByteArray mixColumns = new ByteArray(state.Length);
            // The matrix multiplication against the polynomial a^-1(x) = {0b}x^3 + {0d}x^2 + {09}x + {0e}
            for (int c = 0; c < 4; c++)
            {
                mixColumns[0, c] = (byte)(GaloisMultiply(0x0e, state[0, c]) ^ GaloisMultiply(0x0b, state[1, c]) ^ GaloisMultiply(0x0d, state[2, c]) ^ GaloisMultiply(0x09, state[3, c]));
                mixColumns[1, c] = (byte)(GaloisMultiply(0x09, state[0, c]) ^ GaloisMultiply(0x0e, state[1, c]) ^ GaloisMultiply(0x0b, state[2, c]) ^ GaloisMultiply(0x0d, state[3, c]));
                mixColumns[2, c] = (byte)(GaloisMultiply(0x0d, state[0, c]) ^ GaloisMultiply(0x09, state[1, c]) ^ GaloisMultiply(0x0e, state[2, c]) ^ GaloisMultiply(0x0b, state[3, c]));
                mixColumns[3, c] = (byte)(GaloisMultiply(0x0b, state[0, c]) ^ GaloisMultiply(0x0d, state[1, c]) ^ GaloisMultiply(0x09, state[2, c]) ^ GaloisMultiply(0x0e, state[3, c]));
            }

            return mixColumns;
        }

        public ByteArray MixColumns(ByteArray state)
        {
            ByteArray mixColumns = new ByteArray(state.Length);
            // The matrix multiplication against the polynomial a(x) = {03}x^3 + {01}x^2 + {01}x + {02}
            for (int c = 0; c < 4; c++)
            {
                mixColumns[0, c] = (byte)(GaloisMultiply(0x02, state[0, c]) ^ GaloisMultiply(0x03, state[1, c]) ^ state[2, c] ^ state[3, c]);
                mixColumns[1, c] = (byte)(state[0, c] ^ GaloisMultiply(0x02, state[1, c]) ^ GaloisMultiply(0x03, state[2, c]) ^ state[3, c]);
                mixColumns[2, c] = (byte)(state[0, c] ^ state[1, c] ^ GaloisMultiply(0x02, state[2, c]) ^ GaloisMultiply(0x03, state[3, c]));
                mixColumns[3, c] = (byte)(GaloisMultiply(0x03, state[0, c]) ^ state[1, c] ^ state[2, c] ^ GaloisMultiply(0x02, state[3, c]));
            }

            return mixColumns;
        }

        public ByteArray ShiftRows(ByteArray state)
        {
            ByteArray shiftState = new ByteArray(state.Length);
            for (int i = 0; i < state.Length; i++)
            {
                for (int j = 0; j < state.Length; j++)
                {
                    int columnToTake = (j + i) % state.Length;
                    shiftState[i, j] = state[i, columnToTake];
                }
            }

            return shiftState;
        }

        public ByteArray InvertedShiftRows(ByteArray state)
        {
            ByteArray shiftState = new ByteArray(state.Length);
            for (int i = 0; i < state.Length; i++)
            {
                for (int j = 0; j < state.Length; j++)
                {
                    int columnToTake = (j + i) % state.Length;
                    shiftState[i, columnToTake] = state[i, j];
                }
            }

            return shiftState;
        }

        public ByteArray AddRoundKey(ByteArray state, RoundWords roundKeys4)
        {
            if (roundKeys4.Words.Count() != 4)
            {
                throw new ArgumentOutOfRangeException("roundKeys4", "Round key must have 4 words");
            }

            ByteArray copyState = new ByteArray(state.Length);
            for (int i = 0; i < state.Length; i++)
            {
                for (int j = 0; j < state.Length; j++)
                {
                    byte[] word = roundKeys4.Words.ElementAt(j);
                    byte stateByte = state[i, j];
                    byte wordByte = word[i];
                    copyState[i, j] = (byte)(stateByte ^ wordByte);
                }
            }

            return copyState;
        }

        // Magic
        private byte GaloisMultiply(byte a, byte b)
        {
            byte temp = 0;
            byte highBitSet;
            for (int counter = 0; counter < 8; counter++)
            {
                // If the last bit of b is 1, we xor temp with a.
                if ((b & 1) != 0)
                {
                    temp ^= a;
                }

                // Gets the first bit of 'a'.
                highBitSet = (byte)(a & 0x80);
                // The bit is pushed left and out of context, so that during the next iteration we could see the next bit.
                a <<= 1;

                // If the bit was 1, we xor 'a' with x^5 + x^4 + x^2 + x + 1
                if (highBitSet != 0)
                {
                    a ^= 0x1b;
                }

                // The last bit of 'b' is pushed rightwards out of context, so that during the next iteration we could check if the previous from last bit is 1 or 0.
                b >>= 1;
            }

            return temp;
        }
    }
}
