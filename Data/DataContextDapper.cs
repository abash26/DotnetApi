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

  public T? LoadDataSingle<T>(string sql, DynamicParameters parameters = null)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.QuerySingleOrDefault<T>(sql, parameters);
  }

  public bool ExecuteSql(string sql, DynamicParameters parameters = null)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.Execute(sql, parameters) > 0;
  }

  public int ExecuteSqlWithRowCount(string sql)
  {
    var dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    return dbConnection.Execute(sql);
  }
}