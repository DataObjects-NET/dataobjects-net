// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.03.21

namespace Xtensive.Orm.Tests.Storage.ReadOnlySupport
{
  public class OracleReadOnlyTest : ReadOnlyStorageTestBase
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Oracle);
    }
  }
}