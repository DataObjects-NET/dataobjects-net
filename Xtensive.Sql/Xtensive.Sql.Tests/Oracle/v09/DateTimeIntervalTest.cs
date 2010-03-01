// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.23

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Oracle.v09
{
  [TestFixture, Explicit]
  public class DateTimeIntervalTest : Tests.DateTimeIntervalTest
  {
    protected override string Url { get { return TestUrl.Oracle9; } }
  }
}