// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.SqlServer.v09
{
  [TestFixture]
  public class DateTimeIntervalTest : Sql.DateTimeIntervalTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2005);
    }

    public override void DateTimeSubtractIntervalTest()
    {
      Assert.Ignore("MSSQL DateTime precision issue");
    }
  }
}