// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
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