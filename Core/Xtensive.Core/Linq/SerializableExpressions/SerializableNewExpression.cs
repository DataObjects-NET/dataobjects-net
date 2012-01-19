// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="NewExpression"/>
  /// </summary>
  [Serializable]
  public sealed class SerializableNewExpression : SerializableExpression
  {
    private string ctorName;
    private string[] memberNames;

    /// <summary>
    /// <see cref="NewExpression.Arguments"/>
    /// </summary>
    public SerializableExpression[] Arguments;
    /// <summary>
    /// <see cref="NewExpression.Constructor"/>
    /// </summary>
    [NonSerialized]
    public ConstructorInfo Constructor;
    /// <summary>
    /// <see cref="NewExpression.Members"/>
    /// </summary>
    [NonSerialized]
    public MemberInfo[] Members;

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      ctorName = Constructor.ToSerializableForm();
      memberNames = Members
        .Select(m => m.ToSerializableForm())
        .ToArray();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      Constructor = ctorName.GetConstructorFromSerializableForm();
      Members = memberNames
        .Select(name => name.GetMemberFromSerializableForm())
        .ToArray();
    }
  }
}