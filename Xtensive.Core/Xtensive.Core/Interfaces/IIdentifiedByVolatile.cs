// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Raised when <see cref="IIdentified.Identifier"/> value is changed.
  /// </summary>
  /// <param name="identified">Object, which identifier is changed.</param>
  /// <param name="oldIdentifier">Old identifier value.</param>
  public delegate void IdentifierChanged(IIdentified identified, object oldIdentifier);


  /// <summary>
  /// Indicates that type is possibly identified by volatile
  /// (temporary) identifier.
  /// </summary>
  public interface IIdentifiedByVolatile: IIdentified
  {
    /// <summary>
    /// Indicates that <see cref="IIdentified.Identifier"/> is
    /// volatile. 
    /// Value of this property can change from <see langword="true"/>
    /// to <see langword="false"/>, but never vice versa.
    /// </summary>
    bool IsIdentifierVolatile { get; }

    /// <summary>
    /// Raised when <see cref="IIdentified.Identifier"/> value is changed.
    /// </summary>
    event IdentifierChanged IdentifierChanged;
  }

  /// <summary>
  /// Indicates that type is possibly identified by volatile
  /// (temporary) identifier.
  /// </summary>
  /// <typeparam name="T">The type of identifier.</typeparam>
  public interface IIdentifiedByVolatile<T> : IIdentifiedByVolatile, IIdentified<T>
  {
  }
}