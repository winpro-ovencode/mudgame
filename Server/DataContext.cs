using ServiceStack.OrmLite;

public class DBContext
{
    static OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory("Server=52.79.44.254;Database=study;User Id=root;Password=ovencode;", MySqlDialect.Provider); // ConnectionString
    public static System.Data.IDbConnection Open()
    {
        var con = factory.CreateDbConnection();
        con.Open();
        return con;
    }
}