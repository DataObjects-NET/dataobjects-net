// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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