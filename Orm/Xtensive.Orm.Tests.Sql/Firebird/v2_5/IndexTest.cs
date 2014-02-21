// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.24

using System;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Firebird.v2_5
{
  [TestFixture, Explicit]
  public class IndexTest : Firebird.IndexTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderVersionAtLeast(new Version(2, 5));
    }
  }
}
