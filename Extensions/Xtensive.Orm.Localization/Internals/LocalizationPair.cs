// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// A pair of localizable entity and its localization.
  /// </summary>
  /// <typeparam name="TTarget">The type of the target.</typeparam>
  /// <typeparam name="TLocalization">The type of the localization.</typeparam>
  public struct LocalizationPair<TTarget, TLocalization> where TTarget: Entity where TLocalization: Localization<TTarget>
  {
    /// <summary>
    /// Gets or sets the localizable entity.
    /// </summary>
    public TTarget Target { get; private set; }

    /// <summary>
    /// Gets or sets the localization.
    /// </summary>
    public TLocalization Localization { get; private set; }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="localization">The localization.</param>
    public LocalizationPair(TTarget target, TLocalization localization)
      : this()
    {
      Target = target;
      Localization = localization;
    }
  }
}