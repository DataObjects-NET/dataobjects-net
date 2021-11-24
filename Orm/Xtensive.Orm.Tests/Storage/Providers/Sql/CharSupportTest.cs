// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.20

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Tests.Storage.Providers.Sql.CharSupportTestModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql.CharSupportTestModel
{
  [Serializable]
  [HierarchyRoot]
  class MyEntity : Entity
  {
    [Field, Key]
    public int Id {get; private set;}

    [Field]
    public char Char {get; set;}

    [Field]
    public char? MaybeChar { get; set;}
  }
}

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  public class CharSupportTest : AutoBuildTest
  {
    private string charColumn;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      charColumn = Domain.Model.Types[typeof(MyEntity)].Fields["Char"].Column.Name;

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new MyEntity {Char = 'X'};
        new MyEntity {Char = 'Y'};
        new MyEntity {Char = 'Z'};
        t.Complete();
      }
    }

    [Test]
    public void SelectCharTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var rs = GetRseQuery<MyEntity>();
        var parameterContext = new ParameterContext();
        var result = rs
          .Select(new[] { rs.Header.IndexOf(charColumn) })
          .GetRecordSetReader(Session.Current, parameterContext)
          .ToEnumerable()
          .Select(i => i.GetValueOrDefault<char>(0))
          .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains('X'));
        Assert.IsTrue(result.Contains('Y'));
        Assert.IsTrue(result.Contains('Z'));
        transaction.Complete();
      }
    }

    [Test]
    public void CharParameterTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var y = 'Y';
        var rs = GetRseQuery<MyEntity>();
        var parameterContext = new ParameterContext();
        var result = rs
          .Select(new[] { rs.Header.IndexOf(charColumn) })
          .Filter(t => t.GetValueOrDefault<char>(0) == y)
          .GetRecordSetReader(Session.Current, parameterContext)
          .ToEnumerable()
          .Select(i => i.GetValueOrDefault<char>(0))
          .ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(y, result[0]);
        transaction.Complete();
      }
    }

    [Test]
    public void CharConstantTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var rs = GetRseQuery<MyEntity>();
        var parameterContext = new ParameterContext();
        var result = rs
          .Select(new[] { rs.Header.IndexOf(charColumn) })
          .Filter(t => t.GetValueOrDefault<char>(0)=='Y')
          .GetRecordSetReader(Session.Current, parameterContext)
          .ToEnumerable()
          .Select(i => i.GetValueOrDefault<char>(0))
          .ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual('Y', result[0]);
        transaction.Complete();
      }
    }

    private CompilableProvider GetRseQuery<T>() where T : Entity
    {
      return Domain.Model.Types[typeof(T)].Indexes.PrimaryIndex.GetQuery();
    }
  }
}