// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.27

using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.SqlServerCe
{
  [TestFixture]
  public class ExceptionTypesTest : Sql.ExceptionTypesTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServerCe);
    }

    public override void CheckConstraintTest()
    {
      Assert.Ignore("Check constraints are not supported");
    }

    protected override void AssertExceptionType(SqlExceptionType expected, SqlExceptionType actual)
    {
      if (expected==SqlExceptionType.Deadlock)
        expected = SqlExceptionType.OperationTimeout;
      base.AssertExceptionType(expected, actual);
    }
  }
}