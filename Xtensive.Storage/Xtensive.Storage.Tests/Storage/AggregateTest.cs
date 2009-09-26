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

      all = Query<X>.All.ToList();
    }
    
    [Test]
    public void SumTest()
    {
      Assert.AreEqual(all.Sum(x => x.FByte), Query<X>.All.Sum(x => x.FByte));
      Assert.AreEqual(all.Sum(x => x.FSByte), Query<X>.All.Sum(x => x.FSByte));

      Assert.AreEqual(all.Sum(x => x.FShort), Query<X>.All.Sum(x => x.FShort));
      Assert.AreEqual(all.Sum(x => x.FUShort), Query<X>.All.Sum(x => x.FUShort));

      Assert.AreEqual(all.Sum(x => x.FInt), Query<X>.All.Sum(x => x.FInt));
      Assert.AreEqual(all.Sum(x => x.FUInt), Query<X>.All.Sum(x => x.FUInt));

      Assert.AreEqual(all.Sum(x => x.FLong), Query<X>.All.Sum(x => x.FLong));
      Assert.AreEqual(all.Sum(x => x.FFloat), Query<X>.All.Sum(x => x.FFloat));
      Assert.AreEqual(all.Sum(x => x.FDecimal), Query<X>.All.Sum(x => x.FDecimal));
    }

    [Test]
    public void AverageTest()
    {
      Assert.AreEqual(all.Average(x => x.FByte), Query<X>.All.Average(x => x.FByte));
      Assert.AreEqual(all.Average(x => x.FSByte), Query<X>.All.Average(x => x.FSByte));

      Assert.AreEqual(all.Average(x => x.FShort), Query<X>.All.Average(x => x.FShort));
      Assert.AreEqual(all.Average(x => x.FUShort), Query<X>.All.Average(x => x.FUShort));

      Assert.AreEqual(all.Average(x => x.FInt), Query<X>.All.Average(x => x.FInt));
      Assert.AreEqual(all.Average(x => x.FUInt), Query<X>.All.Average(x => x.FUInt));

      Assert.AreEqual(all.Average(x => x.FLong), Query<X>.All.Average(x => x.FLong));
      Assert.AreEqual(all.Average(x => x.FFloat), Query<X>.All.Average(x => x.FFloat));
      Assert.AreEqual(all.Average(x => x.FDecimal), Query<X>.All.Average(x => x.FDecimal));
    }

    [Test]
    public void MinTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), Query<X>.All.Min(x => x.FByte));
      Assert.AreEqual(all.Min(x => x.FSByte), Query<X>.All.Min(x => x.FSByte));

      Assert.AreEqual(all.Min(x => x.FShort), Query<X>.All.Min(x => x.FShort));
      Assert.AreEqual(all.Min(x => x.FUShort), Query<X>.All.Min(x => x.FUShort));

      Assert.AreEqual(all.Min(x => x.FInt), Query<X>.All.Min(x => x.FInt));
      Assert.AreEqual(all.Min(x => x.FUInt), Query<X>.All.Min(x => x.FUInt));

      Assert.AreEqual(all.Min(x => x.FLong), Query<X>.All.Min(x => x.FLong));
      Assert.AreEqual(all.Min(x => x.FFloat), Query<X>.All.Min(x => x.FFloat));
      Assert.AreEqual(all.Min(x => x.FDecimal), Query<X>.All.Min(x => x.FDecimal));

      Assert.AreEqual(all.Min(x => x.FDateTime), Query<X>.All.Min(x => x.FDateTime));
      Assert.AreEqual(all.Min(x => x.FTimeSpan), Query<X>.All.Min(x => x.FTimeSpan));

    }

    [Test]
    public void MaxTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), Query<X>.All.Max(x => x.FByte));
      Assert.AreEqual(all.Max(x => x.FSByte), Query<X>.All.Max(x => x.FSByte));

      Assert.AreEqual(all.Max(x => x.FShort), Query<X>.All.Max(x => x.FShort));
      Assert.AreEqual(all.Max(x => x.FUShort), Query<X>.All.Max(x => x.FUShort));

      Assert.AreEqual(all.Max(x => x.FInt), Query<X>.All.Max(x => x.FInt));
      Assert.AreEqual(all.Max(x => x.FUInt), Query<X>.All.Max(x => x.FUInt));

      Assert.AreEqual(all.Max(x => x.FLong), Query<X>.All.Max(x => x.FLong));
      Assert.AreEqual(all.Max(x => x.FFloat), Query<X>.All.Max(x => x.FFloat));
      Assert.AreEqual(all.Max(x => x.FDecimal), Query<X>.All.Max(x => x.FDecimal));

      Assert.AreEqual(all.Max(x => x.FDateTime), Query<X>.All.Max(x => x.FDateTime));
      Assert.AreEqual(all.Max(x => x.FTimeSpan), Query<X>.All.Max(x => x.FTimeSpan));
    }
  }
}