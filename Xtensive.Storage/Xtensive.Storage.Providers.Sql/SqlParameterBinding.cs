// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlParameterBinding
  {
    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    public TypeMapping TypeMapping { get; private set; }

    /// <summary>
    /// Gets the parameter reference.
    /// </summary>
    /// <value>The parameter reference.</value>
    public SqlParameterRef ParameterReference { get; private set; }
    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SqlParameterBinding(TypeMapping typeMapping)
    {
      TypeMapping = typeMapping;
      ParameterReference = SqlDml.ParameterRef(this);
    }
  }
}