// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.10.10

using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Drivers.PostgreSql.v9_1
{
  internal class Translator : v9_0.Translator
  {
    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}