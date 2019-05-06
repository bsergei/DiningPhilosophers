using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace DiningPhilosophers.Core
{
    /// <summary>
    /// Wrapper around BufferBlock that allows multiple producers
    /// to push single object, but consumer will get buffered
    /// array of objects. Considers that push of single object
    /// to array buffer is more lightweight operation than BufferBlock.Post
    /// for each single object.
    /// </summary>
    public class BufferedProducerConsumer<T>
    {
        private readonly int bufferLength_;

        private readonly BufferBlock<T[]> pc_ = new BufferBlock<T[]>();
        private volatile T[] buffer_;
        private volatile int bufferIndex_;
        private volatile int pushWorkers_;
        private volatile bool finished_;
        private readonly object bufferSync_ = new object();

        public BufferedProducerConsumer(int bufferLength = 100000)
        {
            buffer_ = new T[bufferLength];
            bufferLength_ = bufferLength;
        }

        public ISourceBlock<T[]> Source
        {
            get { return pc_; }
        }

        public void Push(T value)
        {
            if (finished_)
            {
                return;
            }

            Interlocked.Increment(ref pushWorkers_);
            try
            {
                int workerBufferIndex;
                bool needRetry;
                do
                {
                    needRetry = false;
                    workerBufferIndex = GetNextAvailableIndexInBuffer();
                    if (workerBufferIndex >= bufferLength_)
                    {
                        // Out of available indexes in buffer, send buffer to consumers, if need.
                        needRetry = true;
                        PostBuffer();
                    }
                } while (needRetry);

                buffer_[workerBufferIndex] = value;
            }
            finally
            {
                Interlocked.Decrement(ref pushWorkers_);
            }
        }

        public void Finish()
        {
            finished_ = true;
            while (pushWorkers_ != 0)
            {
                // Wait until all writers finish pushing to buffer.
                Thread.Yield();
            }

            T[] buf = buffer_;
            int idx = Math.Min(bufferIndex_, bufferLength_);
            
            var final = new T[idx];
            Array.Copy(buf, final, idx);

            pc_.Post(final);
            pc_.Complete();
        }

        private void PostBuffer()
        {
            T[] buf = null;
            // Here can be multiple writers.
            // So sync it and check again current bufferIndex value.
            lock (bufferSync_)
            {
                if (bufferIndex_ >= bufferLength_)
                {
                    buf = buffer_;
                    buffer_ = new T[bufferLength_];
                    bufferIndex_ = 0;
                }
            }

            if (buf != null)
            {
                // Send buffer to consumer.
                pc_.Post(buf);
            }
        }

        /// <summary>
        /// Thread-safe getter for next available index in buffer.
        /// </summary>
        private int GetNextAvailableIndexInBuffer()
        {
            int idx;
            int initialIdx;
            do
            {
                initialIdx = bufferIndex_;
                idx = initialIdx + 1;
            } while (Interlocked.CompareExchange(ref bufferIndex_, idx, initialIdx) != initialIdx);

            return idx;
        }
    }
}