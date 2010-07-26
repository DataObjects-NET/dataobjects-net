// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

namespace Xtensive.Sql.SqlServer.v10
{
  internal class Translator : v09.Translator
  {
    public override string DateTimeFormatString { get { return @"'cast ('\'yyyy\-MM\-ddTHH\:mm\:ss\.fff\'' as datetime2)'"; } }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}