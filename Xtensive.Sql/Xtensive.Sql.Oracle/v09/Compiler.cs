// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using System.Diagnostics;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Oracle.v09
{
  internal class Compiler : SqlCompiler
  {
    // Constructors

    protected internal Compiler(SqlDriver driver)
      : base(driver)
    {
    }
  }
}