namespace AES.Listeners
{
    public interface IAesKeyExpanderListener
    {
        void ListenToWord(byte[] word, int round, string command);
    }
}