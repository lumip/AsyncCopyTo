// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AsyncCopyTo.Buffers
{

    public class BufferPool
    {

        private byte[] _backingBuffer;

        private BlockingCollection<Memory<byte>> _freeBuffers;


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

        public ReservedBuffer GetFreeBuffer()
        {
            Memory<byte> buffer;
            if (_freeBuffers.TryTake(out buffer))
            {
                return new ReservedBuffer(this, buffer);
            }
            throw new NoFreeBufferException();
        }

        public async Task<ReservedBuffer> WaitForFreeBuffer(CancellationToken cancellationToken)
        {
            return await Task.Run(() => new ReservedBuffer(this, _freeBuffers.Take(cancellationToken)));
        }

        internal void ReturnBuffer(Memory<byte> buffer)
        {
            _freeBuffers.Add(buffer);
        }
    }
}
