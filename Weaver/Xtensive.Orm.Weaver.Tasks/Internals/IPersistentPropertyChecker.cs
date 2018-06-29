// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.06.23

namespace Xtensive.Orm.Weaver
{
  /// <summary>
  /// Declares API of persistent property checher
  /// </summary>
  internal interface IPersistentPropertyChecker
  {
    /// <summary>
    /// Returns whether there were some properties used to be processed but have to be skiped for some reason
    /// </summary>
    bool HasSkippedProperties { get; }

    /// <summary>
    /// Checks whether the property should be processed.
    /// </summary>
    /// <param name="property">Property information.</param>
    /// <param name="context">Processor context.</param>
    /// <returns>Result of check</returns>
    bool ShouldProcess(PropertyInfo property, ProcessorContext context);
  }
}