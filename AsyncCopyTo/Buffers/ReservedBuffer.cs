// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace AsyncCopyTo.Buffers
{

    /// <summary>
    /// A buffer from a <see cref="BufferPool" /> reserved for use by a specific caller thread/task.
    /// 
    /// Can be obtained from the pool by calling <see cref="BufferPool.GetFreeBuffer" /> or
    /// <see cref="BufferPool.WaitForFreeBuffer(System.Threading.CancellationToken)" />. Provides
    /// the requested buffer/memory via the <see cref="Buffer" /> member, which can be
    /// used freely by the calling code. After use, disposing the object will return it to the buffer pool
    /// for reuse.
    /// </summary>
    public sealed class ReservedBuffer : IDisposable
    {
        private BufferPool _pool;

        private bool _wasDisposed;

        /// <summary>
        /// The actual memory buffer.
        /// </summary>
        public Memory<byte> Buffer { get; }

        internal ReservedBuffer(BufferPool pool, Memory<byte> buffer)
        {
            _pool = pool;
            Buffer = buffer;
            _wasDisposed = false;
        }

        ~ReservedBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_wasDisposed)
            {
                _pool.ReturnBuffer(Buffer);
                _wasDisposed = true;
            }
        }
    } 
}
