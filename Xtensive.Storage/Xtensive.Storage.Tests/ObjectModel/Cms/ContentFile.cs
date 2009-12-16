// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
  public class ContentFile
    : ContentItem
  {
    [Field]
    public string StoredFileID { get; set;}

    [Field]
    public DateTime? LastModificationDate { get; set;}


    public virtual void NotifyModified()
    {
      LastModificationDate = DateTime.Now;
    }
  }
}