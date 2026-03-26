// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Mapping between <see cref="DomainModel"/>
  /// and <see cref="Catalog"/>s, <see cref="Schema"/>s and <see cref="Table"/>s.
  /// </summary>
  public sealed class ModelMapping : LockableBase
  {
    private readonly Dictionary<TypeInfo, Table> tableMap = new Dictionary<TypeInfo, Table>();
    private readonly Dictionary<SequenceInfo, SchemaNode> sequenceMap = new Dictionary<SequenceInfo, SchemaNode>();

    private string temporaryTableDatabase;
    private string temporaryTableSchema;
    private string temporaryTableCollation;

    public string TemporaryTableDatabase
    {
      get { return temporaryTableDatabase; }
      set
      {
        EnsureNotLocked();
        temporaryTableDatabase = value;
      }
    }

    public string TemporaryTableSchema
    {
      get { return temporaryTableSchema; }
      set
      {
        EnsureNotLocked();
        temporaryTableSchema = value;
      }
    }

    public string TemporaryTableCollation
    {
      get { return temporaryTableCollation; }
      set
      {
        EnsureNotLocked();
        temporaryTableCollation = value;
      }
    }

    public Table this[TypeInfo typeInfo]
    {
      get
      {
        Table result;
        tableMap.TryGetValue(typeInfo, out result);
        return result;
      }
    }

    public SchemaNode this[SequenceInfo sequenceInfo]
    {
      get
      {
        SchemaNode result;
        sequenceMap.TryGetValue(sequenceInfo, out result);
        return result;
      }
    }

    public void Register(TypeInfo typeInfo, Table table)
    {
      EnsureNotLocked();
      tableMap[typeInfo] = table;
    }

    public void Register(SequenceInfo sequenceInfo, SchemaNode sequence)
    {
      EnsureNotLocked();
      sequenceMap[sequenceInfo] = sequence;
    }

    internal IList<SchemaNode> GetAllSchemaNodes()
    {
      return tableMap.Values.Union(sequenceMap.Values).ToList();
    }

    // Constructors

    internal ModelMapping()
    {
    }
  }
}