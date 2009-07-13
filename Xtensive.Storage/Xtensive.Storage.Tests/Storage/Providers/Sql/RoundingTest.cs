// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class RoundingTest : AutoBuildTest
  {
    private DisposableSet disposableSet;

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      disposableSet = new DisposableSet();
      disposableSet.Add(Domain.OpenSession());
      disposableSet.Add(Transaction.Open());

      var testValues = new[]
        {
          1.3m, 1.5m, 1.6m,
          2.3m, 2.5m, 2.6m,
          -1.3m, -1.5m, -1.6m,
          -2.3m, -2.5m, -2.6m,
          10.1m, 10.5m, 11.0m,
          20.1m, 20.5m, 21.0m,
          -10.1m, -10.5m, -11.0m,
          -20.1m, -20.5m, -21.0m
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
      var results = Query<X>.All
        .Select(x => new
          {
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
          Assert.AreEqual(Math.Truncate(result.Double), result.DoubleTrunc);
          Assert.AreEqual(Math.Ceiling(result.Double), result.DoubleCeil);
          Assert.AreEqual(Math.Floor(result.Double), result.DoubleFloor);
          Assert.AreEqual(Math.Truncate(result.Decimal), result.DecimalTrunc);
          Assert.AreEqual(Math.Ceiling(result.Decimal), result.DecimalCeil);
          Assert.AreEqual(Math.Floor(result.Decimal), result.DecimalFloor);
      }
    }

    [Test]
    public void RoundDefaultTest()
    {
      var results = Query<X>.All
        .Select(x => new
          {
            Double = x.FDouble,
            DoubleRound = Math.Round(x.FDouble),
            Decimal = x.FDecimal,
            DecimalRound = Math.Round(x.FDecimal),
          });

      foreach (var result in results) {
        Assert.AreEqual(Math.Round(result.Double), result.DoubleRound);
        Assert.AreEqual(Math.Round(result.Decimal), result.DecimalRound);
      }
    }

    [Test]
    public void RoundToEvenTest()
    {
      var results = Query<X>.All
        .Select(x => new
          {
            Double = x.FDouble,
            DoubleRound = Math.Round(x.FDouble, MidpointRounding.ToEven),
            Decimal = x.FDecimal,
            DecimalRound = Math.Round(x.FDecimal, MidpointRounding.ToEven),
          });

      foreach (var result in results) {
        Assert.AreEqual(Math.Round(result.Double, MidpointRounding.ToEven), result.DoubleRound);
        Assert.AreEqual(Math.Round(result.Decimal, MidpointRounding.ToEven), result.DecimalRound);
      }
    }

    [Test]
    public void RoundAwayFromZeroTest()
    {
      var results = Query<X>.All
        .Select(x => new
          {
            Double = x.FDouble,
            DoubleRound = Math.Round(x.FDouble, MidpointRounding.AwayFromZero),
            Decimal = x.FDecimal,
            DecimalRound = Math.Round(x.FDecimal, MidpointRounding.AwayFromZero),
          });

      foreach (var result in results) {
        Assert.AreEqual(Math.Round(result.Double, MidpointRounding.AwayFromZero), result.DoubleRound);
        Assert.AreEqual(Math.Round(result.Decimal, MidpointRounding.AwayFromZero), result.DecimalRound);
      }
    }
  }
}