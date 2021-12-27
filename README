# AsyncCopyTo

A small [CopyToAsync()](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.copytoasync?view=net-6.0) extension for [C# Streams](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=net-6.0) that features progress monitoring.

Features:

- Reports progress to a user provided [IProgress](https://docs.microsoft.com/en-us/dotnet/api/system.iprogress-1?view=net-6.0) instance.
- Reads new chunks from source stream concurrently to writing the previously read chunks to the destination.

## Usage

```c#
Stream sourceStream;
Stream destinationStream;
IProgress<long> progress = new SomeProgressImplementation();
int bufferSize = 1024; // optional

sourceStream.CopyToAsync(destinationStream, progress, bufferSize);

// continue work while source gets copied to destination and with regular progress reports after each bufferSize bytes
```
