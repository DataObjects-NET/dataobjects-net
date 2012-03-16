// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using NUnit.Framework;
using Xtensive.Hashing;

namespace Xtensive.Tests.Hashing
{
  [TestFixture]
  public class TestHasherProviderTest
  {
    [Test]
    public void CombinedTest()
    {
      IHasherProvider customProvider = new TestHasherProvider();
      Hasher<string> stringHasher = customProvider.GetHasher<string>();
      Assert.AreEqual(stringHasher.Implementation.GetType(), typeof(CustomStringHasher));
      Assert.AreEqual(0, stringHasher.GetHash("ABC"));
      Hasher<ulong> ulongHasher = customProvider.GetHasher<ulong>();
      Assert.AreEqual(ulongHasher.Implementation.GetType(), typeof(CustomUInt64Hasher));
      Hasher<byte> byteHasher = customProvider.GetHasher<byte>();
      Assert.AreEqual(byteHasher.Implementation.GetType(), typeof(ByteHasher));
      Hasher<decimal> decimalHasher = customProvider.GetHasher<decimal>();
      Assert.AreEqual(decimalHasher.Implementation.GetType(), Hasher<decimal>.Default.Implementation.GetType());
      Assert.AreEqual(0, decimalHasher.GetHash(1234.343M));
      Hasher<TestHasherProviderTest> testHasher1 = customProvider.GetHasher<TestHasherProviderTest>();
      Assert.AreEqual(testHasher1.Implementation.GetType(), typeof(TestHasherProviderTestHasher));
      Hasher<TestHasherProviderTest> testHasher2 = Hasher<TestHasherProviderTest>.Default;
      Assert.AreEqual(testHasher2.Implementation.GetType(), typeof(TestHasherProviderTestHasher));
    }
  }
}