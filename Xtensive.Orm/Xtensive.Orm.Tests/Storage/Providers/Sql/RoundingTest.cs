// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class RoundingTest : AutoBuildTest
  {
    private const double DoubleDelta = 0.00000001d;
    private const decimal DecimalDelta = 0.000000000001m;

    private DisposableSet disposableSet;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();

      var testValues = new[] {
        1.3m, 1.5m, 1.6m,
        2.3m, 2.5m, 2.6m,
        -1.3m, -1.5m, -1.6m,
        -2.3m, -2.5m, -2.6m,
        10.1m, 10.5m, 11.0m,
        20.1m, 20.5m, 21.0m,
        -10.1m, -10.5m, -11.0m,
        -20.1m, -20.5m, -21.0m,

        0.13m, 0.15m, 0.16m,
        0.23m, 0.25m, 0.26m,
        -.13m, -.15m, -.16m,
        -.23m, -.25m, -.26m,
        1.01m, 1.05m, 1.10m,
        2.01m, 2.05m, 2.10m,
        -1.01m, -1.05m, -1.10m,
        -2.01m, -2.05m, -2.10m
      };
      foreach (var value in testValues)
        new X {FDouble = (double) value, FDecimal = value};
    }

    public override void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
      base.TestFixtureTearDown();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void TruncCeilFloorTest()
    {
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          Double = x.FDouble,
          DoubleTrunc = Math.Truncate(x.FDouble),
          DoubleCeil = Math.Ceiling(x.FDouble),
          DoubleFloor = Math.Floor(x.FDouble),
          Decimal = x.FDecimal,
          DecimalTrunc = Math.Truncate(x.FDecimal),
          DecimalCeil = Math.Ceiling(x.FDecimal),
          DecimalFloor = Math.Floor(x.FDecimal),
        });
      foreach (var result in results) {
        AreEqual(Math.Truncate(result.Double), result.DoubleTrunc);
        AreEqual(Math.Ceiling(result.Double), result.DoubleCeil);
        AreEqual(Math.Floor(result.Double), result.DoubleFloor);
        AreEqual(Math.Truncate(result.Decimal), result.DecimalTrunc);
        AreEqual(Math.Ceiling(result.Decimal), result.DecimalCeil);
        AreEqual(Math.Floor(result.Decimal), result.DecimalFloor);
      }
    }

    [Test]
    public void RoundDefaultTest()
    {
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          Double = x.FDouble,
          DoubleRound = Math.Round(x.FDouble),
          DoubleRound1 = Math.Round(x.FDouble, 1),
          Decimal = x.FDecimal,
          DecimalRound = Math.Round(x.FDecimal),
          DecimalRound1 = Math.Round(x.FDecimal, 1),
        });

      foreach (var result in results) {
        AreEqual(Math.Round(result.Double), result.DoubleRound);
        AreEqual(Math.Round(result.Double, 1), result.DoubleRound1);
        AreEqual(Math.Round(result.Decimal), result.DecimalRound);
        AreEqual(Math.Round(result.Decimal, 1), result.DecimalRound1);
      }
    }

    [Test]
    public void RoundToEvenTest()
    {
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          Double = x.FDouble,
          DoubleRound = Math.Round(x.FDouble, MidpointRounding.ToEven),
          DoubleRound1 = Math.Round(x.FDouble, 1, MidpointRounding.ToEven),
          Decimal = x.FDecimal,
          DecimalRound = Math.Round(x.FDecimal, MidpointRounding.ToEven),
          DecimalRound1 = Math.Round(x.FDecimal, 1, MidpointRounding.ToEven),
        });

      foreach (var result in results) {
        AreEqual(Math.Round(result.Double, MidpointRounding.ToEven), result.DoubleRound);
        AreEqual(Math.Round(result.Double, 1, MidpointRounding.ToEven), result.DoubleRound1);
        AreEqual(Math.Round(result.Decimal, MidpointRounding.ToEven), result.DecimalRound);
        AreEqual(Math.Round(result.Decimal, 1, MidpointRounding.ToEven), result.DecimalRound1);
      }
    }

    [Test]
    public void RoundAwayFromZeroTest()
    {
      var results = Session.Demand().Query.All<X>()
        .Select(x => new {
          Double = x.FDouble,
          DoubleRound = Math.Round(x.FDouble, MidpointRounding.AwayFromZero),
          DoubleRound1 = Math.Round(x.FDouble, 1, MidpointRounding.AwayFromZero),
          Decimal = x.FDecimal,
          DecimalRound = Math.Round(x.FDecimal, MidpointRounding.AwayFromZero),
          DecimalRound1 = Math.Round(x.FDecimal, 1, MidpointRounding.AwayFromZero),
        });

      foreach (var result in results) {
        AreEqual(Math.Round(result.Double, MidpointRounding.AwayFromZero), result.DoubleRound);
        AreEqual(Math.Round(result.Double, 1, MidpointRounding.AwayFromZero), result.DoubleRound1);
        AreEqual(Math.Round(result.Decimal, MidpointRounding.AwayFromZero), result.DecimalRound);
        AreEqual(Math.Round(result.Decimal, 1, MidpointRounding.AwayFromZero), result.DecimalRound1);
      }
    }

    private static void AreEqual(decimal expected, decimal actual)
    {
      if (Math.Abs(expected - actual) > DecimalDelta)
        Assert.Fail("expected {0} actual {1}", expected, actual);
    }

    private void AreEqual(double expected, double actual)
    {
      Assert.AreEqual(expected, actual, DoubleDelta);
    }
  }
}