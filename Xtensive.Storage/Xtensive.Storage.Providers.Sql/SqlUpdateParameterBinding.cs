// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlUpdateParameterBinding : SqlParameterBinding
  {
    public int FieldIndex { get; private set;}

    // Constructors

    /// <summary>
    ///	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="fieldIndex">Index of the field that contain new value.</param>
    /// <param name="typeMapping">The type mapping.</param>
    public SqlUpdateParameterBinding(int fieldIndex, TypeMapping typeMapping)
      : base(typeMapping)
    {
      FieldIndex = fieldIndex;
    }
  }
}