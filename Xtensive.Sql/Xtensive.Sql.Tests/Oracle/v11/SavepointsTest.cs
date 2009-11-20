// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.20

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Oracle.v11
{
  [TestFixture, Explicit]
  public class SavepointsTest : Tests.SavepointsTest
  {
    protected override string Url { get { return TestUrl.Oracle11; } }
  }
}