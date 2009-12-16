// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.12.14

using Xtensive.Storage;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
  public class NewsList
    : ContentItem
  {
    public NewsDirectory NewsDirectory { get; set;}

    public int? PageSize { get; set;}

    [Field]
    public AscxTemplate Template { get; set;}
  }
}