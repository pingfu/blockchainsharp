using System;

namespace BlockChainSharp
{
    /// <summary>
    /// 
    /// </summary>
    public class ByteQueue
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly byte[] _queue;

        /// <summary>
        /// 
        /// </summary>
        private readonly int _maxSize;

        /// <summary>
        /// 
        /// </summary>
        private int _ptrStart;

        /// <summary>
        /// 
        /// </summary>
        private int _ptrEnd;

        /// <summary>
        /// Delcares a new instance of the queue.
        /// </summary>
        /// <param name="maxSize">Defines the maximum size of data this queue will accept</param>
        public ByteQueue(int maxSize)
        {
            // internally, double the size of the byte array, so the queue can be over filled beyond the defined capacity, somewhat.
            _maxSize = maxSize * 2;
            _ptrStart = 0;
            _ptrEnd = 0;
            _queue = new byte[_maxSize];
        }

        /// <summary>
        /// Returns the current number of bytes in the queue
        /// </summary>
        public int Count
        {
            get
            {
                return _ptrEnd - _ptrStart;
            }
        }

        /// <summary>
        /// Returns the maximum size of the data this queue will accept
        /// </summary>
        public int MaxSize { 
            get
            {
                return _maxSize / 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] Peak
        {
            get
            {
                var data = new byte[_ptrEnd - _ptrStart];
                Buffer.BlockCopy(_queue, _ptrStart, data, 0, _ptrEnd - _ptrStart);
                return data;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(byte[] data)
        {
            if (data.Length > MaxSize) throw new IndexOutOfRangeException("Data length exceeds maximum size defined for this queue");
            if (data.Length + Count > _maxSize) throw new IndexOutOfRangeException("Data would cause queue to exceed maximum defined size");

            if (_ptrEnd >= _queue.Length - data.Length)
            {
                // this enqueue would overflow the array, cannot add more data.
                // we'll copy the byteset back to the base of the array and 
                // reset the pointers.
                Buffer.BlockCopy(_queue, _ptrStart, _queue, 0, _ptrEnd - _ptrStart);

                _ptrEnd = _ptrEnd - _ptrStart;
                _ptrStart = 0;
            }

            var insertFrom = _ptrEnd;
            var insertTo = _ptrEnd + data.Length;

            Buffer.BlockCopy(data, 0, _queue, insertFrom, data.Length);

            _ptrEnd = insertTo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Dequeue(int count)
        {
            if (Count <= count)
            {
                // can't do that, the array is empty!
                return null;
            }

            var takeFrom = _ptrStart;
            var takeTo = _ptrStart + count;
            var data = new byte[count];

            Buffer.BlockCopy(_queue, takeFrom, data, 0, data.Length);

            _ptrStart = takeTo;

            return data;
        }
    }
}