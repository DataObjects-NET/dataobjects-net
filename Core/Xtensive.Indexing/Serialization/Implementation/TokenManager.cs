// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.28

using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Serialization.Implementation
{
  /// <summary>
  /// Manages <see cref="Token"/>s in the <see cref="SerializationContext"/>.
  /// </summary>
  public class TokenManager
  {
    private readonly Dictionary<object, Token> objToToken = new Dictionary<object, Token>();
    private readonly Dictionary<int, Token> idToToken = new Dictionary<int, Token>();
    private int nextIdentifier;

    /// <summary>
    /// Adds (registers) the specified token.
    /// </summary>
    /// <param name="token">The token to add.</param>
    public virtual void Add(Token token)
    {
      objToToken.Add(token.Value, token);
      try {
        idToToken.Add(token.Identifier, token);
      }
      catch {
        objToToken.Remove(token.Value);
        throw;
      }
    }

    /// <summary>
    /// Gets the added token by its <see cref="Token.Value"/>.
    /// </summary>
    /// <param name="value">The value of the <see cref="Token"/> to get.</param>
    /// <returns>Found token;
    /// <see langword="null" />, if there is no token with specified value.</returns>
    public virtual Token Get(object value)
    {
      Token result;
      if (objToToken.TryGetValue(value, out result))
        return result;
      else
        return null;
    }

    /// <summary>
    /// Gets the added token by its <see cref="Token.Identifier"/>.
    /// </summary>
    /// <param name="identifier">The identifier of the <see cref="Token"/> to get.</param>
    /// <returns>Found token;
    /// <see langword="null" />, if there is no token with specified identifier.</returns>
    public virtual Token Get(int identifier)
    {
      Token result;
      if (idToToken.TryGetValue(identifier, out result))
        return result;
      else
        return null;
    }

    /// <summary>
    /// Updates the internal structures on change of token <see cref="Token.Identifier"/>.
    /// </summary>
    /// <param name="token">The token which identifier is changed.</param>
    /// <param name="oldIdentifier">The old identifier value.</param>
    public void Update(Token token, int oldIdentifier)
    {
      idToToken.Remove(oldIdentifier);
      try {
        idToToken.Add(token.Identifier, token);
      }
      catch {
        objToToken.Remove(token.Value);
        throw;
      }
    }

    /// <summary>
    /// Gets the next <see cref="Token.Identifier"/> property value.
    /// All the generated identifiers must be positive.
    /// </summary>
    /// <returns>Next <see cref="Token.Identifier"/> property value.</returns>
    public virtual int GetNextIdentifier()
    {
      return nextIdentifier++;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TokenManager()
    {
    }
  }
}