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
  [Serializable]
  [DebuggerDisplay("Type = {Type}, Assembly = {Assembly}, Namespace = {Namespace}")]
  public sealed class TypeRegistration : IEquatable<TypeRegistration>
  {
    private readonly Type type;
    private readonly Assembly assembly;
    private readonly string @namespace;

    /// <summary>
    /// Gets the type registered by this action.
    /// </summary>
    public Type Type
    {
      get { return type; }
    }

    /// <summary>
    /// Gets the assembly registered by this action.
    /// </summary>
    public Assembly Assembly
    {
      get { return assembly; }
    }

    /// <summary>
    /// Gets the namespace registered by this action.
    /// </summary>
    public string Namespace
    {
      get { return @namespace; }
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(TypeRegistration other)
    {
      if (other == null)
        return false;
      return 
        type==other.type && 
        assembly==other.assembly && 
        @namespace==other.@namespace;
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
        int result = (type!=null ? type.GetHashCode() : 0);
        result = (result * 397) ^ (assembly!=null ? assembly.GetHashCode() : 0);
        result = (result * 397) ^ (@namespace!=null ? @namespace.GetHashCode() : 0);
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
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      this.type = type;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    public TypeRegistration(Assembly assembly)
    {
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      this.assembly = assembly;
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
      this.@namespace = @namespace;
    }
  }
}