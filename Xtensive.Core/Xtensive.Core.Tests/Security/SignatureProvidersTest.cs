// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.19

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Xtensive.Security;
using Xtensive.Serialization.Binary;
using Xtensive.Testing;

namespace Xtensive.Tests.Security
{
  [Serializable]
  public static class CryptoTranformGenerator
  {
    private static SymmetricAlgorithm algorithm;

    public static ICryptoTransform CreateEncryptor()
    {
      return algorithm.CreateEncryptor();
    }

    public static ICryptoTransform CreateDecryptor()
    {
      return algorithm.CreateDecryptor();
    }

    static CryptoTranformGenerator()
    {
      algorithm = DES.Create();
      algorithm.GenerateKey();
      algorithm.GenerateIV();
    }
  }

  [TestFixture]
  public class SignatureProvidersTest
  {
    private bool canFinish;

    [Test]
    public void ProvidersTest()
    {
      var providers = new ISignatureProvider[]
        {
          new HashingSignatureProvider(MD5.Create),
          new CryptoSignatureProvider(
            CryptoTranformGenerator.CreateEncryptor,
            CryptoTranformGenerator.CreateDecryptor,
            new HashingSignatureProvider(SHA1.Create)),
          new CachingSignatureProvider(100, new HashingSignatureProvider(SHA256.Create)),
          new CachingSignatureProvider(100,
            new CryptoSignatureProvider(
              CryptoTranformGenerator.CreateEncryptor,
              CryptoTranformGenerator.CreateDecryptor,
              new HashingSignatureProvider(SHA256.Create)))
        };

      foreach (var provider in providers) {
        CombinedTest(provider);
      }
    }
    
    public void CombinedTest(ISignatureProvider provider)
    {
      Log.Debug("AddRemoveSign for: {0}", provider.GetType().Name);
      AddRemoveSignTest(provider);
      Log.Debug("Serialization for: {0}", provider.GetType().Name);
      SerializationTest(provider);
      Log.Debug("Synchronization for: {0}", provider.GetType().Name);
      SynchronizationTest(provider);
    }

    public void AddRemoveSignTest(ISignatureProvider provider)
    {
      var token = "123";// "123aaaÿÿÿ";
      
      var signed = provider.AddSignature(token);
      Assert.AreEqual(token, provider.RemoveSignature(signed));
    }

    public void SerializationTest(ISignatureProvider original)
    {
      var token = "aaabbb";

      var deserialized = new LegacyBinarySerializer().Clone(original) as ISignatureProvider;
      Assert.IsNotNull(deserialized);

      var signedToken = original.AddSignature(token);
      Assert.AreEqual(token, deserialized.RemoveSignature(signedToken));
    }

    public void SynchronizationTest(ISignatureProvider provider)
    {
      var threads = new Thread[10];
      for (int i = 0; i < 10; i++)
      {
        threads[i] = new Thread(AddRemoveSignTest);
      }

      try
      {
        for (int i = 0; i < 10; i++)
        {
          threads[i].Start(provider);
        }
        Thread.Sleep(1000);
      }
      finally
      {
        canFinish = true;
        for (int i = 0; i < 10; i++)
        {
          threads[i].Join();
        }
      }
    }

    public void AddRemoveSignTest(object arg)
    {
      while (!canFinish) {
        var provider = (HashingSignatureProvider) arg;
        var r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        var token = r.Next(1000).ToString();

        var signed = provider.AddSignature(token);
        Assert.AreEqual(token, provider.RemoveSignature(signed));
      }
    }

  }
}