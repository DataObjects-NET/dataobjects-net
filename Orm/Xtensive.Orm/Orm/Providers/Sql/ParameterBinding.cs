// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using Xtensive.Internals.DocTemplates;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Describes SQL parameter binding.
  /// </summary>
  public abstract class ParameterBinding
  {
    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    public TypeMapping TypeMapping { get; private set; }

    /// <summary>
    /// Gets the parameter reference.
    /// </summary>
    public SqlPlaceholder ParameterReference { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected ParameterBinding(TypeMapping typeMapping)
    {
      TypeMapping = typeMapping;
      ParameterReference = SqlDml.Placeholder(this);
    }
  }
}