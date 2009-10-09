// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.24

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.Interfaces.Alphabet;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq.Interfaces
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
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
    public void INamedTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var result = Query<INamed>.All.ToList();
        var index = Domain.Model.Types[typeof (INamed)].Indexes.PrimaryIndex;
        index.Dump();
        Assert.Greater(result.Count, 0);
        foreach (var iNamed in result) {
          Assert.IsNotNull(iNamed);
//          Assert.IsNotNull(iNamed.Name);
          Console.Out.WriteLine(string.Format("Key: {0}; Name: {1}", iNamed.Key, iNamed.Name));
        }
        Assert.AreEqual(15 * EachCount, result.Count);

        var filtered = Query<INamed>.All.Where(i => i.Name == "Name: O'0" || i.Name == "Name: A0" || i.Name == "Name: C0").ToList();
        Assert.AreEqual(3, filtered.Count);
        t.Complete();
      }
    }

  }
}