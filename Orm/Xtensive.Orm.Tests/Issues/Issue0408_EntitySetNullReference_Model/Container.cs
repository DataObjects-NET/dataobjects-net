using System;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
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
      return $"[Container]<{Id}>::Name='{Name}'::Count='{Documents.Count}'"; 
    }
  }
}
