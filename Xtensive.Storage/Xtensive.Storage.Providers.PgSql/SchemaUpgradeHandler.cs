// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.09

using System;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.PgSql
{
  [Serializable]
  public class SchemaUpgradeHandler : Sql.SchemaUpgradeHandler
  {
    /// <inheritdoc/>
    protected override void BuildSequence(Schema schema, GeneratorInfo generator)
    {
      var domainHandler = (DomainHandler) Handlers.DomainHandler;
      var sequence = schema.CreateSequence(generator.MappingName);
      sequence.SequenceDescriptor.StartValue = generator.CacheSize;
      sequence.SequenceDescriptor.Increment = generator.CacheSize;
      sequence.DataType = domainHandler.ValueTypeMapper.BuildSqlValueType(generator.TupleDescriptor[0], 0);
    }
  }
}