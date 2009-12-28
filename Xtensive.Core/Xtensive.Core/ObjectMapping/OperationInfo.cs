// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Information about an operation that modified a state of an transformed object.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Object = {Object.GetType().Name}, Type = {Type}, Property = {Property.SystemProperty}")]
  public struct OperationInfo
  {
    /// <summary>
    /// The original transformed object.
    /// </summary>
    public readonly object Object;

    /// <summary>
    /// The property that has been modified.
    /// </summary>
    public readonly TargetPropertyDescription Property;

    /// <summary>
    /// The operation type.
    /// </summary>
    public readonly OperationType Type;

    /// <summary>
    /// The new property value or the collection's item.
    /// </summary>
    public readonly object Value;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="obj">The value of <see cref="Object"/>.</param>
    /// <param name="type">The value of <see cref="Type"/>.</param>
    /// <param name="property">The value of <see cref="Property"/>.</param>
    /// <param name="value">The value of <see cref="Value"/>.</param>
    public OperationInfo(object obj, OperationType type, TargetPropertyDescription property,
      object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(obj, "obj");

      Object = obj;
      Type = type;
      Property = property;
      Value = value;
    }
  }
}