// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Implementation
{
  [Serializable]
  public sealed class PropertyHandler<TOwner, TValue> : PropertyHandler
  {
    private readonly Func<object, object> untypedGetter;
    private readonly Action<object, object> untypedSetter;

    /// <summary>
    /// Property getter.
    /// </summary>
    public readonly Func<TOwner, TValue> Getter;

    /// <summary>
    /// Property setter.
    /// </summary>
    public readonly Action<TOwner, TValue> Setter;

    #region Properties: OwnerType, ValueType, UntypedGetter, UntypedSetter

    /// <inheritdoc/>
    public override Type OwnerType
    {
      get { return typeof(TOwner); }
    }

    /// <inheritdoc/>
    public override Type ValueType
    {
      get { return typeof(TValue); }
    }

    /// <inheritdoc/>
    public override Func<object, object> UntypedGetter {
      get { return untypedGetter; }
    }

    /// <inheritdoc/>
    public override Action<object, object> UntypedSetter {
      get { return untypedSetter; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="getter">The property getter.</param>
    /// <param name="setter">The property setter.</param>
    public PropertyHandler(string name, Func<TOwner, TValue> getter, Action<TOwner, TValue> setter)
      : base(name)
    {
      Getter = getter;
      Setter = setter;
      untypedGetter = o => Getter((TOwner) o);
      untypedSetter = (o, v) => Setter((TOwner) o, (TValue) v);
    }
  }
}