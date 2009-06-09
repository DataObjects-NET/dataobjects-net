// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.09

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class RoundingTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        RunTest(2.5m);
        RunTest(5.7m);
        RunTest(1.2m);
        RunTest(-2.5m);
        RunTest(-5.7m);
        RunTest(-1.2m);
      }
    }

    private void RunTest(decimal value)
    {
      using (Transaction.Open()) {
        new X
          {
            FFloat = (float) value,
            FDouble = (double) value,
            FDecimal = value,
          };
        var result = Query<X>.All
          .Select(x => new
            {
              Float = x.FFloat,
              FloatRound = Math.Round(x.FFloat),
              FloatTrunc = Math.Truncate(x.FFloat),
              FloatCeil = Math.Ceiling(x.FFloat),
              FloatFloor = Math.Floor(x.FFloat),
              Double = x.FDouble,
              DoubleRound = Math.Round(x.FDouble),
              DoubleTrunc = Math.Truncate(x.FDouble),
              DoubleCeil = Math.Ceiling(x.FDouble),
              DoubleFloor = Math.Floor(x.FDouble),
              Decimal = x.FDecimal,
              DecimalRound = Math.Round(x.FDecimal),
              DecimalTrunc = Math.Truncate(x.FDecimal),
              DecimalCeil = Math.Ceiling(x.FDecimal),
              DecimalFloor = Math.Floor(x.FDecimal),
            }).Single();
        Assert.AreEqual(Math.Round(result.Float), result.FloatRound);
        Assert.AreEqual(Math.Truncate(result.Float), result.FloatTrunc);
        Assert.AreEqual(Math.Ceiling(result.Float), result.FloatCeil);
        Assert.AreEqual(Math.Floor(result.Float), result.FloatFloor);

        Assert.AreEqual(Math.Round(result.Double), result.DoubleRound);
        Assert.AreEqual(Math.Truncate(result.Double), result.DoubleTrunc);
        Assert.AreEqual(Math.Ceiling(result.Double), result.DoubleCeil);
        Assert.AreEqual(Math.Floor(result.Double), result.DoubleFloor);

        Assert.AreEqual(Math.Round(result.Decimal), result.DecimalRound);
        Assert.AreEqual(Math.Truncate(result.Decimal), result.DecimalTrunc);
        Assert.AreEqual(Math.Ceiling(result.Decimal), result.DecimalCeil);
        Assert.AreEqual(Math.Floor(result.Decimal), result.DecimalFloor);
      }
    }
  }
}