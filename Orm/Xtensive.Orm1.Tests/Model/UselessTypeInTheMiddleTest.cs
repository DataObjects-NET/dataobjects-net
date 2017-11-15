// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.UselessTypeInTheMiddleTestModel;

namespace Xtensive.Orm.Tests.Model.UselessTypeInTheMiddleTestModel
{
  [HierarchyRoot]
  public abstract class One : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }

  public class Two : One
  {
    // Absense of fields here leads to errors.
  }

  public class Three : Two
  {
    [Field]
    public int Value { get; private set; }
  }

  [HierarchyRoot]
  public class Referencer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Two Reference { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class UselessTypeInTheMiddleTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration =  base.BuildConfiguration();
      configuration.Types.Register(typeof (One).Assembly, typeof (One).Namespace);
      return configuration;
    }

    [Test]
    public void QueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var three = new Three();
        var twos = session.Query.All<Two>().ToList();
        Assert.AreEqual(1, twos.Count);
        Assert.AreEqual(three, twos[0]);
      }
    }

    [Test]
    public void CreateTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        new Referencer {Reference = new Three()};
        Session.Current.SaveChanges();
      }
    }
  }
}