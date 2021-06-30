// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class Extractor : v10.Extractor
  {
    // Constructors

    protected override string ToUpperInvariantIfNeeded(string schemaName) => schemaName;

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}