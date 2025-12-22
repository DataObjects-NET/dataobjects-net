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
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FByte), Is.EqualTo(all.Sum(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FSByte), Is.EqualTo(all.Sum(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FShort), Is.EqualTo(all.Sum(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FUShort), Is.EqualTo(all.Sum(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FInt), Is.EqualTo(all.Sum(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FUInt), Is.EqualTo(all.Sum(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FLong), Is.EqualTo(all.Sum(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FFloat), Is.EqualTo(all.Sum(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Sum(x => x.FDecimal), Is.EqualTo(all.Sum(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void SumNoLambdaTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Sum(), Is.EqualTo(all.Sum(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Sum(), Is.EqualTo(all.Sum(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Sum(), Is.EqualTo(all.Sum(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Sum(), Is.EqualTo(all.Sum(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void SumByValueItselfTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Sum(x => x), Is.EqualTo(all.Sum(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Sum(x => x), Is.EqualTo(all.Sum(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Sum(x => x), Is.EqualTo(all.Sum(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Sum(x => x), Is.EqualTo(all.Sum(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Sum(x => x), Is.EqualTo(all.Sum(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Sum(x => x), Is.EqualTo(all.Sum(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Sum(x => x), Is.EqualTo(all.Sum(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Sum(x => x), Is.EqualTo(all.Sum(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Sum(x => x), Is.EqualTo(all.Sum(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }


    [Test]
    public void AverageTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider==WellKnown.Provider.Firebird) {
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FByte), Is.EqualTo(Math.Truncate(all.Average(x => x.FByte))), $"Failed for {nameof(X.FByte)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FSByte), Is.EqualTo(Math.Truncate(all.Average(x => x.FSByte))), $"Failed for {nameof(X.FSByte)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FShort), Is.EqualTo(Math.Truncate(all.Average(x => x.FShort))), $"Failed for {nameof(X.FShort)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FUShort), Is.EqualTo(Math.Truncate(all.Average(x => x.FUShort))), $"Failed for {nameof(X.FUShort)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FInt), Is.EqualTo(Math.Truncate(all.Average(x => x.FInt))), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FUInt), Is.EqualTo(Math.Truncate(all.Average(x => x.FUInt))), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FLong), Is.EqualTo(Math.Truncate(all.Average(x => x.FLong))), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FByte), Is.EqualTo(all.Average(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FSByte), Is.EqualTo(all.Average(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FShort), Is.EqualTo(all.Average(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FUShort), Is.EqualTo(all.Average(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FInt), Is.EqualTo(all.Average(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FUInt), Is.EqualTo(all.Average(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Average(x => x.FLong), Is.EqualTo(all.Average(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      }

      Assert.That(GlobalSession.Query.All<X>().Average(x => x.FFloat), Is.EqualTo(all.Average(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Average(x => x.FDecimal), Is.EqualTo(all.Average(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void AverageNoLambdaTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird) {
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), Is.EqualTo(Math.Truncate(all.Average(x => x.FInt))), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FUInt))), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), Is.EqualTo(Math.Truncate(all.Average(x => x.FLong))), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), Is.EqualTo(all.Average(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), Is.EqualTo(all.Average(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), Is.EqualTo(all.Average(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      }

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Average(), Is.EqualTo(all.Average(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Average(), Is.EqualTo(all.Average(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void AverageByValueItselfTest()
    {
      //"If Field is of an integer type, AVG is always rounded towards 0.
      // For instance, 6 non-null INT records with a sum of -11 yield an average of -1, not -2."
      // © Firebird documentation
      // Funny, isn't it?
      if (Domain.Configuration.ConnectionInfo.Provider == WellKnown.Provider.Firebird) {
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FByte))), $"Failed for {nameof(X.FByte)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FSByte))), $"Failed for {nameof(X.FSByte)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FShort))), $"Failed for {nameof(X.FShort)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FUShort))), $"Failed for {nameof(X.FUShort)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), Is.EqualTo(Math.Truncate(all.Average(x => x.FInt))), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), Is.EqualTo(Math.Truncate(all.Average(x => x.FUInt))), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), Is.EqualTo(Math.Truncate(all.Average(x => x.FLong))), $"Failed for {nameof(X.FLong)}");
      }
      else {
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Average(x => x), Is.EqualTo(all.Average(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Average(x => x), Is.EqualTo(all.Average(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Average(x => x), Is.EqualTo(all.Average(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Average(x => x), Is.EqualTo(all.Average(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Average(), Is.EqualTo(all.Average(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Average(x => x), Is.EqualTo(all.Average(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");
        Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Average(), Is.EqualTo(all.Average(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      }

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Average(), Is.EqualTo(all.Average(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Average(), Is.EqualTo(all.Average(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");
    }

    [Test]
    public void MinTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FByte), Is.EqualTo(all.Min(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FSByte), Is.EqualTo(all.Min(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FShort), Is.EqualTo(all.Min(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FUShort), Is.EqualTo(all.Min(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FInt), Is.EqualTo(all.Min(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FUInt), Is.EqualTo(all.Min(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FLong), Is.EqualTo(all.Min(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FFloat), Is.EqualTo(all.Min(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FDecimal), Is.EqualTo(all.Min(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FDateTime), Is.EqualTo(all.Min(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Min(x => x.FTimeSpan), Is.EqualTo(all.Min(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MinNoLambdaTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Min(), Is.EqualTo(all.Min(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Min(), Is.EqualTo(all.Min(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Min(), Is.EqualTo(all.Min(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Min(), Is.EqualTo(all.Min(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Min(), Is.EqualTo(all.Min(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Min(), Is.EqualTo(all.Min(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Min(), Is.EqualTo(all.Min(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Min(), Is.EqualTo(all.Min(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Min(), Is.EqualTo(all.Min(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDateTime).Min(), Is.EqualTo(all.Min(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Min(), Is.EqualTo(all.Min(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MinByValueItselfTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Min(x => x), Is.EqualTo(all.Min(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Min(x => x), Is.EqualTo(all.Min(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Min(x => x), Is.EqualTo(all.Min(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Min(x => x), Is.EqualTo(all.Min(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Min(x => x), Is.EqualTo(all.Min(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Min(x => x), Is.EqualTo(all.Min(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Min(x => x), Is.EqualTo(all.Min(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Min(x => x), Is.EqualTo(all.Min(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Min(x => x), Is.EqualTo(all.Min(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDateTime).Min(x => x), Is.EqualTo(all.Min(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Min(x => x), Is.EqualTo(all.Min(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FByte), Is.EqualTo(all.Max(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FSByte), Is.EqualTo(all.Max(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FShort), Is.EqualTo(all.Max(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FUShort), Is.EqualTo(all.Max(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FInt), Is.EqualTo(all.Max(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FUInt), Is.EqualTo(all.Max(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FLong), Is.EqualTo(all.Max(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FFloat), Is.EqualTo(all.Max(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FDecimal), Is.EqualTo(all.Max(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FDateTime), Is.EqualTo(all.Max(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Max(x => x.FTimeSpan), Is.EqualTo(all.Max(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxNoLambdaTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Max(), Is.EqualTo(all.Max(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Max(), Is.EqualTo(all.Max(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Max(), Is.EqualTo(all.Max(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Max(), Is.EqualTo(all.Max(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Max(), Is.EqualTo(all.Max(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Max(), Is.EqualTo(all.Max(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Max(), Is.EqualTo(all.Max(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Max(), Is.EqualTo(all.Max(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Max(), Is.EqualTo(all.Max(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDateTime).Max(), Is.EqualTo(all.Max(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Max(), Is.EqualTo(all.Max(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }

    [Test]
    public void MaxByValueItselfTest()
    {
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FByte).Max(x => x), Is.EqualTo(all.Max(x => x.FByte)), $"Failed for {nameof(X.FByte)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FSByte).Max(x => x), Is.EqualTo(all.Max(x => x.FSByte)), $"Failed for {nameof(X.FSByte)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FShort).Max(x => x), Is.EqualTo(all.Max(x => x.FShort)), $"Failed for {nameof(X.FShort)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUShort).Max(x => x), Is.EqualTo(all.Max(x => x.FUShort)), $"Failed for {nameof(X.FUShort)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FInt).Max(x => x), Is.EqualTo(all.Max(x => x.FInt)), $"Failed for {nameof(X.FInt)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FUInt).Max(x => x), Is.EqualTo(all.Max(x => x.FUInt)), $"Failed for {nameof(X.FUInt)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FLong).Max(x => x), Is.EqualTo(all.Max(x => x.FLong)), $"Failed for {nameof(X.FLong)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FFloat).Max(x => x), Is.EqualTo(all.Max(x => x.FFloat)), $"Failed for {nameof(X.FFloat)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDecimal).Max(x => x), Is.EqualTo(all.Max(x => x.FDecimal)), $"Failed for {nameof(X.FDecimal)}");

      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FDateTime).Max(x => x), Is.EqualTo(all.Max(x => x.FDateTime)), $"Failed for {nameof(X.FDateTime)}");
      Assert.That(GlobalSession.Query.All<X>().Select(x => x.FTimeSpan).Max(x => x), Is.EqualTo(all.Max(x => x.FTimeSpan)), $"Failed for {nameof(X.FTimeSpan)}");
    }
  }
}