using System;
using System.Text;
using AES.Models;

namespace AES.Listeners
{
    internal class AesCipherListener
    {
        public AesCipherListener(StateTracker stateTracker)
        {
            stateTracker.StateChange += new EventHandler<AesStateChangeEventArgs>(HearState);
        }

        void HearState(object sender, AesStateChangeEventArgs args)
        {
            ListenToState(args.State, args.RoundNumber, args.ActionName);
        }

        private void ListenToState(ByteArray state, int round, string command)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{round}: {command}, Bytes:\n");
            for (int i = 0; i < state.Length; i++)
            {
                for (int j = 0; j < state.Length; j++)
                {
                    byte currentByte = state[i, j];
                    byte[] byteWrap = new byte[] { currentByte };
                    string byteString = BitConverter.ToString(byteWrap);
                    builder.Append(byteString);
                    builder.Append(" ");
                }

                builder.Append("\n");
            }

            Console.WriteLine(builder.ToString());
        }
    }
}
