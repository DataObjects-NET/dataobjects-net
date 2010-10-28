// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.27

using System;
using System.Globalization;
using Xtensive.Orm;

namespace Xtensive.Practices.Localization.Model
{
  [Serializable]
  [HierarchyRoot]
  public class PageLocalization : Localization<Page>
  {
    [Field(Length = 100)]
    public string Title { get; set; }

    [Field]
    public string Content { get; set; }

    [Field]
    public string MyContent { get; set; }

    public PageLocalization(CultureInfo culture, Page target)
      : base(culture, target)
    {
    }
  }
}