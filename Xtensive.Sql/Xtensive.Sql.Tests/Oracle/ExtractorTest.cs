// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.29

using NUnit.Framework;

namespace Xtensive.Sql.Tests.Oracle
{
  public abstract class ExtractorTest : SqlTest
  {
    [Test]
    public void BaseTest()
    {
      var model = ExtractDefaultSchema();
    }
  }
}