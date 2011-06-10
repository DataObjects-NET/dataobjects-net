// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.28

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;
using Xtensive.Disposing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage
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

      all = Session.Demand().Query.All<X>().ToList();
    }
    
    [Test]
    public void SumTest()
    {
      Assert.AreEqual(all.Sum(x => x.FByte), Session.Demand().Query.All<X>().Sum(x => x.FByte));
      Assert.AreEqual(all.Sum(x => x.FSByte), Session.Demand().Query.All<X>().Sum(x => x.FSByte));

      Assert.AreEqual(all.Sum(x => x.FShort), Session.Demand().Query.All<X>().Sum(x => x.FShort));
      Assert.AreEqual(all.Sum(x => x.FUShort), Session.Demand().Query.All<X>().Sum(x => x.FUShort));

      Assert.AreEqual(all.Sum(x => x.FInt), Session.Demand().Query.All<X>().Sum(x => x.FInt));
      Assert.AreEqual(all.Sum(x => x.FUInt), Session.Demand().Query.All<X>().Sum(x => x.FUInt));

      Assert.AreEqual(all.Sum(x => x.FLong), Session.Demand().Query.All<X>().Sum(x => x.FLong));
      Assert.AreEqual(all.Sum(x => x.FFloat), Session.Demand().Query.All<X>().Sum(x => x.FFloat));
      Assert.AreEqual(all.Sum(x => x.FDecimal), Session.Demand().Query.All<X>().Sum(x => x.FDecimal));
    }

    [Test]
    public void AverageTest()
    {
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird)
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FByte)), Session.Demand().Query.All<X>().Average(x => x.FByte));
      else
        Assert.AreEqual(all.Average(x => x.FByte), Session.Demand().Query.All<X>().Average(x => x.FByte));
      Assert.AreEqual(all.Average(x => x.FSByte), Session.Demand().Query.All<X>().Average(x => x.FSByte));

      Assert.AreEqual(all.Average(x => x.FShort), Session.Demand().Query.All<X>().Average(x => x.FShort));
      Assert.AreEqual(all.Average(x => x.FUShort), Session.Demand().Query.All<X>().Average(x => x.FUShort));

      Assert.AreEqual(all.Average(x => x.FInt), Session.Demand().Query.All<X>().Average(x => x.FInt));
      Assert.AreEqual(all.Average(x => x.FUInt), Session.Demand().Query.All<X>().Average(x => x.FUInt));

      Assert.AreEqual(all.Average(x => x.FLong), Session.Demand().Query.All<X>().Average(x => x.FLong));
      Assert.AreEqual(all.Average(x => x.FFloat), Session.Demand().Query.All<X>().Average(x => x.FFloat));
      Assert.AreEqual(all.Average(x => x.FDecimal), Session.Demand().Query.All<X>().Average(x => x.FDecimal));
    }

    [Test]
    public void MinTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), Session.Demand().Query.All<X>().Min(x => x.FByte));
      Assert.AreEqual(all.Min(x => x.FSByte), Session.Demand().Query.All<X>().Min(x => x.FSByte));

      Assert.AreEqual(all.Min(x => x.FShort), Session.Demand().Query.All<X>().Min(x => x.FShort));
      Assert.AreEqual(all.Min(x => x.FUShort), Session.Demand().Query.All<X>().Min(x => x.FUShort));

      Assert.AreEqual(all.Min(x => x.FInt), Session.Demand().Query.All<X>().Min(x => x.FInt));
      Assert.AreEqual(all.Min(x => x.FUInt), Session.Demand().Query.All<X>().Min(x => x.FUInt));

      Assert.AreEqual(all.Min(x => x.FLong), Session.Demand().Query.All<X>().Min(x => x.FLong));
      Assert.AreEqual(all.Min(x => x.FFloat), Session.Demand().Query.All<X>().Min(x => x.FFloat));
      Assert.AreEqual(all.Min(x => x.FDecimal), Session.Demand().Query.All<X>().Min(x => x.FDecimal));

      Assert.AreEqual(all.Min(x => x.FDateTime), Session.Demand().Query.All<X>().Min(x => x.FDateTime));
      Assert.AreEqual(all.Min(x => x.FTimeSpan), Session.Demand().Query.All<X>().Min(x => x.FTimeSpan));

    }

    [Test]
    public void MaxTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), Session.Demand().Query.All<X>().Max(x => x.FByte));
      Assert.AreEqual(all.Max(x => x.FSByte), Session.Demand().Query.All<X>().Max(x => x.FSByte));

      Assert.AreEqual(all.Max(x => x.FShort), Session.Demand().Query.All<X>().Max(x => x.FShort));
      Assert.AreEqual(all.Max(x => x.FUShort), Session.Demand().Query.All<X>().Max(x => x.FUShort));

      Assert.AreEqual(all.Max(x => x.FInt), Session.Demand().Query.All<X>().Max(x => x.FInt));
      Assert.AreEqual(all.Max(x => x.FUInt), Session.Demand().Query.All<X>().Max(x => x.FUInt));

      Assert.AreEqual(all.Max(x => x.FLong), Session.Demand().Query.All<X>().Max(x => x.FLong));
      Assert.AreEqual(all.Max(x => x.FFloat), Session.Demand().Query.All<X>().Max(x => x.FFloat));
      Assert.AreEqual(all.Max(x => x.FDecimal), Session.Demand().Query.All<X>().Max(x => x.FDecimal));

      Assert.AreEqual(all.Max(x => x.FDateTime), Session.Demand().Query.All<X>().Max(x => x.FDateTime));
      Assert.AreEqual(all.Max(x => x.FTimeSpan), Session.Demand().Query.All<X>().Max(x => x.FTimeSpan));
    }
  }
}