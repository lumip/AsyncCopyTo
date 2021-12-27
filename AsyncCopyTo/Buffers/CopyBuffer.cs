// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace AsyncCopyTo.Buffers
{

    internal sealed class CopyBuffer : IDisposable
    {
        public ReservedBuffer Buffer { get; }
        public int BytesRead { get; }

        public CopyBuffer(ReservedBuffer buffer, int bytesRead)
        {
            Buffer = buffer;
            BytesRead = bytesRead;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

}
