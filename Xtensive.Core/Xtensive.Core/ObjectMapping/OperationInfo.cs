// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Diagnostics;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  [Serializable]
  [DebuggerDisplay("Object = {Object.GetType().Name}, Type = {Type}, Property = {Property.SystemProperty}")]
  public struct OperationInfo
  {
    public readonly object Object;

    public readonly TargetPropertyDescription Property;

    public readonly OperationType Type;

    public readonly object Value;


    // Constructors

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