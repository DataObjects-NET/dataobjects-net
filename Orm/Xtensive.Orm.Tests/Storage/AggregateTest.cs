// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.28

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;

using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class AggregateTest : AutoBuildTest
  {
    private List<X> all;

    protected override bool InitGlobalSession => true;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      all = new List<X>();

      for (var i = 0; i < 10; i++) {
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

        all.Add(x);
      }

      GlobalSession.SaveChanges();
    }
    
    [Test]
    public void SumTest()
    {
      Assert.AreEqual(all.Sum(x => x.FByte), GlobalSession.Query.All<X>().Sum(x => x.FByte), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Sum(x => x.FSByte), GlobalSession.Query.All<X>().Sum(x => x.FSByte), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Sum(x => x.FShort), GlobalSession.Query.All<X>().Sum(x => x.FShort), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Sum(x => x.FUShort), GlobalSession.Query.All<X>().Sum(x => x.FUShort), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Sum(x => x.FInt), GlobalSession.Query.All<X>().Sum(x => x.FInt), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Sum(x => x.FUInt), GlobalSession.Query.All<X>().Sum(x => x.FUInt), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Sum(x => x.FLong), GlobalSession.Query.All<X>().Sum(x => x.FLong), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Sum(x => x.FFloat), GlobalSession.Query.All<X>().Sum(x => x.FFloat), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Sum(x => x.FDecimal), GlobalSession.Query.All<X>().Sum(x => x.FDecimal), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void SumNoLambdaTest()
    {
      Assert.AreEqual(all.Sum(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Sum(), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Sum(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Sum(), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Sum(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Sum(), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Sum(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Sum(), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void SumByValueItselfTest()
    {
      Assert.AreEqual(all.Sum(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Sum(x => x), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Sum(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Sum(x => x), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Sum(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Sum(x => x), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Sum(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Sum(x => x), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Sum(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Sum(x => x), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Sum(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Sum(x => x), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Sum(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Sum(x => x), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Sum(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Sum(x => x), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Sum(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Sum(x => x), $"Failed for {nameof(X.FDecimal)}");
    }


    [Test]
    public void AverageTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider==WellKnown.Provider.Firebird) {
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FByte)), GlobalSession.Query.All<X>().Average(x => x.FByte), $"Failed for {nameof(X.FByte)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FSByte)), GlobalSession.Query.All<X>().Average(x => x.FSByte), $"Failed for {nameof(X.FSByte)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FShort)), GlobalSession.Query.All<X>().Average(x => x.FShort), $"Failed for {nameof(X.FShort)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FUShort)), GlobalSession.Query.All<X>().Average(x => x.FUShort), $"Failed for {nameof(X.FUShort)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FInt)), GlobalSession.Query.All<X>().Average(x => x.FInt), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FUInt)), GlobalSession.Query.All<X>().Average(x => x.FUInt), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FLong)), GlobalSession.Query.All<X>().Average(x => x.FLong), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.AreEqual(all.Average(x => x.FByte), GlobalSession.Query.All<X>().Average(x => x.FByte), $"Failed for {nameof(X.FByte)}");
        Assert.AreEqual(all.Average(x => x.FSByte), GlobalSession.Query.All<X>().Average(x => x.FSByte), $"Failed for {nameof(X.FSByte)}");
        Assert.AreEqual(all.Average(x => x.FShort), GlobalSession.Query.All<X>().Average(x => x.FShort), $"Failed for {nameof(X.FShort)}");
        Assert.AreEqual(all.Average(x => x.FUShort), GlobalSession.Query.All<X>().Average(x => x.FUShort), $"Failed for {nameof(X.FUShort)}");
        Assert.AreEqual(all.Average(x => x.FInt), GlobalSession.Query.All<X>().Average(x => x.FInt), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(all.Average(x => x.FUInt), GlobalSession.Query.All<X>().Average(x => x.FUInt), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(all.Average(x => x.FLong), GlobalSession.Query.All<X>().Average(x => x.FLong), $"Failed for {nameof(X.FLong)}");
      }

      Assert.AreEqual(all.Average(x => x.FFloat), GlobalSession.Query.All<X>().Average(x => x.FFloat), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Average(x => x.FDecimal), GlobalSession.Query.All<X>().Average(x => x.FDecimal), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void AverageNoLambdaTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird) {
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FInt)), GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FUInt)), GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FLong)), GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.AreEqual(all.Average(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(all.Average(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(all.Average(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), $"Failed for {nameof(X.FLong)}");
      }

      Assert.AreEqual(all.Average(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Average(), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Average(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Average(), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void AverageByValueItselfTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird) {
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FByte)), GlobalSession.Query.All<X>().Select(x => x.FByte).Average(x => x), $"Failed for {nameof(X.FByte)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FSByte)), GlobalSession.Query.All<X>().Select(x => x.FSByte).Average(x => x), $"Failed for {nameof(X.FSByte)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FShort)), GlobalSession.Query.All<X>().Select(x => x.FShort).Average(x => x), $"Failed for {nameof(X.FShort)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FUShort)), GlobalSession.Query.All<X>().Select(x => x.FUShort).Average(x => x), $"Failed for {nameof(X.FUShort)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FInt)), GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FUInt)), GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(Math.Truncate(all.Average(x => x.FLong)), GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.AreEqual(all.Average(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Average(x => x), $"Failed for {nameof(X.FByte)}");
        Assert.AreEqual(all.Average(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Average(x => x), $"Failed for {nameof(X.FSByte)}");
        Assert.AreEqual(all.Average(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Average(x => x), $"Failed for {nameof(X.FShort)}");
        Assert.AreEqual(all.Average(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Average(x => x), $"Failed for {nameof(X.FUShort)}");
        Assert.AreEqual(all.Average(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), $"Failed for {nameof(X.FInt)}");
        Assert.AreEqual(all.Average(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), $"Failed for {nameof(X.FUInt)}");
        Assert.AreEqual(all.Average(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), $"Failed for {nameof(X.FLong)}");
      }

      Assert.AreEqual(all.Average(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Average(), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Average(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Average(), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void MinTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), GlobalSession.Query.All<X>().Min(x => x.FByte), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Min(x => x.FSByte), GlobalSession.Query.All<X>().Min(x => x.FSByte), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Min(x => x.FShort), GlobalSession.Query.All<X>().Min(x => x.FShort), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Min(x => x.FUShort), GlobalSession.Query.All<X>().Min(x => x.FUShort), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Min(x => x.FInt), GlobalSession.Query.All<X>().Min(x => x.FInt), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Min(x => x.FUInt), GlobalSession.Query.All<X>().Min(x => x.FUInt), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Min(x => x.FLong), GlobalSession.Query.All<X>().Min(x => x.FLong), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Min(x => x.FFloat), GlobalSession.Query.All<X>().Min(x => x.FFloat), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Min(x => x.FDecimal), GlobalSession.Query.All<X>().Min(x => x.FDecimal), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Min(x => x.FDateTime), GlobalSession.Query.All<X>().Min(x => x.FDateTime), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Min(x => x.FTimeSpan), GlobalSession.Query.All<X>().Min(x => x.FTimeSpan), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MinNoLambdaTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Min(), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Min(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Min(), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Min(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Min(), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Min(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Min(), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Min(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Min(), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Min(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Min(), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Min(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Min(), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Min(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Min(), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Min(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Min(), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Min(x => x.FDateTime), GlobalSession.Query.All<X>().Select(x => x.FDateTime).Min(), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Min(x => x.FTimeSpan), GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Min(), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MinByValueItselfTest()
    {
      Assert.AreEqual(all.Min(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Min(x => x), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Min(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Min(x => x), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Min(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Min(x => x), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Min(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Min(x => x), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Min(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Min(x => x), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Min(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Min(x => x), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Min(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Min(x => x), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Min(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Min(x => x), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Min(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Min(x => x), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Min(x => x.FDateTime), GlobalSession.Query.All<X>().Select(x => x.FDateTime).Min(x => x), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Min(x => x.FTimeSpan), GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Min(x => x), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), GlobalSession.Query.All<X>().Max(x => x.FByte), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Max(x => x.FSByte), GlobalSession.Query.All<X>().Max(x => x.FSByte), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Max(x => x.FShort), GlobalSession.Query.All<X>().Max(x => x.FShort), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Max(x => x.FUShort), GlobalSession.Query.All<X>().Max(x => x.FUShort), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Max(x => x.FInt), GlobalSession.Query.All<X>().Max(x => x.FInt), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Max(x => x.FUInt), GlobalSession.Query.All<X>().Max(x => x.FUInt), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Max(x => x.FLong), GlobalSession.Query.All<X>().Max(x => x.FLong), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Max(x => x.FFloat), GlobalSession.Query.All<X>().Max(x => x.FFloat), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Max(x => x.FDecimal), GlobalSession.Query.All<X>().Max(x => x.FDecimal), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Max(x => x.FDateTime), GlobalSession.Query.All<X>().Max(x => x.FDateTime), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Max(x => x.FTimeSpan), GlobalSession.Query.All<X>().Max(x => x.FTimeSpan), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxNoLambdaTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Max(), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Max(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Max(), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Max(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Max(), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Max(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Max(), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Max(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Max(), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Max(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Max(), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Max(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Max(), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Max(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Max(), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Max(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Max(), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Max(x => x.FDateTime), GlobalSession.Query.All<X>().Select(x => x.FDateTime).Max(), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Max(x => x.FTimeSpan), GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Max(), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxByValueItselfTest()
    {
      Assert.AreEqual(all.Max(x => x.FByte), GlobalSession.Query.All<X>().Select(x => x.FByte).Max(x => x), $"Failed for {nameof(X.FByte)}");
      Assert.AreEqual(all.Max(x => x.FSByte), GlobalSession.Query.All<X>().Select(x => x.FSByte).Max(x => x), $"Failed for {nameof(X.FSByte)}");

      Assert.AreEqual(all.Max(x => x.FShort), GlobalSession.Query.All<X>().Select(x => x.FShort).Max(x => x), $"Failed for {nameof(X.FShort)}");
      Assert.AreEqual(all.Max(x => x.FUShort), GlobalSession.Query.All<X>().Select(x => x.FUShort).Max(x => x), $"Failed for {nameof(X.FUShort)}");

      Assert.AreEqual(all.Max(x => x.FInt), GlobalSession.Query.All<X>().Select(x => x.FInt).Max(x => x), $"Failed for {nameof(X.FInt)}");
      Assert.AreEqual(all.Max(x => x.FUInt), GlobalSession.Query.All<X>().Select(x => x.FUInt).Max(x => x), $"Failed for {nameof(X.FUInt)}");

      Assert.AreEqual(all.Max(x => x.FLong), GlobalSession.Query.All<X>().Select(x => x.FLong).Max(x => x), $"Failed for {nameof(X.FLong)}");
      Assert.AreEqual(all.Max(x => x.FFloat), GlobalSession.Query.All<X>().Select(x => x.FFloat).Max(x => x), $"Failed for {nameof(X.FFloat)}");
      Assert.AreEqual(all.Max(x => x.FDecimal), GlobalSession.Query.All<X>().Select(x => x.FDecimal).Max(x => x), $"Failed for {nameof(X.FDecimal)}");

      Assert.AreEqual(all.Max(x => x.FDateTime), GlobalSession.Query.All<X>().Select(x => x.FDateTime).Max(x => x), $"Failed for {nameof(X.FDateTime)}");
      Assert.AreEqual(all.Max(x => x.FTimeSpan), GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Max(x => x), $"Failed for {nameof(X.FTimeSpan)}");
    }
  }
}