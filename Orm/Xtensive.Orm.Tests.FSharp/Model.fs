module Model
open Xtensive.Orm

[<HierarchyRoot>]
type Person() =
  inherit Entity()

  [<Field; Key>]
  member this.Id
    with get() = this.GetFieldValue<int> (nameof this.Id)
    and private set(v: int) = this.SetFieldValue (nameof this.Id, v)

  [<Field>]
  member this.Name
    with get() = this.GetFieldValue<string> (nameof this.Name)
    and set(v: string) = this.SetFieldValue (nameof this.Name, v)

  [<Field>]
  member this.CreateOn
    with get() = this.GetFieldValue<System.DateTime> (nameof this.CreateOn)
    and set(v: System.DateTime) = this.SetFieldValue (nameof this.CreateOn, v)


[<HierarchyRoot>]
type X() =
  inherit Entity()

  [<Field; Key>]
  member this.Id
    with get() = this.GetFieldValue<int> (nameof this.Id)
    and private set(v: int) = this.SetFieldValue (nameof this.Id, v)

  [<Field>]
  member this.BoolField
    with get() = this.GetFieldValue<bool> (nameof this.BoolField)
    and set(v: bool) = this.SetFieldValue (nameof (this.BoolField), v)

  [<Field>]
  member this.Int16Field
    with get() = this.GetFieldValue<int16> (nameof this.Int16Field)
    and set(v: int16) = this.SetFieldValue (nameof (this.Int16Field), v)

    [<Field>]
  member this.UInt16Field
    with get() = this.GetFieldValue<uint16> (nameof this.UInt16Field)
    and set(v: uint16) = this.SetFieldValue (nameof (this.UInt16Field), v)

    [<Field>]
  member this.Int32Field
    with get() = this.GetFieldValue<int32> (nameof this.Int32Field)
    and set(v: int32) = this.SetFieldValue (nameof (this.Int32Field), v)

    [<Field>]
  member this.UInt32Field
    with get() = this.GetFieldValue<uint32> (nameof this.UInt32Field)
    and set(v: uint32) = this.SetFieldValue (nameof (this.UInt32Field), v)

    [<Field>]
  member this.Int64Field
    with get() = this.GetFieldValue<int64> (nameof this.Int64Field)
    and set(v: int64) = this.SetFieldValue (nameof (this.Int64Field), v)

  [<Field>]
  member this.UInt64Field
    with get() = this.GetFieldValue<uint64> (nameof this.UInt64Field)
    and set(v: uint64) = this.SetFieldValue (nameof (this.UInt64Field), v)

  [<Field>]
  member this.SingleField
    with get() = this.GetFieldValue<single> (nameof this.SingleField)
    and set(v: single) = this.SetFieldValue (nameof (this.SingleField), v)

  [<Field>]
  member this.DoubleField
    with get() = this.GetFieldValue<double> (nameof this.DoubleField)
    and set(v: double) = this.SetFieldValue (nameof (this.DoubleField), v)

  [<Field>]
  member this.DecimalField
    with get() = this.GetFieldValue<decimal> (nameof this.DecimalField)
    and set(v: decimal) = this.SetFieldValue (nameof (this.DecimalField), v)

  [<Field>]
  member this.StringField
    with get() = this.GetFieldValue<string> (nameof this.StringField)
    and set(v: string) = this.SetFieldValue (nameof (this.StringField), v)

  [<Field>]
  member this.DateTimeField
    with get() = this.GetFieldValue<System.DateTime> (nameof this.DateTimeField)
    and set(v: System.DateTime) = this.SetFieldValue (nameof this.DateTimeField, v)

  [<Field>]
  member this.TimeSpanField
    with get() = this.GetFieldValue<System.TimeSpan> (nameof this.TimeSpanField)
    and set(v: System.TimeSpan) = this.SetFieldValue (nameof this.TimeSpanField, v)
