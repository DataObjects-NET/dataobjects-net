// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.14

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ElementInit"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableElementInit
  {
    private string methodName;

    /// <summary>
    /// <see cref="ElementInit.AddMethod"/>
    /// </summary>
    [NonSerialized]
    public MethodInfo AddMethod;
    /// <summary>
    /// <see cref="ElementInit.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      methodName = AddMethod.ToSerializableForm();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      AddMethod = methodName.GetMethodFromSerializableForm();
    }
  }
}