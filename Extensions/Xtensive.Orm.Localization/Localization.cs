// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.27

using System;
using System.Globalization;

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// Base localization class.
  /// </summary>
  [Serializable]
  public abstract class Localization : Entity
  {
    /// <summary>
    /// Gets or sets the name of the culture this particular localization is corresponds to.
    /// </summary>
    /// <value>The name of the culture.</value>
    [Field(Length = 10), Key(0)]
    public string CultureName { get; private set; }


    // Constructor

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="target">The target.</param>
    protected Localization(Session session, CultureInfo culture, Entity target)
      : base(session, culture.Name, target)
    {
    }
  }

  /// <summary>
  /// Base localization class with typed reference to localizable one.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  public abstract class Localization<T> : Localization where T : Entity
  {
    /// <summary>
    /// Gets the target that is being localized.
    /// </summary>
    /// <value>The target.</value>
    [Field, Key(1)]
    [Association(PairTo = "Localizations", OnTargetRemove = OnRemoveAction.Cascade)]
    public T Target { get; private set; }


    // Constructor

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="target">The target.</param>
    protected Localization(Session session, CultureInfo culture, T target)
      : base(session, culture, target)
    {
    }
  }
}