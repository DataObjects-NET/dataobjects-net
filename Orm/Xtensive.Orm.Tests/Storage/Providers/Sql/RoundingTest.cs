// Copyright (C) 2009-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.06.09

using System;
using System.Linq;
using NUnit.Framework;
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

    [Test]
    public void TruncateTest()
    {
      Query.All<DecimalContainer>().Select(x => new { Decimal = x.d18_9, DecimalTruncate = Math.Truncate(x.d18_9) })
        .GroupBy(x => x.DecimalTruncate)
        .ForEach(i => i.ForEach(x => AreEqual(Math.Truncate(x.Decimal), x.DecimalTruncate)));

      Query.All<DoubleContainer>().Select(x => new { Double = x.FDouble, DoubleTruncate = Math.Truncate(x.FDouble) })
        .GroupBy(x => x.DoubleTruncate)
        .ForEach(i => i.ForEach(x => AreEqual(Math.Truncate(x.Double), x.DoubleTruncate)));
    }

    [Test]
    public void CeilTest()
    {
      Query.All<DecimalContainer>()
        .Select(x => new { Decimal = x.d18_9, DecimalCeiling = Math.Ceiling(x.d18_9) })
        .GroupBy(x => x.DecimalCeiling)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Ceiling(i.Decimal), x.Key)));

      Query.All<DoubleContainer>()
        .Select(x => new { Double = x.FDouble, DoubleCeiling = Math.Ceiling(x.FDouble) })
        .GroupBy(x => x.DoubleCeiling)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Ceiling(i.Double), x.Key)));
    }

    [Test]
    public void FloorTest()
    {
      Query.All<DecimalContainer>()
        .Select(x => new { Decimal = x.d18_9, DecimalFloor = Math.Floor(x.d18_9) })
        .GroupBy(x => x.DecimalFloor)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Floor(i.Decimal), x.Key)));

      Query.All<DoubleContainer>()
        .Select(x => new { Double = x.FDouble, DoubleFloor = Math.Floor(x.FDouble) })
        .GroupBy(x => x.DoubleFloor)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Floor(i.Double), x.Key)));
    }

    [Test]
    public void RoundDefaultToZeroDigitsTest()
    {
      Query.All<DecimalContainer>()
        .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9) })
        .GroupBy(x => x.DecimalRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal), x.Key)));

      Query.All<DoubleContainer>()
        .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble) })
        .GroupBy(x => x.DoubleRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double), x.Key)));
    }

    [Test]
    public void RoundDefaultToNonZeroDigitsTest()
    {
      if (ExpectNotSupported()) {
        var ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DecimalContainer>().Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1) })
            .GroupBy(x => x.DecimalRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));

        ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DoubleContainer>().Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1) })
            .GroupBy(x => x.DoubleRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));
      }
      else {
        Query.All<DecimalContainer>()
          .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1) })
          .GroupBy(x => x.DecimalRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal, 1), x.Key)));

        Query.All<DoubleContainer>()
          .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1) })
          .GroupBy(x => x.DoubleRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double, 1), x.Key)));
      }

      
    }

    [Test]
    public void RoundToEvenToZeroDigitsTest()
    {
      Query.All<DecimalContainer>()
        .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, MidpointRounding.ToEven) })
        .GroupBy(x => x.DecimalRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal, MidpointRounding.ToEven), x.Key)));

      Query.All<DoubleContainer>()
        .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, MidpointRounding.ToEven) })
        .GroupBy(x => x.DoubleRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double, MidpointRounding.ToEven), x.Key)));
    }

    [Test]
    public void RoundToEvenToNonZeroDigitsTest()
    {
      if (ExpectNotSupported()) {// sqlite has no support for Power operation
        var ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DecimalContainer>().Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1, MidpointRounding.ToEven) })
            .GroupBy(x => x.DecimalRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));

        ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DoubleContainer>().Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1, MidpointRounding.ToEven) })
            .GroupBy(x => x.DoubleRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));
      }
      else {
        Query.All<DecimalContainer>()
          .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1, MidpointRounding.ToEven) })
          .GroupBy(x => x.DecimalRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal, 1, MidpointRounding.ToEven), x.Key)));

        Query.All<DoubleContainer>()
          .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1, MidpointRounding.ToEven) })
          .GroupBy(x => x.DoubleRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double, 1, MidpointRounding.ToEven), x.Key)));
      }
    }

    [Test]
    public void RoundAwayFromZeroToZeroDigitsTest()
    {
      Query.All<DecimalContainer>()
        .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, MidpointRounding.AwayFromZero) })
        .GroupBy(x => x.DecimalRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal, MidpointRounding.AwayFromZero), x.Key)));

      Query.All<DoubleContainer>()
        .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, MidpointRounding.AwayFromZero) })
        .GroupBy(x => x.DoubleRound)
        .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double, MidpointRounding.AwayFromZero), x.Key)));
    }

    [Test]
    public void RoundAwayFromZeroToNonZeroDigitsTest()
    {
      if (ExpectNotSupported()) {
        var ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DecimalContainer>().Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1, MidpointRounding.AwayFromZero) })
            .GroupBy(x => x.DecimalRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));

        ex = Assert.Throws<QueryTranslationException>(() =>
          Query.All<DoubleContainer>().Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1, MidpointRounding.AwayFromZero) })
            .GroupBy(x => x.DoubleRound).Run());
        Assert.That(ex.InnerException, Is.InstanceOf<NotSupportedException>());
        Assert.That(ex.InnerException.Message.Contains("Power", StringComparison.Ordinal));
      }
      else {
        Query.All<DecimalContainer>()
          .Select(x => new { Decimal = x.d18_9, DecimalRound = Math.Round(x.d18_9, 1, MidpointRounding.AwayFromZero) })
          .GroupBy(x => x.DecimalRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Decimal, 1, MidpointRounding.AwayFromZero), x.Key)));

        Query.All<DoubleContainer>()
          .Select(x => new { Double = x.FDouble, DoubleRound = Math.Round(x.FDouble, 1, MidpointRounding.AwayFromZero) })
          .GroupBy(x => x.DoubleRound)
          .ForEach(x => x.ForEach(i => AreEqual(Math.Round(i.Double, 1, MidpointRounding.AwayFromZero), x.Key)));
      }
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      _ = CreateSessionAndTransaction();

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
        -2.01m, -2.05m, -2.10m,

        0.1343m, 0.1524m, 0.1648m,
        0.2324m, 0.2514m, 0.2659m,
        -.1341m, -.1537m, -.1682m,
        -.2332m, -.2541m, -.2612m,
        1.0101m, 1.05752m, 1.10365m,
        2.0185m, 2.0521m, 2.1075m,
        -1.0131m, -1.0584m, -1.1022m,
        -2.0196m, -2.0537m, -2.1063m,

        274486.3m, 274486.5m, 274486.6m,
        -274486.3m, -274486.5m, -274486.6m,
        72244.3m, 72244.5m, 72244.6m,
        -72244.3m, -72244.5m, -72244.6m
      };

      foreach (var value in testValues) {
        _ = new DoubleContainer { FDouble = (double) value };
        _ = new DecimalContainer { d18_9 = value };
        //_ = new X { FDouble = (double) value, FDecimal = value };
      }
    }

    public override void TestFixtureTearDown()
    {
      base.TestFixtureTearDown();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    private static void AreEqual(decimal expected, decimal actual)
    {
      if (Math.Abs(expected - actual) > DecimalDelta) {
        Assert.Fail("expected {0} actual {1}", expected, actual);
      }
    }

    private void AreEqual(double expected, double actual) => Assert.AreEqual(expected, actual, DoubleDelta);

    private bool ExpectNotSupported()
    {
      return StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Sqlite);
    }
  }
}