// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.13

using NUnit.Framework;
using System;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
  public class ExtractorTest : SqlTest
  {
    [Test]
    public void BaseTest()
    {
      var schema = ExtractDefaultSchema();
    }

    public override void SetUp()
    {
      base.SetUp();
      // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
      RealTestFixtureSetUp();
    }

    public override void TearDown()
    {
      base.TearDown();
      // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
      RealTestFixtureTearDown();
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Firebird);
    }
  }
}
