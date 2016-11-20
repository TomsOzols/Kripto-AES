using System;
using AES.Models;

namespace AES
{
    public class StateTracker
    {
        private readonly int byteArraySize;

        private string LastActionName { get; set; }
        private int RoundNumber { get; set; }
        private ByteArray State { get; set; }

        public StateTracker(int byteArraySize)
        {
            this.byteArraySize = byteArraySize;
            State = new ByteArray(byteArraySize);
        }

        public void UpdateState(Func<ByteArray, ByteArray> stateChangeMethod, int roundNumber, string actionName)
        {
            State = stateChangeMethod(State);
            RoundNumber = roundNumber;
            LastActionName = actionName;
            OnStateChange(new AesStateChangeEventArgs(State, RoundNumber, LastActionName));
        }

        public void UpdateState(Func<ByteArray, RoundWords, ByteArray> addRoundKeyMethod, int roundNumber, string actionName, RoundWords roundWords)
        {
            State = addRoundKeyMethod(State, roundWords);
            RoundNumber = roundNumber;
            LastActionName = actionName;
            OnStateChange(new AesStateChangeEventArgs(State, RoundNumber, LastActionName));
        }

        public event EventHandler<AesStateChangeEventArgs> StateChange;

        protected virtual void OnStateChange(AesStateChangeEventArgs e)
        {
            StateChange?.Invoke(this, e);
        }

        public ByteArray GetFinalStateAndClean()
        {
            ByteArray final = State;
            State = new ByteArray(byteArraySize);
            LastActionName = null;
            RoundNumber = 0;
            return final;
        }
    }
}
