// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlParameterBinding<TValueAccessor> : SqlParameterBinding
  {
    /// <summary>
    /// Gets the value accessor.
    /// </summary>
    public TValueAccessor ValueAccessor { get; private set; }

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected SqlParameterBinding(TValueAccessor valueAccessor, DataTypeMapping typeMapping)
      : base(typeMapping)
    {
      ValueAccessor = valueAccessor;
    }
  }
}