// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.31

namespace Xtensive.Aspects.Helpers
{
  /// <summary>
  /// Defines standard aspect message types.
  /// </summary>  
  public enum AspectMessageType
  {
    /// <summary>
    /// "[{0}] attribute is possibly wrongly applied to '{1}'."
    /// </summary>
    AspectPossiblyMissapplied,
    /// <summary>
    /// "[{0}] attribute requires '{1}' to {2}be {3}."
    /// </summary>
    AspectRequiresToBe,
    /// <summary>
    /// "[{0}] attribute requires '{1}' to {2}have {3}."
    /// </summary>
    AspectRequiresToHave,
    /// <summary>
    /// "Multiple [{0}] attributes are applied to '{1}', but there must be a single one."
    /// </summary>
    AspectMustBeSingle,
    /// <summary>
    /// "auto-property"
    /// </summary>
    AutoProperty,
    /// <summary>
    /// "property accessor"
    /// </summary>
    PropertyAccessor,
    /// <summary>
    /// "getter"
    /// </summary>
    Getter,
    /// <summary>
    /// "setter"
    /// </summary>
    Setter,
    /// <summary>
    /// "public"
    /// </summary>
    Public,
    /// <summary>
    /// "non-public"
    /// </summary>
    NonPublic,
    /// <summary>
    /// "not "
    /// </summary>
    Not,
  }
}