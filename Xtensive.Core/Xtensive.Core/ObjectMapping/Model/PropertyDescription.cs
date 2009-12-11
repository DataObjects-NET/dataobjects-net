// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.ObjectMapping.Model
{
  [Serializable]
  [DebuggerDisplay("{SystemProperty} in {DeclaringType}")]
  public abstract class PropertyDescription : LockableBase
  {
    private static readonly HashSet<Type> primitiveTypes;

    private bool isCollection;

    public readonly bool IsPrimitive;

    public TypeDescription DeclaringType { get; private set; }

    public readonly PropertyInfo SystemProperty;
    
    public bool IsCollection
    {
      get { return isCollection; }
      set{
        this.EnsureNotLocked();
        isCollection = value;
      }
    }

    public readonly Type UnderlyingType;

    internal static bool IsPropertyPrimitive(PropertyInfo propertyInfo)
    {
      return primitiveTypes.Contains(propertyInfo.PropertyType);
    }


    // Constructors

    protected PropertyDescription(PropertyInfo systemProperty, TypeDescription declaringType)
    {
      ArgumentValidator.EnsureArgumentNotNull(systemProperty, "systemProperty");
      ArgumentValidator.EnsureArgumentNotNull(declaringType, "declaringType");

      SystemProperty = systemProperty;
      DeclaringType = declaringType;
      IsPrimitive = primitiveTypes.Contains(systemProperty.PropertyType);
    }

    static PropertyDescription()
    {
      primitiveTypes = new HashSet<Type> {
        typeof (Int16), typeof (Int32), typeof (Int64), typeof (Byte), typeof (UInt16), typeof (UInt32),
        typeof (UInt64), typeof(Guid), typeof (Byte), typeof (Char), typeof (String), typeof (Decimal),
        typeof (Single), typeof (Double), typeof (DateTime), typeof (TimeSpan), typeof (DateTimeOffset)
      };
    }
  }
}