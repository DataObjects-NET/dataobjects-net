// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2013.12.03

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture, Explicit]
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2008);
    }
  }
}
