// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
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