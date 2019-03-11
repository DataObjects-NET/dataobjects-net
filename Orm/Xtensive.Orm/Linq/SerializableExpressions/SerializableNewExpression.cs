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

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddArray("Arguments", Arguments);
      info.AddValue("Ctor", Constructor.ToSerializableForm());
      var memberNames = new string[Members.Length];
      for (int i = 0; i < memberNames.Length; i++)
        memberNames[i] = Members[i].ToSerializableForm();
      info.AddArray("Members", memberNames);
    }

    public SerializableNewExpression()
    {
    }

    public SerializableNewExpression(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Arguments = info.GetArrayFromSerializableForm<SerializableExpression>("Arguments");
      Constructor = info.GetString("Ctor").GetConstructorFromSerializableForm();
      var memberNames = info.GetArrayFromSerializableForm<string>("Members");
      Members = new MemberInfo[memberNames.Length];
      for (int i = 0; i < memberNames.Length; i++)
        Members[i] = memberNames[i].GetMemberFromSerializableForm();
    }
  }
}