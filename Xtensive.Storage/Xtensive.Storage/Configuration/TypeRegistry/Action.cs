// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using System;
using System.Reflection;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// Represents a single registration call to <see cref="Registry"/>.
  /// </summary>
  [Serializable]
  public sealed class Action: IEquatable<Action>
  {
    private readonly Assembly assembly;
    private readonly string nameSpace;

    /// <summary>
    /// Gets the assembly.
    /// </summary>
    public Assembly Assembly
    {
      get { return assembly; }
    }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    public string Namespace
    {
      get { return nameSpace; }
    }

    /// <summary>
    /// Determines whether two <see cref="Action"/> instances are equal.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current <see langword="Object"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the 
    /// current <see langword="Object"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as Action);
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see langword="GetHashCode"/> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return assembly.GetHashCode() + 29*(nameSpace != null ? nameSpace.GetHashCode() : 0);
    }

    #region IEquatable<Action> Members

    /// <summary>
    /// Determines whether two <see cref="Action"/> instances are equal.
    /// </summary>
    /// <param name="call">The <see cref="Action"/> to compare with the current <see langword="Object"/>.</param>
    /// <returns><see langword="true"/> if the specified <see cref="Action"/> is equal to the 
    /// current <see langword="Object"/>; otherwise, <see langword="false"/>.</returns>
    public bool Equals(Action call)
    {
      if (call == null)
        return false;
      return Equals(assembly, call.assembly) && Equals(nameSpace, call.nameSpace);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Action"/> class.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="nameSpace">The namespace.</param>
    public Action(Assembly assembly, string nameSpace)
      : this(assembly)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(nameSpace, "nameSpace");
      this.nameSpace = nameSpace;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Action"/> class.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public Action(Assembly assembly)
    {
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      this.assembly = assembly;
    }
  }
}