// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.VistaDb.v3
{
  internal class Driver : VistaDb.Driver
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

    public Driver()
      : base(new ServerInfoProvider())
    {
    }
  }
}
