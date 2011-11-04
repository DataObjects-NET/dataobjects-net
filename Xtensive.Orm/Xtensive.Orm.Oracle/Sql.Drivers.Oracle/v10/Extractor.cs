// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System.Collections.Generic;

namespace Xtensive.Sql.Oracle.v10
{
  internal class Extractor : v09.Extractor
  {
    protected override void RegisterReplacements(Dictionary<string, string> replacements)
    {
      base.RegisterReplacements(replacements);
      replacements[TableFilterPlaceholder] = "NOT LIKE 'BIN$%'";
      replacements[IndexesFilterPlaceholder] = "indexes.DROPPED = 'NO'";
    }
   

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}