using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data;
using System.Data.SqlClient;

namespace jqGridExample.Models
{
    public class jqGridExampleDbContext : DbContext
    {
        public jqGridExampleDbContext() : base("jqGridExample")
        {
            this.Database.Initialize(true);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new jqGridExampleInitializer());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DataTable GetDataTable(string procedureName, IDictionary<string, object> parameters)
        {
            procedureName = ValidateProcedureName(procedureName);
            DataTable result = null;
            using (SqlConnection conn = new SqlConnection(this.Database.Connection.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (string key in parameters.Keys)
                {
                    cmd.Parameters.Add(new SqlParameter(key, parameters[key]));
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds.Tables.Count > 0)
                        result = ds.Tables[0];
                }
            }
            return result;
        }

        public IEnumerable<GridColumn> DeriveColumnList(string procedureName, IDictionary<string, object> parameters)
        {
            procedureName = ValidateProcedureName(procedureName);
            List<GridColumn> results = new List<GridColumn>();
            using (SqlConnection conn = new SqlConnection(this.Database.Connection.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (string key in parameters.Keys)
                {
                    cmd.Parameters.Add(new SqlParameter(key, parameters[key]));
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.FillSchema(ds, SchemaType.Source);

                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataColumn col in ds.Tables[0].Columns)
                        {
                            GridColumn column = new GridColumn();
                            column.Name = col.ColumnName;
                            column.Caption = col.ColumnName;
                            column.DataType = col.DataType.ToString();
                            column.IsHidden = false;
                            results.Add(column);
                        }
                    }
                }
            }
            return results;
        }
        public IEnumerable<GridParameter> DeriveParameters(string procedureName)
        {
            procedureName = ValidateProcedureName(procedureName);
            List<GridParameter> result = new List<GridParameter>();
            using (SqlConnection conn = new SqlConnection(this.Database.Connection.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlCommandBuilder.DeriveParameters(cmd);

                //Do not include the return value
                cmd.Parameters.RemoveAt(0);

                foreach (SqlParameter parameter in cmd.Parameters)
                {
                    GridParameter data = new GridParameter();
                    data.Name = parameter.ParameterName.TrimStart('@');
                    data.DataType = ConvertToDotNetType(parameter.DbType).ToString();
                    data.Caption = parameter.ParameterName.TrimStart('@');
                    data.IsHidden = true;
                    result.Add(data);
                }
            }
            return result;
        }
        public IEnumerable<string> GetGridProcedures()
        {
            List<string> result = new List<string>();
            using (SqlConnection conn = new SqlConnection(this.Database.Connection.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.AllGridProcedures", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        result.Add(row["ProcedureName"].ToString());
                    }
                }
            }
            return result;
        }
        private string ValidateProcedureName(string procedureName)
        {
            if (procedureName.Substring(0, 5).ToLower() != "grid.")
            {
                return "Grid." + procedureName;
            }
            return procedureName;
        }
        private Type ConvertToDotNetType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Binary:
                    return typeof(byte[]);
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                    return typeof(string);
                case DbType.Currency:
                case DbType.Decimal:
                    return typeof(decimal);
                case DbType.Double:
                    return typeof(double);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return typeof(DateTime);
                case DbType.Boolean:
                    return typeof(bool);
                case DbType.Byte:
                    return typeof(byte);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                    return typeof(int);
                default:
                    return typeof(string);
            }
        }
    }

    public class jqGridExampleInitializer : DropCreateDatabaseIfModelChanges<jqGridExampleDbContext>
    {
        protected override void Seed(jqGridExampleDbContext context)
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, Name = "Appliances" },
                new Category { CategoryId = 2, Name = "Electrical" },
                new Category { CategoryId = 3, Name = "Lumber" },
            };
            categories.ForEach(c => context.Categories.Add(c));

            var products = new List<Product>
            {
                new Product { ProductId = 1, CategoryId = 1, Name = "Washer", Description = "Clothes Washer", Price = 400 },
                new Product { ProductId = 2, CategoryId = 1, Name = "Dryer", Description = "Clothes Dryer", Price = 450 },
                new Product { ProductId = 3, CategoryId = 1, Name = "Refridgerator", Description = "", Price = 800 },
                new Product { ProductId = 4, CategoryId = 2, Name = "50' Wire", Description = "", Price = 15 },
                new Product { ProductId = 5, CategoryId = 2, Name = "Outlet", Description = "", Price = 1.75M },
                new Product { ProductId = 6, CategoryId = 2, Name = "Switch", Description = "", Price = 1.5M },
                new Product { ProductId = 7, CategoryId = 3, Name = "2 x 4", Description = "2 x 4 Framing", Price = 2.5M }
            };
            products.ForEach(p => context.Products.Add(p));

            string gridSchema = "CREATE SCHEMA Grid";
            string allCategoriesProcedure = @"CREATE PROCEDURE Grid.AllCategories
AS
BEGIN
	SELECT
		CategoryId,
		Name
	FROM dbo.Categories
	ORDER BY Name

RETURN
END";

            string productsByCategoryIdProcedure = @"CREATE PROCEDURE Grid.ProductsByCategoryId
(
	@CategoryId int
)
AS
BEGIN
	SELECT
		ProductId,
		Name,
		[Description],
		Price
	FROM dbo.Products
	WHERE CategoryId = @CategoryId
	ORDER BY Name

RETURN
END";
            string allGridProcedures = @"CREATE PROCEDURE dbo.AllGridProcedures
AS
BEGIN
	SELECT
		name as ProcedureName
	FROM sys.procedures
	WHERE SCHEMA_NAME(schema_id) = 'Grid'
	
RETURN
END";

            using (SqlConnection conn = new SqlConnection(context.Database.Connection.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(gridSchema, conn);
                cmd.CommandType = CommandType.Text;
                int result = cmd.ExecuteNonQuery();

                cmd.CommandText = allCategoriesProcedure;
                result = cmd.ExecuteNonQuery();

                cmd.CommandText = productsByCategoryIdProcedure;
                result = cmd.ExecuteNonQuery();

                cmd.CommandText = allGridProcedures;
                result = cmd.ExecuteNonQuery();
            }

            base.Seed(context);
        }
    }
}