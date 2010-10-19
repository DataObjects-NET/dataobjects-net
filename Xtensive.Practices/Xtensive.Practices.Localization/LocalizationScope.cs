// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.01

using System.Globalization;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Practices.Localization
{
  /// <summary>
  /// Scope of localization.
  /// </summary>
  public class LocalizationScope : Scope<LocalizationContext>
  {
    /// <summary>
    /// Gets the current localization scope.
    /// </summary>
    /// <value>The current localization scope. Returns <see langword="null" /> in case it is absent.</value>
    public static LocalizationScope Current
    {
      get { return (LocalizationScope) CurrentScope; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new LocalizationContext Context
    {
      get { return CurrentContext; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="culture">The culture.</param>
    public LocalizationScope(CultureInfo culture)
      : this(culture, LocalizationPolicy.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="policy">The policy.</param>
    public LocalizationScope(CultureInfo culture, LocalizationPolicy policy)
      : base(new LocalizationContext(culture, policy))
    {
    }
  }
}