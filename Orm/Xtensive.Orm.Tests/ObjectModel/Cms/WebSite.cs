// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  public class WebSite
    : ContentDirectory
  {
    [Field]
    public string Title { get; set;}

    [Field, Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<WebSiteUrl> Urls { get; private set; }

    [Field, Association(OnOwnerRemove = OnRemoveAction.Clear)]
    public Page MissingPage { get; set;}
    
    public WebSite()
    {
      WebSite = this;
      Name = "";
    }
  }
}