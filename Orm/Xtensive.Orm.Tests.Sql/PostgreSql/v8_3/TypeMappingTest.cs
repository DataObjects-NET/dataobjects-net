// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.08

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.PostgreSql.v8_3
{
  [TestFixture]
  public class TypeMappingTest : Sql.TypeMappingTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      Require.ProviderVersionAtLeast(StorageProviderVersion.PostgreSql83);
    }
  }
}