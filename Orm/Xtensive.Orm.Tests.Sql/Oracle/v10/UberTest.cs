﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Oracle.v10
{
  [TestFixture, Explicit]
  public class UberTest : Oracle.UberTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderVersionAtLeast(StorageProviderVersion.Oracle10);
    }
  }
}
