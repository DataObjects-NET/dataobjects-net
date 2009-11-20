// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Oracle.DataAccess.Client;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Oracle.v10
{
  internal class Driver : Oracle.Driver
  {
    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    // Constructors

    public Driver(OracleConnection connection, Version version)
      : base(new ServerInfoProvider(connection, version))
    {
    }
 }
}