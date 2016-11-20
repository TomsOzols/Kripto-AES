using System;
using System.Text;

namespace AES.Listeners
{
    internal class AesKeyExpanderListener : IAesKeyExpanderListener
    {
        public void ListenToWord(byte[] word, int round, string command)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{round}: {command}, Bytes: ");
            for (int i = 0; i < word.Length; i++)
            {
                byte currentByte = word[i];
                byte[] byteWrap = new byte[] { currentByte };
                string byteString = BitConverter.ToString(byteWrap);
                builder.Append(byteString);
                builder.Append(" ");
            }

            Console.WriteLine(builder.ToString());
        }
    }
}