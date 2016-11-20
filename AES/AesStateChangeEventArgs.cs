using System;
using AES.Models;

namespace AES
{
    public class AesStateChangeEventArgs : EventArgs
    {
        public string ActionName { get; private set; }
        public int RoundNumber { get; private set; }
        public ByteArray State { get; private set; }

        // Real nice and safe state.
        public AesStateChangeEventArgs(ByteArray state, int roundNumber, string actionName)
        {
            this.State = state;
            this.RoundNumber = roundNumber;
            this.ActionName = actionName;
        }
    }
}