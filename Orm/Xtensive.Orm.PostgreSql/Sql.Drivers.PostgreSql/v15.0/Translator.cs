// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Text;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v15_0
{
  internal class Translator : v12_0.Translator
  {
    protected override void AppendIndexStorageParameters(StringBuilder builder, Index index)
    {
      if (index.IsUnique) { }
        _ = builder.Append("NULLS NOT DISTINCT");

      if (index.FillFactor != null) {
        if (builder.Length > 0)
          _ = builder.Append(" ");
        _ = builder.AppendFormat("WITH(FILLFACTOR={0})", index.FillFactor);
      }
    }

    // Constructors

    public Translator(PostgreSql.Driver driver)
      : base(driver)
    {
    }
  }
}
