// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests
{
  public abstract class UberTest : SqlTest
  {
    protected const string Table1Name = "table_1";
    protected const string Table2Name = "table_2";

    protected Schema TestSchema { get; private set; }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      TestSchema = ExtractAllSchemas().DefaultSchema;
      EnsureTableNotExists(TestSchema, Table1Name);
      EnsureTableNotExists(TestSchema, Table2Name);
    }
  }
}