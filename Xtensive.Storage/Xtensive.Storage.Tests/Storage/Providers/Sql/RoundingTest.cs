// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class RoundingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      var testValues = new [] {
        1.3m, 1.5m, 1.6m,
        2.3m, 2.5m, 2.6m,
        -1.3m, -1.5m, -1.6m,
        -2.3m, -2.5m, -2.6m};

      using (Domain.OpenSession())
        foreach (var value in testValues)
          RunTest(value);
    }

    private void RunTest(decimal value)
    {
      using (var t = Transaction.Open()) {
        new X {FDouble = (double) value, FDecimal = value};
        var result = Query<X>.All
          .Select(x => new
            {
              Id = x.Id,
              Double = x.FDouble,
              DoubleRound = Math.Round(x.FDouble),
              DoubleRoundAwayFromZero = Math.Round(x.FDouble, MidpointRounding.AwayFromZero),
              DoubleRoundToEven = Math.Round(x.FDouble, MidpointRounding.ToEven),
              //DoubleRound1 = Math.Round(x.FDouble, 1),
              DoubleTrunc = Math.Truncate(x.FDouble),
              DoubleCeil = Math.Ceiling(x.FDouble),
              DoubleFloor = Math.Floor(x.FDouble),

              Decimal = x.FDecimal,
              DecimalRound = Math.Round(x.FDecimal),
              DecimalRoundAwayFromZero = Math.Round(x.FDecimal, MidpointRounding.AwayFromZero),
              DecimalRoundToEven = Math.Round(x.FDecimal, MidpointRounding.ToEven),
              //DecimalRound1 = Math.Round(x.FDecimal, 1),
              DecimalTrunc = Math.Truncate(x.FDecimal),
              DecimalCeil = Math.Ceiling(x.FDecimal),
              DecimalFloor = Math.Floor(x.FDecimal),
            })
          .OrderByDescending(item => item.Id)
          .First();

        t.Complete();

        Assert.AreEqual(Math.Round(result.Double), result.DoubleRound);
        Assert.AreEqual(Math.Round(result.Double, MidpointRounding.AwayFromZero), result.DoubleRoundAwayFromZero);
        Assert.AreEqual(Math.Round(result.Double, MidpointRounding.ToEven), result.DoubleRoundToEven);
        //Assert.AreEqual(Math.Round(result.Double, 1), result.DoubleRound1);
        Assert.AreEqual(Math.Truncate(result.Double), result.DoubleTrunc);
        Assert.AreEqual(Math.Ceiling(result.Double), result.DoubleCeil);
        Assert.AreEqual(Math.Floor(result.Double), result.DoubleFloor);

        Assert.AreEqual(Math.Round(result.Decimal), result.DecimalRound);
        Assert.AreEqual(Math.Round(result.Decimal, MidpointRounding.AwayFromZero), result.DecimalRoundAwayFromZero);
        Assert.AreEqual(Math.Round(result.Decimal, MidpointRounding.ToEven), result.DecimalRoundToEven);
        //Assert.AreEqual(Math.Round(result.Decimal, 1), result.DecimalRound1);
        Assert.AreEqual(Math.Truncate(result.Decimal), result.DecimalTrunc);
        Assert.AreEqual(Math.Ceiling(result.Decimal), result.DecimalCeil);
        Assert.AreEqual(Math.Floor(result.Decimal), result.DecimalFloor);
      }
    }
  }
}