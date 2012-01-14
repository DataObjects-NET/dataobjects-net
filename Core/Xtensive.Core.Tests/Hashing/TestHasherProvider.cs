// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using Xtensive.Hashing;

namespace Xtensive.Tests.Hashing
{
  public class TestHasherProvider : HasherProvider
  {
    private readonly IHasher<String> customStringHasher;
    private readonly IHasher<ulong> customUInt64Hasher;

    protected override TAssociate CreateAssociate<TKey, TAssociate>(out Type foundFor)
    {
      if (typeof(TKey) == typeof(string)) {
        foundFor = typeof (string);
        return (TAssociate) customStringHasher;
      }
      if (typeof(TKey) == typeof(ulong)) {
        foundFor = typeof (ulong);
        return (TAssociate) customUInt64Hasher;
      }
      return base.CreateAssociate<TKey, TAssociate>(out foundFor);
    }


    // Constructors

    public TestHasherProvider()
    {
      Type t = typeof (ByteHasher);
      AddHighPriorityLocation(t.Assembly, t.Namespace, true);
      customStringHasher = new CustomStringHasher(this);
      customUInt64Hasher = new CustomUInt64Hasher(this);
    }
  }
}