// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Drivers.Firebird.Resources;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal partial class Extractor : v3_0.Extractor
  {
    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
