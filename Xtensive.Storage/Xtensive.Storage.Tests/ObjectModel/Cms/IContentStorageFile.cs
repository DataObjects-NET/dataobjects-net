// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.01

using System.IO;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
  public interface IContentStorageFile
  {
    string ID { get; }

    Stream Open();
  }
}