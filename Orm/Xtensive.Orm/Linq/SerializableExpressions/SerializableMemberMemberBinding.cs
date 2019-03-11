// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.15

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xtensive.Linq.SerializableExpressions.Internals;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberMemberBinding"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableMemberMemberBinding : SerializableMemberBinding
  {
    /// <summary>
    /// <see cref="MemberMemberBinding.Bindings"/>
    /// </summary>
    public SerializableMemberBinding[] Bindings;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Bindings", Bindings);
    }

    public SerializableMemberMemberBinding()
    {
    }

    public SerializableMemberMemberBinding(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Bindings = info.GetArrayFromSerializableForm<SerializableMemberBinding>("Bindings");
    }
  }
}