// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using System.Data.Common;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_4
{
  internal class Extractor : v8_3.Extractor
  {
    protected override void ReadSequenceDescriptor(DbDataReader reader, SequenceDescriptor descriptor)
    {
      base.ReadSequenceDescriptor(reader, descriptor);
      descriptor.StartValue = Convert.ToInt64(reader["start_value"]);
    }

    // Consructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}