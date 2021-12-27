// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

using AsyncCopyTo.Buffers;


namespace AsyncCopyTo
{


    /// <summary>
    /// Extension methods for Stream.
    /// </summary>
    public static class StreamExtensions
    {

        /// <summary>
        /// Asynchronous version of CopyTo with IProgress interface for progress reports.
        /// 
        /// Features concurrent reading and writing.
        /// </summary>
        public static async Task CopyToAsync(
            this Stream source, Stream destination, IProgress<long> progress, int bufferSize = 0x1000, CancellationToken cancellationToken = default(CancellationToken), int bufferCount = 2)
        {
            BufferPool buffers = new BufferPool(bufferSize, bufferCount);

            using (BlockingCollection<CopyBuffer> copyBlocks = new BlockingCollection<CopyBuffer>())
            {
                _ = ReadToBuffers(source, buffers, copyBlocks, cancellationToken);
                await WriteFromBuffers(destination, copyBlocks, progress, cancellationToken);
            }
        }

        private static async Task ReadToBuffers(
            Stream source, BufferPool buffers, BlockingCollection<CopyBuffer> copyBlocks, CancellationToken cancellationToken)
        {
            int bytesRead;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReservedBuffer buffer = await buffers.WaitForFreeBuffer(cancellationToken);
                bytesRead = await source.ReadAsync(buffer.Buffer, cancellationToken);
                if (bytesRead > 0)
                {
                    copyBlocks.Add(new CopyBuffer(buffer, bytesRead));
                }
                else
                {
                    buffer.Dispose();
                }
            }
            while (bytesRead > 0);
            copyBlocks.CompleteAdding();
        }

        private static async Task WriteFromBuffers(
            Stream destination, BlockingCollection<CopyBuffer> copyBlocks, IProgress<long> progress, CancellationToken cancellationToken)
        {
            long totalBytes = 0;
            while (true)
            {
                try
                {
                    using (CopyBuffer buffer = copyBlocks.Take(cancellationToken))
                    {
                        await destination.WriteAsync(buffer.Buffer.Buffer.Slice(0, buffer.BytesRead), cancellationToken);
                        totalBytes += buffer.BytesRead;
                        progress.Report(totalBytes);
                    }
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }
    }
}
