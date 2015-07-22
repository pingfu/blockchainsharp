using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BlockChainSharp;

namespace console
{
    /// <summary>
    /// blockchain file format | blockchain format | blockchain c# library | c# blockchain | blockchain unique addresses | c# blockchain parser
    /// bitcoin does key exist in blockchain | view the bitcoin blockchain | bitcoin blockchain
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public static BlockChainReader Bcr = new BlockChainReader(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Bitcoin\blocks"));
        
        /// <summary>
        /// 
        /// </summary>
        public static Timer Timer;

        /// <summary>
        /// 
        /// </summary>
        public static HashSet<byte[]> PublicKeyDictionary = new HashSet<byte[]>();

        /// <summary>
        /// 
        /// </summary>
        public static Int64 N = -1; // first read increments to block 0

        /// <summary>
        /// 
        /// </summary>
        public static Int64 T = 0;  // transaction output counter

        /// <summary>
        /// 
        /// </summary>
        public static Int64 L = 0;  // average transactions per interval

        /// <summary>
        /// 
        /// </summary>
        public static Int64 LastCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Timer = new Timer(UpdateConsoleWindow, null, 500, 500);

            while (Bcr.Read())
            {
                N++;
                L += Bcr.CurrentBlock.TransactionCount;
                
                /*
                 * todo: Storing each unique coin in a hashset drops an out of memory exception at around 6 million
                 * todo:    -- expensive operation, should it be a feature flag?
                 * 
                foreach (var output in Bcr.CurrentBlock.Transactions.SelectMany(transaction => transaction.Outputs))
                {
                    ThreadPool.QueueUserWorkItem(StoreBitcoinAddress, output.EcdsaPublickey);
                    T++;
                }*/
            }
            Console.WriteLine("finished");
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static void UpdateConsoleWindow(Object obj)
        {
            var blockDelta = N - LastCount;

            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("currentFile:                 {0}", Bcr.WorkingFile.Current.FullName);
            Console.WriteLine();
            Console.WriteLine("observedBlocks:              {0:n0} blocks", N);
            Console.WriteLine("deltaReads:                  {0:n0}", blockDelta);
            Console.WriteLine();
            Console.WriteLine("queueBufferSize:             {0:n0} bytes", Bcr.QueueLength);
            Console.WriteLine("averageTransactionsObserved: {0:n0}", (L / blockDelta));
            Console.WriteLine("observedTransactions:        {0:n0}", T);
            Console.WriteLine();
            Console.WriteLine("uniqueCoinAddresses:         {0:n0}", PublicKeyDictionary.Count);
            Console.WriteLine();
            LastCount = N;
            L = 0;

            /*
            Console.WriteLine("magicBytes:                  {0}", BitConverter.ToString(Bcr.CurrentBlock.MagicBytes));
            Console.WriteLine("blockSize:                   {0}", Bcr.CurrentBlock.BlockSize);
            Console.WriteLine("blockFormatVersion:          {0}", Bcr.CurrentBlock.BlockFormatVersion);
            Console.WriteLine("previousBlockHash:           {0}", BitConverter.ToString(Bcr.CurrentBlock.PreviousBlockHash).Replace("-", ""));
            Console.WriteLine("markleRoot:                  {0}", BitConverter.ToString(Bcr.CurrentBlock.MerkleRoot).Replace("-", ""));
            Console.WriteLine("timeStamp:                   {0}", Bcr.CurrentBlock.TimeStamp);
            Console.WriteLine("bits:                        {0}", Bcr.CurrentBlock.Bits);
            Console.WriteLine("nonce:                       {0}", Bcr.CurrentBlock.Nonce);
            Console.WriteLine("transactionCount:            {0}", Bcr.CurrentBlock.TransactionCount);
            Console.WriteLine();

            foreach (var transaction in Bcr.CurrentBlock.Transactions)
            {
                Console.WriteLine(" + transactionVersionNumber: {0}", transaction.TransactionVersionNumber);
                Console.WriteLine("   inputCount:               {0}", transaction.InputCount);
                Console.WriteLine("   outputCount:              {0}", transaction.OutputCount);
                Console.WriteLine("   lockTime:                 {0}", transaction.LockTime);

                Console.WriteLine();

                foreach (var intput in transaction.Inputs)
                {
                    Console.WriteLine("   -> inputTransactionHash:  {0}", BitConverter.ToString(intput.InputTransactionHash).Replace("-", ""));
                    Console.WriteLine("    > inputTransactionIndex: {0}", intput.InputTransactionIndex);
                    Console.WriteLine("    > responseScriptLength:  {0}", intput.ResponseScriptLength);
                    Console.WriteLine("    > responseScript:        {0}", Encoding.ASCII.GetString(intput.ResponseScript));
                    Console.WriteLine("    > sequenceNumber:        {0}", intput.SequenceNumber);
                    Console.WriteLine();
                }

                foreach (var output in transaction.Outputs)
                {
                    Console.WriteLine("   -> outputValue:           {0}", output.OutputValue / 100000000 + " BTC");
                    Console.WriteLine("    > challengeScriptLength: {0}", output.ChallengeScriptLength);
                    Console.WriteLine("    > challengeScript:       {0}", BitConverter.ToString(output.ChallengeScript).Replace("-", string.Empty));
                    Console.WriteLine("    > ecdsaPublickey:        {0}", BitConverter.ToString(output.EcdsaPublickey).Replace("-", string.Empty));
                    Console.WriteLine("    > bitcoinAddress:        {0}", output.BitcoinAddress);
                    Console.WriteLine();
                }
            } */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ecdsaPublickey"></param>
        public static void StoreBitcoinAddress(object ecdsaPublickey)
        {
            var bytes = (byte[]) ecdsaPublickey;

            if (!PublicKeyDictionary.Contains(bytes))
            {
                PublicKeyDictionary.Add(bytes);
            }
        }
    }
}
