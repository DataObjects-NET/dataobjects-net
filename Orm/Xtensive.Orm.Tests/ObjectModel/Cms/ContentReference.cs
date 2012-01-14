// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.12.14

using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  [HierarchyRoot]
  public class ContentReference
    : Entity
  {
    [Field, Key]
    public int ID { get; private set;}

    [Field, Association(PairTo = "References", OnTargetRemove = OnRemoveAction.Cascade)]
    public HtmlContentItem Owner { get; set;}

    [Field]
    public ContentReferenceType ReferenceType { get; set;}

    [Field]
    public WebSite WebSite { get; set;}
    
    [Field]
    public string ContentPath { get; set;}

    [Field]
    public string AbsoluteContentPath {
      get { return GetFieldValue<string>("AbsoluteContentPath");}
      set {
        if (value==null)
          throw new ArgumentException("value");
        value = value.TrimStart('/');
        SetFieldValue("AbsoluteContentPath", value);
      }
    }

    [Field]
    public int ContentID { get; set;}
  }
}