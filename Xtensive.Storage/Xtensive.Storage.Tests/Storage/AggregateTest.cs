// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.28

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class AggregateTest : AutoBuildTest
  {
    private List<X> all;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      CreateSessionAndTransaction();

      for (int i = 0; i < 10; i++) {
        var x = new X();
        x.FByte = (byte) i;
        x.FSByte = (sbyte) i;
        x.FShort = (short) i;
        x.FUShort = (ushort) i;
        x.FInt = i;
        x.FUInt = (uint) i;
        x.FLong = i;
        x.FULong = (ulong) i;
        x.FDecimal = i;
        x.FFloat = i;
        x.FDouble = i;
        x.FDateTime = new DateTime(2009, 1, i + 1);
        x.FTimeSpan = new TimeSpan(i, 0, 0, 0);
      }

      all = Query.All<X>().ToList();
    }
    
    [Test]
    public void SumTest()
    {
      Assert.AreEqual(all.Sum(x => x.FByte), Query.All<X>().Sum(x => x.FByte));
      Assert.AreEqual(all.Sum(x => x.FSByte), Query.All<X>().Sum(x => x.FSByte));

      Assert.AreEqual(all.Sum(x => x.FShort), Query.All<X>().Sum(x => x.FShort));
      Assert.AreEqual(all.Sum(x => x.FUShort), Query.All<X>().Sum(x => x.FUShort));

      Assert.AreEqual(all.Sum(x => x.FInt), Query.All<X>().Sum(x => x.FInt));
      Assert.AreEqual(all.Sum(x => x.FUInt), Query.All<X>().Sum(x => x.FUInt));

      Assert.AreEqual(all.Sum(x => x.FLong), Query.All<X>().Sum(x => x.FLong));
      Assert.AreEqual(all.Sum(x => x.FFloat), Query.All<X>().Sum(x => x.FFloat));
      Assert.AreEqual(all.Sum(x => x.FDecimal), Query.All<X>().Sum(x => x.FDecimal));
    }

    [Test]
    public void AverageTest()
    {
      Assert.AreEqual(all.Average(x => x.FByte), Query.All<X>().Average(x => x.FByte));
      Assert.AreEqual(all.Average(x => x.FSByte), Query.All<X>().Average(x => x.FSByte));

      Assert.AreEqual(all.Average(x => x.FShort), Query.All<X>().Average(x => x.FShort));
      Assert.AreEqual(all.Average(x => x.FUShort), Query.All<X>().Average(x => x.FUShort));

      Assert.AreEqual(all.Average(x => x.FInt), Query.All<X>().Average(x => x.FInt));
      Assert.AreEqual(all.Average(x => x.FUInt), Query.All<X>().Average(x => x.FUInt));

      Assert.AreEqual(all.Average(x => x.FLong), Query.All<X>().Average(x => x.FLong));
      Assert.AreEqual(all.Average(x => x.FFloat), Query.All<X>().Average(x => x.FFloat));
      Assert.AreEqual(all.Average(x => x.FDecimal), Query.All<X>().Average(x => x.FDecimal));
    }

    [Test]
    public void MinTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), Query.All<X>().Min(x => x.FByte));
      Assert.AreEqual(all.Min(x => x.FSByte), Query.All<X>().Min(x => x.FSByte));

      Assert.AreEqual(all.Min(x => x.FShort), Query.All<X>().Min(x => x.FShort));
      Assert.AreEqual(all.Min(x => x.FUShort), Query.All<X>().Min(x => x.FUShort));

      Assert.AreEqual(all.Min(x => x.FInt), Query.All<X>().Min(x => x.FInt));
      Assert.AreEqual(all.Min(x => x.FUInt), Query.All<X>().Min(x => x.FUInt));

      Assert.AreEqual(all.Min(x => x.FLong), Query.All<X>().Min(x => x.FLong));
      Assert.AreEqual(all.Min(x => x.FFloat), Query.All<X>().Min(x => x.FFloat));
      Assert.AreEqual(all.Min(x => x.FDecimal), Query.All<X>().Min(x => x.FDecimal));

      Assert.AreEqual(all.Min(x => x.FDateTime), Query.All<X>().Min(x => x.FDateTime));
      Assert.AreEqual(all.Min(x => x.FTimeSpan), Query.All<X>().Min(x => x.FTimeSpan));

    }

    [Test]
    public void MaxTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), Query.All<X>().Max(x => x.FByte));
      Assert.AreEqual(all.Max(x => x.FSByte), Query.All<X>().Max(x => x.FSByte));

      Assert.AreEqual(all.Max(x => x.FShort), Query.All<X>().Max(x => x.FShort));
      Assert.AreEqual(all.Max(x => x.FUShort), Query.All<X>().Max(x => x.FUShort));

      Assert.AreEqual(all.Max(x => x.FInt), Query.All<X>().Max(x => x.FInt));
      Assert.AreEqual(all.Max(x => x.FUInt), Query.All<X>().Max(x => x.FUInt));

      Assert.AreEqual(all.Max(x => x.FLong), Query.All<X>().Max(x => x.FLong));
      Assert.AreEqual(all.Max(x => x.FFloat), Query.All<X>().Max(x => x.FFloat));
      Assert.AreEqual(all.Max(x => x.FDecimal), Query.All<X>().Max(x => x.FDecimal));

      Assert.AreEqual(all.Max(x => x.FDateTime), Query.All<X>().Max(x => x.FDateTime));
      Assert.AreEqual(all.Max(x => x.FTimeSpan), Query.All<X>().Max(x => x.FTimeSpan));
    }
  }
}