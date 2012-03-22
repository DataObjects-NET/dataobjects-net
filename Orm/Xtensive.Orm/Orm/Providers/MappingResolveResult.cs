// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal struct MappingResolveResult
  {
    public readonly Schema Schema;

    public readonly string Name;

    public Table GetTable()
    {
      return Schema.Tables[Name];
    }

    public Sequence GetSequence()
    {
      return Schema.Sequences[Name];
    }

    public MappingResolveResult(Schema schema, string name)
    {
      Schema = schema;
      Name = name;
    }
  }
}