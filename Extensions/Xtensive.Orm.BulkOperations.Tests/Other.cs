// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Ovchinnikov
// Created:    2012.02.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TestCommon.Model;

namespace Xtensive.Orm.BulkOperations.Tests
{
  internal class Other : BulkOperationBaseTest
  {
    [Test]
    public void CompositeKeyUpdate()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var date1 = FixDateTimeForPK(DateTime.Now);
        var date2 = FixDateTimeForPK(DateTime.Now.AddDays(1));
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var foo1 = new Bar2(session, date1, id1) { Name = "test" };
        var foo2 = new Bar2(session, date2, id1);
        var foo3 = new Bar2(session, date2, id2) { Name = "test" };
        var updated = session.Query.All<Bar2>().Where(a => a.Name == "test").Set(a => a.Name, "abccba").Update();
        Assert.That(updated, Is.EqualTo(2));
        Assert.That(foo1.Name, Is.EqualTo("abccba"));
        Assert.That(foo3.Name, Is.EqualTo("abccba"));
        Assert.That(foo2.Name, Is.Null);
        trx.Complete();
      }
    }

    [Test]
    public async Task CompositeKeyUpdateAsync()
    {
      await using (var session = await Domain.OpenSessionAsync())
      await using (var trx = session.OpenTransaction()) {
        var date1 = FixDateTimeForPK(DateTime.Now);
        var date2 = FixDateTimeForPK(DateTime.Now.AddDays(1));
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var foo1 = new Bar2(session, date1, id1) { Name = "test" };
        var foo2 = new Bar2(session, date2, id1);
        var foo3 = new Bar2(session, date2, id2) { Name = "test" };
        var updated = await session.Query.All<Bar2>().Where(a => a.Name == "test").Set(a => a.Name, "abccba").UpdateAsync();
        Assert.That(updated, Is.EqualTo(2));
        Assert.That(foo1.Name, Is.EqualTo("abccba"));
        Assert.That(foo3.Name, Is.EqualTo("abccba"));
        Assert.That(foo2.Name, Is.Null);
        trx.Complete();
      }
    }

    [Test]
    public void SimpleDelete()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var bar3 = new Bar(session);
        _ = bar3.Foo.Add(new Foo(session) { Name = "Foo" });
        var s = "test";

        var deleted = session.Query.All<Bar>().Where(a => a.Name == s).Delete();
        Assert.That(bar1.IsRemoved, Is.True);
        Assert.That(bar2.IsRemoved, Is.False);
        Assert.That(bar3.IsRemoved, Is.False);
        Assert.That(deleted, Is.EqualTo(1));

        _ = session.Query.All<Bar>().Where(a => a.Foo.Any(b => b.Name == "Foo")).Update(a => new Bar(null) { Name = "" });
        deleted = session.Query.All<Bar>().Where(a => a.Foo.Count(b => b.Name == "Foo") == 0).Delete();
        Assert.That(bar2.IsRemoved, Is.True);
        Assert.That(bar3.IsRemoved, Is.False);
        Assert.That(deleted, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public async Task SimpleDeleteAsync()
    {
      await using (var session = await Domain.OpenSessionAsync())
      await using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var bar3 = new Bar(session);
        _ = bar3.Foo.Add(new Foo(session) { Name = "Foo" });
        var s = "test";

        var deleted = await session.Query.All<Bar>().Where(a => a.Name == s).DeleteAsync();
        Assert.That(bar1.IsRemoved, Is.True);
        Assert.That(bar2.IsRemoved, Is.False);
        Assert.That(bar3.IsRemoved, Is.False);
        Assert.That(deleted, Is.EqualTo(1));

        _ = await session.Query.All<Bar>()
          .Where(a => a.Foo.Any(b => b.Name == "Foo"))
          .UpdateAsync(a => new Bar(null) { Name = "" });

        deleted = await session.Query.All<Bar>().Where(a => a.Foo.Count(b => b.Name == "Foo") == 0).DeleteAsync();
        Assert.That(bar2.IsRemoved, Is.True);
        Assert.That(bar3.IsRemoved, Is.False);
        Assert.That(deleted, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public void SimpleInsert()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var s1 = "abccba";
        var i = 5;
        var key =
          session.Query.Insert(() => new Bar(session) { Name = "test1" + s1, Count = i * 2 + 1, Description = null });
        Assert.That(key, Is.EqualTo(Key.Create<Bar>(session.Domain, 1)));
        var bar = session.Query.Single<Bar>(key);
        Assert.That(bar.Name, Is.EqualTo("test1abccba"));
        Assert.That(bar.Count, Is.EqualTo(11));
        Assert.That(bar.Description, Is.Null);

        key =
          session.Query.Insert(
            () => new Bar(session) { Id = 100, Name = "test" + s1, Count = i * 2 + 1, Description = null });
        Assert.That(key, Is.EqualTo(Key.Create<Bar>(session.Domain, 100)));
        bar = session.Query.Single<Bar>(key);
        Assert.That(bar.Name, Is.EqualTo("testabccba"));
        Assert.That(bar.Count, Is.EqualTo(11));
        Assert.That(bar.Description, Is.Null);

        trx.Complete();
      }
    }

    [Test]
    public async Task SimpleInsertAsync()
    {
      await using (var session = await Domain.OpenSessionAsync())
      await using (var trx = session.OpenTransaction()) {
        var s1 = "abccba";
        var i = 5;
        var key = await session.Query.InsertAsync(
          () => new Bar(session) { Name = "test1" + s1, Count = i * 2 + 1, Description = null });
        Assert.That(key, Is.EqualTo(Key.Create<Bar>(session.Domain, 1)));
        var bar = session.Query.Single<Bar>(key);
        Assert.That(bar.Name, Is.EqualTo("test1abccba"));
        Assert.That(bar.Count, Is.EqualTo(11));
        Assert.That(bar.Description, Is.Null);

        key = await session.Query.InsertAsync(
          () => new Bar(session) { Id = 100, Name = "test" + s1, Count = i * 2 + 1, Description = null });
        Assert.That(key, Is.EqualTo(Key.Create<Bar>(session.Domain, 100)));
        bar = session.Query.Single<Bar>(key);
        Assert.That(bar.Name, Is.EqualTo("testabccba"));
        Assert.That(bar.Count, Is.EqualTo(11));
        Assert.That(bar.Description, Is.Null);

        trx.Complete();
      }
    }

    [Test]
    public void SimpleUpdate()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var s = "test";
        var s1 = "abccba";
        var updated =
          session.Query.All<Bar>().Where(a => a.Name.Contains(s)).Update(
            a => new Bar(session) { Name = a.Name + s1, Count = a.Count * 2, Description = null });
        Assert.That(bar1.Name, Is.EqualTo("testabccba"));
        Assert.That(bar1.Description, Is.Null);
        Assert.That(bar1.Count, Is.EqualTo(6));
        Assert.That(updated, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public async Task SimpleUpdateAsync()
    {
      await using (var session = await Domain.OpenSessionAsync())
      await using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var s = "test";
        var s1 = "abccba";
        var updated = await session.Query.All<Bar>()
          .Where(a => a.Name.Contains(s))
          .UpdateAsync(a => new Bar(session) { Name = a.Name + s1, Count = a.Count * 2, Description = null });
        Assert.That(bar1.Name, Is.EqualTo("testabccba"));
        Assert.That(bar1.Description, Is.Null);
        Assert.That(bar1.Count, Is.EqualTo(6));
        Assert.That(updated, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public void SubqueryInsert()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        _ = new Foo(session) { Name = "test" };
        _ = new Bar(session) { Name = "test1" };
        session.SaveChanges();
        Key key = null;
        session.AssertCommandCount(
          Is.EqualTo(1),
          () => { key = session.Query.Insert(() => new Bar(session) { Count = session.Query.All<Bar>().Max(b => b.Count) }); });
        var bar = session.Query.Single<Bar>(key);
        Assert.That(bar.Count, Is.EqualTo(0));
        trx.Complete();
      }
    }

    [Test]
    public void SubqueryUpdate1()
    {
      CheckSetFromSelectSupported();
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar = new Bar(session);
        var bar2 = new Bar(session) { Count = 1 };
        _ = new Foo(session) { Bar = bar, Name = "test" };
        _ = new Foo(session) { Bar = bar, Name = "test1" };
        _ = session.Query.All<Bar>().Where(a => a.Count == a.Foo.Count - 2).Set(a => a.Count, a => a.Foo.Count).Update();
        Assert.That(bar.Count, Is.EqualTo(2));

        session.AssertCommandCount(
          Is.EqualTo(1),
          () => session.Query.All<Bar>().Where(a => a.Count == session.Query.All<Bar>().Max(b => b.Count) + 1).Set(a => a.Count, a => session.Query.All<Bar>().Min(b => b.Count)).Update());
        Assert.That(bar.Count, Is.EqualTo(2));

        session.AssertCommandCount(
          Is.EqualTo(1),
          () => session.Query.All<Bar>().Where(a => a.Count == session.Query.All<Bar>().Max(b => b.Count) + 1).Set(a => a.Count, a => session.Query.All<Foo>().Count()).Update());

        trx.Complete();
      }
      
    }

    [Test]
    public void SubqueryUpdate2()
    {
      CheckSetFromSelectUnsupported();
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar = new Bar(session);
        var bar2 = new Bar(session) { Count = 1 };
        _ = new Foo(session) { Bar = bar, Name = "test" };
        _ = new Foo(session) { Bar = bar, Name = "test1" };
        _ = Assert.Throws<StorageException>(
          () => session.Query.All<Bar>().Where(a => a.Count == session.Query.All<Bar>().Max(b => b.Count) + 1)
            .Set(a => a.Count, a => session.Query.All<Bar>().Min(b => b.Count))
            .Update());

        trx.Complete();
      }
    }

    [Test]
    public void UpdateViaSet1()
    {
      CheckGroupSetApplication();
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var s = "test";
        var s1 = "abccba";
        var updated = session.Query.All<Bar>().Where(a => a.Name.Contains(s))
          .Set(a => a.Name, s1)
          .Set(a => a.Count, a => a.Count * 2)
          .Set(a => a.Description, a => a.Name + s1)
          .Update();
        Assert.That(bar1.Name, Is.EqualTo("abccba"));
        Assert.That(bar1.Description, Is.EqualTo("testabccba"));
        Assert.That(bar1.Count, Is.EqualTo(6));
        Assert.That(updated, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public void UpdateViaSet2()
    {
      CheckOneByOneSetApplication();
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session) { Name = "test", Count = 3 };
        var bar2 = new Bar(session);
        var s = "test";
        var s1 = "abccba";
        var updated =
          session.Query.All<Bar>().Where(a => a.Name.Contains(s)).Set(a => a.Name, s1).Set(
            a => a.Count, a => a.Count * 2).Set(a => a.Description, a => a.Name + s1).Update();
        Assert.That(bar1.Name, Is.EqualTo("abccba"));
        Assert.That(bar1.Description, Is.EqualTo("abccbaabccba"));
        Assert.That(bar1.Count, Is.EqualTo(6));
        Assert.That(updated, Is.EqualTo(1));
        trx.Complete();
      }
    }

    [Test]
    public void UpdateWithReferenceToUpdatingEntity()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var foo1 = new Foo(session) {Name = "Test"};
        var foo2 = new Foo(session);
        var foo3 = new Foo(session) {Name = "Test1"};
        var bar1 = new Bar(session) {Name = "Test"};
        var bar2 = new Bar(session);
        _ = Assert.Throws<NotSupportedException>(()=>session.Query.All<Foo>().Set(a => a.Bar, a => session.Query.All<Bar>().First(b => b.Name==a.Name)).Update());
        Assert.That(foo1.Bar, Is.Null);
        Assert.That(foo2.Bar, Is.Null);
        Assert.That(foo3.Bar, Is.Null);
        trx.Complete();
      }
    }
    [Test]
    public void In()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var bar1 = new Bar(session, 1);
        var bar3 = new Bar(session, 3);
        var bar5 = new Bar(session, 5);
        var bar6 = new Bar(session, 6);
        var list = new List<int>() {1, 2, 3, 4, 5};

        _ = session.Query.All<Bar>()
          .Where(a => a.Id.In(IncludeAlgorithm.Auto, list))
          .Set(a => a.Count, 2)
          .Update();
        Assert.That(bar1.Count, Is.EqualTo(2));
        Assert.That(bar3.Count, Is.EqualTo(2));
        Assert.That(bar5.Count, Is.EqualTo(2));
        Assert.That(bar6.Count, Is.EqualTo(0));

        _ = session.Query.All<Bar>()
          .Where(a => a.Id.In(IncludeAlgorithm.ComplexCondition, list))
          .Set(a => a.Count, 3)
          .Update();
        Assert.That(bar1.Count, Is.EqualTo(3));
        Assert.That(bar3.Count, Is.EqualTo(3));
        Assert.That(bar5.Count, Is.EqualTo(3));
        Assert.That(bar6.Count, Is.EqualTo(0));

        _ = session.Query.All<Bar>()
          .Where(a => a.Id.In(list))
          .Set(a => a.Count, 4)
          .Update();
        Assert.That(bar1.Count, Is.EqualTo(4));
        Assert.That(bar3.Count, Is.EqualTo(4));
        Assert.That(bar5.Count, Is.EqualTo(4));
        Assert.That(bar6.Count, Is.EqualTo(0));

        _ = session.Query.All<Bar>()
          .Where(a => a.Id.In(1, 2, 3, 4, 5))
          .Set(a => a.Count, 5)
          .Update();
        Assert.That(bar1.Count, Is.EqualTo(5));
        Assert.That(bar3.Count, Is.EqualTo(5));
        Assert.That(bar5.Count, Is.EqualTo(5));
        Assert.That(bar6.Count, Is.EqualTo(0));

        trx.Complete();
      }
    }

    [Test]
    public void InWithCombinationWithFieldUsageUpdate()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {
        var idsToUpdate = new[] { 99, 100, 102 };
        var prefix = "abc";

        var bar1 = new Bar(session, 100) { Name = "test1", Count = 3 };
        var bar2 = new Bar(session, 101) { Name = "test2", Count = 4 };
        var bar3 = new Bar(session, 102) { Name = "test3", Count = 5 };
        session.SaveChanges();

        var updatedCount =  session.Query.All<Bar>()
          .Where(b => b.Id.In(IncludeAlgorithm.ComplexCondition, idsToUpdate))
          .Update(bar => new Bar(session) { Name = prefix + bar.Name });
        Assert.That(updatedCount, Is.EqualTo(2));

        var all = session.Query.All<Bar>().Where(b=> b.Id == 100 || b.Id == 101 || b.Id == 102).ToList();
        var updatedEntities = all.Where(b => b.Id.In(idsToUpdate));
        Assert.That(updatedEntities.All(e => e.Name.StartsWith(prefix)), Is.True);

        var leftEntities = all.Where(b => !b.Id.In(idsToUpdate));
        Assert.That(leftEntities.All(e => e.Name.StartsWith(prefix)), Is.False);

        trx.Complete();
      }
    }

    private void CheckSetFromSelectSupported()
    {
      if (Domain.StorageProviderInfo.ProviderName == WellKnown.Provider.MySql) {
        throw new IgnoreException("Test is inconclusive for current provider");
      }
    }

    private void CheckSetFromSelectUnsupported()
    {
      if (Domain.StorageProviderInfo.ProviderName != WellKnown.Provider.MySql) {
        throw new IgnoreException("Test is inconclusive for current provider");
      }
    }

    private void CheckOneByOneSetApplication()
    {
      if (Domain.StorageProviderInfo.ProviderName != WellKnown.Provider.MySql) {
        throw new IgnoreException("Test is inconclusive for current provider");
      }
    }

    private void CheckGroupSetApplication()
    {
      if (Domain.StorageProviderInfo.ProviderName == WellKnown.Provider.MySql) {
        throw new IgnoreException("Test is inconclusive for current provider");
      }
    }

    private DateTime FixDateTimeForPK(DateTime origin)
    {
      var currentProvider = Domain.StorageProviderInfo.ProviderName;

      long? divider;
      switch (currentProvider) {
        case WellKnown.Provider.MySql:
          divider = 10000000;
          break;
        case WellKnown.Provider.Firebird:
          divider = 1000;
          break;
        case WellKnown.Provider.PostgreSql:
          divider = 10;
          break;
        default:
          divider = (long?) null;
          break;
      }

      if (!divider.HasValue) {
        return origin;
      }
      var ticks = origin.Ticks;
      var newTicks = ticks - (ticks % divider.Value);
      return new DateTime(newTicks);
    }
  }
}
