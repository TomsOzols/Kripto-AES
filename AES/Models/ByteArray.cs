using System;

namespace AES.Models
{
    public class ByteArray
    {
        public ByteArray(byte[] bytes, int size)
        {
            if (bytes.Length != size * size)
            {
                throw new ArgumentOutOfRangeException();
            }

            Length = size;
            Bytes = new byte[size, size];
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    Bytes[i, j] = bytes[j * Length + i];
                }
            }
        }

        public byte this[int row, int column]
        {
            get
            {
                return Bytes[row, column];
            }
            set
            {
                Bytes[row, column] = value;
            }
        }

        public ByteArray(int size)
        {
            this.Length = size;
            Bytes = new byte[size, size];
        }

        private byte[,] Bytes { get; set; }

        public int Length { get; private set; }

        public byte[] Bytes1dArray
        {
            get
            {
                byte[] array = new byte[Length * Length];
                for (int i = 0; i < Length; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        array[i * Length + j] = Bytes[j, i];
                    }
                }

                return array;
            }
        }
    }
}