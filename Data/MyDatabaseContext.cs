using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNetCoreSqlDb.Models
{
    public class MyDatabaseContext : DbContext
    {
        ILogger<MyDatabaseContext> _logger;

        public MyDatabaseContext(DbContextOptions<MyDatabaseContext> options, ILogger<MyDatabaseContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<DotNetCoreSqlDb.Models.Todo> Todo { get; set; }


        public void ExecuteSP()
        {
            SqlConnection connection = null;
            try
            {
                _logger.LogInformation("Calling stored procedure to update description");
                connection = new SqlConnection("Server=tcp:XXXXXXXXX.database.windows.net,1433;Initial Catalog=cntsodb;Persist Security Info=False;User ID=XXXXXXXXX;Password=XXXXXXXXX;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                connection.Open();
                SqlCommand sqlCommand = new SqlCommand("UpdateTodoItem",connection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@id", 2);
                sqlCommand.Parameters.AddWithValue("@description", "Description " + Guid.NewGuid().ToString());
                sqlCommand.ExecuteNonQuery();                
                _logger.LogInformation("Called UpdateTodoItem stored procedure to update description successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to execute stored procedure");
                throw new Exception("Failed to execute stored procedure",ex);
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

    }
}
