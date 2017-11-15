// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  public class Page
    : HtmlContentItem
  {
    [Field]
    public string KeyWords { get; set;}
    
    [Field]
    public string Title { get; set;}

    [Field]
    public string Description { get; set;}

    [Field]
    public bool UseDefaultTemplate { get; set;}

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public PageTemplate Template { get; set;}
  }
}