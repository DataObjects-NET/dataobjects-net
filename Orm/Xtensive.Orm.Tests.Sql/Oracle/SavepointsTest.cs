// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.20

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  [TestFixture, Explicit]
  public class SavepointsTest : Sql.SavepointsTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Oracle);
    }
  }
}