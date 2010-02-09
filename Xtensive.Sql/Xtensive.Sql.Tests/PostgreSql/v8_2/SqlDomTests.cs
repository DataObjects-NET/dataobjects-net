// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using NUnit.Framework;

namespace Xtensive.Sql.Tests.PostgreSql.v8_2
{
  [TestFixture]
  public class SqlDomTests : v8_1.SqlDomTests
  {
    protected override string Url { get { return TestUrl.PostgreSql82; } }
  }
}