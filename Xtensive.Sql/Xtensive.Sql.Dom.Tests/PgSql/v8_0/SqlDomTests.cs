using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.PgSql;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Database.Providers;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;
using DataTable=System.Data.DataTable;
using PgSqlDriver=Xtensive.Sql.Dom.PgSql.PgSqlDriver;
using UniqueConstraint=Xtensive.Sql.Dom.Database.UniqueConstraint;

namespace Xtensive.Sql.Dom.Tests.PgSql.v8_0
{
  [TestFixture]
  public class SqlDomTests
  {
    protected virtual string Url
    {
      get { return "pgsql://do4test:do4testpwd@localhost:8032/do4test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5"; }
    }

    private SqlDriver mDriver;

    protected SqlDriver Driver
    {
      get { return mDriver; }
    }

    private SqlConnection mConnection;

    protected SqlConnection Connection
    {
      get { return mConnection; }
    }

    private Model mDbModel;

    protected Catalog MyCatalog
    {
      get { return mDbModel.DefaultServer.DefaultCatalog; }
    }

    [TestFixtureSetUp]
    public virtual void FixtureSetup()
    {
      ConnectionInfo ci = new ConnectionInfo(Url);
      mDriver = new PgSqlDriver(ci);
      CreateModel();
      mConnection = mDriver.CreateConnection(ci) as SqlConnection;
      try {
        Connection.Open();
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
    }


    protected void CreateModel()
    {
      {
        try {
          mDbModel = new Model("default");
        }
        catch (Exception ex) {
          int z = 7;
        }

        mDbModel.CreateServer("localhost");
        mDbModel.DefaultServer.CreateCatalog("do4test");

        mDbModel.DefaultServer.CreateUser("do4test");

        ForeignKey fk;
        Index idx;

        Schema sch1 = MyCatalog.CreateSchema("Sch1");

        Sequence t1_seq = sch1.CreateSequence("T1Seq");
        t1_seq.DataType = new SqlValueType(SqlDataType.Int64);

        Table sch1_t1 = sch1.CreateTable("T1");

        sch1_t1.CreateColumn("Id", new SqlValueType(SqlDataType.Int32));
        sch1_t1.TableColumns["Id"].DefaultValue = Sql.NextValue(t1_seq);

        sch1_t1.CreateColumn("Char", new SqlValueType(SqlDataType.Char, 1));
        sch1_t1.TableColumns["Char"].IsNullable = false;
        sch1_t1.TableColumns["Char"].DefaultValue = "'";
        sch1_t1.CreateColumn("Char?", new SqlValueType(SqlDataType.Char, 1));
        sch1_t1.TableColumns["Char?"].IsNullable = true;
        sch1_t1.CreateColumn("Char7", new SqlValueType(SqlDataType.Char, 7));
        sch1_t1.TableColumns["Char7"].IsNullable = false;
        sch1_t1.TableColumns["Char7"].DefaultValue = "Char7";
        sch1_t1.CreateColumn("Char8?", new SqlValueType(SqlDataType.Char, 8));
        sch1_t1.TableColumns["Char8?"].IsNullable = true;
        sch1_t1.CreateColumn("Varchar9", new SqlValueType(SqlDataType.VarChar, 9));
        sch1_t1.TableColumns["Varchar9"].IsNullable = false;
        sch1_t1.TableColumns["Varchar9"].DefaultValue = Sql.Literal("Varchar9");
        sch1_t1.CreateColumn("Varchar10?", new SqlValueType(SqlDataType.VarChar, 10));
        sch1_t1.TableColumns["Varchar10?"].IsNullable = true;
        sch1_t1.CreateColumn("Text", new SqlValueType(SqlDataType.Text));
        sch1_t1.TableColumns["Text"].IsNullable = false;
        sch1_t1.TableColumns["Text"].DefaultValue = Sql.Concat(Sql.Literal("Text'"), Sql.SessionUser());
        sch1_t1.CreateColumn("Text?", new SqlValueType(SqlDataType.Text));
        sch1_t1.TableColumns["Text?"].IsNullable = true;
        sch1_t1.CreateColumn("Bool", new SqlValueType(SqlDataType.Boolean));
        sch1_t1.TableColumns["Bool"].IsNullable = false;
        sch1_t1.TableColumns["Bool"].DefaultValue = false; // false;// Sql.Or(Sql.Literal(true), Sql.Literal(false));
        sch1_t1.CreateColumn("Bool?", new SqlValueType(SqlDataType.Boolean));
        sch1_t1.TableColumns["Bool?"].IsNullable = true;
        sch1_t1.CreateColumn("Int2", new SqlValueType(SqlDataType.Int16));
        sch1_t1.TableColumns["Int2"].IsNullable = false;
        sch1_t1.TableColumns["Int2"].DefaultValue = (Sql.Literal(2) + 1) * 7;
        sch1_t1.CreateColumn("Int2?", new SqlValueType(SqlDataType.Int16));
        sch1_t1.TableColumns["Int2?"].IsNullable = true;
        sch1_t1.CreateColumn("Int4", new SqlValueType(SqlDataType.Int32));
        sch1_t1.TableColumns["Int4"].IsNullable = false;
        sch1_t1.TableColumns["Int4"].DefaultValue = 41;
        sch1_t1.CreateColumn("Int4?", new SqlValueType(SqlDataType.Int32));
        sch1_t1.TableColumns["Int4?"].IsNullable = true;
        sch1_t1.CreateColumn("Int8", new SqlValueType(SqlDataType.Int64));
        sch1_t1.TableColumns["Int8"].IsNullable = false;
        sch1_t1.TableColumns["Int8"].DefaultValue = Sql.Square(9);
        sch1_t1.CreateColumn("Int8?", new SqlValueType(SqlDataType.Int64));
        sch1_t1.TableColumns["Int8?"].IsNullable = true;
        sch1_t1.CreateColumn("Float4", new SqlValueType(SqlDataType.Float));
        sch1_t1.TableColumns["Float4"].IsNullable = false;
        sch1_t1.TableColumns["Float4"].DefaultValue = Sql.Pi();
        sch1_t1.CreateColumn("Float4?", new SqlValueType(SqlDataType.Float));
        sch1_t1.TableColumns["Float4?"].IsNullable = true;
        sch1_t1.CreateColumn("Float8", new SqlValueType(SqlDataType.Double));
        sch1_t1.TableColumns["Float8"].IsNullable = false;
        sch1_t1.TableColumns["Float8"].DefaultValue = Sql.Rand(7);
        sch1_t1.CreateColumn("Float8?", new SqlValueType(SqlDataType.Double));
        sch1_t1.TableColumns["Float8?"].IsNullable = true;
        sch1_t1.CreateColumn("Decimal28_10", new SqlValueType(SqlDataType.Decimal, 28, 10));
        sch1_t1.TableColumns["Decimal28_10"].IsNullable = false;
        sch1_t1.TableColumns["Decimal28_10"].DefaultValue = Sql.Tan(Sql.Pi() / 4);
        sch1_t1.CreateColumn("Decimal28_10?", new SqlValueType(SqlDataType.Decimal, 28, 10));
        sch1_t1.TableColumns["Decimal28_10?"].IsNullable = true;
        sch1_t1.CreateColumn("Decimal50", new SqlValueType(SqlDataType.Decimal, 50, 0));
        sch1_t1.TableColumns["Decimal50"].IsNullable = false;
        sch1_t1.TableColumns["Decimal50"].DefaultValue = Sql.Power(3, 20);
        sch1_t1.CreateColumn("Decimal50?", new SqlValueType(SqlDataType.Decimal, 50, 0));
        sch1_t1.TableColumns["Decimal50?"].IsNullable = true;
        sch1_t1.CreateColumn("DateTime", new SqlValueType(SqlDataType.DateTime));
        sch1_t1.TableColumns["DateTime"].IsNullable = false;
        sch1_t1.TableColumns["DateTime"].DefaultValue = Sql.CurrentDate();
        sch1_t1.CreateColumn("DateTime?", new SqlValueType(SqlDataType.DateTime));
        sch1_t1.TableColumns["DateTime?"].IsNullable = true;
        SqlTableRef t1_ref = Sql.TableRef(sch1_t1);

        sch1_t1.CreatePrimaryKey("T1_pk", sch1_t1.TableColumns["Id"]);

        sch1_t1.CreateUniqueConstraint("T1_u1", sch1_t1.TableColumns["Char?"], sch1_t1.TableColumns["Char7"]);
        sch1_t1.CreateUniqueConstraint("T1_u2", sch1_t1.TableColumns["Text?"]);
        sch1_t1.CreateUniqueConstraint("T1_u3", sch1_t1.TableColumns["Int8"], sch1_t1.TableColumns["Int4"]);
        sch1_t1.CreateUniqueConstraint("T1_u4", sch1_t1.TableColumns["Int2"], sch1_t1.TableColumns["Bool"], sch1_t1.TableColumns["Bool?"]);
        sch1_t1.CreateUniqueConstraint("T1_u5", sch1_t1.TableColumns["DateTime"], sch1_t1.TableColumns["Float8"], sch1_t1.TableColumns["Decimal28_10"], sch1_t1.TableColumns["Float4"]);

        sch1_t1.CreateCheckConstraint("T1_ch1", t1_ref["Bool?"]);
        sch1_t1.CreateCheckConstraint("T1_ch2", Sql.Between(t1_ref["Int2"], 2, 200));
        sch1_t1.CreateCheckConstraint("T1_ch3", Sql.Between(t1_ref["Int4"], 4, 400));
        sch1_t1.CreateCheckConstraint("T1_ch4", t1_ref["Int8"] > t1_ref["Int4"]);
        sch1_t1.CreateCheckConstraint("T1_ch5", t1_ref["Decimal50"] >= t1_ref["Decimal28_10"]);

        fk = sch1_t1.CreateForeignKey("fk_Int4?_Id");
        fk.Columns.Add(sch1_t1.TableColumns["Int4?"]);
        fk.ReferencedTable = sch1_t1;
        fk.ReferencedColumns.Add(sch1_t1.TableColumns["Id"]);
        fk.OnUpdate = ReferentialAction.SetNull;
        fk.OnDelete = ReferentialAction.SetDefault;
        fk.IsDeferrable = false;

        idx = sch1_t1.CreateIndex("T1_ix1");
        idx.CreateIndexColumn(sch1_t1.TableColumns["Decimal50"]);
        idx.IsClustered = true;

        idx = sch1_t1.CreateIndex("T1_ix2");
        idx.CreateIndexColumn(sch1_t1.TableColumns["DateTime"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Int2"]);
        idx.FillFactor = 80;

        idx = sch1_t1.CreateIndex("T1_ix3");
        idx.CreateIndexColumn(sch1_t1.TableColumns["Char7"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Bool"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Float8?"]);
        idx.IsUnique = true;

        idx = sch1_t1.CreateIndex("T1_ix4");
        idx.CreateIndexColumn(sch1_t1.TableColumns["Varchar10?"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Varchar9"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Char7"]);
        idx.CreateIndexColumn(sch1_t1.TableColumns["Text?"]);

        {
          SqlSelect viewdef = Sql.Select(t1_ref);
          foreach (SqlTableColumn tc in t1_ref.Columns) {
            viewdef.Columns.Add(tc);
          }
          string cmdText = Driver.Compile(viewdef).CommandText;
          sch1.CreateView("T1View", Sql.Native(cmdText));
        }

        //t2

        Table sch1_t2 = sch1.CreateTable("T2");
        {
          sch1_t2.CreateColumn("Int8", new SqlValueType(SqlDataType.Int64));
          sch1_t2.TableColumns["Int8"].IsNullable = false;
          sch1_t2.TableColumns["Int8"].DefaultValue = 0;
          sch1_t2.CreateColumn("Int4", new SqlValueType(SqlDataType.Int32));
          sch1_t2.TableColumns["Int4"].IsNullable = false;
          sch1_t2.TableColumns["Int4"].DefaultValue = 0;

          SqlTableRef t2_ref = Sql.TableRef(sch1_t2);

          fk = sch1_t2.CreateForeignKey("fk_t2_Int8_Int4_t1_Int8_Int4");
          fk.Columns.Add(sch1_t2.TableColumns["Int8"]);
          fk.Columns.Add(sch1_t2.TableColumns["Int4"]);
          fk.ReferencedTable = sch1_t1;
          fk.ReferencedColumns.Add(sch1_t1.TableColumns["Int8"]);
          fk.ReferencedColumns.Add(sch1_t1.TableColumns["Int4"]);
          fk.OnUpdate = ReferentialAction.Cascade;
          fk.OnDelete = ReferentialAction.Restrict;
          fk.IsDeferrable = false;
        }

        //sch2

        Schema sch2 = MyCatalog.CreateSchema("Sch2");
        {
          //t3

          Table sch2_t3 = sch2.CreateTable("T3");

          sch2_t3.CreateColumn("Int8", new SqlValueType(SqlDataType.Int64));
          sch2_t3.TableColumns["Int8"].IsNullable = false;
          sch2_t3.TableColumns["Int8"].DefaultValue = 0;
          sch2_t3.CreateColumn("Int4", new SqlValueType(SqlDataType.Int32));
          sch2_t3.TableColumns["Int4"].IsNullable = false;
          sch2_t3.TableColumns["Int4"].DefaultValue = 0;

          SqlTableRef t3_ref = Sql.TableRef(sch2_t3);

          fk = sch2_t3.CreateForeignKey("fk_t3_Int8_Int4_t1_Int8_Int4");
          fk.Columns.Add(sch2_t3.TableColumns["Int8"]);
          fk.Columns.Add(sch2_t3.TableColumns["Int4"]);
          fk.ReferencedTable = sch1_t1;
          fk.ReferencedColumns.Add(sch1_t1.TableColumns["Int8"]);
          fk.ReferencedColumns.Add(sch1_t1.TableColumns["Int4"]);
          fk.OnUpdate = ReferentialAction.NoAction;
          fk.OnDelete = ReferentialAction.Cascade;
          fk.IsDeferrable = false;

          //some sequences
          Sequence sch2_seq1 = sch2.CreateSequence("Seq1");
          sch2_seq1.DataType = new SqlValueType(SqlDataType.Int64);

          Sequence sch2_seq2 = sch2.CreateSequence("Seq2");
          sch2_seq2.DataType = new SqlValueType(SqlDataType.Int64);
          sch2_seq2.SequenceDescriptor.StartValue = 100;
          sch2_seq2.SequenceDescriptor.Increment = 10;
          sch2_seq2.SequenceDescriptor.MinValue = 1;
          sch2_seq2.SequenceDescriptor.MaxValue = 1000;
          sch2_seq2.SequenceDescriptor.IsCyclic = true;
        }
      }

      #region AdventureWorks model

      /*
      {
        Model model = mDbModel;
        model.DefaultServer.DefaultCatalog.CreateSchema("HumanResources");
        model.DefaultServer.DefaultCatalog.CreateSchema("Person");
        model.DefaultServer.DefaultCatalog.CreateSchema("Production");
        model.DefaultServer.DefaultCatalog.CreateSchema("Purchasing");
        model.DefaultServer.DefaultCatalog.CreateSchema("Sales");

        Table t;
        View v;
        TableColumn c;
        IConstraint cs;

        t = MyCatalog.Schemas["Production"].CreateTable("TransactionHistoryArchive");
        t.CreateColumn("TransactionID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TransactionDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("TransactionType", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("CreditCard");
        t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CardType", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CardNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ExpMonth", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ExpYear", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("Document");
        t.CreateColumn("DocumentID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("FileName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("FileExtension", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Revision", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ChangeNumber", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Status", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("DocumentSummary", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Document", new SqlValueType(SqlDataType.VarBinaryMax));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("Illustration");
        t.CreateColumn("IllustrationID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Diagram", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductDescription");
        t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Description", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SpecialOffer");
        t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Description", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("DiscountPct", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Type", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Category", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("MinQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("MaxQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductPhoto");
        t.CreateColumn("ProductPhotoID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ThumbNailPhoto", new SqlValueType(SqlDataType.VarBinaryMax));
        t.CreateColumn("ThumbnailPhotoFileName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("LargePhoto", new SqlValueType(SqlDataType.VarBinaryMax));
        t.CreateColumn("LargePhotoFileName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("Customer");
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CustomerType", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("CustomerAddress");
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("EmployeeAddress");
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef ea = Sql.TableRef(t);

        t = MyCatalog.Schemas["Purchasing"].CreateTable("VendorAddress");
        t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Purchasing"].CreateTable("ProductVendor");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AverageLeadTime", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StandardPrice", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("LastReceiptCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("LastReceiptDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("MinOrderQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("MaxOrderQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("OnOrderQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("BillOfMaterials");
        t.CreateColumn("BillOfMaterialsID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductAssemblyID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ComponentID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("BOMLevel", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("PerAssemblyQty", new SqlValueType(SqlDataType.Decimal));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Purchasing"].CreateTable("PurchaseOrderHeader");
        t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("RevisionNumber", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("Status", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("OrderDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ShipDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("SubTotal", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("TaxAmt", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Freight", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("TotalDue", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Purchasing"].CreateTable("VendorContact");
        t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("ContactCreditCard");
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("StoreContact");
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Purchasing"].CreateTable("PurchaseOrderDetail");
        t.CreateColumn("PurchaseOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("PurchaseOrderDetailID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("DueDate", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("UnitPrice", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReceivedQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("RejectedQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StockedQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("WorkOrderRouting");
        t.CreateColumn("WorkOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("OperationSequence", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ScheduledStartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ScheduledEndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ActualStartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ActualEndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ActualResourceHrs", new SqlValueType(SqlDataType.Decimal));
        t.CreateColumn("PlannedCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("CountryRegionCurrency");
        t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CurrencyCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductModelProductDescriptionCulture");
        t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductDescriptionID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CultureID", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("CurrencyRate");
        t.CreateColumn("CurrencyRateID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CurrencyRateDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("FromCurrencyCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ToCurrencyCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("AverageRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("EndOfDayRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesOrderDetail");
        t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SalesOrderDetailID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CarrierTrackingNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("UnitPrice", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("UnitPriceDiscount", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("LineTotal", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesOrderHeaderSalesReason");
        t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SalesReasonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("EmployeeDepartmentHistory");
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("DepartmentID", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ShiftID", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductDocument");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("DocumentID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("EmployeePayHistory");
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("RateChangeDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("Rate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("PayFrequency", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesPerson");
        t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SalesQuota", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Bonus", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("CommissionPct", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("SalesYTD", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("SalesLastYear", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesPersonQuotaHistory");
        t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("QuotaDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("SalesQuota", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesTerritoryHistory");
        t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductModelIllustration");
        t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("IllustrationID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductInventory");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Shelf", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Bin", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("WorkOrder");
        t.CreateColumn("WorkOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("OrderQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StockedQty", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ScrappedQty", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("DueDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ScrapReasonID", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("TransactionHistory");
        t.CreateColumn("TransactionID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TransactionDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("TransactionType", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ActualCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("ShoppingCartItem");
        t.CreateColumn("ShoppingCartItemID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ShoppingCartID", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Quantity", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("DateCreated", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductListPriceHistory");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ListPrice", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SpecialOfferProduct");
        t.CreateColumn("SpecialOfferID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductCostHistory");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StartDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("StandardCost", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Person"].CreateTable("Address");
        t.CreateColumn("AddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AddressLine1", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("AddressLine2", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("City", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("PostalCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef a = Sql.TableRef(t);

        t = MyCatalog.Schemas["Purchasing"].CreateTable("Vendor");
        t.CreateColumn("VendorID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CreditRating", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("PreferredVendorStatus", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("ActiveFlag", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("PurchasingWebServiceURL", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesOrderHeader");
        t.CreateColumn("SalesOrderID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("RevisionNumber", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("OrderDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("DueDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ShipDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("Status", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("OnlineOrderFlag", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("SalesOrderNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("PurchaseOrderNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("AccountNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("BillToAddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ShipToAddressID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CreditCardID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("CreditCardApprovalCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CurrencyRateID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("SubTotal", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("TaxAmt", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Freight", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("TotalDue", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Comment", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("Employee");
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("NationalIDNumber", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("LoginID", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ManagerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("BirthDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("MaritalStatus", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Gender", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("HireDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("SalariedFlag", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("VacationHours", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("SickLeaveHours", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("CurrentFlag", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef e = Sql.TableRef(t);

        t = MyCatalog.Schemas["Production"].CreateTable("ProductProductPhoto");
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductPhotoID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Primary", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Person"].CreateTable("StateProvince");
        t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StateProvinceCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("IsOnlyStateProvinceFlag", new SqlValueType(SqlDataType.Boolean));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef sp = Sql.TableRef(t);

        t = MyCatalog.Schemas["Production"].CreateTable("ProductModel");
        t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CatalogDescription", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("Instructions", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("Product");
        c = t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        c.SequenceDescriptor = new SequenceDescriptor(c, 1, 1);
        c.IsNullable = false;
        c = t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar, 50));
        c.IsNullable = false;
        c = t.CreateColumn("ProductNumber", new SqlValueType(SqlDataType.VarChar, 25));
        c.IsNullable = false;
        c = t.CreateColumn("MakeFlag", new SqlValueType(SqlDataType.Boolean));
        c.DefaultValue = 1;
        c.IsNullable = false;
        c = t.CreateColumn("FinishedGoodsFlag", new SqlValueType(SqlDataType.Boolean));
        c.DefaultValue = 1;
        c.IsNullable = false;
        c = t.CreateColumn("Color", new SqlValueType(SqlDataType.VarChar, 15));
        c = t.CreateColumn("SafetyStockLevel", new SqlValueType(SqlDataType.Int16));
        c.IsNullable = false;
        c = t.CreateColumn("ReorderPoint", new SqlValueType(SqlDataType.Int16));
        c.IsNullable = false;
        c = t.CreateColumn("StandardCost", new SqlValueType(SqlDataType.Money));
        c.IsNullable = false;
        c = t.CreateColumn("ListPrice", new SqlValueType(SqlDataType.Money));
        c.IsNullable = false;
        c = t.CreateColumn("Size", new SqlValueType(SqlDataType.VarChar, 5));
        c = t.CreateColumn("SizeUnitMeasureCode", new SqlValueType(SqlDataType.Char, 3));
        c = t.CreateColumn("WeightUnitMeasureCode", new SqlValueType(SqlDataType.Char, 3));
        t.CreateColumn("Weight", new SqlValueType(SqlDataType.Decimal, 8, 2));
        c = t.CreateColumn("DaysToManufacture", new SqlValueType(SqlDataType.Int32));
        c.IsNullable = false;
        c = t.CreateColumn("ProductLine", new SqlValueType(SqlDataType.Char, 2));
        c = t.CreateColumn("Class", new SqlValueType(SqlDataType.Char, 2));
        c = t.CreateColumn("Style", new SqlValueType(SqlDataType.Char, 2));
        t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductModelID", new SqlValueType(SqlDataType.Int32));
        c = t.CreateColumn("SellStartDate", new SqlValueType(SqlDataType.DateTime));
        c.IsNullable = false;
        t.CreateColumn("SellEndDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("DiscontinuedDate", new SqlValueType(SqlDataType.DateTime));
        c = t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        c.DefaultValue = Sql.CurrentDate();
        c.IsNullable = false;
        SqlTableRef st = Sql.TableRef(t);
        SqlTableColumn col;
        col = st["Class"];
        t.CreateCheckConstraint("CK_Product_Class", Sql.Upper(st["Class"]) == 'H' ||
                                Sql.Upper(st["Class"]) == 'M' ||
                                Sql.Upper(st["Class"]) == 'L' ||
                                Sql.IsNull(st["Class"]));
        t.CreateCheckConstraint("CK_Product_DaysToManufacture", Sql.Upper(st["DaysToManufacture"]) >= 0);
        t.CreateCheckConstraint("CK_Product_ListPrice", Sql.Upper(st["ListPrice"]) >= 0);
        t.CreateCheckConstraint("CK_Product_ProductLine", Sql.Upper(st["ProductLine"]) == 'R' ||
                                    Sql.Upper(st["ProductLine"]) == 'M' ||
                                    Sql.Upper(st["ProductLine"]) == 'T' ||
                                    Sql.Upper(st["ProductLine"]) == 'S' ||
                                    Sql.IsNull(st["ProductLine"]));
        t.CreateCheckConstraint("CK_Product_ReorderPoint", Sql.Upper(st["ReorderPoint"]) > 0);
        t.CreateCheckConstraint("CK_Product_SafetyStockLevel", Sql.Upper(st["SafetyStockLevel"]) > 0);
        t.CreateCheckConstraint("CK_Product_SellEndDate", Sql.Upper(st["SellEndDate"]) > st["SellStartDate"] ||
                                    Sql.IsNull(st["SellEndDate"]));
        t.CreateCheckConstraint("CK_Product_StandardCost", Sql.Upper(st["StandardCost"]) >= 0);
        t.CreateCheckConstraint("CK_Product_Style", Sql.Upper(st["Style"]) == 'U' ||
                                Sql.Upper(st["Style"]) == 'M' ||
                                Sql.Upper(st["Style"]) == 'W' ||
                                Sql.IsNull(st["Style"]));
        t.CreateCheckConstraint("CK_Product_Weight", Sql.Upper(st["Weight"]) > 0);
        t.CreatePrimaryKey("PK_Product_ProductID", t.TableColumns["ProductID"]);
        cs = t.CreateForeignKey("FK_Product_ProductModel_ProductModelID");
        ((ForeignKey)cs).Columns.Add(t.TableColumns["ProductModelID"]);
        ((ForeignKey)cs).ReferencedColumns.Add(MyCatalog.Schemas["Production"].Tables["ProductModel"].TableColumns["ProductModelID"]);

        t = MyCatalog.Schemas["Person"].CreateTable("Contact");
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("NameStyle", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Title", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("FirstName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("MiddleName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("LastName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Suffix", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("EmailAddress", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("EmailPromotion", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Phone", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("PasswordHash", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("PasswordSalt", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("AdditionalContactInfo", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef cRef = Sql.TableRef(t);

        t = MyCatalog.Schemas["Production"].CreateTable("UnitMeasure");
        t.CreateColumn("UnitMeasureCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductReview");
        t.CreateColumn("ProductReviewID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ReviewerName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ReviewDate", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EmailAddress", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Rating", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Comments", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductSubcategory");
        t.CreateColumn("ProductSubcategoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ProductCategoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Person"].CreateTable("AddressType");
        t.CreateColumn("AddressTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesReason");
        t.CreateColumn("SalesReasonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ReasonType", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("Department");
        t.CreateColumn("DepartmentID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("GroupName", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Person"].CreateTable("CountryRegion");
        t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));
        SqlTableRef cr = Sql.TableRef(t);

        t = MyCatalog.Schemas["Production"].CreateTable("Culture");
        t.CreateColumn("CultureID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("Currency");
        t.CreateColumn("CurrencyCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Person"].CreateTable("ContactType");
        t.CreateColumn("ContactTypeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesTaxRate");
        t.CreateColumn("SalesTaxRateID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("StateProvinceID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("TaxType", new SqlValueType(SqlDataType.Int16));
        t.CreateColumn("TaxRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("Location");
        t.CreateColumn("LocationID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CostRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("Availability", new SqlValueType(SqlDataType.Decimal));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("SalesTerritory");
        t.CreateColumn("TerritoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("CountryRegionCode", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("Group", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("TaxRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("SalesYTD", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("SalesLastYear", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("CostYTD", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("CostLastYear", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ScrapReason");
        t.CreateColumn("ScrapReasonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("Shift");
        t.CreateColumn("ShiftID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("StartTime", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("EndTime", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Production"].CreateTable("ProductCategory");
        t.CreateColumn("ProductCategoryID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Purchasing"].CreateTable("ShipMethod");
        t.CreateColumn("ShipMethodID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("ShipBase", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ShipRate", new SqlValueType(SqlDataType.Money));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("Store");
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Name", new SqlValueType(SqlDataType.VarChar));
        t.CreateColumn("SalesPersonID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Demographics", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["Sales"].CreateTable("Individual");
        t.CreateColumn("CustomerID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("ContactID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Demographics", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        t = MyCatalog.Schemas["HumanResources"].CreateTable("JobCandidate");
        t.CreateColumn("JobCandidateID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("EmployeeID", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("Resume", new SqlValueType(SqlDataType.Text));
        t.CreateColumn("ModifiedDate", new SqlValueType(SqlDataType.DateTime));

        SqlSelect select = Sql.Select(e);
        select.From.InnerJoin(cRef, cRef["ContactID"] == e["ContactID"]);
        select.From.InnerJoin(ea, e["EmployeeID"] == ea["EmployeeID"]);
        select.From.InnerJoin(a, ea["AddressID"] == a["AddressID"]);
        select.From.InnerJoin(sp, sp["StateProvinceID"] == a["StateProvinceID"]);
        select.From.InnerJoin(cr, cr["CountryRegionCode"] == sp["CountryRegionCode"]);
        select.Columns.AddRange(e["EmployeeID"], cRef["Title"], cRef["FirstName"], cRef["MiddleName"],
                    cRef["LastName"], cRef["Suffix"]);
        select.Columns.Add(e["Title"], "JobTitle");
        select.Columns.AddRange(cRef["Phone"], cRef["EmailAddress"], cRef["EmailPromotion"],
                    a["AddressLine1"], a["AddressLine2"], a["City"]);
        select.Columns.Add(sp["Name"], "StateProvinceName");
        select.Columns.Add(a["PostalCode"]);
        select.Columns.Add(cr["Name"], "CountryRegionName");
        select.Columns.Add(cRef["AdditionalContactInfo"]);

        v = MyCatalog.Schemas["HumanResources"].CreateView("vEmployee",
          Sql.Native(mDriver.Compile(select).CommandText));
      }
      /**/

      #endregion
    }

    [TestFixtureTearDown]
    public virtual void FixtureTearDown()
    {
      if (Connection!=null) {
        if (Connection.State==ConnectionState.Open)
          Connection.Close();
      }
    }


    [Test]
    public void ReadServerInfoTest()
    {
      ServerInfo si = mDriver.ServerInfo;
    }

    [Test]
    public virtual void ModelTest()
    {
      DbTransaction trx = null;
      try {
        trx = Connection.BeginTransaction();

        //Create model
        using (SqlCommand cmd = new SqlCommand(Connection)) {
          SqlBatch batch = Sql.Batch();
          foreach (Schema sch in MyCatalog.Schemas) {
            batch.Add(Sql.Create(sch));
          }
          cmd.Statement = batch;
          cmd.ExecuteNonQuery();
        }

        //Extract initial model
        {
          Model model = new SqlModelProvider(Connection).Build();

          new CatalogComparer(Connection)
            .CompareCatalogs(MyCatalog, model.DefaultServer.DefaultCatalog);
        }

        //Alter model
        using (SqlCommand cmd = new SqlCommand(Connection)) {
          SqlBatch batch = Sql.Batch();
          cmd.Statement = batch;

          //Alter sequence
          {
            Sequence seq = MyCatalog.Schemas["Sch2"].Sequences["Seq2"];
            seq.SequenceDescriptor.Increment = 4;
            seq.SequenceDescriptor.MaxValue = null;
            seq.SequenceDescriptor.MinValue = null;
            seq.SequenceDescriptor.StartValue = 22;
            seq.SequenceDescriptor.IsCyclic = false;
            SqlAlterSequence stmt = Sql.Alter(seq, seq.SequenceDescriptor);
            batch.Add(stmt);
          }

          //Alter table
          {
            Table t = MyCatalog.Schemas["Sch1"].Tables["T1"];
            TableColumn col = null;
            //Add column
            {
              col = t.CreateColumn("newCol", new SqlValueType(SqlDataType.Decimal, 30, 10));
              col.IsNullable = false;
              SqlAlterTable stmt = Sql.Alter(t, Sql.AddColumn(col));
              batch.Add(stmt);
            }
            //Set default
            {
              col.DefaultValue = 543.21m;
              SqlAlterTable stmt = Sql.Alter(t, Sql.SetDefault(col.DefaultValue, col));
              batch.Add(stmt);
            }

            //Add check constraint
            {
              CheckConstraint cc = t.CreateCheckConstraint("newCheckConstraint", Sql.TableRef(t).Columns["newCol"] > 0);
              SqlAlterTable stmt = Sql.Alter(t, Sql.AddConstraint(cc));
              batch.Add(stmt);
            }

            //Add unique constraint
            {
              UniqueConstraint uc = t.CreateUniqueConstraint("newUniqueConstraint", t.TableColumns["newCol"]);
              SqlAlterTable stmt = Sql.Alter(t, Sql.AddConstraint(uc));
              batch.Add(stmt);
            }

            //Add foreign key
            {
              ForeignKey fk = t.CreateForeignKey("newForeignKey");
              fk.Columns.Add(col);
              fk.ReferencedTable = t;
              fk.ReferencedColumns.Add(col);
              fk.MatchType = SqlMatchType.Full;
              fk.OnDelete = ReferentialAction.SetDefault;
              fk.OnUpdate = ReferentialAction.Cascade;
              SqlAlterTable stmt = Sql.Alter(t, Sql.AddConstraint(fk));
              batch.Add(stmt);
            }
          }
          //Execute
          cmd.ExecuteNonQuery();
        }

        //Extract altered model
        {
          Model model = new SqlModelProvider(Connection).Build();

          new CatalogComparer(Connection)
            .CompareCatalogs(MyCatalog, model.DefaultServer.DefaultCatalog);
        }


        //Alter model again
        using (SqlCommand cmd = new SqlCommand(Connection)) {
          SqlBatch batch = Sql.Batch();
          cmd.Statement = batch;

          //Alter table
          {
            Table t = MyCatalog.Schemas["Sch1"].Tables["T1"];
            TableColumn col = t.TableColumns["newCol"];
            Assert.IsNotNull(col);
            //Drop constraints
            {
              SqlAlterTable stmt = null;

              CheckConstraint cc = t.TableConstraints["newCheckConstraint"] as CheckConstraint;
              Assert.IsNotNull(cc);
              t.TableConstraints.Remove(cc);
              stmt = Sql.Alter(t, Sql.DropConstraint(cc));
              batch.Add(stmt);

              ForeignKey fk = t.TableConstraints["newForeignKey"] as ForeignKey;
              Assert.IsNotNull(fk);
              t.TableConstraints.Remove(fk);
              stmt = Sql.Alter(t, Sql.DropConstraint(fk));
              batch.Add(stmt);

              UniqueConstraint uc = t.TableConstraints["newUniqueConstraint"] as UniqueConstraint;
              Assert.IsNotNull(uc);
              t.TableConstraints.Remove(uc);
              stmt = Sql.Alter(t, Sql.DropConstraint(uc));
              batch.Add(stmt);
            }

            //Drop default
            {
              col.DefaultValue = null;
              SqlAlterTable stmt = Sql.Alter(t, Sql.DropDefault(col));
              batch.Add(stmt);
            }

            //Drop column
            {
              t.TableColumns.Remove(col);
              SqlAlterTable stmt = Sql.Alter(t, Sql.DropColumn(col));
              batch.Add(stmt);
            }
          }
          //Execute
          cmd.ExecuteNonQuery();
        }

        //Extract altered model again
        {
          Model model = new SqlModelProvider(Connection).Build();

          new CatalogComparer(Connection)
            .CompareCatalogs(MyCatalog, model.DefaultServer.DefaultCatalog);
        }


        //Manipulate data
        using (SqlCommand cmd = new SqlCommand(Connection)) {
          SqlBatch batch = Sql.Batch();
          SqlInsert insert;
          SqlUpdate update;
          SqlDelete delete;
          SqlTableRef t1 = Sql.TableRef(MyCatalog.Schemas["Sch1"].Tables["T1"]);
          //Should insert default values
          //insert = Sql.Insert(t1);
          //batch.Add(insert);

          insert = Sql.Insert(t1);
          insert.Values.Add(t1["Text?"], Sql.Null);
          insert.Values.Add(t1["Int8"], Int64.MaxValue);
          batch.Add(insert);

          //set FK to self
          update = Sql.Update(t1);
          update.Values.Add(t1["Int4?"], Sql.FunctionCall("currval", @"""Sch1"".""T1Seq"""));
          update.Values.Add(t1["Text?"], "new text");
          batch.Add(update);

          delete = Sql.Delete(t1);
          batch.Add(delete);

          cmd.Statement = batch;
          cmd.ExecuteNonQuery();
        }


        //Drop model
        using (SqlCommand cmd = new SqlCommand(Connection)) {
          SqlBatch batch = Sql.Batch();

          foreach (Schema sch in MyCatalog.Schemas) {
            batch.Add(Sql.Drop(sch));
          }

          cmd.Statement = batch;
          cmd.ExecuteNonQuery();
        }
      }
      finally {
        trx.Rollback();
      }
    }

    [Test]
    public void ExpressionTest()
    {
      Guid guid = Guid.NewGuid();
      try {
        SqlSelect q = Sql.Select();

        #region Limits

        q.Columns.Add(Sql.Power(Sql.Cast(10, SqlDataType.Decimal), 50));
        q.Columns.Add(Sql.Cast(Sql.Literal(DateTime.MinValue), SqlDataType.DateTime), "datetime min");
        //casting in the DBMS rounds to 100000101 00:00:00 somewhy (?)
        //q.Columns.Add(Sql.Cast(Sql.Literal(DateTime.MaxValue), SqlDataType.DateTime, Driver.ServerInfoProvider.MaxDateTimePrecision,0), "datetime max");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int16.MinValue), SqlDataType.Int16), "int16 min");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int16.MaxValue), SqlDataType.Int16), "int16 max");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int32.MinValue), SqlDataType.Int32), "int32 min");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int32.MaxValue), SqlDataType.Int32), "int32 max");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int64.MinValue), SqlDataType.Int64), "int64 min");
        q.Columns.Add(Sql.Cast(Sql.Literal(Int64.MaxValue), SqlDataType.Int64), "int64 max");
        q.Columns.Add(Sql.Cast(Sql.Literal(Decimal.MinValue), SqlDataType.Decimal), "decimal min");
        q.Columns.Add(Sql.Cast(Sql.Literal(Decimal.MaxValue), SqlDataType.Decimal), "decimal max");
        q.Columns.Add(Sql.Cast(Sql.Literal(Single.MinValue), SqlDataType.Float), "float min");
        q.Columns.Add(Sql.Cast(Sql.Literal(Single.MaxValue), SqlDataType.Float), "float max");

        //1.79769313486231E308  instead of 1.79769313486232E308 
        //q.Columns.Add(Sql.Cast(Sql.Literal(Double.MinValue), SqlDataType.Double), "double min");
        //1.79769313486231E308  instead of 1.79769313486232E308 
//        q.Columns.Add(Sql.Cast(Sql.Literal(Double.MaxValue), SqlDataType.Double), "double max");

        q.Columns.Add(TimeSpan.MinValue, "interval min");
        q.Columns.Add(TimeSpan.MaxValue, "interval max");

        #endregion

        #region Literal

        q.Columns.Add((byte) 5, "literal_byte");
        q.Columns.Add((sbyte) 5, "literal_sbyte");
        q.Columns.Add((short) 5, "literal_short");
        q.Columns.Add((ushort) 5, "literal_ushort");
        q.Columns.Add(5, "literal_int");
        q.Columns.Add((uint) 5, "literal_uint");
        q.Columns.Add((long) 5, "literal_long");
        q.Columns.Add((ulong) 5, "literal_ulong");
        q.Columns.Add((decimal) 5.12, "literal_decimal");
        q.Columns.Add((float) 5.12, "literal_float");
        q.Columns.Add(5.12, "literal_double");
        q.Columns.Add('\\', "literal_char");
        q.Columns.Add(@"'\", "literal_string");
        q.Columns.Add(true, "literal_bool");
        q.Columns.Add(!Sql.Literal(false), "not_false");
        q.Columns.Add(Sql.Literal(false)!=true, "false_neq_true");
        q.Columns.Add(new DateTime(2004, 10, 22, 13, 45, 32, 987), "literal_datetime");
        q.Columns.Add(TimeSpan.FromTicks(0), "literal_timespan1");
        q.Columns.Add(TimeSpan.FromTicks(-5997695706986593470L), "literal_timespan2");
        q.Columns.Add(TimeSpan.FromTicks(1626708734287608436L), "literal_timespan3");
        q.Columns.Add(Guid.NewGuid().ToByteArray(), "bytearray");
        q.Columns.Add(new byte[] {0, 1, 0, 1, 0, 143, 240, 255, 0}, "bytearray2");

        #endregion

        #region Bit operators

        q.Columns.Add(Sql.BitNot(Sql.BitNot(5435))==5435, "bitnot");
        q.Columns.Add(Sql.BitAnd(6, 3)==2, "bitand1");
        q.Columns.Add(Sql.BitAnd(Sql.BitOr(3, 6), 10)==2, "bitand2");
        q.Columns.Add(Sql.BitXor(3, 6)==5, "bitxor1");

        #endregion

        #region Math operations

        q.Columns.Add(Sql.Literal(-1234567) < -123456, "less1");
        q.Columns.Add(Sql.Literal(-123456.7) < -12345.6, "less2");
        q.Columns.Add(Sql.Literal(-123456.7m) < -12345.6m, "less3");
        q.Columns.Add(Sql.Literal(12) <= 12, "leq1");
        q.Columns.Add(Sql.Literal(12) >= 12, "geq1");
        q.Columns.Add(Sql.Literal(13) >= 12, "geq2");
        q.Columns.Add(Sql.Literal(12)==12, "eq");
        q.Columns.Add(Sql.Literal(12)!=13, "neq");
        q.Columns.Add(Sql.Literal(14) > 13, "greater");
        q.Columns.Add(Sql.Square(4)==16, "square");
        q.Columns.Add(Sql.Between(Sql.Rand(), 0, 1), "rand2");
        q.Columns.Add(Sql.Between(Sql.Rand(8), 0, 1), "rand");
        q.Columns.Add(Sql.Abs(10.3m)==10.3m, "abs1");
        q.Columns.Add(Sql.Abs(-140.3m)==140.3m, "abs2");
        q.Columns.Add(Sql.Literal(20) % 7==6, "modulo");
        q.Columns.Add(-Sql.Literal(5)==-5, "negate");
        q.Columns.Add(Sql.Literal(2) + 3 * Sql.Literal(4)==14, "right operation order");
        q.Columns.Add(Sql.Cast(3.4564, SqlDataType.Int16)==3, "cast");
        q.Columns.Add(Sql.Cast(-3.4564, SqlDataType.Int16)==-3, "cast2");
        q.Columns.Add(Sql.Cast(Sql.Floor(3.4786), SqlDataType.Int32)==3, "floor");
        q.Columns.Add(Sql.Ceiling(3.4564)==4, "ceiling");
        q.Columns.Add(Sql.Not(Sql.Equals(5, 6)), "not");
        q.Columns.Add(Sql.NotEquals(5, 6), "notequals");
        q.Columns.Add(Sql.Exp(0)==1, "exp");
        q.Columns.Add(Sql.Power(5, 3)==125, "power");
        q.Columns.Add(Sql.Sign(55)==1, "sign1");
        q.Columns.Add(Sql.Sign(-55)==-1, "sign2");
        q.Columns.Add(Sql.Sign(0)==0, "sign3");
        q.Columns.Add(Sql.Sqrt(64)==8, "sqrt");

        #endregion

        #region String operations

        q.Columns.Add('\\'=='\\', "char_equals");
        q.Columns.Add('\\'!='\'', "char_not_equals");
        q.Columns.Add(@"'\"==@"'\", "string_equals");
        q.Columns.Add(@"'\"!=@"'\\", "string_not_equals");
        q.Columns.Add(Sql.Length('F')==1, "char_length1");
        q.Columns.Add(Sql.CharLength('0')==1, "charlength1");
        q.Columns.Add(Sql.Length(Sql.Literal('F'))==1, "char_length4");
        q.Columns.Add(Sql.CharLength("0123456789")==10, "charlength2");
        q.Columns.Add(Sql.Length(@"'\""""")==4, "string_length1");
        q.Columns.Add(Sql.Length(Sql.Literal(@"'\"""""))==4, "string_length2");
        q.Columns.Add(Sql.Trim("  555    ")=="555", "trim1");
        q.Columns.Add(Sql.Trim("  555    ", SqlTrimType.Both)=="555", "trim_both");
        q.Columns.Add(Sql.Trim("  555    ", SqlTrimType.Leading)=="555    ", "trim_leading");
        q.Columns.Add(Sql.Trim("  555    ", SqlTrimType.Trailing)=="  555", "trim_trailing");
        q.Columns.Add(Sql.Trim("555cccc", SqlTrimType.Trailing, 'c')=="555", "trim_trailing2");
        q.Columns.Add(Sql.Trim("555cccc", SqlTrimType.Leading, '5')=="cccc", "trim_leading2");
        q.Columns.Add(Sql.Like("Xtensive", "X%"), "like_%");
        q.Columns.Add(!Sql.Like("Xtensive", "%ee%"), "like_%ee%_1");
        q.Columns.Add(Sql.NotLike("Xtensive", "%ee%"), "like_%ee%_2");
        q.Columns.Add(Sql.Like("Xtensive", "X__n__v_"), "like_Xnv");
        q.Columns.Add(Sql.Substring("Xtensive", 2, 3)=="ens", "substring0");
        q.Columns.Add(Sql.Substring("Xtensive", 2)=="ensive", "substring with one parameter");
        q.Columns.Add(Sql.Position('E', "WEB")==1, "position0");
        q.Columns.Add(2 * Sql.Position('E', "WEB")==2, "position1");
        q.Columns.Add(2 * Sql.Position('E', "WEB") / 2==1, "position2");
        q.Columns.Add(Sql.Concat('0', Sql.Concat("12345", "6789"))=="0123456789", "concat");

        #endregion

        #region Datetime, interval operations

        q.Columns.Add(Sql.CurrentDate()==Sql.FunctionCall("date_trunc", "day", Sql.CurrentTimeStamp()), "current_date_current_timestamp");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Year, new DateTime(2004, 10, 24))==2004, "extract_year");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Month, new DateTime(2004, 10, 24))==10, "extract_month");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Day, new DateTime(2004, 10, 24))==24, "extract_day");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Hour, new DateTime(2004, 10, 24))==0, "extract_hour");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Minute, new DateTime(2000, 9, 12, 23, 45, 11, 234))==45, "extract_minute");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Second, new DateTime(2000, 9, 12, 23, 45, 11, 234))==11, "extract_second");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Millisecond, new DateTime(2000, 9, 12, 23, 45, 11, 234))==234, "extract_milliseconds");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Day, new TimeSpan(1, -2, 200, -40, 432))==1, "interval_extract_day");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Hour, new TimeSpan(1, -2, 200, -40, 432))==1, "interval_extract_hour");
        q.Columns.Add(Sql.Extract(SqlDateTimePart.Minute, new TimeSpan(1, -2, 200, -40, 432))==19, "interval_extract_minute");
        //8.0: small difference (7E-13), 8.2: OK
        //q.Columns.Add(Sql.Extract(SqlDateTimePart.Second, new TimeSpan(1, -2, 200, -40, 432)) == 20.432m, "interval_extract_second");
        //8.0: small difference (7E-13), 8.2: OK
        //q.Columns.Add(Sql.Extract(SqlDateTimePart.Millisecond, new TimeSpan(1, -2, 200, -40, 432)) == 20432, "interval_extract_millisecond");
        q.Columns.Add(!Sql.Overlaps(Sql.Row(new DateTime(2003, 10, 26), new DateTime(2003, 10, 27)), Sql.Row(new DateTime(2003, 10, 27), new DateTime(2003, 10, 28))), "overlaps1");
        q.Columns.Add(Sql.Overlaps(Sql.Row(new DateTime(2004, 10, 26), new DateTime(2004, 10, 27, 1, 1, 1)), Sql.Row(new DateTime(2004, 10, 27), new DateTime(2004, 10, 28))), "overlaps2");
        q.Columns.Add(Sql.Overlaps(Sql.Literal(new DateTime(2005, 10, 26)), new DateTime(2005, 10, 27, 1, 1, 1), new DateTime(2005, 10, 27), new DateTime(2005, 10, 28)), "overlaps3");
        q.Columns.Add(Sql.Overlaps(Sql.Literal(new DateTime(2006, 10, 26)), new TimeSpan(1, 1, 1, 1), new DateTime(2006, 10, 27), new TimeSpan(1, 0, 0, 0)), "overlaps4");

        q.Columns.Add(Sql.Literal(new TimeSpan(12, 13, 14, 15))==Sql.Literal(new TimeSpan(12, 13, 14, 15)));
        q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 20, 13, 54, 1, 527)==new TimeSpan(0), "datetime_sub_1");
        q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 2, 13, 54, 1, 527)==new TimeSpan(18, 0, 0, 0), "datetime_sub_2");
        q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 2, 15, 54, 1, 527)==new TimeSpan(17, 22, 0, 0), "datetime_sub_3");
        q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 2, 15, 32, 1, 527)==new TimeSpan(17, 22, 22, 0), "datetime_sub_4");
        q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 2, 15, 32, 43, 527)==new TimeSpan(17, 22, 21, 18), "datetime_sub_5");
        //not equal
        //q.Columns.Add(Sql.Literal(new DateTime(2005, 10, 20, 13, 54, 01, 527)) - new DateTime(2005, 10, 2, 15, 32, 43, 211) == new TimeSpan(17, 22, 21, 18, 316), "datetime_sub_6");

        #endregion

        #region Arrays

        q.Columns.Add(Sql.Array(true, false, true));
        q.Columns.Add(Sql.Array((byte) 1, (byte) 2, (byte) 3));
        q.Columns.Add(new byte[] {1, 2, 3});
        q.Columns.Add(Sql.Array((ushort) 1, (ushort) 2, (ushort) 3));
        q.Columns.Add(Sql.Array((sbyte) -1, (sbyte) -2, (sbyte) -3));
        q.Columns.Add(Sql.Array((short) -1, (short) -2, (short) -3));
        q.Columns.Add(Sql.Array(-100000, -200000, -300000));
        q.Columns.Add(Sql.Array(-100000000, -200000000, -300000000));
        q.Columns.Add(Sql.Array(-100000000000, -200000000000, -300000000000));
        q.Columns.Add(Sql.Array(100000000, 200000000, 300000000));
        q.Columns.Add(Sql.Array(100000000000, 200000000000, 300000000000));
        q.Columns.Add(Sql.Array("C", "#"));
        q.Columns.Add(Sql.Array(DateTime.Now, DateTime.Now, DateTime.Now));
        q.Columns.Add(Sql.Array(new TimeSpan(0), new TimeSpan(97593738954343L), new TimeSpan(-99875392765867893L)));
        q.Columns.Add(Sql.Array("123", "456", "789"));
        q.Columns.Add(Sql.Array(1.5, 2.6, 3.7));
        q.Columns.Add(Sql.Array(1.5m, 2.6m, 3.7m));
        q.Columns.Add(Sql.Array(new string[] {}));

        #endregion

        #region In, NotIn array

        q.Columns.Add(!Sql.In(true, Sql.Array(new bool[0])));
        q.Columns.Add(Sql.NotIn(true, Sql.Array(new bool[0])));
        q.Columns.Add(Sql.NotIn(true, Sql.Array(false)));
        q.Columns.Add(Sql.In(true, Sql.Array(true, false)));
        q.Columns.Add(Sql.NotIn('T', Sql.Array(new char[0])));
        q.Columns.Add(Sql.NotIn('Z', Sql.Array('g', 'i')));
        q.Columns.Add(Sql.In('I', Sql.Array('u', 'I', 'K')));
        q.Columns.Add(Sql.NotIn("Ő", Sql.Array(new string[0])));
        q.Columns.Add(Sql.NotIn("Űr", Sql.Array("Ö", "í")));
        q.Columns.Add(Sql.In("Őz", Sql.Array("6gfwerw", "", "Őz")));
        q.Columns.Add(Sql.In("", Sql.Array("6gfwerw", "", "Őz")));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new byte[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new byte[] {12, 76, 45, 91})));
        q.Columns.Add(Sql.In(168, Sql.Array(new byte[] {233, 128, 168})));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new sbyte[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new sbyte[] {12, 76, 45, 91})));
        q.Columns.Add(Sql.In(-33, Sql.Array(new sbyte[] {-33, 127, 68})));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new int[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new[] {1280942, 76, -45, 1994794321})));
        q.Columns.Add(Sql.In(168, Sql.Array(new[] {2864333, -1238974228, 168})));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new uint[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new uint[] {12, 76, 45, 91})));
        q.Columns.Add(Sql.In(168, Sql.Array(new uint[] {2923733, 10702228, 168})));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new long[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new long[] {12, 76, -49328755, 91})));
        q.Columns.Add(Sql.In(168, Sql.Array(new[] {-928438632233, 128, 168})));
        q.Columns.Add(Sql.NotIn(35, Sql.Array(new ulong[0])));
        q.Columns.Add(Sql.NotIn(98, Sql.Array(new ulong[] {1219859843, 76, 454876485389732323, 91})));
        q.Columns.Add(Sql.In(233, Sql.Array(new ulong[] {233, 128, 15978327274368})));
        q.Columns.Add(Sql.NotIn(DateTime.Now, Sql.Array(new DateTime[0])));
        q.Columns.Add(Sql.NotIn(new DateTime(1974, 11, 12), Sql.Array(new[] {new DateTime(1994, 11, 12), new DateTime(1874, 1, 3), new DateTime(2004, 12, 19)})));
        q.Columns.Add(Sql.In(new DateTime(1974, 11, 12, 3, 12, 45, 397), Sql.Array(new[] {new DateTime(1994, 11, 12), new DateTime(1974, 11, 12, 3, 12, 45, 397), new DateTime(2004, 12, 19)})));
        q.Columns.Add(Sql.NotIn(new DateTime(1974, 11, 12, 3, 12, 45, 397), Sql.Array(new[] {new DateTime(1994, 11, 12), new DateTime(1974, 11, 12, 3, 12, 45, 398), new DateTime(2004, 12, 19)})));

        #endregion

        #region All, any, some

        q.Columns.Add(3 > Sql.All(new Common.PgSql.Func<SqlSelect>(delegate {
          SqlSelect q2 = Sql.Select();
          q2.Columns.Add(2);
          return q2;
        })()));
        q.Columns.Add(3 > Sql.Any(new Common.PgSql.Func<SqlSelect>(delegate {
          SqlSelect q2 = Sql.Select();
          q2.Columns.Add(2);
          return q2;
        })()));
        q.Columns.Add(3 > Sql.Some(new Common.PgSql.Func<SqlSelect>(delegate {
          SqlSelect q2 = Sql.Select();
          q2.Columns.Add(2);
          return q2;
        })()));

        #endregion

        #region Row

        q.Columns.Add(Sql.Row(Sql.Acos(Sql.Literal(0.4F)), 7, 9), "arithmetic row");
        q.Columns.Add(Sql.Row("A", "B", "C"), "string row2");
        q.Columns.Add(Sql.Row(Sql.Concat("A", "A"), "B", "C"), "string row1");

        #endregion

        #region Case

        q.Columns.Add(Sql.Case(Sql.Concat("s", "")).Add("s", Sql.Literal(true) || true));
        q.Columns.Add(Sql.Case(Sql.And(true, false)).Add(false, Sql.Length(Sql.Concat("abc", "def"))==6));

        #endregion

        #region Users

        q.Columns.Add(Sql.SessionUser()==Connection.ConnectionInfo.User, "session_user");
        q.Columns.Add(Sql.CurrentUser()==Connection.ConnectionInfo.User, "current_user");
        q.Columns.Add(Sql.User()==Connection.ConnectionInfo.User, "user");
        //q.Columns.Add(Sql.SystemUser());

        #endregion

        #region Other functions, oparators

        q.Columns.Add(Sql.Coalesce(Sql.Null, 3)==3, "coalesce1");
        q.Columns.Add(Sql.Coalesce(Sql.Null, Sql.Null + 5, 3)==3, "coalesce2");
        q.Columns.Add(Sql.Coalesce(Sql.Null, Sql.Null + 5, 3, 9)==3, "coalesce3");
        q.Columns.Add(Sql.IsNotNull('R'), "isnotnull");
        q.Columns.Add(Sql.IsNull(Sql.Null - 7), "isnull1");
        q.Columns.Add(Sql.IsNull(Sql.NullIf(5, 5)), "isnull2");
        q.Columns.Add(Sql.NullIf(5, 6)==5, "nullif1");
        q.Columns.Add(Sql.In('E', Sql.Row("'J'", 'E', '\'')), "in_row");
        q.Columns.Add(Sql.NotIn('E', Sql.Row("'J'", '\\', 'Z')), "not_in_row");
        q.Columns.Add(Sql.NotBetween("between'", "bezier'", "lagrange'"), "not_between");
        q.Columns.Add(Sql.Length(Sql.Cast("", SqlDataType.VarBinaryMax))==0, "cast_bytea1");
        q.Columns.Add(Sql.Length(Sql.Cast(@"\050", SqlDataType.VarBinaryMax))==1, "cast_bytea2");
        q.Columns.Add(Sql.Length(Sql.Cast(@"abc\\", SqlDataType.VarBinaryMax))==4, "cast_bytea5");

        #endregion

        using (SqlCommand cmd = Connection.CreateCommand(q)) {
          using (DbDataReader dr = cmd.ExecuteReader()) {
            while (dr.Read()) {
              for (int i = 0; i < dr.FieldCount; i++) {
                object value = dr.GetValue(i);
                if (value is bool) {
                  Assert.IsTrue(dr.GetBoolean(i), "'" + dr.GetName(i) + "' column is not true!");
                }
                else if (i==dr.GetOrdinal("guid literal")) {
                  Assert.IsTrue(guid.Equals(new Guid(dr["guid literal"] as byte[])));
                }
              }
            }
          }
        }
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
    }

    [Test]
    public void UniquePredicateTest()
    {
      DbTransaction trx = Connection.BeginTransaction();
      try {
        SqlBatch batch = Sql.Batch();
        TemporaryTable t = MyCatalog.DefaultSchema.CreateTemporaryTable("unique_pred_test");
        t.PreserveRows = false;
        t.CreateColumn("id", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("col1", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        t.CreateColumn("col2", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        batch.Add(Sql.Create(t));
        SqlTableRef tref = Sql.TableRef(t);

        Proc<int, SqlExpression, SqlExpression> insert = delegate(int id, SqlExpression expr1, SqlExpression expr2) {
          SqlInsert ins2 = Sql.Insert(tref);
          ins2.Values.Add(tref["id"], id);
          ins2.Values.Add(tref["col1"], expr1);
          ins2.Values.Add(tref["col2"], expr2);
          batch.Add(ins2);
        };
        //unique part
        insert(01, Sql.Null, Sql.Null);
        insert(02, Sql.Null, Sql.Null);
        insert(03, Sql.Null, 1);
        insert(04, 1, Sql.Null);
        insert(05, 2, 3);
        insert(06, Sql.Null, 2);
        insert(07, 3, Sql.Null);
        insert(08, 3, 2);
        //non-unique part
        insert(10, Sql.Null, 2);
        insert(11, 3, Sql.Null);
        insert(12, 3, 2);

        ExecuteNonQuery(batch);

        //query

        SqlSelect initialQuery = Sql.Select(tref);
        initialQuery.Columns.Add(tref["col1"]);
        initialQuery.Columns.Add(tref["col2"]);
        {
          SqlSelect q = Sql.Select();
          q.Columns.Add(1);
          q.Where = Sql.Unique(new Common.PgSql.Func<SqlSelect>(delegate {
            SqlSelect q2 = Sql.Select(tref);
            q2.Columns.Add(tref["col1"]);
            q2.Columns.Add(tref["col2"]);

//            q2 = initialQuery.Clone() as SqlSelect;
            q2.Where = tref["id"] < 10;
            return q2;
          })());
          AssertQueryExists(q);
        }

        {
          SqlSelect q = Sql.Select();
          q.Columns.Add(1);
          q.Where = Sql.Unique(new Common.PgSql.Func<SqlSelect>(delegate {
            SqlSelect q2 = Sql.Select(tref);
            q2.Columns.Add(tref["col1"]);
            q2.Columns.Add(tref["col2"]);
            return q2;
          })());

          AssertQueryNotExists(q);
        }
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
      finally {
        trx.Rollback();
      }
    }

    [Test]
    public void MatchPredicateTest()
    {
      DbTransaction trx = Connection.BeginTransaction();
      try {
        SqlBatch batch = Sql.Batch();
        TemporaryTable t = MyCatalog.DefaultSchema.CreateTemporaryTable("match_pred_test");
        t.PreserveRows = false;
        t.CreateColumn("id", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("col1", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        t.CreateColumn("col2", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        batch.Add(Sql.Create(t));
        SqlTableRef tref = Sql.TableRef(t);

        //fill
        {
          int id = 1;
          Proc<SqlExpression, SqlExpression> insert = delegate(SqlExpression expr1, SqlExpression expr2) {
            SqlInsert ins2 = Sql.Insert(tref);
            ins2.Values.Add(tref["id"], id);
            ins2.Values.Add(tref["col1"], expr1);
            ins2.Values.Add(tref["col2"], expr2);
            batch.Add(ins2);
            id++;
          };
          //unique part
          insert(Sql.Null, Sql.Null);
          insert(Sql.Null, Sql.Null);
          insert(Sql.Null, 2);
          insert(1, Sql.Null);
          insert(1, 2);
          insert(5, Sql.Null);
          insert(Sql.Null, 6);
          //non-unique part
          insert(Sql.Null, 4);
          insert(Sql.Null, 4);
          insert(3, Sql.Null);
          insert(3, Sql.Null);
          insert(3, 4);
          insert(3, 4);
        }

        ExecuteNonQuery(batch);

        //query
        SqlSelect testQuery = Sql.Select(tref);
        testQuery.Columns.Add(tref["col1"]);
        testQuery.Columns.Add(tref["col2"]);

        Proc<SqlExpression, SqlExpression, bool, SqlMatchType> matchTesterExists
          = delegate(SqlExpression col1, SqlExpression col2, bool unique, SqlMatchType matchType) {
            SqlSelect q = Sql.Select();
            q.Columns.Add(1);
            q.Where = Sql.Match(Sql.Row(col1, col2), Sql.SubQuery(testQuery).Query, unique, matchType);
            AssertQueryExists(q);
          };

        Proc<SqlExpression, SqlExpression, bool, SqlMatchType> matchTesterNotExists
          = delegate(SqlExpression col1, SqlExpression col2, bool unique, SqlMatchType matchType) {
            SqlSelect q = Sql.Select();
            q.Columns.Add(1);
            q.Where = Sql.Match(Sql.Row(col1, col2), Sql.SubQuery(testQuery).Query, unique, matchType);
            AssertQueryNotExists(q);
          };

        //match simple
        {
          matchTesterExists(Sql.Null, Sql.Null, false, SqlMatchType.None);
          matchTesterExists(Sql.Null, Sql.Null, true, SqlMatchType.None);
          matchTesterExists(Sql.Null, 2, false, SqlMatchType.None);
          matchTesterExists(Sql.Null, 2, true, SqlMatchType.None);
          matchTesterExists(Sql.Null, 4, false, SqlMatchType.None);
          matchTesterExists(Sql.Null, 4, true, SqlMatchType.None);
          matchTesterExists(Sql.Null, 3, false, SqlMatchType.None);
          matchTesterExists(Sql.Null, 3, true, SqlMatchType.None);
          matchTesterExists(9, Sql.Null, false, SqlMatchType.None);
          matchTesterExists(9, Sql.Null, true, SqlMatchType.None);
          matchTesterExists(9, Sql.Null + 2, true, SqlMatchType.None);
          matchTesterExists(1, 2, false, SqlMatchType.None);
          matchTesterExists(1, 1 + Sql.Literal(1), false, SqlMatchType.None);
          matchTesterExists(1, 2, true, SqlMatchType.None);
          matchTesterExists(1, 3 - Sql.Literal(1), true, SqlMatchType.None);
          matchTesterExists(3, 4, false, SqlMatchType.None);
          matchTesterExists(3 + Sql.Literal(0), 4, false, SqlMatchType.None);
          matchTesterNotExists(3, 4, true, SqlMatchType.None);
          matchTesterNotExists(3, 4 + Sql.Literal(0), true, SqlMatchType.None);
          matchTesterNotExists(3, 3, false, SqlMatchType.None);
          matchTesterNotExists(3 + Sql.Literal(0), 3, false, SqlMatchType.None);
          matchTesterNotExists(3, 3, true, SqlMatchType.None);
          matchTesterNotExists(3, 3 + Sql.Literal(0), true, SqlMatchType.None);
          matchTesterNotExists(1, 3, false, SqlMatchType.None);
          matchTesterNotExists(1 - Sql.Literal(0), 3, false, SqlMatchType.None);
          matchTesterNotExists(1, 3, true, SqlMatchType.None);
          matchTesterNotExists(1 + Sql.Literal(0), 3, true, SqlMatchType.None);
        }

        //match full
        {
          matchTesterExists(Sql.Null, Sql.Null, false, SqlMatchType.Full);
          matchTesterExists(Sql.Null, Sql.Null, true, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 2, false, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 2, true, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 4, false, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 4, true, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 3, false, SqlMatchType.Full);
          matchTesterNotExists(Sql.Null, 3, true, SqlMatchType.Full);
          matchTesterNotExists(9, Sql.Null, false, SqlMatchType.Full);
          matchTesterNotExists(9, Sql.Null, true, SqlMatchType.Full);
          matchTesterExists(1, 2, false, SqlMatchType.Full);
          matchTesterExists(1, 2, true, SqlMatchType.Full);
          matchTesterExists(3, 4, false, SqlMatchType.Full);
          matchTesterNotExists(3, 4, true, SqlMatchType.Full);
          matchTesterNotExists(9, 9, false, SqlMatchType.Full);
          matchTesterNotExists(9, 9, true, SqlMatchType.Full);
          matchTesterNotExists(1, 9, false, SqlMatchType.Full);
          matchTesterNotExists(1, 9, true, SqlMatchType.Full);
        }

        //match partial
        {
          matchTesterExists(Sql.Null, Sql.Null, false, SqlMatchType.Partial);
          matchTesterExists(Sql.Null + 1, Sql.Null - 2, false, SqlMatchType.Partial);

          matchTesterExists(Sql.Null, Sql.Null, true, SqlMatchType.Partial);
          matchTesterExists(Sql.Null + 2, Sql.Null - 3, true, SqlMatchType.Partial);

          matchTesterExists(Sql.Null, 2, false, SqlMatchType.Partial);
          matchTesterExists(Sql.Null * 4, 1 + Sql.Literal(1), false, SqlMatchType.Partial);

          matchTesterNotExists(Sql.Null, 2, true, SqlMatchType.Partial);
          matchTesterNotExists(Sql.Null * 5, 3 - Sql.Literal(1), true, SqlMatchType.Partial);

          matchTesterExists(Sql.Null, 4, false, SqlMatchType.Partial);
          matchTesterExists(Sql.Null - 8, Sql.Literal(3) + 1, false, SqlMatchType.Partial);

          matchTesterNotExists(Sql.Null, 4, true, SqlMatchType.Partial);
          matchTesterNotExists(Sql.Null / 7, 4 + Sql.Literal(0), true, SqlMatchType.Partial);

          matchTesterNotExists(Sql.Null, 3, false, SqlMatchType.Partial);
          matchTesterNotExists(4 * Sql.Null, 3, false, SqlMatchType.Partial);

          matchTesterNotExists(Sql.Null, 3, true, SqlMatchType.Partial);
          matchTesterNotExists(Sql.Null - 6, 3, true, SqlMatchType.Partial);

          matchTesterExists(5, Sql.Null, false, SqlMatchType.Partial);
          matchTesterExists(1 * Sql.Literal(5), Sql.Null, false, SqlMatchType.Partial);

          matchTesterExists(5, Sql.Null, true, SqlMatchType.Partial);
          matchTesterExists(5, Sql.Null / 2, true, SqlMatchType.Partial);

          matchTesterExists(Sql.Null, 6, false, SqlMatchType.Partial);
          matchTesterExists(Sql.Null - 0, 6, false, SqlMatchType.Partial);

          matchTesterExists(Sql.Null, 6, true, SqlMatchType.Partial);
          matchTesterExists(Sql.Null, Sql.Literal(1) + 5, true, SqlMatchType.Partial);

          matchTesterNotExists(9, Sql.Null, false, SqlMatchType.Partial);
          matchTesterNotExists(9, Sql.Null, true, SqlMatchType.Partial);

          matchTesterExists(1, 2, false, SqlMatchType.Partial);
          matchTesterExists(1, 1 + Sql.Literal(1), false, SqlMatchType.Partial);

          matchTesterExists(1, 2, true, SqlMatchType.Partial);
          matchTesterExists(1, 3 - Sql.Literal(1), true, SqlMatchType.Partial);

          matchTesterExists(3, 4, false, SqlMatchType.Partial);
          matchTesterExists(3, Sql.Square(2), false, SqlMatchType.Partial);

          matchTesterNotExists(3, 4, true, SqlMatchType.Partial);
          matchTesterNotExists(Sql.Literal(4) - 1, 4, true, SqlMatchType.Partial);

          matchTesterNotExists(9, 9, false, SqlMatchType.Partial);
          matchTesterNotExists(9, 3 * Sql.Literal(3), false, SqlMatchType.Partial);

          matchTesterNotExists(9, 9, true, SqlMatchType.Partial);
          matchTesterNotExists(9, 3 * Sql.Literal(3), true, SqlMatchType.Partial);

          matchTesterNotExists(1, 9, false, SqlMatchType.Partial);
          matchTesterNotExists(1 + Sql.Literal(0), 9, false, SqlMatchType.Partial);

          matchTesterNotExists(1, 9, true, SqlMatchType.Partial);
          matchTesterNotExists(1 + Sql.Literal(0), 9, true, SqlMatchType.Partial);
        }
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
      finally {
        trx.Rollback();
      }
    }

    [Test]
    public void AggregateTest()
    {
      DbTransaction trx = Connection.BeginTransaction();
      try {
        SqlBatch batch = Sql.Batch();
        TemporaryTable t = MyCatalog.DefaultSchema.CreateTemporaryTable("agg_test");
        t.PreserveRows = false;
        t.CreateColumn("id", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("col1", new SqlValueType(SqlDataType.Int32)).IsNullable = true;
        batch.Add(Sql.Create(t));
        SqlTableRef tref = Sql.TableRef(t);

        Proc<int, SqlExpression> insert = delegate(int id, SqlExpression expr1) {
          SqlInsert ins2 = Sql.Insert(tref);
          ins2.Values.Add(tref["id"], id);
          ins2.Values.Add(tref["col1"], expr1);
          batch.Add(ins2);
        };
        //unique part
        insert(01, 1);
        insert(02, Sql.Null);
        insert(03, 3);
        insert(04, 4);
        insert(05, 5);
        insert(06, Sql.Null);
        insert(07, 7);
        insert(08, 8);
        //non-unique part
        insert(10, 3);
        insert(11, 4);
        insert(12, 7);

        ExecuteNonQuery(batch);

        //query

        {
          SqlSelect q = Sql.Select(tref);
          q.Columns.Add(Sql.Count(Sql.Asterisk)); //0
          q.Columns.Add(Sql.Count(tref["col1"])); //1
          q.Columns.Add(Sql.Count(tref["col1"], true)); //2
          q.Columns.Add(Sql.Min(tref["col1"])); //3
          q.Columns.Add(Sql.Min(tref["col1"], true)); //4
          q.Columns.Add(Sql.Max(tref["col1"])); //5
          q.Columns.Add(Sql.Max(tref["col1"], true)); //6
          q.Columns.Add(Sql.Sum(tref["col1"])); //7
          q.Columns.Add(Sql.Sum(tref["col1"], true)); //8
          q.Columns.Add(Sql.Cast(Sql.Avg(tref["col1"]), SqlDataType.Double)); //9
          q.Columns.Add(Sql.Avg(tref["col1"], true)); //10 -->decimal


          using (DbDataReader dr = ExecuteQuery(q)) {
            Assert.IsTrue(dr.Read());

            //int32-vel nem megy át count()->int64
            Assert.AreEqual(11, dr.GetInt64(0)); //count of rows
            Assert.AreEqual(9, dr.GetInt64(1)); //count of not null values
            Assert.AreEqual(6, dr.GetInt64(2)); //count of unique not null values
            Assert.AreEqual(1, Convert.ToInt32(dr[3])); //min
            Assert.AreEqual(1, Convert.ToInt32(dr[4])); //min
            Assert.AreEqual(8, Convert.ToInt32(dr[5])); //max
            Assert.AreEqual(8, Convert.ToInt32(dr[6])); //max
            Assert.AreEqual(42, Convert.ToInt32(dr[7])); //sum not null values
            Assert.AreEqual(28, Convert.ToInt32(dr[8])); //sum not null unique values
            Assert.AreEqual(Math.Round(42d / 9, 10), Math.Round(dr.GetDouble(9), 10)); //avg not null values
            Assert.AreEqual(Math.Round(28d / 6, 10), Math.Round(dr.GetDecimal(10), 10)); //avg not null unique values

            Assert.IsFalse(dr.Read());

            long l = 4;
            //ez megy
            int m = (int) l;
            //ez nem megy:
            //long n = (long)((object)m);
          }
        }
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
      finally {
        trx.Rollback();
      }
    }


    [Test]
    public void IntOrderByTest1()
    {
      SqlSelect q = Sql.Select();
      q.Columns.Add(2, "col");
      q.OrderBy.Add(1);
      using (ExecuteQuery(q)) {
      }
    }

    [Test]
    public void IntOrderByTest2()
    {
      SqlSelect q = Sql.Select();
      q.Columns.Add(2, "col");
      q.OrderBy.Add(Sql.Order(1));
      using (ExecuteQuery(q)) {
      }
    }

    [Test]
    public void SetOperationTest()
    {
      Common.PgSql.Func<int, SqlSelect> selectCreator = delegate(int n) {
        SqlSelect q = Sql.Select();
        q.Columns.Add(n, "col");
        return q;
      };

      ISqlCompileUnit unit;
      SqlCompilerResults res;

      {
        unit = Sql.Union(selectCreator(1), selectCreator(2));
        res = Driver.Compile(unit);
      }

      {
        SqlSelect q = Sql.Select();
        q.Columns.Add(Sql.In(1, Sql.Union(selectCreator(1), selectCreator(2))));
        using (DbDataReader dr = ExecuteQuery(q)) {
          bool first = true;
          while (dr.Read()) {
            if (!first)
              Assert.Fail(">1 row");
            Assert.AreEqual(true, dr.GetBoolean(0));
            first = false;
          }
          if (first)
            Assert.Fail("empty result");
        }
      }
    }

    [Test]
    public void UnionAndIntersectTest()
    {
      SqlSelect q = Sql.Select(Sql.QueryRef(
        (SingleNumberSelect(1)
          .Union(SingleNumberSelect(2)))
          .Intersect(SingleNumberSelect(3)
            .Union(SingleNumberSelect(4)))));
      q.Columns.Add(1);

      AssertQueryNotExists(q);
    }

    [Test]
    public void UnionAndExceptTest()
    {
      SqlSelect q = Sql.Select(Sql.QueryRef(
        (SingleNumberSelect(1)
          .Union(SingleNumberSelect(2)))
          .Except(SingleNumberSelect(1)
            .Union(SingleNumberSelect(2)))));
      q.Columns.Add(1);
      AssertQueryNotExists(q);
    }

    [Test]
    public void UnionAllTest()
    {
      SqlSelect q = Sql.Select(Sql.QueryRef(
        SingleNumberSelect(1).UnionAll(SingleNumberSelect(1))));
      q.Columns.Add(Sql.Count(Sql.Asterisk));

      SqlSelect q2 = Sql.Select();
      q2.Where = 2==Sql.SubQuery(q);
      q2.Columns.Add(5);
      AssertQueryExists(q2);
    }

    [Test]
    public void UnionAllWithOrderByTest()
    {
      SqlSelect q1 = SingleNumberSelect(2);

      SqlSelect q2 = SingleNumberSelect(1);
      q2.OrderBy.Add(q2.Columns[0]);

      SqlQueryExpression union = Sql.UnionAll(q1, q2);

      using (DbDataReader dr = ExecuteQuery(union)) {
        int i;
        for (i = 0; dr.Read(); i++) {
          int value = Convert.ToInt32(dr[0]);
          if (i==0)
            Assert.AreEqual(2, value, "First value not 2");
          else if (i==1)
            Assert.AreEqual(1, value, "Second value not 1");
          else
            Assert.Fail("More than 2 rows");
        }
        if (i!=2)
          Assert.Fail("Not 2 rows");
      }
    }

    [Test]
    public void CursorTest()
    {
      DbTransaction trx = Connection.BeginTransaction();
      try {
        SqlBatch batch = Sql.Batch();
        TemporaryTable t = MyCatalog.DefaultSchema.CreateTemporaryTable("cursor_test");
        t.PreserveRows = false;
        t.CreateColumn("id", new SqlValueType(SqlDataType.Int32));
        t.CreateColumn("dt", new SqlValueType(SqlDataType.DateTime));
        t.CreateColumn("bool", new SqlValueType(SqlDataType.Boolean));
        batch.Add(Sql.Create(t));
        SqlTableRef tref = Sql.TableRef(t);

        //fill
        {
          int id = 1;
          Proc<SqlExpression, SqlExpression> insert = delegate(SqlExpression expr1, SqlExpression expr2) {
            SqlInsert ins2 = Sql.Insert(tref);
            ins2.Values.Add(tref["id"], id);
            ins2.Values.Add(tref["dt"], expr1);
            ins2.Values.Add(tref["bool"], id % 2==0);
            batch.Add(ins2);
            id++;
          };
          insert(new DateTime(2001, 1, 1, 1, 1, 1, 111), 0);
          insert(new DateTime(2002, 2, 2, 2, 2, 2, 222), 0);
          insert(new DateTime(2003, 3, 3, 3, 3, 3, 333), 0);
          insert(new DateTime(2004, 4, 4, 4, 4, 4, 444), 0);
          insert(new DateTime(2005, 5, 5, 5, 5, 5, 555), 0);
          insert(new DateTime(2006, 6, 6, 6, 6, 6, 666), 0);
          insert(new DateTime(2007, 7, 7, 7, 7, 7, 777), 0);
          insert(new DateTime(2008, 8, 8, 8, 8, 8, 888), 0);
          insert(new DateTime(2009, 9, 9, 9, 9, 9, 999), 0);
        }

        ExecuteNonQuery(batch);
        //query
        {
          SqlSelect testQuery = Sql.Select(tref);
          testQuery.Columns.Add(Sql.Asterisk);
          testQuery.OrderBy.Add(tref["id"]);

          SqlCursor mycursor = Sql.Cursor("mycursor", testQuery);
          mycursor.Insensitive = true;
          mycursor.WithHold = false;
          mycursor.Scroll = true;

          ExecuteNonQuery(mycursor.Declare());

          Common.PgSql.Func<ISqlCompileUnit, DataTable> Execute = delegate(ISqlCompileUnit stmt) {
            using (DbDataReader dr = ExecuteQuery(stmt)) {
              DataTable result = new DataTable();
              result.Columns.Add("id", typeof (int));
              result.Columns.Add("dt", typeof (DateTime));
              result.Columns.Add("bool", typeof (bool));
              while (dr.Read()) {
                result.Rows.Add(Convert.ToInt32(dr["id"]), dr["dt"], dr["bool"]);
              }
              return result;
            }
          };


          {
            ExecuteNonQuery(mycursor.Open());
            {
              DataTable result;
              SqlFetch f;

              f = mycursor.Fetch(SqlFetchOption.Next);
              result = Execute(f); //fetch first row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(1, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, 1);
              result = Execute(f); //fetch second row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(2, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, -1);
              result = Execute(f); //fetch first row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(1, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, 0);
              result = Execute(f); //fetch same row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(1, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, 5);
              result = Execute(f); //fetch 6th row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(6, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, -3);
              result = Execute(f); //fetch 3th row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(3, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Last);
              result = Execute(f); //fetch last row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(9, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Prior);
              result = Execute(f); //fetch prior row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(8, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.Absolute, 4);
              result = Execute(f); //fetch 4th row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(4, (int) result.Rows[0][0]);

              f = mycursor.Fetch(SqlFetchOption.First);
              result = Execute(f); //fetch 1th row
              Assert.AreEqual(1, result.Rows.Count);
              Assert.AreEqual(1, (int) result.Rows[0][0]);

              /*
              f = mycursor.Fetch(SqlFetchOption.Relative,3);
              result = Execute(f);//fetch next 3 rows
              Assert.AreEqual(3, result.Rows.Count);
              Assert.AreEqual(2, (int)result.Rows[0][0]);
              Assert.AreEqual(3, (int)result.Rows[1][0]);
              Assert.AreEqual(4, (int)result.Rows[2][0]);

              f = mycursor.Fetch(SqlFetchOption.Relative, 6);
              result = Execute(f);//fetch next 6 rows (only 5 remaining)
              Assert.AreEqual(5, result.Rows.Count);
              Assert.AreEqual(5, (int)result.Rows[0][0]);
              Assert.AreEqual(6, (int)result.Rows[1][0]);
              Assert.AreEqual(7, (int)result.Rows[2][0]);
              Assert.AreEqual(8, (int)result.Rows[3][0]);
              Assert.AreEqual(9, (int)result.Rows[4][0]);
              /**/
            }
            ExecuteNonQuery(mycursor.Close());

            ExecuteNonQuery(mycursor.Open());
            {
            }
            ExecuteNonQuery(mycursor.Close());
          }

          {
          }
        }
      }
      catch (Exception ex) {
        Assert.Fail(ex.ToString());
      }
      finally {
        trx.Rollback();
      }
    }

/**/


    protected void ExecuteNonQuery(ISqlCompileUnit stmt)
    {
      using (SqlCommand cmd = Connection.CreateCommand(stmt)) {
        int result = cmd.ExecuteNonQuery();
      }
    }

    protected DbDataReader ExecuteQuery(ISqlCompileUnit stmt)
    {
      using (SqlCommand cmd = Connection.CreateCommand(stmt)) {
        return cmd.ExecuteReader();
      }
    }

    protected void AssertQueryExists(ISqlCompileUnit q)
    {
      using (SqlCommand cmd = Connection.CreateCommand(q)) {
        using (DbDataReader dr = cmd.ExecuteReader()) {
          bool exists = false;
          while (dr.Read()) {
            exists = true;
            break;
          }
          if (!exists)
            Assert.Fail("Query not exists.");
        }
      }
    }

    protected void AssertQueryNotExists(ISqlCompileUnit q)
    {
      using (SqlCommand cmd = Connection.CreateCommand(q)) {
        using (DbDataReader dr = cmd.ExecuteReader()) {
          while (dr.Read()) {
            Assert.Fail("Query exists.");
          }
        }
      }
    }

    protected SqlSelect SingleNumberSelect(int n)
    {
      SqlSelect q = Sql.Select();
      q.Columns.Add(n, "col");
      return q;
    }
  }
}