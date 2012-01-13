// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.08

using NUnit.Framework;

namespace Xtensive.Sql.Tests.PostgreSql.v8_2
{
  [TestFixture]
  public class TypeMappingTest : Tests.TypeMappingTest
  {
    protected override string Url { get { return TestUrl.PostgreSql82; } }
  }
}