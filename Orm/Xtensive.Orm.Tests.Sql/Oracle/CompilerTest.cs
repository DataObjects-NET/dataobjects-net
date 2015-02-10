// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  public class CompilerTest : Sql.CompilerTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Oracle);
    }
  }
}
