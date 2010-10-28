// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.16

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  public class AscxTemplate
    : ContentItem
  {
    [Field]
    public string TemplateTypeName { get; set;}

    [Field]
    public string AscxHeaderFileID { get; set;}

    [Field]
    public string StoredAscxFileID { get; set;}

    [Field]
    public DateTime? LastModificationDate { get; set;}


    public virtual void NotifyModified()
    {
      LastModificationDate = DateTime.Now;
    }
  }
}