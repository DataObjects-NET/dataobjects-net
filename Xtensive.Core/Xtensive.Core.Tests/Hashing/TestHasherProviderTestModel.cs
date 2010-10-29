// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using Xtensive.Hashing;

namespace Xtensive.Tests.Hashing
{
  internal class CustomStringHasher : HasherBase<string>
  {
    public override long GetHash(string value)
    {
      return 0;
    }

    public override long[] GetHashes(string value, int count)
    {
      return new long[count];
    }

    public CustomStringHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }

  public class CustomUInt64Hasher : HasherBase<ulong>
  {
    public override long GetHash(ulong value)
    {
      return 0;
    }

    public override long[] GetHashes(ulong value, int count)
    {
      long[] result = new long[count];
      for (int i = 0; i < count; i++) {
        result[i] = 0;
      }
      return result;
    }

    public CustomUInt64Hasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }

  internal class ByteHasher : HasherBase<byte>
  {
    public override long GetHash(byte value)
    {
      throw new NotImplementedException();
    }

    public override long[] GetHashes(byte value, int count)
    {
      throw new NotImplementedException();
    }

    public ByteHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }

  public class TestHasherProviderTestHasher: HasherBase<TestHasherProviderTest>
  {
    public override long GetHash(TestHasherProviderTest value)
    {
      throw new NotImplementedException();
    }

    public override long[] GetHashes(TestHasherProviderTest value, int count)
    {
      throw new NotImplementedException();
    }

    public TestHasherProviderTestHasher(IHasherProvider provider)
      : base(provider)
    {
    }
  }
}