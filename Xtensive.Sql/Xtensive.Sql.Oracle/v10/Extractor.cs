// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

namespace Xtensive.Sql.Oracle.v10
{
  internal class Extractor : v09.Extractor
  {
    protected override string ApplyTableFilter(string query)
    {
      return query.Replace(TableFilterPlaceholder, "NOT LIKE 'BIN$%'");
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}