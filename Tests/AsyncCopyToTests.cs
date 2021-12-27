// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using NUnit.Framework;

using System;
using System.IO;

using AsyncCopyTo;

namespace Tests;

public class TestProgressReporter : IProgress<long>
{
    public void Report(long value)
    {
    }
}

public class AsyncCopyToTests
{

    private byte[]? buffer;
    private byte[]? outBuffer;
    private byte[]? outBufferReference;

    [SetUp]
    public void Setup()
    {
        buffer = new byte[1024];
        for (int i = 0; i < buffer.Length; i++) buffer[i] = (byte)i;

        outBuffer = new byte[1024];
        outBufferReference = new byte[1024];
        for (int i = 0 ; i < outBuffer.Length; i++)
        {
            outBuffer[i] = 1;
            outBufferReference[i] = 1;
        }
    }

    [Test]
    public void TestCopyCompleteEvenDivision()
    {
        Stream readStream = new MemoryStream(buffer!, false);       
        Stream writeStream = new MemoryStream(outBuffer!, true);

        readStream.CopyToAsync(writeStream, new TestProgressReporter(), 128).Wait();

        CollectionAssert.AreEqual(buffer, outBuffer);
    }

    [Test]
    public void TestCopyCompleteOddDivision()
    {
        Stream readStream = new MemoryStream(buffer!, false);       
        Stream writeStream = new MemoryStream(outBuffer!, true);

        readStream.CopyToAsync(writeStream, new TestProgressReporter(), 30).Wait();

        CollectionAssert.AreEqual(buffer, outBuffer);
    }

    [Test]
    public void TestCopyPartialEvenDivision()
    {
        Stream readStream = new MemoryStream(buffer!, 0, 60, false);       
        Stream writeStream = new MemoryStream(outBuffer!, true);

        readStream.CopyToAsync(writeStream, new TestProgressReporter(), 30).Wait();

        Assert.That(buffer.AsSpan(0, 60).SequenceEqual(outBuffer.AsSpan(0, 60)));
        Assert.That(outBufferReference.AsSpan(60).SequenceEqual(outBuffer.AsSpan(60)));
    }

    [Test]
    public void TestCopyPartialOddDivision()
    {
        Stream readStream = new MemoryStream(buffer!, 0, 60, false);       
        Stream writeStream = new MemoryStream(outBuffer!, true);

        readStream.CopyToAsync(writeStream, new TestProgressReporter(), 21).Wait();

        Assert.That(buffer.AsSpan(0, 60).SequenceEqual(outBuffer.AsSpan(0, 60)));
        Assert.That(outBufferReference.AsSpan(60).SequenceEqual(outBuffer.AsSpan(60)));
    }

    
}