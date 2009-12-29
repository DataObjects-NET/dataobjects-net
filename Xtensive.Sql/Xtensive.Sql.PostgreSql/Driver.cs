using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql
{
  internal abstract class Driver : SqlDriver
  {
    protected override ValueTypeMapping.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    public override SqlConnection CreateConnection()
    {
      return new Connection(this);
    }

    // Constructors

    protected Driver(CoreServerInfo coreServerInfo)
      : base(coreServerInfo)
    {
    }
  }
}