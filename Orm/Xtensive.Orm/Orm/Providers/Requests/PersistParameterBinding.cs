// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A binding of a parameter for <see cref="PersistRequest"/>.
  /// </summary>
  public sealed class PersistParameterBinding : ParameterBinding
  {
    public int FieldIndex { get; private set; }


    // Constructors

    public PersistParameterBinding(TypeMapping typeMapping, int fieldIndex, ParameterTransmissionType transmissionType)
      : base(typeMapping, transmissionType)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(fieldIndex, -1, "fieldIndex");
      ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");

      FieldIndex = fieldIndex;
    }

    public PersistParameterBinding(TypeMapping typeMapping, int fieldIndex)
      : this(typeMapping, fieldIndex, ParameterTransmissionType.Regular)
    {
    }
  }
}