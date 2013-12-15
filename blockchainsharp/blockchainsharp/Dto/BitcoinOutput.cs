using System;

namespace BlockChainSharp.Dto
{
    public class BitcoinOutput
    {
        public UInt64 OutputValue { get; set; }
        public Int64 ChallengeScriptLength { get; set; }
        public byte[] ChallengeScript { get; set; }
    }
}