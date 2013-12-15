using System;

namespace BlockChainSharp.Dto
{
    public class BitcoinOutput
    {
        /// <summary>
        /// 
        /// </summary>
        public UInt64 OutputValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int64 ChallengeScriptLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte[] ChallengeScript { get; set; }

        /// <summary>
        /// This is not part of the blockchain, it is extracted from ChallengeScript.
        /// </summary>
        public byte[] EcdsaPublickey { get; set; }

        /// <summary>
        /// This is not part of the blockchain, it is computing from the EcdsaPublicKey.
        /// </summary>
        public string BitcoinAddress { get; set; }
    }
}