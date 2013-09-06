// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.09.06

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Standard base type for <see cref="IPropertyValidator"/> implementation.
  /// </summary>
  public abstract class PropertyValidator : Attribute, IPropertyValidator
  {
    private bool isImmediate;

    /// <summary>
    /// Gets or sets value indicating if current validator is immediate.
    /// </summary>
    public bool IsImmediate
    {
      get { return isImmediate; }
      set
      {
        if (Domain!=null)
          throw Exceptions.ObjectIsReadOnly(null);
        isImmediate = value;
      }
    }

    /// <summary>
    /// Gets domain this instance is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets type this instance is bound to.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Gets field this instance is bound to.
    /// </summary>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Configures this instance.
    /// </summary>
    /// <param name="domain">A domain this validator is bound to.</param>
    /// <param name="type">A type this validator is bound to.</param>
    /// <param name="field">A persitent field this validator is bound to.</param>
    public virtual void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      if (Domain!=null)
        throw Exceptions.ObjectIsReadOnly(null);

      Domain = domain;
      Type = type;
      Field = field;
    }

    /// <summary>
    /// Validates specified object considering new value of a persistent field.
    /// </summary>
    /// <param name="target">An object to validate.</param>
    /// <param name="fieldValue">Persistent field value.</param>
    public abstract void Validate(Persistent target, object fieldValue);

    /// <summary>
    /// Creates new unconfigured <see cref="IPropertyValidator"/> instance
    /// with the same parameters.
    /// </summary>
    public abstract void CreateNew();
  }
}