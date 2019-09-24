
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TermProject
{
    class Database
    {

        private string path;
        private SQLiteAsyncConnection connection;

        public Database(string filename)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            this.path = Path.Combine(path, filename);
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            try
            {
                connection = new SQLiteAsyncConnection(path);
                connection.CreateTableAsync<Stock>();
                connection.CreateTableAsync<DataItem>();
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void InsertStock(Stock s)
        {
            try
            {
                connection.InsertAsync(s);
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void UpdateStock(Stock s)
        {
            try
            {
                var rowsAffected = connection.UpdateAsync(s);
                if (rowsAffected.Result == 0)
                {
                    OutputLine("Stock not in DB");
                    rowsAffected = connection.InsertAsync(s);
                } else {
                    OutputLine("Stock in DB");
                }
            }
            catch(SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void deleteStock(Stock s)
        {
            try
            {
                connection.DeleteAsync(s);
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void InsertDataItem(DataItem d)
        {
            try
            {
                connection.InsertAsync(d);
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void DeleteAll()
        {
            try
            {
                connection.ExecuteAsync("DELETE FROM Stock");
                OutputLine("All stock data deleted");
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public async Task<List<Stock>> GetStocks()
        {
            try
            {
                List<Stock> list = await connection.QueryAsync<Stock>("SELECT * FROM Stock");
                return list;
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
                return new List<Stock>();
            }
        }

        public async void DisplayStocks()
        {
            try
            {
                List<Stock> list = await connection.QueryAsync<Stock>("SELECT * FROM Stock");
                foreach(Stock s in list)
                {
                    OutputLine(s.ToString());
                }
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public async void DisplayDataItems()
        {
            try
            {
                List<DataItem> list = await connection.QueryAsync<DataItem>("SELECT * FROM DataItem");
                foreach(DataItem d in list)
                {
                    OutputLine(d.ToString());
                }
            }
            catch(SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public void UpdateDataItem(DataItem item)
        {
            try
            {
                connection.UpdateAsync(item);
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public async Task<String> getDataItem(String name)
        {
            try
            {
                List<DataItem> list = await connection.QueryAsync<DataItem>("SELECT * FROM DataItem");
                foreach (DataItem d in list)
                {
                    if (d.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        //OutputLine("found money data item");
                        //OutputLine("money: " + d.Value);
                        return d.Value;
                    }
                }
                return "Not In Table";
            }
            catch(SQLiteException ex)
            {
                OutputLine(ex.Message);
                return "Error!";
            }
        }

        public async void GetRowCount()
        {
            try
            {
                var count = await connection.ExecuteScalarAsync<int>("SELECT Count(*) FROM Stock");
                OutputLine(count + " rows retrieved");
            }
            catch (SQLiteException ex)
            {
                OutputLine(ex.Message);
            }
        }

        public static void OutputLine(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

    }
}