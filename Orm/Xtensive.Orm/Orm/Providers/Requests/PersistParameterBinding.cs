// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A binding of a parameter for <see cref="PersistRequest"/>.
  /// </summary>
  public sealed class PersistParameterBinding : ParameterBinding
  {
    public int RowIndex { get; private set; }

    public int FieldIndex { get; private set; }

    public PersistParameterBindingType BindingType { get; private set; }

    // Constructors

    public PersistParameterBinding(TypeMapping typeMapping, int rowIndex, int fieldIndex,
      ParameterTransmissionType transmissionType, PersistParameterBindingType bindingType)
      : base(typeMapping, transmissionType)
    {
      RowIndex = rowIndex;
      FieldIndex = fieldIndex;
      BindingType = bindingType;
    }

    public PersistParameterBinding(TypeMapping typeMapping, int fieldIndex, ParameterTransmissionType transmissionType, PersistParameterBindingType bindingType)
      : this(typeMapping, 0, fieldIndex, transmissionType, bindingType)
    {
    }

    public PersistParameterBinding(TypeMapping typeMapping, int rowIndex, int fieldIndex, ParameterTransmissionType transmissionType)
      : this(typeMapping, rowIndex, fieldIndex, transmissionType, PersistParameterBindingType.Regular)
    {
    }

    public PersistParameterBinding(TypeMapping typeMapping, int fieldIndex, ParameterTransmissionType transmissionType)
      : this(typeMapping, fieldIndex, transmissionType, PersistParameterBindingType.Regular)
    {
    }

    public PersistParameterBinding(TypeMapping typeMapping, int rowIndex, int fieldIndex)
      : this(typeMapping, rowIndex, fieldIndex, ParameterTransmissionType.Regular, PersistParameterBindingType.Regular)
    {
    }

    public PersistParameterBinding(TypeMapping typeMapping, int fieldIndex)
      : this(typeMapping, fieldIndex, ParameterTransmissionType.Regular, PersistParameterBindingType.Regular)
    {
    }
  }
}