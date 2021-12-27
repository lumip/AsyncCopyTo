// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AsyncCopyTo.Buffers
{

    /// <summary>
    /// Maintains a thread-safe pool of byte buffers of a given size and provides functionality for waiting on free buffers.
    /// </summary>
    public class BufferPool
    {

        private byte[] _backingBuffer;

        private BlockingCollection<Memory<byte>> _freeBuffers;


        /// <summary>
        /// Initializes the buffer pool.
        /// </summary>
        /// <param name="bufferSize">The size of each buffer.</param>
        /// <param name="numBuffers">The number of buffers in the pool.</param>
        public BufferPool(int bufferSize, int numBuffers)
        {
            _backingBuffer = new byte[numBuffers * bufferSize];
            _freeBuffers = new BlockingCollection<Memory<byte>>(numBuffers);

            for (int i = 0; i < numBuffers; i++)
            {
                _freeBuffers.Add(
                    new Memory<byte>(_backingBuffer, i * bufferSize, bufferSize)
                );
            }
        }

        /// <summary>
        /// Get a free buffer (non-blocking).
        /// </summary>
        /// <returns>A <see cref="ReservedBuffer" /> instance representing a buffer reserved for the caller.</returns>
        /// <exception cref="NoBufferException">Thrown when no free buffer is available.</exception>
        public ReservedBuffer GetFreeBuffer()
        {
            Memory<byte> buffer;
            if (_freeBuffers.TryTake(out buffer))
            {
                return new ReservedBuffer(this, buffer);
            }
            throw new NoFreeBufferException();
        }

        /// <summary>
        /// Wait until a free buffer becomes available.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
        /// <returns>A <see cref="ReservedBuffer" /> instance representing a buffer reserved for the caller.</returns>
        public async Task<ReservedBuffer> WaitForFreeBuffer(CancellationToken cancellationToken)
        {
            return await Task.Run(() => new ReservedBuffer(this, _freeBuffers.Take(cancellationToken)));
        }

        /// <summary>
        /// Returns a previously reserved buffer to the pool for future use.
        /// </summary>
        /// <param name="buffer">The buffer made available once more to the pool.</param>
        internal void ReturnBuffer(Memory<byte> buffer)
        {
            _freeBuffers.Add(buffer);
        }
    }
}
