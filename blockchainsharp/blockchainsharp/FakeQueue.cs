using System;

namespace BlockChainSharp
{
    public class FakeQueue
    {
        private byte[] _queue;

        /// <summary>
        /// 
        /// </summary>
        public FakeQueue()
        {
            _queue = new byte[0];
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

            Array.Copy(_queue, 0, newArray, 0, _queue.Length);
            Array.Copy(data, 0, newArray, _queue.Length, data.Length);

            _queue = newArray;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Dequeue(long count)
        {
            var requestedBytes = new byte[count];
            var remainingBytes = new byte[_queue.Length - count];

            Array.Copy(_queue, 0, requestedBytes, 0, count);
            Array.Copy(_queue, count, remainingBytes, 0, _queue.Length - count);

            _queue = remainingBytes;

            return requestedBytes;
        }
    }
}
