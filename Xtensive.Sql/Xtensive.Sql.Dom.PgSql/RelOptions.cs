namespace Xtensive.Sql.Dom.PgSql
{
  public class RelOptions
  {
    internal RelOptions()
    {
    }

    #region FillFactor property

    private byte? mFillFactor;

    public byte? FillFactor
    {
      get { return mFillFactor; }
      set { mFillFactor = value; }
    }

    #endregion
  }
}