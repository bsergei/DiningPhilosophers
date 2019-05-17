using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace DiningPhilosophers.Utils
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
        private InterlockedIndex bufferIndex_ = new InterlockedIndex(-1);
        
        private volatile int workers_;
        private volatile int workersInPostBuffer_;
        
        private volatile bool finished_;
        private readonly object bufferSync_ = new object();

        public BufferedProducerConsumer(int bufferLength = 100000)
        {
            buffer_ = new T[bufferLength];
            bufferLength_ = bufferLength;
        }

        public IReceivableSourceBlock<T[]> Source
        {
            get { return pc_; }
        }

        public void Push(T value)
        {
            if (finished_)
            {
                return;
            }

            Interlocked.Increment(ref workers_);
            try
            {
                if (finished_)
                {
                    return;
                }

                int workerBufferIndex;
                bool needRetry;
                do
                {
                    needRetry = false;
                    workerBufferIndex = bufferIndex_.GetNextAvailableIndex();
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
                Interlocked.Decrement(ref workers_);
            }
        }

        public void Finish()
        {
            finished_ = true;
            while (workers_ != 0)
            {
                // Wait until all writers finish pushing to buffer.
                Thread.Yield();
            }

            T[] buf = buffer_;
            int finalCount = Math.Min(bufferIndex_.Value + 1, bufferLength_);

            if (finalCount > 0)
            {
                var final = new T[finalCount];
                Array.Copy(buf, final, finalCount);

                pc_.Post(final);
            }

            pc_.Complete();
        }

        private void PostBuffer()
        {
            Interlocked.Increment(ref workersInPostBuffer_);
            try
            {
                T[] oldBuffer = null;
                // Here can be multiple readers.
                // So sync it and check again current bufferIndex value.
                lock (bufferSync_)
                {
                    if (bufferIndex_.Value >= bufferLength_)
                    {
                        // Create new buffer.
                        var newBuffer = new T[bufferLength_];

                        // Wait until all workers locked bufferSync.
                        while (workers_ != workersInPostBuffer_)
                        {
                            Thread.Yield();
                        }

                        // Swap buffer and reset index.
                        oldBuffer = buffer_;
                        buffer_ = newBuffer;
                        bufferIndex_.Value = -1;
                    }
                }

                if (oldBuffer != null)
                {
                    // Send buffer to consumer.
                    pc_.Post(oldBuffer);
                }
            }
            finally
            {
                Interlocked.Decrement(ref workersInPostBuffer_);
            }
        }
    }
}