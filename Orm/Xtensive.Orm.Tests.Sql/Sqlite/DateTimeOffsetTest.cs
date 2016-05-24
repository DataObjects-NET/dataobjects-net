// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.04.26

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  [TestFixture]
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    protected override bool IsNanosecondSupported
    {
      get { return false; }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
    }
  }
}
