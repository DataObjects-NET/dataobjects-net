// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using Npgsql;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.PostgreSql.v8_2
{
  internal class Driver : PostgreSql.Driver
  {
    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    // Constructors

    public Driver(NpgsqlConnection connection, Version version)
      : base(new ServerInfoProvider(connection, version))
    {
    }
  }
}