// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER //DO_DATEONLY

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class ConstructorTest : DateTimeBaseTest
  {
    [Test]
    public void CtorHMSM()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedTime = new TimeOnly(e.Hour, e.Minute, e.Second, e.Millisecond) })
          .Where(a => a.ConstructedTime == FirstMillisecondTimeOnly).OrderBy(a => a.Entity.Id).ToList(3);
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
          .Where(a => a.ConstructedTime == FirstTimeOnly).OrderBy(a => a.Entity.Id).ToList(3);
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
  }
}
#endif