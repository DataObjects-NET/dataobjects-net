// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  public class NullParametersTest : AutoBuildTest
  {
    private DisposableSet disposableSet;

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      disposableSet = new DisposableSet();
      disposableSet.Add(Domain.OpenSession());
      disposableSet.Add(Transaction.Open());

      new X {FString = "Xtensive"};
      new X {FString = null};
    }
    
    public override void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
      base.TestFixtureTearDown();
    }

    [Test]
    public void CompareWithNullParameterTest()
    {
      string value = null;
      var result = Query<X>.All.Where(x => x.FString==value).ToList();
      Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void SelectNullParameter()
    {
      string value = null;
      var result = Query<X>.All.Select(x => new {x.Id, Value = value}).ToList();
      foreach (var i in result)
        Assert.IsNull(i.Value);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return config;
    }
  }
}
