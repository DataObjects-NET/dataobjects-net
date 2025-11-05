// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Describes SQL parameter binding.
  /// </summary>
  public abstract class ParameterBinding
  {
    public TypeMapping TypeMapping { get; }

    public ParameterTransmissionType TransmissionType { get; }

    public SqlExpression ParameterReference { get; }

    public static IReadOnlyCollection<T> NormalizeBindings<T>(IEnumerable<T> bindings) where T : ParameterBinding =>
      bindings != null ? new HashSet<T>(bindings) : Array.Empty<T>();


    // Constructors

    protected ParameterBinding(TypeMapping typeMapping, ParameterTransmissionType transmissionType)
    {
      TypeMapping = typeMapping;
      TransmissionType = transmissionType;
      ParameterReference = SqlDml.Placeholder(this);
    }
  }
}
