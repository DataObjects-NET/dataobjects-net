// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.UselessTypeInTheMiddleTestModel;

namespace Xtensive.Storage.Tests.Model.UselessTypeInTheMiddleTestModel
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

namespace Xtensive.Storage.Tests.Model
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
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var three = new Three();
        var twos = Query.All<Two>().ToList();
        Assert.AreEqual(1, twos.Count);
        Assert.AreEqual(three, twos[0]);
      }
    }

    [Test]
    public void CreateTest()
    {
      using (Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        new Referencer {Reference = new Three()};
        Session.Current.SaveChanges();
      }
    }
  }
}