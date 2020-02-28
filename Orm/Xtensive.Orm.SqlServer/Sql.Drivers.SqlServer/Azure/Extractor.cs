// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.02

namespace Xtensive.Sql.Drivers.SqlServer.Azure
{
  internal class Extractor : v12.Extractor
  {
    protected override void ExtractFulltextIndexes()
    {
    }


    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}