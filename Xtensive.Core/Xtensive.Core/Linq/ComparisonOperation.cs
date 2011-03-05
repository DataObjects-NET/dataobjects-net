// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

namespace Xtensive.Linq
{
  /// <summary>
  /// Comparison operation.
  /// </summary>
  public enum ComparisonOperation
  {
    /// <summary>
    /// The operation is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The equality operation.
    /// </summary>
    Equal,

    /// <summary>
    /// The non-equality operation.
    /// </summary>
    NotEqual,

    /// <summary>
    /// The 'less than' operation.
    /// </summary>
    LessThan,

    /// <summary>
    /// The 'less than or equal' operation
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// The 'greater than' operation.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// The 'greater than or equal' operation.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// The operation corresponding to a call to one of following methods: 
    /// <see cref="string.StartsWith(string)"/>, 
    /// <see cref="string.StartsWith(string,bool,System.Globalization.CultureInfo)"/>, 
    /// <see cref="string.StartsWith(string,System.StringComparison)"/>.
    /// </summary>
    LikeStartsWith,

    /// <summary>
    /// The operation corresponding to a call to one of following methods: 
    /// <see cref="string.EndsWith(string)"/>, 
    /// <see cref="string.EndsWith(string,bool,System.Globalization.CultureInfo)"/>, 
    /// <see cref="string.EndsWith(string,System.StringComparison)"/>.
    /// </summary>
    LikeEndsWith,

    /// <summary>
    /// The negation of the 'like starts with' operation.
    /// </summary>
    NotLikeStartsWith,

    /// <summary>
    /// The negation of the 'like ends with' operation.
    /// </summary>
    NotLikeEndsWith
  }
}