// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Describes a single type registration call to <see cref="TypeRegistry"/>.
  /// </summary>
  [DebuggerDisplay("Type = {Type}, Assembly = {Assembly}, Namespace = {Namespace}")]
  public sealed class TypeRegistration : IEquatable<TypeRegistration>
  {
    /// <summary>
    /// Gets the type registered by this action.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the assembly registered by this action.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the namespace registered by this action.
    /// </summary>
    public string Namespace { get; }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(TypeRegistration other)
    {
      if (other is null)
        return false;
      return 
        Type==other.Type && 
        Assembly==other.Assembly && 
        Namespace==other.Namespace;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as TypeRegistration);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Type is not null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ (Assembly is not null ? Assembly.GetHashCode() : 0);
        result = (result * 397) ^ (Namespace is not null ? Namespace.GetHashCode() : 0);
        return result;
      }
    }

    /// <inheritdoc/>
    public static bool operator ==(TypeRegistration left, TypeRegistration right)
    {
      return Equals(left, right);
    }

    /// <inheritdoc/>
    public static bool operator !=(TypeRegistration left, TypeRegistration right)
    {
      return !Equals(left, right);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="type">The type to register.</param>
    public TypeRegistration(Type type)
    {
      Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    public TypeRegistration(Assembly assembly)
    {
      Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    /// <param name="namespace">The namespace to register.</param>
    public TypeRegistration(Assembly assembly, string @namespace)
      : this(assembly)
    {
      ArgumentValidator.EnsureArgumentNotNull(@namespace, "@namespace");
      Namespace = @namespace;
    }
  }
}