// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.27

using System;
using Xtensive.Orm;

namespace Xtensive.Orm.Localization.Tests.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Page : Entity, ILocalizable<PageLocalization>
  {
    [Field, Key]
    public int Id { get; private set; }

    // Non-persistent field
    public string Title
    {
      get { return Localizations.Current.Title; }
      set { Localizations.Current.Title = value; }
    }

    // Non-persistent field
    public string Content
    {
      get { return Localizations.Current.Content; }
      set { Localizations.Current.Content = value; }
    }

    /// <inheritdoc/>
    [Field]
    public LocalizationSet<PageLocalization> Localizations { get; private set; }

    public Page(Session session)
      : base(session)
    {
    }
  }
}