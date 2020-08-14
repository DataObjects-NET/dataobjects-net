// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using System.Collections.Generic;

namespace Xtensive.Sql.Drivers.Oracle.v10
{
  internal class Extractor : v09.Extractor
  {
    protected override void RegisterReplacements(Dictionary<string, string> replacements, IReadOnlyCollection<string> targetSchemes)
    {
      base.RegisterReplacements(replacements, targetSchemes);
      replacements[TableFilterPlaceholder] = "NOT LIKE 'BIN$%'";
      replacements[IndexesFilterPlaceholder] = "indexes.DROPPED = 'NO'";
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}