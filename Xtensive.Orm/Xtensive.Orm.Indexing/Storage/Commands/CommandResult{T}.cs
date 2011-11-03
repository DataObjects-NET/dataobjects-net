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
  /// Typed version of <see cref="CommandResult"/>.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="CommandResult.Value"/></typeparam>
  [Serializable]
  public sealed class CommandResult<T> : CommandResult, IEquatable<CommandResult<T>>
  {
    private static CommandResult<T> @default = new CommandResult<T>(default(T));

    /// <summary>
    /// Gets the default command result for <typeparamref name="T"/> type.
    /// </summary>
    public static CommandResult<T> Default {
      get { return @default; }
    }

    /// <summary>
    /// Gets the result value.
    /// <see langword="null"/>, if <see cref="CommandResult.Error"/> is set.
    /// </summary>
    public new T Value { get; private set; }

    /// <inheritdoc/>
    public override bool IsDefault {
      get {
        return !HasError && Equals(Value, default(T));
      }
    }

    /// <inheritdoc/>
    protected override object GetValueInternal()
    {
      return Value;
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(CommandResult<T> obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.Value, Value) && Equals(obj.Error, Error);
    }

    /// <inheritdoc/>
    public override bool Equals(CommandResult obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.Value, Value) && Equals(obj.Error, Error);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (Value.GetHashCode() * 397) ^ (Error!=null ? Error.GetHashCode() : 0);
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The value.</param>
    public CommandResult(T value)
    {
      Value = value;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="error">The error.</param>
    public CommandResult(Exception error)
    {
      Error = error;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// Sets <see cref="CommandResult.Error"/> property to
    /// caught exception, if <paramref name="valueGenerator"/>
    /// fails.
    /// </summary>
    /// <param name="valueGenerator">The value generator.</param>
    public CommandResult(Func<T> valueGenerator)
    {
      try {
        Value = valueGenerator.Invoke();
      }
      catch (Exception e) {
        Error = e;
      }
    }
  }
}