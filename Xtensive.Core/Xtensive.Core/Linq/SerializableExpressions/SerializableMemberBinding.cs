// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core.Linq.SerializableExpressions.Internals;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberBinding"/>
  /// </summary>
  [Serializable]
  public abstract class SerializableMemberBinding
  {
    private string memberName;

    /// <summary>
    /// <see cref="MemberBinding.BindingType"/>
    /// </summary>
    public MemberBindingType BindingType;
    /// <summary>
    /// <see cref="MemberBinding.Member"/>
    /// </summary>
    [NonSerialized]
    public MemberInfo Member;

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      memberName = Member.ToSerializableForm();
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      Member = memberName.GetMemberFromSerializableForm();
    }
  }
}