// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.19

using System;
using NUnit.Framework;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Sql.Firebird.v2_5
{
  [TestFixture, Explicit]
  public class TypeMappingTest : Firebird.TypeMappingTest
  {
    public override void SetUp()
    {
      base.SetUp();
      TestHelpers.StartTraceToLogFile(this);
    }

    public override void TearDown()
    {
      base.TearDown();
      TestHelpers.StopTraceToLogFile(this);
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderVersionAtLeast(new Version(2, 5));
    }

    public TypeMappingTest()
    {
    }
  }
}
