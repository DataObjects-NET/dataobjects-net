// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.20

using System;
using System.Collections;

namespace Xtensive.Core
{
  /// <summary>
  /// Describes possible enumeration states.
  /// </summary>
  [Flags]
  public enum EnumerationState
  {
    /// <summary>
    /// Enumeration isn't started yet - i.e. there were no calls to <see cref="IEnumerator.MoveNext"/>.
    /// Next state can be <see cref="Started"/>, <see cref="Finishing"/> or <see cref="Finished"/>.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Enumeration is started - i.e. there were just sucessful calls to <see cref="IEnumerator.MoveNext"/>.
    /// Next state can be <see cref="Finishing"/> or <see cref="Finished"/>.
    /// </summary>
    Started = 1,

    /// <summary>
    /// Enumeration is finishing - i.e. next call to <see cref="IEnumerator.MoveNext"/> will 
    /// definitely return <see langwrd="false"/>.
    /// Next state can be <see cref="Finished"/>.
    /// </summary>
    Finishing = 3,

    /// <summary>
    /// Enumeration is finished - i.e. any call to <see cref="IEnumerator.MoveNext"/> will fail.
    /// Next state can be <see cref="Finished"/>.
    /// </summary>
    Finished = 4,
  }
}