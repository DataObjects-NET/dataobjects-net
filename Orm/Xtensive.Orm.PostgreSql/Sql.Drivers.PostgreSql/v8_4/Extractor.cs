// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using System.Data.Common;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_4
{
  /// <inheritdoc/>
  internal class Extractor : v8_3.Extractor
  {
    /// <inheritdoc/>
    protected override void ReadSequenceDescriptor(DbDataReader dataReader, ExtractionContext context)
    {
      var seqId = Convert.ToInt64(dataReader["id"]);
      var descriptor = context.SequenceMap[seqId].SequenceDescriptor;

      descriptor.Increment = Convert.ToInt64(dataReader["increment_by"]);
      descriptor.IsCyclic = Convert.ToBoolean(dataReader["is_cycled"]);
      descriptor.MinValue = Convert.ToInt64(dataReader["min_value"]);
      descriptor.MaxValue = Convert.ToInt64(dataReader["max_value"]);
      descriptor.StartValue = Convert.ToInt64(dataReader["start_value"]);
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}