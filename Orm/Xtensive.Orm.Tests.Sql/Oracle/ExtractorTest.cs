// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.29

using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  public abstract class ExtractorTest : SqlTest
  {
    [Test]
    public void BaseTest()
    {
      var schema = ExtractDefaultSchema();
    }
  }
}