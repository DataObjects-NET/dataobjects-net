module Model
open Xtensive.Orm

[<HierarchyRoot>]
type Person() =
  inherit Entity()

  [<Field; Key>]
  member this.Id
    with get() = this.GetFieldValue<int> "Id"
    and private set(v: int) = this.SetFieldValue ("Id", v)

  [<Field>]
  member this.Name
    with get() = this.GetFieldValue<string> "Name"
    and set(v: string) = this.SetFieldValue ("Name", v)
