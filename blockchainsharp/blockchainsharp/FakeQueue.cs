using System;

namespace BlockChainSharp
{
    public class FakeQueue
    {
        private byte[] _queue;

        private Int32 ptrStart;

        private Int32 ptrEnd;

        private Int32 prtHead;

        /// <summary>
        /// 
        /// </summary>
        public FakeQueue()
        {
            _queue = new byte[1048576];

            ptrStart = 0;
            ptrEnd = 0;
            prtHead = 1048576;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int64 QueueLength
        {
            get
            {
                return _queue.Length; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(byte[] data)
        {
            var newArray = new byte[_queue.Length + data.Length];

            Buffer.BlockCopy(_queue, 0, newArray, 0, _queue.Length);
            Buffer.BlockCopy(data, 0, newArray, _queue.Length, data.Length);

            _queue = newArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Dequeue(int count)
        {
            var requestedBytes = new byte[count];
            var remainingBytes = new byte[_queue.Length - count];

            Buffer.BlockCopy(_queue, 0, requestedBytes, 0, count);
            Buffer.BlockCopy(_queue, count, remainingBytes, 0, _queue.Length - count);

            _queue = remainingBytes;

            return requestedBytes;
        }
    }
}