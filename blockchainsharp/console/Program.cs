using System;
using System.IO;
using System.Text;
using BlockChainSharp;


namespace console
{
    /// <summary>
    /// blockchain file format
    /// blockchain format
    /// blockchain c# library
    /// c# blockchain
    /// blockchain unique addresses
    /// c# blockchain parser
    /// bitcoin does key exist in blockchain
    /// view the bitcoin blockchain
    /// bitcoin blockchain
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool ShowDebug = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var bcr = new BlockChainReader(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Bitcoin\blocks"));
            var n = 1;

            while (bcr.Read())
            {
                if (!ShowDebug) continue;

                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("currentFile:                 {0}", bcr.WorkingFile.Current.FullName);
                Console.WriteLine("blockNumber:                 {0}", n);
                Console.WriteLine("magicBytes:                  {0}", BitConverter.ToString(bcr.CurrentBlock.MagicBytes));
                Console.WriteLine("blockSize:                   {0}", bcr.CurrentBlock.BlockSize);
                Console.WriteLine("blockFormatVersion:          {0}", bcr.CurrentBlock.BlockFormatVersion);
                Console.WriteLine("previousBlockHash:           {0}", BitConverter.ToString(bcr.CurrentBlock.PreviousBlockHash));
                Console.WriteLine("markleRoot:                  {0}", BitConverter.ToString(bcr.CurrentBlock.MerkleRoot));
                Console.WriteLine("timeStamp:                   {0}", bcr.CurrentBlock.TimeStamp);
                Console.WriteLine("bits:                        {0}", bcr.CurrentBlock.Bits);
                Console.WriteLine("nonce:                       {0}", bcr.CurrentBlock.Nonce);
                Console.WriteLine("transactionCount:            {0}", bcr.CurrentBlock.TransactionCount);
                Console.WriteLine();

                foreach (var transaction in bcr.CurrentBlock.Transactions)
                {
                    Console.WriteLine(" transactionVersionNumber:   {0}", transaction.TransactionVersionNumber);
                    Console.WriteLine(" lockTime:                   {0}", transaction.LockTime);
                    Console.WriteLine(" inputCount:                 {0}", transaction.InputCount);
                    Console.WriteLine(" outputCount:                {0}", transaction.OutputCount);

                    Console.WriteLine();

                    foreach (var intput in transaction.Inputs)
                    {
                        Console.WriteLine(" -> inputTransactionHash:    {0}", BitConverter.ToString(intput.InputTransactionHash));
                        Console.WriteLine("  > inputTransactionIndex:   {0}", intput.InputTransactionIndex);
                        Console.WriteLine("  > responseScriptLength:    {0}", intput.ResponseScriptLength);
                        Console.WriteLine("  > responseScript:          {0}", Encoding.ASCII.GetString(intput.ResponseScript));
                        Console.WriteLine("  > sequenceNumber:          {0}", intput.SequenceNumber);
                        Console.WriteLine();
                    }

                    foreach (var output in transaction.Outputs)
                    {
                        Console.WriteLine(" -> outputValue:             {0}", output.OutputValue / 100000000 + " BTC");
                        Console.WriteLine("  > challengeScriptLength:   {0}", output.ChallengeScriptLength);
                        Console.WriteLine("  > challengeScript:         {0}", BitConverter.ToString(output.ChallengeScript).Replace("-", string.Empty));
                        Console.WriteLine("  > ecdsaPublickey:          {0}", BitConverter.ToString(output.EcdsaPublickey).Replace("-", string.Empty));
                        Console.WriteLine("  > bitcoinAddress:          {0}", output.BitcoinAddress);
                        Console.WriteLine();
                    }
                }
                n++;

                Console.ReadLine();
            }
            
            Console.WriteLine("finished");
            Console.ReadLine();
        }
    }
}
