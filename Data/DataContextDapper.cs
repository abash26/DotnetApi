using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Data;

class DataContextDapper
{
  private readonly IConfiguration _config;

  public DataContextDapper(IConfiguration config)
  {
    _config = config;
  }

  public IEnumerable<T> LoadData<T>(string sql, DynamicParameters parameters = null)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    var results = dbConnection.Query<T>(sql, parameters);
    return results;
  }

  public T LoadDataSingle<T>(string sql)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.QuerySingle<T>(sql);
  }

  public bool ExecuteSql(string sql)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.Execute(sql) > 0;
  }

  public int ExecuteSqlWithRowCount(string sql)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.Execute(sql);
  }

  public bool ExecuteSqlWithParam(string sql, List<SqlParameter> parameters)
  {
    var command = new SqlCommand(sql);
    foreach (SqlParameter parameter in parameters)
    {
      command.Parameters.Add(parameter);
    }

    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    dbConnection.Open();

    command.Connection = dbConnection;

    var rowsAffected = command.ExecuteNonQuery();
    dbConnection.Close();

    return rowsAffected > 0;
  }
}