// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A binding of a parameter for <see cref="PersistRequest"/>.
  /// </summary>
  public sealed class PersistParameterBinding : ParameterBinding
  {
    /// <summary>
    /// Gets the type of the binding.
    /// </summary>
    public PersistParameterBindingType BindingType { get; private set; }

    /// <summary>
    /// Gets the index of the field to extract value from.
    /// </summary>
    public int FieldIndex { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="fieldIndex">Index of the field that contain new value.</param>
    /// <param name="typeMapping">The type mapping.</param>
    /// <param name="bindingType">Type of the binding.</param>
    public PersistParameterBinding(int fieldIndex, TypeMapping typeMapping, PersistParameterBindingType bindingType)
      : base(typeMapping)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(fieldIndex, -1, "fieldIndex");
      ArgumentValidator.EnsureArgumentNotNull(typeMapping, "typeMapping");

      FieldIndex = fieldIndex;
      BindingType = bindingType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="fieldIndex">Index of the field that contain new value.</param>
    /// <param name="typeMapping">The type mapping.</param>
    public PersistParameterBinding(int fieldIndex, TypeMapping typeMapping)
      : this(fieldIndex, typeMapping, PersistParameterBindingType.Regular)
    {
    }
  }
}