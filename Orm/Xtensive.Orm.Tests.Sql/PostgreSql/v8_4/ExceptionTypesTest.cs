// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.08

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.PostgreSql.v8_4
{
  [TestFixture]
  public class ExceptionTypesTest : Sql.ExceptionTypesTest
  {
    protected override string Url { get { return TestUrl.PostgreSql84; } }
  }
}