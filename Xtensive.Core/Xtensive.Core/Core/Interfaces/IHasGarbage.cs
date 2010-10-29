// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.09

namespace Xtensive.Core
{
  /// <summary>
  /// Describes a class that might have some "garbage" inside it during the operation.
  /// </summary>
  public interface IHasGarbage
  {
    /// <summary>
    /// Collects the garbage.
    /// </summary>
    /// <remarks>
    /// <note type="caution" id="Caution">This method usually takes the time proportional to the size of the object it is invoked on.</note>
    /// </remarks>
    void CollectGarbage();
  }
}