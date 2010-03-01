// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using System.Data.Common;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.PostgreSql.v8_4
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