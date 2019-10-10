// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.10

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_1
{
  internal class Translator : v9_0.Translator
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