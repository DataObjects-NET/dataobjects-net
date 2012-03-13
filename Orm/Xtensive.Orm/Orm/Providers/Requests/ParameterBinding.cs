// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Describes SQL parameter binding.
  /// </summary>
  public abstract class ParameterBinding
  {
    public TypeMapping TypeMapping { get; private set; }

    public SqlExpression ParameterReference { get; private set; }


    public static IEnumerable<T> NormalizeBindings<T>(IEnumerable<T> bindings)
      where T : ParameterBinding
    {
      return bindings!=null ? new HashSet<T>(bindings) : Enumerable.Empty<T>();
    }


    // Constructors

    protected ParameterBinding(TypeMapping typeMapping)
    {
      TypeMapping = typeMapping;
      ParameterReference = SqlDml.Placeholder(this);
    }
  }
}