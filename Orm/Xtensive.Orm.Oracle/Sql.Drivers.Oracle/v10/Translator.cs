// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.Oracle.v10
{
  internal class Translator : v09.Translator
  {
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
      FloatFormatString = base.FloatFormatString + "f";
      DoubleFormatString = base.DoubleFormatString + "d";
    }
  }
}