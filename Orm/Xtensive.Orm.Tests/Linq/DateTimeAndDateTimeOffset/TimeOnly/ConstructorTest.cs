// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class ConstructorTest : DateTimeBaseTest
  {
    protected override void CheckRequirements() => Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Sqlite);

    [Test]
    public void CtorHMSM()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedTime = new TimeOnly(e.Hour, e.Minute, e.Second, e.Millisecond) })
          .Where(a => a.ConstructedTime == FirstMillisecondTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorHMS()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedTime = new TimeOnly(e.Hour, e.Minute, e.Second)
          })
          .Where(a => a.ConstructedTime == FirstTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorHM()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new { Entity = e, ConstructedTime = new TimeOnly(e.Hour, e.Minute) })
          .Where(a => a.ConstructedTime == FirstTimeOnly.Add(TimeSpan.FromSeconds(-5)))
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorTicksLiteralValue()
    {
      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var ticksPerMinute = new TimeOnly(0, 1).Ticks;
      var ticksPerSecond = new TimeOnly(0, 0, 1).Ticks;
      var testTicks = ticksPerHour * FirstTimeOnly.Hour +
        ticksPerMinute * FirstTimeOnly.Minute +
        ticksPerSecond * FirstTimeOnly.Second;

      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new { Entity = e, ConstructedTime = new TimeOnly(testTicks) })
          .Where(a => a.ConstructedTime == FirstTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorTicksLiteralExpressions()
    {
      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var ticksPerMinute = new TimeOnly(0, 1).Ticks;
      var ticksPerSecond = new TimeOnly(0, 0, 1).Ticks;
      var testTicks = ticksPerHour * FirstTimeOnly.Hour +
        ticksPerMinute * FirstTimeOnly.Minute +
        ticksPerSecond * FirstTimeOnly.Second;

      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new { Entity = e, ConstructedTime = new TimeOnly(testTicks + 1000 - 1000) })
          .Where(a => a.ConstructedTime == FirstTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorTicksFromIntervalTicks()
    {
      Require.ProviderIsNot(StorageProvider.MySql);

      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new { Entity = e, ConstructedTime = new TimeOnly(e.TimeSpan.Ticks) })
          .Where(a => a.ConstructedTime == FirstMillisecondTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void TicksFromColumnsBasedExpression()
    {
      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var ticksPerMinute = new TimeOnly(0, 1).Ticks;
      var ticksPerSecond = new TimeOnly(0, 0, 1).Ticks;
      var testTicks = ticksPerHour * FirstTimeOnly.Hour +
        ticksPerMinute * FirstTimeOnly.Minute +
        ticksPerSecond * FirstTimeOnly.Second;

      Assert.That(new TimeOnly(testTicks).Ticks, Is.EqualTo(testTicks));
      Assert.That(new TimeOnly(testTicks), Is.EqualTo(FirstTimeOnly));

      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedTime = new TimeOnly(e.Hour * ticksPerHour + e.Minute * ticksPerMinute + e.Second * ticksPerSecond)
          })
          .Where(a => a.ConstructedTime == FirstTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void TicksFromColumnsBasedExpressionMilliseconds()
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.MySql)) {
        Require.ProviderVersionAtLeast(StorageProviderVersion.MySql56);
      }

      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var ticksPerMinute = new TimeOnly(0, 1).Ticks;
      var ticksPerSecond = new TimeOnly(0, 0, 1).Ticks;
      var ticksPerMillisecond = new TimeOnly(0,0,0,1).Ticks;
      var testTicks = ticksPerHour * FirstMillisecondTimeOnly.Hour +
        ticksPerMinute * FirstMillisecondTimeOnly.Minute +
        ticksPerSecond * FirstMillisecondTimeOnly.Second +
        ticksPerMillisecond * FirstMillisecondTimeOnly.Millisecond;

      Assert.That(new TimeOnly(testTicks).Ticks, Is.EqualTo(testTicks));
      Assert.That(new TimeOnly(testTicks), Is.EqualTo(FirstMillisecondTimeOnly));

      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedTime = new TimeOnly(
              e.Hour * ticksPerHour + e.Minute * ticksPerMinute + e.Second * ticksPerSecond + e.Millisecond * ticksPerMillisecond)
          })
          .Where(a => a.ConstructedTime == FirstMillisecondTimeOnly)
          .OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void HourOverflowTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Sqlite,
        "These providers don't throw exceptions on hour value overflow but return NULL or Max possible value, so no support for time constructor");

      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var testTicks = FirstMillisecondTimeOnly.Hour + ticksPerHour * 25;

      _ = Assert.Throws<ArgumentOutOfRangeException>(() => new TimeOnly(testTicks));

      ExecuteInsideSession((s) => {
        _ = Assert.Throws(GetExceptionType(),
          () => s.Query.All<AllPossiblePartsEntity>()
            .Select(e => new {
              Entity = e,
              ConstructedTime = new TimeOnly(e.Hour + ticksPerHour * 25)
            })
            .Where(a => a.ConstructedTime == FirstMillisecondTimeOnly)
            .OrderBy(a => a.Entity.Id).Run());
      });

      static Type GetExceptionType()
      {
        return StorageProviderInfo.Instance.Provider switch {
          StorageProvider.SqlServer => typeof(SyntaxErrorException),
          _ => typeof(StorageException)
        };
      }
    }
  }
}