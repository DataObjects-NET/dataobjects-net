// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2006.04.21

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture]
  public class DateTimeIntervalTest : Sql.DateTimeIntervalTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
    }
  }
}
