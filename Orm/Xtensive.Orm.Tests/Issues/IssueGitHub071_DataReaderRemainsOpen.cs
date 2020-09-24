// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGitHub071_DataReaderRemainsOpenModel;

namespace Xtensive.Orm.Tests.Issues.IssueGitHub071_DataReaderRemainsOpenModel
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Item : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    public Item(Session session, Guid id)
      : base(session, id)
    { }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueGitHub071_DataReaderRemainsOpen : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Item));
      return config;
    }

    [Test]
    public void ConcatOfEmptySequencesTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var list = new List<Guid>();
        var sequence = Enumerable.Empty<Guid>().Concat(session.Query.All<Item>()
          .Where(item => item.Id == Guid.Empty)
          .Select(item => item.Id));
        foreach (var guid in sequence) {
          list.Add(guid);
        }

        Assert.AreEqual(0, list.Count);
        tx.Complete();
      }
    }

    [Test]
    public void ConcatOfEmptySequencesTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var list = new List<Guid>();
        var sequence = session.Query.All<Item>()
          .Where(item => item.Id == Guid.Empty)
          .Select(item => item.Id).AsEnumerable().Concat(Enumerable.Empty<Guid>());
        foreach (var guid in sequence) {
          list.Add(guid);
        }

        Assert.AreEqual(0, list.Count);
        tx.Complete();
      }
    }

    [Test]
    public void UnionOfEmptySequencesTest1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var list = new List<Guid>();
        var sequence = Enumerable.Empty<Guid>().Union(session.Query.All<Item>()
          .Where(item => item.Id == Guid.Empty)
          .Select(item => item.Id));
        foreach (var guid in sequence) {
          list.Add(guid);
        }

        Assert.AreEqual(0, list.Count);
        tx.Complete();
      }
    }

    [Test]
    public void UnionOfEmptySequencesTest2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var list = new List<Guid>();
        var sequence = session.Query.All<Item>()
          .Where(item => item.Id == Guid.Empty)
          .Select(item => item.Id).AsEnumerable().Union(Enumerable.Empty<Guid>());
        foreach (var guid in sequence) {
          list.Add(guid);
        }

        Assert.AreEqual(0, list.Count);
        tx.Complete();
      }
    }

    [Test]
    public void JustEmptyQueryResult()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var list = new List<Guid>();
        var sequence = session.Query.All<Item>()
          .Where(item => item.Id == Guid.Empty)
          .Select(item => item.Id);
        foreach (var guid in sequence) {
          list.Add(guid);
        }

        Assert.AreEqual(0, list.Count);
        tx.Complete();
      }
    }
  }
}
