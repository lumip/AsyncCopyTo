// SPDX-License-Identifier: MIT
// Copyright 2021 Lukas <lumip> Prediger

using System;

namespace AsyncCopyTo.Buffers
{
    public class NoFreeBufferException : Exception
    {
        public NoFreeBufferException() : base("No free buffer was available!") { }
    }

}
