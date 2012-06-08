// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.Drivers.MySql.v5_0
{
  internal class Driver : MySql.Driver
  {
    /// <inheritdoc/>
    protected override Sql.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    /// <inheritdoc/>
    protected override SqlCompiler CreateCompiler()
    {
      return new Compiler(this);
    }

    /// <inheritdoc/>
    protected override Model.Extractor CreateExtractor()
    {
      return new Extractor(this);
    }

    /// <inheritdoc/>
    protected override SqlTranslator CreateTranslator()
    {
      return new Translator(this);
    }

    /// <inheritdoc/>
    protected override Info.ServerInfoProvider CreateServerInfoProvider()
    {
      return new ServerInfoProvider(this);
    }

    // Constructors

    public Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}