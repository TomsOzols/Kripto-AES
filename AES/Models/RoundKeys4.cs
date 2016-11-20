using System.Collections.Generic;

namespace AES.Models
{
    public class RoundWords
    {
        public RoundWords()
        {
            Words = new List<byte[]>();
        }

        public IEnumerable<byte[]> Words { get; set; }
    }
}