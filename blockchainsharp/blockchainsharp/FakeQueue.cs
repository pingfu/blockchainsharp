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
            prtHead = _queue.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        public Int64 QueueLength
        {
            get
            {
                //return _queue.Length; 
                return ptrEnd - ptrStart;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(byte[] data)
        {
            if (ptrEnd >= prtHead - data.Length)
            {
                // this enqueue would overflow the array, cannot add more data.
                // we'll copy the byteset back to the base of the array and 
                // reset the pointers.
                Buffer.BlockCopy(_queue, ptrStart, _queue, 0, ptrEnd - ptrStart);

                ptrEnd = ptrEnd - ptrStart;
                ptrStart = 0;
            }

            var insertFrom = ptrEnd;
            var insertTo = ptrEnd + data.Length;

            Buffer.BlockCopy(data, 0, _queue, insertFrom, data.Length);

            ptrEnd = insertTo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Dequeue(int count)
        {
            if (QueueLength <= count)
            {
                // can't do that, the array is empty!
                Console.WriteLine("empty");
            }

            var takeFrom = ptrStart;
            var takeTo = ptrStart + count;
            var data = new byte[count];

            Buffer.BlockCopy(_queue, takeFrom, data, 0, data.Length);

            ptrStart = takeTo;

            return data;
        }
    }
}