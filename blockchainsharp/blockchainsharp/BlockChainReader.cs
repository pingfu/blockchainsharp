using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BlockChainSharp.Dto;
using BlockChainSharp.PInvoke;
using BlockChainSharp.Util;

namespace BlockChainSharp
{
    public class BlockChainReader
    {
        /// <summary>
        /// default constructor, loads blockchain from default location
        /// </summary>
        public BlockChainReader()
        {
            _blockChainDirectory = new DirectoryInfo(_defaultBlockChainDirectory);
            CreateBlockChainEnumerator();
        }

        /// <summary>
        /// overloaded constructor allowing a custom blockchain directory
        /// </summary>
        /// <param name="blockChainDirectory"></param>
        public BlockChainReader(DirectoryInfo blockChainDirectory)
        {
            _blockChainDirectory = blockChainDirectory;
            CreateBlockChainEnumerator();
        }

        /// <summary>
        /// Holds the most recently parsed blockchain item from an execution of Read(). Allows the caller access to the data.
        /// </summary>
        public BitcoinBlock CurrentBlock;

        /// <summary>
        /// default location of the blockchain files
        /// </summary>
        private readonly String _defaultBlockChainDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Bitcoin\blocks";

        /// <summary>
        /// Returns an ordered enumerable collection of file information about each blockchain data file
        /// </summary>
        private static IEnumerable<FileInfo> _blockChainFiles;

        /// <summary>
        /// Provides an enumerator interface to the BlockChainFiles enumerable
        /// </summary>
        public IEnumerator<FileInfo> WorkingFile;

        /// <summary>
        /// location of blockchain files
        /// </summary>
        private static DirectoryInfo _blockChainDirectory;

        /// <summary>
        /// Provides a view of the current cluster size on disk. Declared class level to reduce PInvoke calls
        /// </summary>
        private long _readLength;

        /// <summary>
        /// Provides a reference to the current position in the FileStream between calls to Read()
        /// </summary>
        private long _position;

        /// <summary>
        /// The open FileStream spools data to this array before it is copied to a FIFO queue for simpler consumption
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Provides a reference to the open FileStream
        /// </summary>
        private FileStream _currentFile;

        /// <summary>
        /// Provides a sequential view of the blockchain to the Read() method
        /// </summary>
        private readonly Queue<byte> _byteQueue = new Queue<byte>();

        /// <summary>
        /// Assigns the enumerator we use on the enumerable the files on disk comprising the blockchain
        /// </summary>
        private void CreateBlockChainEnumerator()
        {
            _blockChainFiles = _blockChainDirectory.EnumerateFiles("blk*.dat").OrderBy(x => x.FullName);
            WorkingFile = _blockChainFiles.GetEnumerator();
        }

        /// <summary>
        /// Reads a single transaction off the blockchain in sequential order
        /// 
        /// thanks 
        /// https://en.bitcoin.it/wiki/Protocol_specification#block
        /// http://james.lab6.com/2012/01/12/bitcoin-285-bytes-that-changed-the-world/
        /// https://code.google.com/p/blockchain/source/browse/trunk/BlockChain.h
        /// </summary>
        public Boolean Read()
        {
            var newBlock = new BitcoinBlock { MagicBytes = Dequeue(4) };

            if (BitConverter.ToUInt32(newBlock.MagicBytes, 0) != 3652501241)
            {
                throw new Exception("Invalid magic number at the start of this block");
            }

            newBlock.BlockSize = BitConverter.ToUInt32(Dequeue(4), 0);
            newBlock.BlockFormatVersion = BitConverter.ToUInt32(Dequeue(4), 0);
            newBlock.PreviousBlockHash = ReverseArray(Dequeue(32));
            newBlock.MerkleRoot = ReverseArray(Dequeue(32));
            newBlock.TimeStamp = Helper.UnixToDateTime(BitConverter.ToUInt32(Dequeue(4), 0));
            newBlock.Bits = BitConverter.ToUInt32(Dequeue(4), 0);
            newBlock.Nonce = BitConverter.ToUInt32(Dequeue(4), 0);
            newBlock.TransactionCount = ReadVariableLengthInteger(Dequeue(1));

            for (var t = 0; t < newBlock.TransactionCount; t++)
            {
                var newTransaction = new BitcoinTransaction();
                newTransaction.TransactionVersionNumber = BitConverter.ToUInt32(Dequeue(4), 0);
                newTransaction.InputCount = ReadVariableLengthInteger(Dequeue(1));

                for (var i = 0; i < newTransaction.InputCount; i++)
                {
                    var newInput = new BitcoinInput();
                    newInput.InputTransactionHash = Dequeue(32);
                    newInput.InputTransactionIndex = BitConverter.ToUInt32(Dequeue(4), 0);
                    newInput.ResponseScriptLength = ReadVariableLengthInteger(Dequeue(1));
                    newInput.ResponseScript = Dequeue(newInput.ResponseScriptLength);
                    newInput.SequenceNumber = BitConverter.ToUInt32(Dequeue(4), 0);
                    newTransaction.Inputs.Add(newInput);
                }

                newTransaction.OutputCount = ReadVariableLengthInteger(Dequeue(1));

                for (var o = 0; o < newTransaction.OutputCount; o++)
                {
                    var newOutput = new BitcoinOutput();
                    newOutput.OutputValue = BitConverter.ToUInt64(Dequeue(8), 0);
                    newOutput.ChallengeScriptLength = ReadVariableLengthInteger(Dequeue(1));
                    newOutput.ChallengeScript = Dequeue(newOutput.ChallengeScriptLength);
                    newOutput.EcdsaPublickey = ExtractPublicKey(newOutput.ChallengeScript);
                    newOutput.BitcoinAddress = ComputeBitcoinAddress(newOutput.EcdsaPublickey);
                    newTransaction.Outputs.Add(newOutput);
                }

                newTransaction.LockTime = BitConverter.ToUInt32(Dequeue(4), 0);
                newBlock.Transactions.Add(newTransaction);
            }

            CurrentBlock = newBlock;
            return true;
        }

        /// <summary>
        /// Pulls the specified number of bytes out of the queue
        /// </summary>
        /// <param name="count">Number of bytes to dequeue</param>
        /// <returns></returns>
        private byte[] Dequeue(Int64 count)
        {
            if (_byteQueue.Count < 2097152) 
            {
                BlockChainReadAhead();
            }

            var byteArray = new byte[count];

            for (var i = 0; i < count; i++)
            {
                // todo: a fifo queue operating byte by byte is WAY too slow, what can we replace this with?
                // todo: BufferedStream operating on a BinaryReader?
                // todo: light fifo memory stream?
                // todo: Array.Copy?
                byteArray[i] = _byteQueue.Dequeue();
            }

            return byteArray;
        }

        /// <summary>
        /// Provides seemless read-ahead to the blockchain queue across the entire blockchain file set.
        /// </summary>
        private void BlockChainReadAhead()
        {
            if (WorkingFile.Current != null && _currentFile != null && _position == WorkingFile.Current.Length )
            {
                // the last call to this method finished up the current file, dispose
                _currentFile.Close();
                _currentFile.Dispose();
                _currentFile = null;
            }

            if (_currentFile == null)
            {
                if (WorkingFile.MoveNext() == false)
                {
                    // no where left to go, we've hit the end of the blockchain.
                    return;
                }

                // setup to read from the next file in the blockchain
                _readLength = Disk.GetDiskClusterSize(_blockChainDirectory);
                _data = new byte[_readLength];
                _currentFile = new FileStream(WorkingFile.Current.FullName, FileMode.Open);
                _position = 0;
            }

            if (WorkingFile.Current != null && (_position + _readLength) >= WorkingFile.Current.Length)
            {
                // this is the final chunk of the current working file, shrink the reader to avoid an overflow
                _readLength = WorkingFile.Current.Length - _position;
            }

            // read the block
            _currentFile.Position = _position;
            _currentFile.Read(_data, 0, checked((int)_readLength));
            _position += _readLength;
            
            // replenish the queue with a read-ahead function when there is less than 2MB of data waiting
            for (var n = 0; n < _readLength; n++)
            {
                _byteQueue.Enqueue(_data[n]);
            }
        }

        /// <summary>
        /// Handles read-ahead for variable length integer markers.
        /// A single byte marks either the number, or the size of the field.
        /// https://en.bitcoin.it/wiki/Protocol_specification#Variable_length_integer
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private Int64 ReadVariableLengthInteger(IList<byte> bytes)
        {
            var length = (uint)bytes[0];

            switch (length)
            {
                case 253: return BitConverter.ToInt16(Dequeue(2), 0); // uint16_t 
                case 254: return BitConverter.ToInt32(Dequeue(4), 0); // uint32_t
                case 255: return BitConverter.ToInt64(Dequeue(8), 0); // uint64_t
                default: return length;
            }
        }

        /// <summary>
        /// Reverse the endianness of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private byte[] ReverseArray(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }

        /// <summary>
        /// Extract the ECDSA public key from the ChallengeScript value of the blockchain
        /// </summary>
        /// <returns></returns>
        private static byte[] ExtractPublicKey(byte[] challengeScript)
        {
            var publicKey = new byte[65];

            if (challengeScript.Length == 67 && challengeScript[0] == 0x41 && challengeScript[66] == 0xAC)
            {
                Array.Copy(challengeScript, 1, publicKey, 0, 65);
            }

            return publicKey;
        }

        /// <summary>
        /// Thanks http://gobittest.appspot.com/Address
        /// </summary>
        /// <param name="ecdsaPublickey">Standard format ecdsa public key taken from the blockchain</param>
        /// <returns></returns>
        private static string ComputeBitcoinAddress(byte[] ecdsaPublickey)
        {
            // step1: byte[32] -- sha256 ecdsaPublicKey
            // step2: byte[20] -- ripemd-160 hash step1
            // step3: byte[21] -- add network bytes to step2
            // step4: byte[32] -- sha256 step3
            // step5: byte[32] -- sha256 step4
            // step6: byte[4]  -- get first four bytes of step 5
            // step7: byte[25] -- add step6 to the end of step3

            using (var sha256Managed = new SHA256Managed())
            {
                using (var ripeMd160Managed = new RIPEMD160Managed())
                {
                    var step3 = new byte[21];
                    var step6 = new byte[4];
                    var step7 = new byte[25];

                    var step1 = sha256Managed.ComputeHash(ecdsaPublickey);
                    var step2 = ripeMd160Managed.ComputeHash(step1);
                    Array.Copy(step2, 0, step3, 1, 20);
                    var step4 = sha256Managed.ComputeHash(step3);
                    var step5 = sha256Managed.ComputeHash(step4);
                    Array.Copy(step5, 0, step6, 0, 4);
                    Array.Copy(step3, 0, step7, 0, 21);
                    Array.Copy(step6, 0, step7, 21, 4);

                    return Base58Encoding.Encode(step7);
                }
            }
        }
    }
}