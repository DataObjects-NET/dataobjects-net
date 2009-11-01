// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Base interface for defining functions operating with tuple fields.
  /// </summary>
  /// <typeparam name="TFunctionData">The type of intermediate data holder for this function.</typeparam>
  /// <typeparam name="TResult">The type of action result.</typeparam>
  public interface ITupleFunctionHandler<TFunctionData, TResult>: ITupleActionHandler<TFunctionData>
    where TFunctionData: struct, ITupleFunctionData<TResult>
  {
  }
}