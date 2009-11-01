// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.27

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// An intermediate data holder for 
  /// <see cref="ITupleFunctionHandler{TActionInfo,TResult}"/>
  /// </summary>
  /// <typeparam name="TResult">The type of function result.</typeparam>
  public interface ITupleFunctionData<TResult>
  {
    /// <summary>
    /// Gets function result.
    /// </summary>
    TResult Result { get; }
  }
}