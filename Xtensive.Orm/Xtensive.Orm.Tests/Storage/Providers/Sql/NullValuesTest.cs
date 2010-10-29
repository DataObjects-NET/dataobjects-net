// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.25

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Disposing;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Storage.Providers.Sql
{
  public class NullValuesTest : AutoBuildTest
  {
    private bool emptyStringIsNull;

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
      emptyStringIsNull = ProviderInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
      
      new X {FString = "Xtensive"};
      new X {FString = null};
      new X {FString = string.Empty};
    }

    [Test]
    public void CompareWithEmptyStringParameterTest()
    {
      var value = string.Empty;
      var result = Session.Demand().Query.All<X>().Where(x => x.FString==value).ToList();
      if (emptyStringIsNull)
        CheckIsNull(result, 2);
      else
        CheckIsEmpty(result, 1);
    }
    
    [Test]
    public void CompareWithNullStringParameterTest()
    {
      string value = null;
      var result = Session.Demand().Query.All<X>().Where(x => x.FString==value).ToList();
      CheckIsNull(result, emptyStringIsNull ? 2 : 1);
    }

    [Test]
    public void CompareWithEmptyStringConstantTest()
    {
      // NOTE: string.Empty should not be used here, because it is translated via string parameter
      var result = Session.Demand().Query.All<X>().Where(x => x.FString=="").ToList();
      if (emptyStringIsNull)
        CheckIsNull(result, 2);
      else
        CheckIsEmpty(result, 1);
    }

    [Test]
    public void CompareWithNullStringConstantTest()
    {
      var result = Session.Demand().Query.All<X>().Where(x => x.FString==null).ToList();
      CheckIsNull(result, emptyStringIsNull ? 2 : 1);
    }

    [Test]
    public void SelectNullStringParameter()
    {
      string value = null;
      var result = Session.Demand().Query.All<X>().Select(x => new {x.Id, Value = value}).ToList();
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
