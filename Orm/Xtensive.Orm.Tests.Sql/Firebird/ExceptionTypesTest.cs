// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.21

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
  public abstract class ExceptionTypesTest : Sql.ExceptionTypesTest
  {
    public override void SetUp()
    {
      TestHelpers.StartTraceToLogFile(this);
      base.SetUp();
      // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
      RealTestFixtureSetUp();
    }

    public override void TearDown()
    {
      base.TearDown();
      // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
      RealTestFixtureTearDown();
      TestHelpers.StopTraceToLogFile(this);
    }
  }
}