using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [HierarchyRoot] 
  public class Container : Entity
  {
    [Field, Key]
    public long Id {get ; private set;}

    [Field]
    public string Name { get; set; }

    [Field, Association(PairTo = "Container", OnTargetRemove = OnRemoveAction.Deny)]
    public EntitySet<Document> Documents { get; private set; }

    public virtual T CreateDocument<T>(string name) where T : Document, new()
    {
      T doc = new T() { Name = name };
      Documents.Add(doc);
      return doc;
    }

    public override string ToString()
    {
      return String.Format("[Container]<{0}>::Name='{1}'::Count='{2}'", Id, Name, Documents.Count); 
    }
  }
}
