// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq.Interfaces
{
  [Serializable]
  public class AlphabetTest : AutoBuildTest
  {
    const int EachCount = 10;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(INamed).Assembly, typeof(INamed).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          // ClassTable
          for (int i = 0; i < EachCount; i++)
            new A() {Name = "Name: A" + i};
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new B() { Name = "Name: B" + i, Tag = "Tag: B" + i};
            named.Name = "Name: B'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new C() { Name = "Name: C" + i };
            tagged.Tag = "Tag: C'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new D() { Name = "Name: D" + i, Tag= "Tag: D" + i, First = "First: D" + i, Second = "Second: D" + i };
            named.Name = "Name: D'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new E() { Name = "Name: E" + i, Tag = "Tag: E" + i, First = "First: E" + i, Second = "Second: E" + i };
            var composite = (IComposite) named;
            named.Name = "Name: E'" + i;
            composite.First = "First: E'" + i;
          }

          // ConcreteTable
          for (int i = 0; i < EachCount; i++)
            new F() {Name = "Name: F" + i};
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new G() { Name = "Name: G" + i, Tag = "Tag: G" + i};
            named.Name = "Name: G'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new H() { Name = "Name: H" + i };
            tagged.Tag = "Tag: H'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new I() { Name = "Name: I" + i, Tag= "Tag: I" + i, First = "First: I" + i, Second = "Second: I" + i };
            named.Name = "Name: I'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new J() { Name = "Name: J" + i, Tag = "Tag: J" + i, First = "First: J" + i, Second = "Second: J" + i };
            var composite = (IComposite) named;
            named.Name = "Name: J'" + i;
            composite.First = "First: J'" + i;
          }

          // SingleTable
          for (int i = 0; i < EachCount; i++)
            new K() {Name = "Name: K" + i};
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new L() { Name = "Name: L" + i, Tag = "Tag: L" + i};
            named.Name = "Name: L'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new M() { Name = "Name: M" + i };
            tagged.Tag = "Tag: M'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new N() { Name = "Name: N" + i, Tag= "Tag: N" + i, First = "First: N" + i, Second = "Second: N" + i };
            named.Name = "Name: N'" + i;
          }
          for (int i = 0; i < EachCount; i++) {
            var named = (INamed)new O() { Name = "Name: O" + i, Tag = "Tag: O" + i, First = "First: O" + i, Second = "Second: O" + i };
            var composite = (IComposite) named;
            named.Name = "Name: O'" + i;
            composite.First = "First: O'" + i;
          }
          
          t.Complete();
        }
      }
    }

    [Test]
    public void QueryNamedTest()
    {
      var primaryIndex = Domain.Model.Types[typeof(INamed)].Indexes.PrimaryIndex;
      primaryIndex.Dump();
      var secondaryIndex = Domain.Model.Types[typeof(INamed)].Indexes.GetIndex("Name");
      secondaryIndex.Dump();

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<INamed>().ToList();
        Assert.Greater(result.Count, 0);
        foreach (var iNamed in result) {
          Assert.IsNotNull(iNamed);
          Assert.IsNotNull(iNamed.Name);
          Console.Out.WriteLine(string.Format("Key: {0}; {1}", iNamed.Key, iNamed.Name));
        }
        Assert.AreEqual(15 * EachCount, result.Count);

        var filtered = session.Query.All<INamed>().Where(i => i.Name == "Name: O'0" || i.Name == "Name: A0" || i.Name == "Name: C0").ToList();
        Assert.AreEqual(3, filtered.Count);

        var startsWith = session.Query.All<INamed>().Where(i => i.Name.StartsWith("Name: J'") || i.Name.StartsWith("Name: L'") || i.Name.StartsWith("Name: C'")).ToList();
        Assert.AreEqual(2 * EachCount, startsWith.Count);

        t.Complete();
      }
    }

    [Test]
    public void QueryTaggedTest()
    {
      var index = Domain.Model.Types[typeof(ITagged)].Indexes.PrimaryIndex;
      index.Dump();

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<ITagged>().ToList();
        Assert.Greater(result.Count, 0);
        foreach (var iTagged in result) {
          Assert.IsNotNull(iTagged);
          Assert.IsNotNull(iTagged.Tag);
          Console.Out.WriteLine(string.Format("Key: {0}; {1}", iTagged.Key, iTagged.Tag));
        }
        Assert.AreEqual(12 * EachCount, result.Count);

        var filtered = session.Query.All<ITagged>().Where(i => i.Tag == "Tag: C'0" || i.Tag == "Tag: D0" || i.Tag == "Tag: M'0").ToList();
        Assert.AreEqual(3, filtered.Count);

        var startsWith = session.Query.All<ITagged>().Where(i => i.Tag.StartsWith("Tag: H'") || i.Tag.StartsWith("Tag: C'") || i.Tag.StartsWith("Tag: J'")).ToList();
        Assert.AreEqual(2 * EachCount, startsWith.Count);

        t.Complete();
      }
    }


    [Test]
    public void QueryCompositeTest()
    {
      var index = Domain.Model.Types[typeof(IComposite)].Indexes.PrimaryIndex;
      index.Dump();

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var result = session.Query.All<IComposite>().ToList();
        Assert.Greater(result.Count, 0);
        foreach (var iComposite in result) {
          Assert.IsNotNull(iComposite);
          Assert.IsNotNull(iComposite.First);
          Assert.IsNotNull(iComposite.Second);
          Console.Out.WriteLine(string.Format("Key: {0}; {1}; {2}", iComposite.Key, iComposite.First, iComposite.Second));
        }
        Assert.AreEqual(6 * EachCount, result.Count);

        var filtered = session.Query.All<IComposite>().Where(i => i.First == "First: O'0" || i.Second == "Second: O0" || i.First == "First: E'0").ToList();
        Assert.AreEqual(2, filtered.Count);

        var startsWith = session.Query.All<IComposite>().Where(i => i.First.StartsWith("First: O'") || i.Second.StartsWith("Second: O") || i.First.StartsWith("First: E'")).ToList();
        Assert.AreEqual(2 * EachCount, startsWith.Count);

        t.Complete();
      }
    }

    [Test]
    public void FetchTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var named = session.Query.Single<INamed>(33L);
        Assert.IsNotNull(named);
        Assert.AreEqual("Name: D'2", named.Name);
        named = session.Query.Single<INamed>(Key.Create<INamed>(51L));
        Assert.IsNotNull(named);
        Assert.AreEqual("Name: F0", named.Name);
        t.Complete();
      }

      const int totalCount = EachCount * 15;
      var names = new List<string>(totalCount);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (long i = 1; i <= totalCount; i++) {
          var key = Key.Create<INamed>(i);
          var named = session.Query.Single<INamed>(key);
          Assert.IsNotNull(named);
          Assert.IsNotNull(named.Name);
          names.Add(named.Name);
        }
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var namedQuery = session.Query.All<INamed>()
          .Select(i => i.Name)
          .OrderBy(i=>i)
          .ToList();
        Assert.AreEqual(totalCount, namedQuery.Count);
        Assert.IsTrue(namedQuery.SequenceEqual(names));
        t.Complete();
      }
    }
  }
}