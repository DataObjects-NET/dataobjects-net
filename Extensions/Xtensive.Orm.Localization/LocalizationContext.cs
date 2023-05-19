// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.30

using System;
using System.Globalization;
using System.Threading;
using Xtensive.Core;

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// Localization context.
  /// </summary>
  public class LocalizationContext
  {
    private static LocalizationContext current;

    /// <summary>
    /// Gets or sets the culture of this instance.
    /// </summary>
    /// <value>The culture.</value>
    public CultureInfo Culture { get; private set; }

    /// <summary>
    /// Gets or sets the name of the culture.
    /// </summary>
    /// <value>The name of the culture.</value>
    public string CultureName { get; private set; }

    /// <summary>
    /// Gets or sets the localization policy.
    /// </summary>
    /// <value>The policy.</value>
    public LocalizationPolicy Policy { get; private set; }

    /// <summary>
    /// Gets the current localization context.
    /// </summary>
    /// <value>The current localization context.</value>
    public static LocalizationContext Current
    {
      get
      {
        var scope = LocalizationScope.Current;
        if (scope!=null)
          return scope.Context;

        var threadCulture = Thread.CurrentThread.CurrentCulture;
        if (current==null || (current!=null && current.Culture!=threadCulture))
          current = new LocalizationContext(threadCulture);

        return current;
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationContext"/> class.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <exception cref="ArgumentNullException"/>
    public LocalizationContext(CultureInfo culture)
      : this(culture, LocalizationPolicy.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationContext"/> class.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="policy">The policy.</param>
    /// <exception cref="ArgumentNullException"/>
    public LocalizationContext(CultureInfo culture, LocalizationPolicy policy)
    {
      ArgumentNullException.ThrowIfNull(culture);
      Culture = culture;
      CultureName = culture.Name;
      Policy = policy;
    }
  }
}