using System;
using System.Collections.Generic;

namespace BlockChainSharp.Dto
{
    public class BitcoinTransaction
    {
        public UInt32 TransactionVersionNumber { get; set; }
        public Int64 InputCount { get; set; }
        public Int64 OutputCount { get; set; }
        public List<BitcoinInput> Inputs { get; set; }
        public List<BitcoinOutput> Outputs { get; set; }
        public UInt32 LockTime { get; set; }

        public BitcoinTransaction()
        {
            Inputs = new List<BitcoinInput>();
            Outputs = new List<BitcoinOutput>();
        }
    }
}