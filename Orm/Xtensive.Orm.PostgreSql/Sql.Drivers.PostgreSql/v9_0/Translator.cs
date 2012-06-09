// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.06

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_0
{
  internal class Translator : v8_4.Translator
  {
    public override string Translate(SqlCompilerContext context, object literalValue)
    {
      var array = literalValue as byte[];
      if (array==null)
        return base.Translate(context, literalValue);

      var result = new StringBuilder(array.Length * 2 + 6);

      result.Append(@"E'\\x");
      result.AppendHexArray(array);
      result.Append("'");

      return result.ToString();
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}