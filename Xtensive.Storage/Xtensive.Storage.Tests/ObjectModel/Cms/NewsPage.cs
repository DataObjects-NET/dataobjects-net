// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.12.14

using System;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
  [Serializable]
  public class NewsPage 
    : Page
  {
    [Field]
    public DateTime? NewsDate { get; set;}
  }
}