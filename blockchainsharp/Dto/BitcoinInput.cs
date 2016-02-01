using System;

namespace BlockChainSharp.Dto
{
    public class BitcoinInput
    {
        public byte[] InputTransactionHash { get; set; }
        public UInt32 InputTransactionIndex { get; set; }
        public Int64 ResponseScriptLength { get; set; }
        public byte[] ResponseScript { get; set; }
        public UInt32 SequenceNumber { get; set; }       
    }
}