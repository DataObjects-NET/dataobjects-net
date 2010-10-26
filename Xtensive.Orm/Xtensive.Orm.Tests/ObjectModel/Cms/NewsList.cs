// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.12.14

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  public class NewsList
    : ContentItem
  {
    public NewsDirectory NewsDirectory { get; set;}

    public int? PageSize { get; set;}

    [Field]
    public AscxTemplate Template { get; set;}
  }
}