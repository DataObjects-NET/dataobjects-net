// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.16

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  public class Menu
    : ContentItem
  {
    [Field]
    public string StoredFileID { get; set;}

    [Field]
    public AscxTemplate Template { get; set;}
  }
}