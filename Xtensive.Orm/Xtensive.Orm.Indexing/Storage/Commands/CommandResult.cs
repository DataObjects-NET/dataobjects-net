// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.02

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// An abstract base class for <see cref="Command"/> execution result.
  /// </summary>
  [Serializable]
  public abstract class CommandResult : IEquatable<CommandResult>
  {
    /// <summary>
    /// Gets the result value.
    /// <see langword="null" />, if <see cref="Error"/> is set.
    /// </summary>
    public object Value {
      get { return GetValueInternal(); }
    }

    /// <summary>
    /// Gets the error.
    /// If set, the <see cref="Value"/> is <see langword="null" />.
    /// </summary>
    public Exception Error { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Error"/> != <see langword="null" />.
    /// </summary>
    public bool HasError {
      get { return Value!=null; }
    }

    /// <summary>
    /// Gets a value indicating whether this result is a default one.
    /// </summary>
    public abstract bool IsDefault { get; }

    /// <summary>
    /// Gets the untyped result value.
    /// </summary>
    /// <returns>Untyped result value.</returns>
    protected abstract object GetValueInternal();

    #region Equality members

    /// <inheritdoc/>
    public abstract bool Equals(CommandResult obj);

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (CommandResult))
        return false;
      return Equals((CommandResult) obj);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(CommandResult left, CommandResult right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(CommandResult left, CommandResult right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return HasError ? Error.ToString() : Value.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected CommandResult()
    {
    }
  }
}