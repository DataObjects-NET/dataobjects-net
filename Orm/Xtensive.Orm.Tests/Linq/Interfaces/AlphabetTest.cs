// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;

namespace Xtensive.Orm.Tests.Linq.Interfaces
{
  [Serializable]
  public class AlphabetTest : AutoBuildTest
  {
    const int EachCount = 10;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(INamed).Assembly, typeof(INamed).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          // ClassTable
          for (var i = 0; i < EachCount; i++) {
            _ = new A() {Name = "Name: A" + i};
          }

          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new B() { Name = "Name: B" + i, Tag = "Tag: B" + i};
            named.Name = "Name: B'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new C() { Name = "Name: C" + i };
            tagged.Tag = "Tag: C'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new D() { Name = "Name: D" + i, Tag= "Tag: D" + i, First = "First: D" + i, Second = "Second: D" + i };
            named.Name = "Name: D'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new E() { Name = "Name: E" + i, Tag = "Tag: E" + i, First = "First: E" + i, Second = "Second: E" + i };
            var composite = (IComposite) named;
            named.Name = "Name: E'" + i;
            composite.First = "First: E'" + i;
          }

          // ConcreteTable
          for (var i = 0; i < EachCount; i++) {
            _ = new F() {Name = "Name: F" + i};
          }

          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new G() { Name = "Name: G" + i, Tag = "Tag: G" + i};
            named.Name = "Name: G'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new H() { Name = "Name: H" + i };
            tagged.Tag = "Tag: H'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new I() { Name = "Name: I" + i, Tag= "Tag: I" + i, First = "First: I" + i, Second = "Second: I" + i };
            named.Name = "Name: I'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new J() { Name = "Name: J" + i, Tag = "Tag: J" + i, First = "First: J" + i, Second = "Second: J" + i };
            var composite = (IComposite) named;
            named.Name = "Name: J'" + i;
            composite.First = "First: J'" + i;
          }

          // SingleTable
          for (var i = 0; i < EachCount; i++) {
            _ = new K() {Name = "Name: K" + i};
          }

          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new L() { Name = "Name: L" + i, Tag = "Tag: L" + i};
            named.Name = "Name: L'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var tagged = (ITagged)new M() { Name = "Name: M" + i };
            tagged.Tag = "Tag: M'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
            var named = (INamed)new N() { Name = "Name: N" + i, Tag= "Tag: N" + i, First = "First: N" + i, Second = "Second: N" + i };
            named.Name = "Name: N'" + i;
          }
          for (var i = 0; i < EachCount; i++) {
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
        Assert.That(result.Count, Is.GreaterThan(0));
        foreach (var iNamed in result) {
          Assert.That(iNamed, Is.Not.Null);
          Assert.That(iNamed.Name, Is.Not.Null);
          Console.Out.WriteLine($"Key: {iNamed.Key}; {iNamed.Name}");
        }
        Assert.That(result.Count, Is.EqualTo(17 * EachCount));

        var filtered = session.Query.All<INamed>().Where(i => i.Name == "Name: O'0" || i.Name == "Name: A0" || i.Name == "Name: C0").ToList();
        Assert.That(filtered.Count, Is.EqualTo(3));

        var startsWith = session.Query.All<INamed>().Where(i => i.Name.StartsWith("Name: J'") || i.Name.StartsWith("Name: L'") || i.Name.StartsWith("Name: C'")).ToList();
        Assert.That(startsWith.Count, Is.EqualTo(2 * EachCount));

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
        Assert.That(result.Count, Is.GreaterThan(0));
        foreach (var iTagged in result) {
          Assert.That(iTagged, Is.Not.Null);
          Assert.That(iTagged.Tag, Is.Not.Null);
          Console.Out.WriteLine($"Key: {iTagged.Key}; {iTagged.Tag}");
        }
        Assert.That(result.Count, Is.EqualTo(12 * EachCount));

        var filtered = session.Query.All<ITagged>().Where(i => i.Tag == "Tag: C'0" || i.Tag == "Tag: D0" || i.Tag == "Tag: M'0").ToList();
        Assert.That(filtered.Count, Is.EqualTo(3));

        var startsWith = session.Query.All<ITagged>().Where(i => i.Tag.StartsWith("Tag: H'") || i.Tag.StartsWith("Tag: C'") || i.Tag.StartsWith("Tag: J'")).ToList();
        Assert.That(startsWith.Count, Is.EqualTo(2 * EachCount));

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
        Assert.That(result.Count, Is.GreaterThan(0));
        foreach (var iComposite in result) {
          Assert.That(iComposite, Is.Not.Null);
          Assert.That(iComposite.First, Is.Not.Null);
          Assert.That(iComposite.Second, Is.Not.Null);
          Console.Out.WriteLine($"Key: {iComposite.Key}; {iComposite.First}; {iComposite.Second}");
        }
        Assert.That(result.Count, Is.EqualTo(6 * EachCount));

        var filtered = session.Query.All<IComposite>().Where(i => i.First == "First: O'0" || i.Second == "Second: O0" || i.First == "First: E'0").ToList();
        Assert.That(filtered.Count, Is.EqualTo(2));

        var startsWith = session.Query.All<IComposite>().Where(i => i.First.StartsWith("First: O'") || i.Second.StartsWith("Second: O") || i.First.StartsWith("First: E'")).ToList();
        Assert.That(startsWith.Count, Is.EqualTo(2 * EachCount));

        t.Complete();
      }
    }

    [Test]
    public void FetchTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.Sequences);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var named = session.Query.Single<INamed>(33L);
        Assert.That(named, Is.Not.Null);
        Assert.That(named.Name, Is.EqualTo("Name: D'2"));
        named = session.Query.Single<INamed>(Key.Create<INamed>(Domain, 51L));
        Assert.That(named, Is.Not.Null);
        Assert.That(named.Name, Is.EqualTo("Name: F0"));
        t.Complete();
      }

      const int totalCount = EachCount * 15;
      var names = new List<string>(totalCount);

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (long i = 1; i <= totalCount; i++) {
          var key = Key.Create<INamed>(Domain, i);
          var named = session.Query.Single<INamed>(key);
          Assert.That(named, Is.Not.Null);
          Assert.That(named.Name, Is.Not.Null);
          names.Add(named.Name);
        }
        t.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var key = Key.Create<INamed>(Domain, 41L);
        var namedQuery = session.Query.All<INamed>()
          .Select(i => new { i.Key, i.Name})
          .OrderBy(i => i.Name)
          .ToList(170);
        Assert.That(namedQuery.Count, Is.EqualTo(170)); // some records are represented twice

        var groupByKey = namedQuery.GroupBy(i => i.Key, (a, b)=> new { Key = a, Items = b.ToList(2) }).ToList(150);
        Assert.That(groupByKey.Count, Is.EqualTo(totalCount));

        var abc = names.Zip(groupByKey,
          (name, group) => new {
            name1 = name,
            name2 = group.Items.Count == 1
              ? group.Items[0].Name
              : (group.Items[0].Name == name)
                ? group.Items[0].Name
                : group.Items[1].Name });

        foreach(var anon in abc) {
          Assert.That(anon.name2, Is.EqualTo(anon.name1));
        }

        t.Complete();
      }
    }
  }
}