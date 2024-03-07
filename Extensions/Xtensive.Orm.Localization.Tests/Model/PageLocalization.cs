// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.11.27

using System.Globalization;
using Xtensive.Orm;

namespace Xtensive.Orm.Localization.Tests.Model
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

    public PageLocalization(Session session, CultureInfo culture, Page target)
      : base(session, culture, target)
    {
    }
  }
}