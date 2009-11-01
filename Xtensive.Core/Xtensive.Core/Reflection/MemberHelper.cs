// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.13

using System.Reflection;
using System.Text;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// <see cref="MemberInfo"/> related helper \ extension methods.
  /// </summary>
  public static class MemberHelper
  {
    /// <summary>
    /// Builds correct full generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <returns>Full member name.</returns>
    public static string GetFullName(this MemberInfo member)
    {
      return new StringBuilder()
        .Append(member.DeclaringType.GetFullName())
        .Append(".")
        .Append(member.Name)
        .ToString();
    }

    /// <summary>
    /// Builds correct short generic type and member name.
    /// </summary>
    /// <param name="member">A <see cref="MemberInfo"/> object which name is built.</param>
    /// <returns>Short member name.</returns>
    public static string GetShortName(this MemberInfo member)
    {
      return new StringBuilder()
        .Append(member.DeclaringType.GetShortName())
        .Append(".")
        .Append(member.Name)
        .ToString();
    }
  }
}