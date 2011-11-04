// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Oracle.v10
{
  internal class Translator : v09.Translator
  {
    public override string FloatFormatString { get { return base.FloatFormatString + "f"; } }
    public override string DoubleFormatString { get { return base.DoubleFormatString + "d"; } }
    
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}