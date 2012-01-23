// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.23

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Xtensive.Sql.Tests.PostgreSql.v8_3
{
  [TestFixture]
  public class ExtractorTest : PostgreSql.ExtractorTest
  {
    protected override string Url
    {
      get { return TestUrl.PostgreSql83; }
    }

    [Test]
    public void FullTextIndexExtractorTest()
    {
      var schema = Driver.ExtractDefaultSchema(Connection);
    }
  }
}