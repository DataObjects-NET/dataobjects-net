// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Oracle.v11
{
  [TestFixture, Explicit]
  public class UberTest : Oracle.UberTest
  {
    protected override string Url { get {return TestUrl.Oracle11; } }
  }
}