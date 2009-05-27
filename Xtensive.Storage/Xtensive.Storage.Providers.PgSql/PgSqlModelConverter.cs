// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.26

using System;
using Xtensive.Modelling;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.PgSql
{
  internal sealed class PgSqlModelConverter : SqlModelConverter
  {
    protected override IPathNode VisitSequence(Sequence sequence)
    {
      var sequenceInfo = new SequenceInfo(StorageInfo, sequence.Name) {
                           Current = GetNextGeneratorValue(sequence.Name),
                           Increment = sequence.SequenceDescriptor.Increment.Value,
                           StartValue = sequence.SequenceDescriptor.StartValue.Value,
                           Type = ValueTypeConverter.Invoke(sequence.DataType)
                         };
      return sequenceInfo;
    }

    protected override long GetNextGeneratorValue(string generatorName)
    {
      var sequence = Schema.Sequences[generatorName];
      var sqlNext = SqlFactory.Select();
      sqlNext.Columns.Add(SqlFactory.NextValue(sequence));
      return (long) CommandExecutor.Invoke(sqlNext);
    }


    // Constructor

    /// <inheritdoc/>
    public PgSqlModelConverter(Schema storageSchema, Func<ISqlCompileUnit, object> commandExecutor, 
      Func<SqlValueType, TypeInfo> valueTypeConverter)
      : base(storageSchema, commandExecutor, valueTypeConverter)
    {}
  }
}