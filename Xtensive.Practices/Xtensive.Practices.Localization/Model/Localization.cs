// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.27

using System;
using System.Globalization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage;

namespace Xtensive.Practices.Localization.Model
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="target">The target.</param>
    protected Localization(CultureInfo culture, Entity target)
      : base(culture.Name, target)
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="target">The target.</param>
    protected Localization(CultureInfo culture, T target)
      : base(culture, target)
    {
    }
  }
}