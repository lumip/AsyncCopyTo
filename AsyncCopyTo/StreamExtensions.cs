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
        /// Features concurrent reading and writing, i.e., it will fill new buffers by reading from the source
        /// concurrently to writing out previously read buffers to the destination for maximum throughput.
        /// Progress reports are made after each completed write of a buffer, i.e., <paramref name="bufferSize" /> bytes.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="progress">The IProgress instance to which progress reports will be made.</param>
        /// <param name="bufferSize">The size, in bytes, of the internal copying buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is None.</param>
        /// <param name="bufferCount">The number of internal copying buffers for concurrent reading and writing. The default value is 2.</param>
        public static async Task CopyToAsync(
            this Stream source, Stream destination, IProgress<long> progress, int bufferSize = 81920, CancellationToken cancellationToken = default(CancellationToken), int bufferCount = 2)
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
