// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.02.06

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  class CompilerTest : Sql.CompilerTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }
  }
}
