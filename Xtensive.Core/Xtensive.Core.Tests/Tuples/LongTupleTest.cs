// Copyright (C) 2003-2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2011.04.18

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Tuple=Xtensive.Tuples.Tuple;

namespace Xtensive.Tests.Tuples
{
  [TestFixture]
  public class LongTupleTest
  {
    [Test]
    public void CombinedTest()
    {
      int minLength = 30;
      int maxLength = 200;
      int iterationCount = 1000;
      var rnd = RandomManager.CreateRandom();
      var generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<int>();
      for (int i = 0; i < iterationCount; i++) {
        int count = rnd.Next(maxLength - minLength);
        var cloner = Cloner.Default;

        // Preparing data for test
        var fields = new Type[count];
        var data = new int[count];
        for (int j = 0; j < count; j++) {
          data[j] = generator.GetInstance(rnd);
          fields[j] = typeof (int);
        }

        {
          // Testing writes (untyped)
          var tuple = Tuple.Create(fields);
          for (int j = 0; j < count; j++)
            tuple.SetValue(j, (object) data[j]);
          // Testing reads (untyped)
          for (int j = 0; j < count; j++)
            Assert.AreEqual(data[j], tuple.GetValue(j));
          // Testing serialization 
          var tuple2 = cloner.Clone(tuple);
          Assert.AreEqual(tuple, tuple2);
          Assert.AreEqual(tuple.Descriptor, tuple2.Descriptor);
          // Testing reads (untyped));
          for (int j = 0; j < count; j++)
            Assert.AreEqual(data[j], tuple2.GetValue(j));
        }

        {
          // Testing writes (typed)
          var tuple = Tuple.Create(fields);
          for (int j = 0; j < count; j++)
            tuple.SetValue(j, data[j]);
          // Testing reads (typed)
          for (int j = 0; j < count; j++)
            Assert.AreEqual(data[j], tuple.GetValue<int>(j));
        }
      }
    }
  }
}