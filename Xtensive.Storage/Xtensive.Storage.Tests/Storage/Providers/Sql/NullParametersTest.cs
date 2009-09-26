// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.25

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  public class NullParametersTest : AutoBuildTest
  {
    private DisposableSet disposableSet;
    private bool emptyStringIsNull;

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      emptyStringIsNull = ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
      CreateSessionAndTransaction();

      new X {FString = "Xtensive"};
      new X {FString = null};
      new X {FString = string.Empty};
    }
    
    public override void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
      base.TestFixtureTearDown();
    }

    [Test]
    public void CompareWithEmptyStringParameterTest()
    {
      var value = string.Empty;
      var result = Query<X>.All.Where(x => x.FString==value).ToList();
      if (emptyStringIsNull)
        CheckIsNull(result, 2);
      else
        CheckIsEmpty(result, 1);
    }

    [Test]
    public void CompareWithNullParameterTest()
    {
      string value = null;
      var result = Query<X>.All.Where(x => x.FString==value).ToList();
      CheckIsNull(result, emptyStringIsNull ? 2 : 1);
    }

    [Test]
    public void SelectNullParameter()
    {
      string value = null;
      var result = Query<X>.All.Select(x => new {x.Id, Value = value}).ToList();
      foreach (var item in result)
        Assert.IsNull(item.Value);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return config;
    }

    private static void CheckIsNull(ICollection<X> items, int expectedCount)
    {
      Assert.AreEqual(expectedCount, items.Count);
      foreach (var item in items)
        Assert.IsNull(item.FString);
    }

    private static void CheckIsEmpty(ICollection<X> items, int expectedCount)
    {
      Assert.AreEqual(expectedCount, items.Count);
      foreach (var item in items)
        Assert.IsEmpty(item.FString);
    }
  }
}
