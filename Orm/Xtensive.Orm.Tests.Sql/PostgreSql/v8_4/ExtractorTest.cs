// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.23

using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.PostgreSql.v8_4
{
  [TestFixture]
  public class ExtractorTest: v8_3.ExtractorTest
  {
    protected override string Url { get { return TestUrl.PostgreSql84; } }
  }
}