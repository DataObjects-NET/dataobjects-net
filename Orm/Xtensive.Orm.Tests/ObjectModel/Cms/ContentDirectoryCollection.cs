// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.03

using System.Runtime.Serialization;
using Xtensive.Orm;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  public class ContentDirectoryCollection<TContentItem>
    : EntitySet<TContentItem>
    where TContentItem : ContentDirectory
  {
    protected override void OnAdding(Entity item)
    {
      ((ContentDirectory) item).WebSite = ((ContentDirectory) Owner).WebSite;
      base.OnAdding(item);
    }

    protected ContentDirectoryCollection(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }

    protected ContentDirectoryCollection(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}