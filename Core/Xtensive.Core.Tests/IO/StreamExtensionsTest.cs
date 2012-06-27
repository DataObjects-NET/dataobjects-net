// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.11

using System;
using NUnit.Framework;
using System.IO;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Testing;

namespace Xtensive.Tests.IO
{
  [TestFixture]
  public class StreamExtensionsTest
  {
    private readonly static string LargeFileName = typeof(StreamExtensionsTest).GetShortName()+".BigFile.bin";
    private readonly static int largeCopyBlockSize = 1024 * 1024 * 4;
    private readonly static int LargeBufferSize  = 256*256;
    private readonly static int LargeBufferCount = 100;

    [Test]
    public void Erase()
    {
      byte[] buffer = new byte[] {0, 1, 2, 3, 4, 5, 6, 7};
      MemoryStream stream = new MemoryStream(buffer);

      stream.Erase(100);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }

      stream.Seek(2, SeekOrigin.Begin);
      stream.Erase(3);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < 2; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }
      for (int i = 2; i < 2 + 3; i++) {
        Assert.AreEqual(0, stream.ReadByte());
      }
      for (int i = 2 + 3; i < buffer.Length; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }

      stream = new MemoryStream(buffer);
      stream.Seek(0, SeekOrigin.Begin);
      stream.Erase(0);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }

      stream.Seek(0, SeekOrigin.Begin);
      stream.Erase(1110);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length; i++) {
        Assert.AreEqual(0, stream.ReadByte());
      }
    }

    [Test]
    public void EraseToEnd()
    {
      byte[] buffer = new byte[] {0, 1, 2, 3, 4, 5};
      MemoryStream stream = new MemoryStream(buffer);

      StreamExtensions.Erase(stream);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }

      stream.Seek(buffer.Length/2, SeekOrigin.Begin);
      StreamExtensions.Erase(stream);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length/2; i++) {
        Assert.AreEqual(buffer[i], stream.ReadByte());
      }
      for (int i = buffer.Length/2; i < buffer.Length; i++) {
        Assert.AreEqual(0, stream.ReadByte());
      }

      stream.Seek(0, SeekOrigin.Begin);
      StreamExtensions.Erase(stream);
      stream.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(buffer.Length, stream.Length);
      for (int i = 0; i < buffer.Length; i++) {
        Assert.AreEqual(0, stream.ReadByte());
      }
    }

    [Test]
    public void WriteTo()
    {
      byte[] sourceBuffer = new byte[] {0, 1, 2, 3, 4, 5};
      byte[] destinationBuffer = new byte[] {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19};
      MemoryStream source = new MemoryStream(sourceBuffer);
      MemoryStream destination = new MemoryStream(10000);
      destination.Write(destinationBuffer, 0, destinationBuffer.Length);

      destination.Seek(4, SeekOrigin.Begin);
      source.Seek(-2, SeekOrigin.End);
      StreamExtensions.CopyTo(source, destination, 4);

      destination.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(destinationBuffer.Length, destination.Length);
      for (int i = 0; i < 4; i++) {
        Assert.AreEqual(destinationBuffer[i], destination.ReadByte());
      }
      for (int i = 4; i < 6; i++) {
        Assert.AreEqual(sourceBuffer[i], destination.ReadByte());
      }
      for (int i = 6; i < destinationBuffer.Length; i++) {
        Assert.AreEqual(destinationBuffer[i], destination.ReadByte());
      }
    }

    [Test]
    public void WriteToEnd()
    {
      byte[] sourceBuffer = new byte[] {0, 1, 2, 3, 4, 5};
      byte[] destinationBuffer = new byte[] {6, 7, 8, 9};
      MemoryStream source = new MemoryStream(sourceBuffer);
      MemoryStream destination = new MemoryStream(10000);
      destination.Write(destinationBuffer, 0, destinationBuffer.Length);

      destination.Seek(0, SeekOrigin.End);
      StreamExtensions.CopyTo(source, destination);

      destination.Seek(0, SeekOrigin.Begin);
      Assert.AreEqual(sourceBuffer.Length + destinationBuffer.Length, destination.Length);
      for (int i = 0; i < destinationBuffer.Length; i++) {
        Assert.AreEqual(destinationBuffer[i], destination.ReadByte());
      }
      for (int i = 0; i < sourceBuffer.Length; i++) {
        Assert.AreEqual(sourceBuffer[i], destination.ReadByte());
      }
    }

    [Test]
    public void InstreamCopySmall()
    {
      byte[] buffer = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};
      byte[] expectedResult = new byte[] {9, 10, 11, 12, 13, 14, 15, 7, 8, 9, 10, 11, 12, 13, 14, 15};
      Stream stream = new MemoryStream(buffer);
      stream.Copy(9, 100);
      for (int i = 0; i < buffer.Length; i++)
        Assert.AreEqual(buffer[i], expectedResult[i]);
    }

    [Test]
    public void InstreamCopySmallForward()
    {
      byte[] buffer = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
      byte[] expectedResult = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 7, 8, 9, 11, 12, 13, 14, 15 };
      Stream stream = new MemoryStream(buffer);
      stream.Seek(8, SeekOrigin.Begin);
      stream.Copy(7, 3);
      for (int i = 0; i < buffer.Length; i++)
        Assert.AreEqual(buffer[i], expectedResult[i]);
    }

    [Test]
    public void InstreamCopySmallReverse()
    {
      byte[] buffer = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
      byte[] expectedResult = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 11, 12, 13, 14, 15 };
      Stream stream = new MemoryStream(buffer);
      stream.Seek(8, SeekOrigin.Begin);
      stream.Copy(9, 3);
      for (int i = 0; i < buffer.Length; i++)
        Assert.AreEqual(buffer[i], expectedResult[i]);
    }

    [Test]
    public void InstreamCopyLargeForward()
    {
      byte[] buffer = new byte[LargeBufferSize];
      int buffersCount = LargeBufferCount; // 100*10e6 bytes
      string fileName = Path.Combine(TestHelper.TestFolderName, LargeFileName);
      if (File.Exists(fileName))
        File.Delete(fileName);
      try {
        Stream stream = new FileStream(fileName, FileMode.Create);
        Random random = RandomManager.CreateRandom();
        // Fill file
        for (long i = 0; i < buffersCount; i++) {
          random.NextBytes(buffer);
          stream.Write(buffer, 0, buffer.Length);
        }
        long bytesToCopy = largeCopyBlockSize;
        stream.Seek(bytesToCopy + 10, SeekOrigin.Begin);
        stream.Copy(0, bytesToCopy);
        stream.Flush();
        // Verify
        stream.Close();
        stream.Dispose();

        Stream verificationStream1 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        verificationStream1.Seek(bytesToCopy + 10, SeekOrigin.Begin);
        Stream verificationStream2 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        verificationStream2.Seek(0, SeekOrigin.Begin);
        for (int i = 0; i < bytesToCopy; i++) {
          int byte1 = verificationStream1.ReadByte();
          int byte2 = verificationStream2.ReadByte();
          if (byte1!=byte2) {
            Log.Info("Error at position: {0}", i);
          }
          Assert.AreEqual(byte1, byte2);
        }
        verificationStream1.Close();
        verificationStream1.Dispose();
        verificationStream2.Close();
        verificationStream2.Dispose();
      }
      finally {
        if (File.Exists(fileName))
          File.Delete(fileName);
      }
    }

    [Test]
    public void InstreamCopyLargeReverse()
    {
      byte[] buffer = new byte[LargeBufferSize];
      int buffersCount = LargeBufferCount; 
      string fileName = Path.Combine(TestHelper.TestFolderName, LargeFileName);
      if (File.Exists(fileName))
        File.Delete(fileName);
      try {
        Stream stream = new FileStream(fileName, FileMode.Create);
        Random random = RandomManager.CreateRandom();
        // Fill file
        for (long i = 0; i < buffersCount; i++) {
          random.NextBytes(buffer);
          stream.Write(buffer, 0, buffer.Length);
        }
        long originalPosition = stream.Position;
        long bytesToCopy = largeCopyBlockSize;
        stream.Copy(0, bytesToCopy);
        stream.Flush();
        // Verify
        Assert.AreEqual(stream.Position, originalPosition + bytesToCopy);
        stream.Close();
        stream.Dispose();

        Stream verificationStream1 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        verificationStream1.Seek(originalPosition, SeekOrigin.Begin);
        Stream verificationStream2 = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        verificationStream2.Seek(0, SeekOrigin.Begin);
        for (int i = 0; i < bytesToCopy; i++) {
          int byte1 = verificationStream1.ReadByte();
          int byte2 = verificationStream2.ReadByte();
          if (byte1!=byte2) {
            Log.Info("Error at position: {0}", i);
          }
          Assert.AreEqual(byte1, byte2);
        }
        verificationStream1.Close();
        verificationStream1.Dispose();
        verificationStream2.Close();
        verificationStream2.Dispose();
      }
      finally {
        if (File.Exists(fileName))
          File.Delete(fileName);
      }
    }
  }
}