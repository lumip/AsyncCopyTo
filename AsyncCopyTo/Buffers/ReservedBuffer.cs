// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace AsyncCopyTo.Buffers
{

    public sealed class ReservedBuffer : IDisposable
    {
        private BufferPool _pool;

        private bool _wasDisposed;

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
