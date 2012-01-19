// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.08

using System;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests.ObjectModel.Cms
{
  [Serializable]
  [HierarchyRoot]
  public class WebSiteUrl
    : Entity
  {
    [Field, Key]
    public int ID { get; private set;}
    
    [Field]
    public string Host { 
      get { return GetFieldValue<string>("Host");}
      set {
        if (value!=null) {
          SetFieldValue("Host", value.ToLowerInvariant());
        }
      }
    }

    [Field]
    public string Path { 
      get { return GetFieldValue<string>("Path");}
      set {
        if (value!=null) {
          if (value[0]!='/')
            throw new ArgumentException("Url path should start with '/'", "value");
          SetFieldValue("Path", value.ToLowerInvariant());
        }
      }
    }

    [NotNullConstraint]
    [Field, Association(PairTo = "Urls", OnTargetRemove = OnRemoveAction.Cascade)]
    public WebSite WebSite { get; set;}
  }
}