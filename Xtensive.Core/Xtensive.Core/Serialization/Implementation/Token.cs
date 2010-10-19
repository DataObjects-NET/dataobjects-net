// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.28

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Uniquely identifies the <see cref="Value"/> in current
  /// <see cref="SerializationContext"/>.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("#{Identifier}, Value = {Value}")]
  public abstract class Token
  {
    private int identifier;
    private object value;

    /// <summary>
    /// Gets or sets the identifier of the token.
    /// Note: normally identifier changes (becomes actual) 
    /// on the first token serialization.
    /// </summary>
    public int Identifier {
      get { return identifier; }
      set {
        int oldIdentifier = identifier;
        try {
          identifier = value;
          SerializationContext.Current.TokenManager.Update(this, oldIdentifier);
        }
        catch {
          identifier = oldIdentifier;
          throw;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the token.
    /// </summary>
    public object Value {
      get { return value; }
      private set { this.value = value; }
    }

    #region Get, GetOrCreate static methods

    /// <summary>
    /// Gets the token with specified <see cref="Value"/>
    /// using current <see cref="SerializationContext"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <param name="value">The value to get the token for.</param>
    /// <returns>
    /// Found token;
    /// <see langword="null"/>, if there is no token with specified value.
    /// </returns>
    public static Token<T> Get<T>(SerializationContext context, T value)
    {
      return (Token<T>) context.TokenManager.Get(value);
    }

    /// <summary>
    /// Gets the token with specified <see cref="Identifier"/>
    /// using current <see cref="SerializationContext"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <param name="identifier">The identifier to get the token for.</param>
    /// <returns>
    /// Found token;
    /// <see langword="null"/>, if there is no token with specified identifier.
    /// </returns>
    public static Token<T> Get<T>(SerializationContext context, int identifier)
    {
      return (Token<T>) context.TokenManager.Get(identifier);
    }

    /// <summary>
    /// Gets or creates the token with specified value
    /// using current <see cref="SerializationContext"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <param name="value">The value to get or create the token for.</param>
    /// <returns>
    /// Found or newly created token with the specified value.
    /// </returns>
    public static Token<T> GetOrCreate<T>(SerializationContext context, T value)
    {
      var tokenManager = context.TokenManager;
      var token = (Token<T>) tokenManager.Get(value);
      if (token==null)
        token = new Token<T>(value, ~tokenManager.GetNextIdentifier());
      return token;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    /// <param name="identifier">The <see cref="Identifier"/> property value.</param>
    protected Token(object value, int identifier)
    {
      this.value = value;
      this.identifier = identifier;
    }
  }
}