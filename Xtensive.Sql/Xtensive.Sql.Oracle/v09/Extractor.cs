// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Extractor : Model.Extractor
  {
    public override Catalog Extract()
    {
      throw new NotImplementedException();
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}