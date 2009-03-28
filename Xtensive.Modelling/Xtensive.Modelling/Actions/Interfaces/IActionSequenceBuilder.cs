// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// <see cref="NodeAction"/> builder.
  /// </summary>
  public interface INodeActionSequenceBuilder
  {
    /// <summary>
    /// Builds the specified sequence by appending
    /// new <see cref="NodeAction"/> objects to it.
    /// </summary>
    /// <param name="sequence">The sequence to build.</param>
    void Build(ActionSequence sequence);
  }
}