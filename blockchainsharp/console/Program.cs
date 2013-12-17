using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static BlockChainReader Bcr = new BlockChainReader(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Bitcoin\blocks"));
        
        public static Timer Timer;

        public static HashSet<byte[]> PublicKeyDictionary = new HashSet<byte[]>();

        public static Int64 N = -1; // first read increments to block 0

        public static Int64 T = 0;  // transaction output counter

        public static Int64 L = 0;  // average transactions per interval

        public static Int64 LastCount = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Console.WriteLine("ready...");
            Console.ReadLine();

            Timer = new Timer(TimerCallback, null, 600, 100);

            while (Bcr.Read())
            {
                N++;
                L += Bcr.CurrentBlock.TransactionCount;

                foreach (var output in Bcr.CurrentBlock.Transactions.SelectMany(transaction => transaction.Outputs))
                {
                    ThreadPool.QueueUserWorkItem(ProcessOutput, output.EcdsaPublickey);
                    T++;
                }
            }
            Console.WriteLine("finished");
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public static void TimerCallback(Object obj)
        {
            try
            {
                var blockDelta = N - LastCount;

                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("currentFile:                 {0}", Bcr.WorkingFile.Current.FullName);
                Console.WriteLine();
                Console.WriteLine("deltaReads:                  {0:n0} blocks", blockDelta);
                Console.WriteLine("averageTransactionsObserved: {0:n0}", (L / blockDelta));
                Console.WriteLine();
                Console.WriteLine("observedBlocks:              {0:n0}", N);
                Console.WriteLine("observedTransactions:        {0:n0}", T);
                Console.WriteLine();
                Console.WriteLine("queueBufferSize:             {0:n0} bytes", Bcr.QueueLength);
                Console.WriteLine("uniqueCoinAddresses:         {0:n0}", PublicKeyDictionary.Count);
                Console.WriteLine();
                LastCount = N;
                L = 0;
            }
            catch (Exception)
            {
                // keep on truckin..
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ecdsaPublickey"></param>
        public static void ProcessOutput(object ecdsaPublickey)
        {
            var bytes = (byte[]) ecdsaPublickey;

            if (!PublicKeyDictionary.Contains(bytes))
            {
                PublicKeyDictionary.Add(bytes);
            }
        }
    }
}
