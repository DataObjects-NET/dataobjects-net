// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Mappings;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlParameterBinding
  {
    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    public DataTypeMapping TypeMapping { get; private set; }

    /// <summary>
    /// Gets the parameter reference.
    /// </summary>
    /// <value>The parameter reference.</value>
    public SqlParameterRef ParameterReference { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SqlParameterBinding(DataTypeMapping typeMapping)
    {
      TypeMapping = typeMapping;
      ParameterReference = SqlFactory.ParameterRef(this);
    }
  }
}