// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.08.31

using System;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{  
  [Serializable]
  [HierarchyRoot]
  public class ContentDirectory
    : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public PageTemplate DefaultTemplate { get; set;}
    
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public Page DefaultPage { get; set;}

    [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public WebSite WebSite { get; set; }
    
    [Field]
    public string Name { 
      get { return GetFieldValue<string>("Name");}
      set {
        if (value==null)
          throw new ArgumentException("value");
        var oldPath = ParentDirectory==null ? Name : GetPath();
        SetFieldValue("Name", value.ToLowerInvariant());
        var newPath = GetPath();
      }
    }

    [Field, Association(PairTo = "Directories", OnTargetRemove = OnRemoveAction.Cascade)]
    public ContentDirectory ParentDirectory {
      get{ return GetFieldValue<ContentDirectory>("ParentDirectory");}
      set {
        var oldPath = ParentDirectory==null ? Name : GetPath();
        SetFieldValue("ParentDirectory", value);
        var newPath = GetPath();
      }
    }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public ContentItemCollection<ContentItem> Items { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public ContentDirectoryCollection<ContentDirectory> Directories { get; private set; }

    public ContentDirectory GetSubDirectory(string subDirectoryName)
    {
      return Session.Demand().Query.All<ContentDirectory>().Where(dir => dir.Name==subDirectoryName && dir.ParentDirectory==this).FirstOrDefault();
    }

    public ContentItem GetContentItem(string contentItemName)
    {
      return Session.Demand().Query.All<ContentItem>().Where(item => item.Name==contentItemName && item.ParentDirectory==this).FirstOrDefault();
    }

    private void BuildPathRecursive(StringBuilder stringBuilder)
    {
      if (ParentDirectory!=null) {
        ParentDirectory.BuildPathRecursive(stringBuilder);
        stringBuilder.Append('/');
      }
      stringBuilder.Append(Name);
    }
    
    public string GetPath()
    {
      var result = new StringBuilder();
      BuildPathRecursive(result);
      return result.ToString();
    }

    protected override void OnValidate()
    {
      base.OnValidate();
      if (WebSite==null)
        throw new InvalidOperationException("WebSite");
      if (ParentDirectory==null && WebSite!=this)
        throw new InvalidOperationException("ParentDirectory");
    }

    public bool IsSubdirectoryOf(ContentDirectory directory)
    {
      ContentDirectory parent = ParentDirectory;
      while(parent!=null) {
        if (parent==directory)
          return true;
        parent = parent.ParentDirectory;
      }
      return false;
    }
  }
}