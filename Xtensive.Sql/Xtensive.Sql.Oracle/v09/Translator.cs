// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Translator : SqlTranslator
  {
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}