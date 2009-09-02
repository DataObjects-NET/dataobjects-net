// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.02

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Linq.OtherTests.ContentItemTestModel;

namespace Xtensive.Storage.Tests.Linq.OtherTests.ContentItemTestModel
{
  public class WebSite
    : ContentDirectory
  {
    [Field]
    public string DomainName { get; set; }

    [Field]
    public string VirtualDirectoryName { get; set; }


    public WebSite()
    {
      WebSite = this;
      Name = "";
    }
  }

  public class ContentItemCollection<TContentItem>
    : EntitySet<TContentItem>
    where TContentItem : ContentItem
  {
    protected override void OnAdding(Entity item)
    {
      ((ContentItem) item).WebSite = ((ContentItem) Owner).WebSite;
      base.OnAdding(item);
    }

    protected ContentItemCollection(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }

    protected ContentItemCollection(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }

  public class ContentDirectory
    : ContentItem
  {
    [Field]
    public string DefaultTemplate { get; set; }

    [Field]
    public Page DefaultPage { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public ContentItemCollection<ContentItem> Items { get; private set; }
  }

  [HierarchyRoot]
  public class ContentItem
    : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public WebSite WebSite { get; set; }

    [Field]
    public string Name
    {
      get { return GetFieldValue<string>("Name"); }
      set
      {
        if (value==null)
          throw new ArgumentException("value");
        SetFieldValue("Name", value.ToLowerInvariant());
      }
    }

    [Field, Association(PairTo = "Items", OnTargetRemove = OnRemoveAction.Cascade)]
    public ContentDirectory ParentDirectory { get; set; }

    private void BuildPathRecursive(StringBuilder stringBuilder)
    {
      if (ParentDirectory!=null) {
        ParentDirectory.BuildPathRecursive(stringBuilder);
        stringBuilder.Append('/');
      }
      stringBuilder.Append(Name);
    }

    protected override void OnValidate()
    {
      base.OnValidate();
//      if (WebSite==null)
//        throw new InvalidOperationException("WebSite is null");
//      if (ParentDirectory==null && WebSite!=this)
//        throw new InvalidOperationException("ParentDirectory is null");
    }

    public string GetPath()
    {
      var result = new StringBuilder();
      BuildPathRecursive(result);
      return result.ToString();
    }
  }


  public class ContentFile
    : ContentItem
  {
    [Field]
    public string StoredFileID { get; set; }

    [Field]
    public DateTime? LastModificationDate { get; set; }

    public virtual void NotifyModified()
    {
      LastModificationDate = DateTime.Now;
    }
  }

  public class Page
    : ContentFile
  {
    [Field]
    public string KeyWords { get; set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public string ContentStorageFileID { get; set; }

    [Field]
    public bool UseDefaultTemplate { get; set; }

    [Field]
    public string Template { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Linq.OtherTests
{
  public class ContentItemTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (WebSite).Assembly, typeof (WebSite).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          
          var google = new WebSite {DomainName = "google.com"};
          var fragment1 = new ContentDirectory {Name = "fragmenT1", ParentDirectory = google};
          var fragment2 = new ContentDirectory {Name = "fragment2", ParentDirectory = google};
          //
          var fragment1_1 = new ContentDirectory {Name = "Fragment1_1", ParentDirectory = fragment1};
          var fragment1_2 = new ContentDirectory {Name = "Fragment1_2", ParentDirectory = fragment1};
          var fragment1_3 = new ContentDirectory {Name = "Fragment1_3", ParentDirectory = fragment1};

          var page1_2_1 = new Page {Name = "home", ParentDirectory = fragment1_2};
          
          var query = Query<ContentItem>
            .All
            .Where(item => item.ParentDirectory.ParentDirectory.Name.ToLower()=="fragment1")
            .Where(item => item.ParentDirectory.Name.ToLower()=="fragment1_2");
            //.Where(contentItem => Equals(contentItem.Name.ToLowerInvariant(), value(Xtensive.Cms.Model.ContentSearchService + < > c__DisplayClass2).queryFragments[(ArrayLength(value(Xtensive.Cms.Model.ContentSearchService + < > c__DisplayClass2).queryFragments) - 1)]))

          QueryDumper.Dump(query);
        }
      }
    }
  }
}