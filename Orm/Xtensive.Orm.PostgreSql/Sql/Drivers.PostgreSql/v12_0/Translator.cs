// Copyright (C) 2020-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;

namespace Xtensive.Sql.Drivers.PostgreSql.v12
{
  internal class Translator : v11.Translator
  {

    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}