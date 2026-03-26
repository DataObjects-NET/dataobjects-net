// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class ConstructorTest : DateTimeBaseTest
  {
    [Test]
    public void CtorYMDHMSM()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedDate = new DateTime(e.Year, e.Month, e.Day, e.Hour, e.Minute, e.Second, e.Millisecond) })
          .Where(a => a.ConstructedDate == FirstMillisecondDateTime).OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorYMDHMS()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new {
            Entity = e,
            ConstructedDate = new DateTime(e.Year, e.Month, e.Day, e.Hour, e.Minute, e.Second)
          })
          .Where(a => a.ConstructedDate == FirstDateTime).OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }

    [Test]
    public void CtorYMD()
    {
      ExecuteInsideSession((s) => {
        var result = s.Query.All<AllPossiblePartsEntity>()
          .Select(e => new { Entity = e, ConstructedDate = new DateTime(e.Year, e.Month, e.Day) })
          .Where(a => a.ConstructedDate == FirstDateTime.Date).OrderBy(a => a.Entity.Id).ToList(3);
        Assert.That(result.Count, Is.EqualTo(1));
      });
    }
  }
}
