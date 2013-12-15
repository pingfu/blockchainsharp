using System;
using System.Collections.Generic;

namespace BlockChainSharp.Dto
{
    public class BitcoinBlock
    {
        public byte[] MagicBytes { get; set; }
        public UInt32 BlockSize { get; set; }
        public UInt32 BlockFormatVersion { get; set; }
        public byte[] PreviousBlockHash { get; set; }
        public byte[] MerkleRoot { get; set; }
        public DateTime TimeStamp { get; set; }
        public UInt32 Bits { get; set; }
        public UInt32 Nonce;
        public Int64 TransactionCount { get; set; }
        public List<BitcoinTransaction> Transactions { get; set; }

        public BitcoinBlock()
        {
            Transactions = new List<BitcoinTransaction>();
        }
    }
}
