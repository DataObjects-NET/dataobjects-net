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
  /// A serializable representation of <see cref="MethodInfo"/>.
  /// </summary>
  [Serializable]
  [DataContract]
  public sealed class SerializableMethodInfo
  {
    [DataMember, JsonInclude]
    public string DeclaredType;

    [DataMember, JsonInclude]
    public string Name;

    [DataMember, JsonInclude]
    public SerializableType[] GenericParameters;

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="MethodInfo"/> to <see cref="SerializableMethodInfo"/>.
    /// </summary>
    /// <param name="method">The method to create reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator SerializableMethodInfo(MethodInfo method)
    {
      if (method == null)
        return null;
      var type = method.DeclaringType.AssemblyQualifiedName;
      var reference = new SerializableMethodInfo { DeclaredType = type };
      reference.Name = (method.IsGenericMethod) ? method.GetGenericMethodDefinition().ToString() : method.ToString();

      if (method.IsGenericMethod) {
        reference.GenericParameters = method.GetGenericArguments().Select(ty => (SerializableType) ty).ToArray();
      }
      return reference;
    }

    /// <summary>
    /// Implicit conversion of <see cref="SerializableMethodInfo"/> to <see cref="MethodInfo"/>.
    /// </summary>
    /// <param name="reference">The serializable version to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator MethodInfo(SerializableMethodInfo reference)
    {
      if (reference == null)
        return null;
      var name = reference.Name;
      var type = Type.GetType(reference.DeclaredType);
      var method = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault(m => m.ToString() == name);
      if (method == null)
        method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault(m => m.ToString() == name);
      if (method == null)
        throw new ArgumentException("There is no such method found");

      if (method.IsGenericMethod)
        method = method.MakeGenericMethod(reference.GenericParameters.Select(p => (Type) p).ToArray());
      return method;
    }

    #endregion
  }
}