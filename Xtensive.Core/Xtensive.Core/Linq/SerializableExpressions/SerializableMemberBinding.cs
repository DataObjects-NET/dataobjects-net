// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberBinding"/>
  /// </summary>
  [Serializable]
  public abstract class SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberBinding.BindingType"/>
    /// </summary>
    public MemberBindingType BindingType;
    /// <summary>
    /// <see cref="MemberBinding.Member"/>
    /// </summary>
    public MemberInfo Member;
  }
}