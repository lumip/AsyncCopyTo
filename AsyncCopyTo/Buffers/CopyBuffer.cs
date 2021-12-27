// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace AsyncCopyTo.Buffers
{

    /// <summary>
    /// A buffer used during asynchronous copying; after reading, before writing.
    /// 
    /// Annotates a <see cref="ReservedBuffer" /> instance by the number of bytes
    /// that were read into it from the source stream.
    /// </summary>
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
