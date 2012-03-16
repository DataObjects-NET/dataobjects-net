// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;
using Xtensive.Diagnostics;
using Xtensive.Testing;

namespace Xtensive.Tests.Hashing
{
  [TestFixture]
  public class AlgorithmTests
  {
    [Test]
    [Explicit]
    [Category("Performance")]
    public void HashAlgorithmsPerformanceTest()
    {
      HashAlgorithm sha256Provider = new SHA256CryptoServiceProvider(); // 256bit / 4long
      HashAlgorithm sha384Provider = new SHA384CryptoServiceProvider(); // 384bit / 6long
      HashAlgorithm sha512Provider = new SHA512CryptoServiceProvider(); // 512bit / 8long
      HashAlgorithm md5Provider = new MD5CryptoServiceProvider(); // 128bit / 2long
      HashAlgorithm ripemd160Provider = new RIPEMD160Managed(); // 160bit / 2.5long

      byte[] buffer = new byte[] {43, 64, 87, 34, 12, 74, 12, 94, 222, 154, 241};
      using (new Measurement("SHA256", ushort.MaxValue)) {
        for (int i = 0; i < ushort.MaxValue; i++)
          sha256Provider.ComputeHash(buffer);
      }
      using (new Measurement("SHA384", ushort.MaxValue)) {
        for (int i = 0; i < ushort.MaxValue; i++)
          sha384Provider.ComputeHash(buffer);
      }
      using (new Measurement("SHA512", ushort.MaxValue)) {
        for (int i = 0; i < ushort.MaxValue; i++)
          sha512Provider.ComputeHash(buffer);
      }
      using (new Measurement("MD5", ushort.MaxValue)) {
        for (int i = 0; i < ushort.MaxValue; i++)
          md5Provider.ComputeHash(buffer);
      }
      using (new Measurement("RIPEMD160", ushort.MaxValue)) {
        for (int i = 0; i < ushort.MaxValue; i++)
          ripemd160Provider.ComputeHash(buffer);
      }
    }


    private const int BITS_IN_int = (sizeof (long)*8);
    private const int THREE_QUARTERS = ((int)((BITS_IN_int*3)/4));
    private const int ONE_EIGHTH = ((int)(BITS_IN_int/8));
    private const ulong HIGH_BITS = (~((long)(~0) >> ONE_EIGHTH));

    private ulong HashPJW(byte[] datum)
    {
      ulong hash_value = 0, i;
      for (int iteration = 0; iteration < datum.Length; iteration++) {
        hash_value = (hash_value << ONE_EIGHTH) + datum[iteration];
        if ((i = hash_value & HIGH_BITS)!=0)
          hash_value = (hash_value ^ (i >> THREE_QUARTERS)) & ~HIGH_BITS;
      }
      return (hash_value);
    }

    private const ulong FNV_prime = 1099511628211;
    private const ulong offset_basis = 14695981039346656037;

    public long FNV1aHash(byte[] datum)
    {
      unchecked{
        ulong hash = offset_basis;
        for (int i = 0; i < datum.Length; i++) {
          hash = hash*FNV_prime;
          hash = hash ^ datum[i];
        }
        return (long)hash;
      }
    }

    [Test]
    public void TestAlgorithm()
    {
      foreach (int value in InstanceGeneratorProvider.Default.GetInstanceGenerator<int>().GetInstances(RandomManager.CreateRandom(), 100))
      {
        byte[] bytes = BitConverter.GetBytes(value);
        Log.Info("HashPJW/FNV1aHash {0}({0:X}) {1:X} {2:X}", value, HashPJW(bytes), FNV1aHash(bytes));
      }
    }

    [Test]
    public void PerformanceCompare()
    {
      int count = 10000000;
      List<short> values = new List<short>(InstanceGeneratorProvider.Default.GetInstanceGenerator<short>().GetInstances(RandomManager.CreateRandom(), count));
      using (new Measurement("HashPJW", count)) {
        for (int i = 0; i < count; i++) {
          byte[] bytes = BitConverter.GetBytes(values[i]);
          HashPJW(bytes);
        }
      }
      using (new Measurement("FNV1aHash", count)) {
        for (int i = 0; i < count; i++) {
          byte[] bytes = BitConverter.GetBytes(values[i]);
          FNV1aHash(bytes);
        }
      }
    }
  }
}