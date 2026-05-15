// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="MemberInfo"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableMemberInfo
  {
    [DataMember, JsonInclude]
    public string DeclaredType;

    [DataMember, JsonInclude]
    public string Member;

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="Type"/> to <see cref="SerializableMemberInfo"/>.
    /// </summary>
    /// <param name="member">The member to create serializable version.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator SerializableMemberInfo(MemberInfo member)
    {
      if (member == null)
        return null;
      return new SerializableMemberInfo {
        DeclaredType = member.DeclaringType.AssemblyQualifiedName,
        Member = member.ToString()
      };
    }

    /// <summary>
    /// Implicit conversion of <see cref="SerializableMemberInfo"/> to <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="reference">The serializable version to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator MemberInfo(SerializableMemberInfo reference)
    {
      if (reference == null)
        return null;

      var name = reference.Member;
      var member = Type.GetType(reference.DeclaredType).GetMembers().First(m => m.ToString() == name);
      return member;
    }

    #endregion
  }
}