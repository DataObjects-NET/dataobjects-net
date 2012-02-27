// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using Xtensive.Core;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A binding of a parameter for <see cref="PersistRequest"/>.
  /// </summary>
  public sealed class PersistParameterBinding : ParameterBinding
  {
    public PersistParameterBindingType BindingType { get; private set; }

    public int FieldIndex { get; private set; }


    // Constructors

    public PersistParameterBinding(int fieldIndex, TypeMapping typeMapping, PersistParameterBindingType bindingType)
      : base(typeMapping)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(fieldIndex, -1, "fieldIndex");
      ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");

      FieldIndex = fieldIndex;
      BindingType = bindingType;
    }

    public PersistParameterBinding(int fieldIndex, TypeMapping typeMapping)
      : this(fieldIndex, typeMapping, PersistParameterBindingType.Regular)
    {
    }
  }
}