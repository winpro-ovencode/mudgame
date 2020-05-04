using ServiceStack.OrmLite;

public class DBContext
{
    static OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory("Server=15.164.99.198;Database=study;User Id=study;Password=0000;", MySqlDialect.Provider); // ConnectionString
    public static System.Data.IDbConnection Open()
    {
        var con = factory.CreateDbConnection();
        con.Open();
        return con;
    }
}