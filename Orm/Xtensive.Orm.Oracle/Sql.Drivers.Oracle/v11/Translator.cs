// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Oracle.v11
{
  internal class Translator : v10.Translator
  {
    public override string Translate(SqlValueType type)
    {
      // we need to explicitly specify maximum interval precision
      if (type.Type == SqlType.Interval)
        return "INTERVAL DAY(6) TO SECOND(6)";
      return base.Translate(type);
    }

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}