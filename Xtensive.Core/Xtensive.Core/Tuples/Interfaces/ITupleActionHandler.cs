// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Base interface for defining actions operating with tuple fields.
  /// </summary>
  /// <typeparam name="TActionData">The type of intermediate data holder for this action.</typeparam>
  public interface ITupleActionHandler<TActionData>
    where TActionData: struct
  {
    /// <summary>
    /// Handles execution of some action for specified tuple field providing
    /// tuple field type as generic method argument.
    /// </summary>
    /// <param name="actionData">Parameter of the action.</param>
    /// <param name="fieldIndex">Index of the field the action is executed for.</param>
    /// <returns><see langword="True"/>, execution of action handlers should be stopped;
    /// otherwise, <see langword="false"/>.</returns>
    /// <typeparam name="TFieldType">The type of field this action is executed for.</typeparam>
    bool Execute<TFieldType>(ref TActionData actionData, int fieldIndex);
  }
}