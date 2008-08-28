// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.IO;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Testing;
using System.Linq;

namespace Xtensive.Core.Tests.Serialization
{
  [TestFixture]
  public class BinarySerializerTest
  {
    private static bool warmup = false;
    private static bool profile = false;
    private static int  baseSize = 1000;

    [Test]
    public void CombinedTest()
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      
      TestCloning("1", 1);
      TestCloning("int [3]", new [] {1,2,3});
      
//      var ints = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).ToArray();
//      TestCloning(string.Format("int [{0}]", ints.Length), ints);
//
//      var guids = InstanceGenerationUtils<Guid>.GetInstances(r, 0).Take(baseSize).ToArray();
//      TestCloning(string.Format("Guid [{0}]", guids.Length), guids);

      var objects = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).Cast<object>().ToArray();
      TestCloning(string.Format("Object [{0}]", objects.Length), objects);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      profile = true;
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);

      var objects = InstanceGenerationUtils<int>.GetInstances(r, 0).Take(baseSize).Cast<object>().ToArray();
      TestCloning(string.Format("Object [{0}]", objects.Length), objects);
    }

    private static void TestCloning<T>(string title, T source)
    {
      using (Log.InfoRegion("Cloning: {0}", title)) {
        warmup = true;
        var c1 = profile ? source : CloneByNativeSerialization(source);
        var c2 = CloneByOurSerialization(source);
        warmup = false;
        c1 = profile ? source : CloneByNativeSerialization(source);
        c2 = CloneByOurSerialization(source);
        Assert.AreEqual(source, c1);
      }
    }

    private static T CloneByOurSerialization<T>(T source)
    {
      var ms = new MemoryStream();
      var s = BinarySerializer.Instance;
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Xtensive cloning", 1)) {
        s.Serialize(ms, source);
        ms.Position = 0;
        return (T) s.Deserialize(ms);
      }
    }

    private static T CloneByNativeSerialization<T>(T source)
    {
      var ms = new MemoryStream();
      var s = LegacyBinarySerializer.Instance;
      TestHelper.CollectGarbage();
      using (warmup ? null : new Measurement("Native cloning  ", 1)) {
        s.Serialize(ms, source);
        ms.Position = 0;
        return (T) s.Deserialize(ms);
      }
    }
  }
}