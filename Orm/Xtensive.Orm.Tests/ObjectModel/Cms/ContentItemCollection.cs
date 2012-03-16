// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.02

using System.Runtime.Serialization;
using Xtensive.Orm;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  public class ContentItemCollection<TContentItem>
    : EntitySet<TContentItem>
    where TContentItem : ContentItem
  {

    protected override void OnAdding(Entity item)
    {
      ((ContentItem) item).WebSite = ((ContentDirectory) Owner).WebSite;
      base.OnAdding(item);
    }


    protected ContentItemCollection(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }

    protected ContentItemCollection(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}