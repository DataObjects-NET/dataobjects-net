// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


namespace Xtensive.Orm.Tests.Storage.SchemaSharing.Model
{
  [HierarchyRoot]
  public class Product : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class PriceList : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public DateTime CreatedOn { get; set; }

    [Field]
    public bool IsArchived { get; set; }

    [Field]
    public EntitySet<PriceListItem> Items { get; set; }
  }

  [HierarchyRoot]
  public class PriceListItem : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
    public PriceList PriceList { get; set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.None, OnTargetRemove = OnRemoveAction.Deny)]
    public Product Product { get; set; }

    [Field]
    public double Price { get; set; }

    [Field]
    public Currency Currency { get; set; }
  }

  [HierarchyRoot]
  public class Currency : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string ShortName { get; set; }

    [Field]
    public string Symbol { get; set; }
  }

  [HierarchyRoot]
  public class TypeForUgrade : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }
  }
}
