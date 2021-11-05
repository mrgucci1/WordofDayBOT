using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordofDayBOT
{
    class dataTableObjects
    {
        public DataTable getWordOfDayTable(DataTable tbl)
        {
            tbl.Columns.Add(new DataColumn("insertdate", typeof(DateTime)));
            tbl.Columns.Add(new DataColumn("wordofday", typeof(string)));
            tbl.Columns.Add(new DataColumn("definition", typeof(string)));
            tbl.Columns.Add(new DataColumn("example", typeof(string)));
            tbl.Columns.Add(new DataColumn("longexample", typeof(string)));
            tbl.Columns.Add(new DataColumn("funfact", typeof(string)));
            return tbl;
        }
        public void tableInsert(DataTable tbl, string database, string tableName)
        {
            sqlConnection sql = new sqlConnection();
            SqlConnection cnn = sql.connect(database);
            cnn.Open();
            SqlBulkCopy objBulk = new SqlBulkCopy(cnn);
            SqlCommand cmd = cnn.CreateCommand();
            cmd.CommandText = $"DELETE FROM {tableName} where insertdate = '{DateTime.Now.ToString("MM-dd-yyyy")}'";
            var numEffect = cmd.ExecuteNonQuery();
            objBulk.DestinationTableName = tableName;
            objBulk.BulkCopyTimeout = 120;
            //Map DataTable Headers to SQL Database Headers
            foreach (DataColumn column in tbl.Columns)
            {
                objBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
            Console.WriteLine("Inserting....");
            objBulk.WriteToServer(tbl);
            cnn.Close();
        }
    }
}
