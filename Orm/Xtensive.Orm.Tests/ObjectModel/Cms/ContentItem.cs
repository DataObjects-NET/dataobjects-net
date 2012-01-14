// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31


using System;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  [HierarchyRoot]
  public class ContentItem
    : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public WebSite WebSite { get; set; }
    
    [Field, Association(PairTo = "Items", OnTargetRemove = OnRemoveAction.Cascade)]
    public ContentDirectory ParentDirectory {
      get { return GetFieldValue<ContentDirectory>("ParentDirectory");}
      set {
        var oldPath = ParentDirectory==null ? Name : GetPath();
        SetFieldValue("ParentDirectory", value);
      }
    }

    [Field]
    public string Name { 
      get { return GetFieldValue<string>("Name");}
      set {
        if (value==null)
          throw new ArgumentException("value");
        var oldPath = ParentDirectory==null ? Name : GetPath();
        var newValue = value.ToLowerInvariant();
        SetFieldValue("Name", newValue);
      }
    }
    
    public string GetPath()
    {
      string directoryPath = ParentDirectory==null ? null : ParentDirectory.GetPath();
      if (String.IsNullOrEmpty(directoryPath))
        return Name;
      return directoryPath + "/" + Name;
    }

    protected override void OnValidate()
    {
      base.OnValidate();
      if (WebSite == null)
        throw new InvalidOperationException("WebSite");
      if (ParentDirectory == null)
        throw new InvalidOperationException("ParentDirectory");
    }
  }
}