// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.30

using System;
using System.Globalization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// Set of localizations of <typeparamref name="TItem"/> type.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class LocalizationSet<TItem> : EntitySet<TItem> where TItem : Localization
  {
    private TItem current;

    /// <summary>
    /// Gets the appropriate <see paramref="TItem"/> according to the specified culture.
    /// </summary>
    public TItem this[CultureInfo culture]
    {
      get
      {
        var key = Key.Create(Session.Domain, Session.StorageNode.Id, typeof (TItem), TypeReferenceAccuracy.BaseType, culture.Name, Owner.Key);
        var result = Session.Query.SingleOrDefault<TItem>(key);
        if (result!=null)
          return result;

        // If we are here, then requested localization is absent, we are to create new or wrap the default one
        // TODO: Apply policy rules. For now we create new item.
        result = Create(culture);
        return result;
      }
    }

    /// <summary>
    /// Gets the currently active localization.
    /// </summary>
    /// <value>The current localization.</value>
    public TItem Current
    {
      get { return GetCurrent(); }
    }

    private TItem GetCurrent()
    {
      var context = LocalizationContext.Current;

      // Checking whether current localization fits current localization context
      if (current != null && current.CultureName == context.CultureName)
        return current;

      // If current localization is absent, fetching the requested one
      var result = this[context.Culture];
      if (result != null)
        current = result;
      return result;
    }

    private TItem Create(CultureInfo culture)
    {
      // TODO: Cache delegate on type initialization stage
      return (TItem) Activator.CreateInstance(typeof (TItem), Session, culture, Owner);
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="field">The field.</param>
    public LocalizationSet(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}