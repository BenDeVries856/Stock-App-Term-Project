using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using System.Collections.Generic;

namespace TermProject
{
    [Activity(Label = "TermProject", MainLauncher = true)]
    public class MyStocksActivity : Activity
    {

        private Button btnRefresh;
        private Button btnBuyStocks;
        private ListView lvMyStocks;
        private List<Stock> stockListData;
        private double? money;
        private StockListAdapter stockListAdapter;
        private Database db;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MyStocks);
            db = new Database("StocksApp.db");
            btnRefresh = FindViewById<Button>(Resource.Id.btnRefresh);
            btnBuyStocks = FindViewById<Button>(Resource.Id.btnBuyStocks);
            lvMyStocks = FindViewById<ListView>(Resource.Id.lvMyStocks);
            btnBuyStocks.Click += BuyStocks;
            btnRefresh.Click += delegate {
                GetMyStocks();
                calculateNetWorth();
                UpdateAll();
            };
            lvMyStocks.ItemClick += SellStock;
            GetMyStocks();
        }

        public void BuyStocks(object sender, EventArgs ea)
        {
            var intent = new Intent(this, typeof(BuyStocksActivity));
            StartActivity(intent);
        }

        public async void GetMyStocks()
        {
            stockListData = await db.GetStocks();
            String value = await db.getDataItem("money");
            money = Convert.ToDouble(value);
            stockListAdapter = new StockListAdapter(this, stockListData);
            lvMyStocks.Adapter = stockListAdapter;
            calculateNetWorth();
        }

        public void SellStock(object sender, AdapterView.ItemClickEventArgs e)
        {
            Stock stock = this.stockListAdapter[e.Position];
            double? cost = stock.AskingPrice * stock.shares;
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Sell Stock?");
            alert.SetMessage("Sell " + stock.shares + " shares for " + string.Format("{0:C}", cost) + "?");
            alert.SetPositiveButton("Sell", (senderAlert, args) => {
                money += cost;
                db.deleteStock(stock);
                db.UpdateDataItem(new DataItem("money", money.ToString()));
                GetMyStocks();
            });
            alert.SetNegativeButton("Cancel", (senderAlert, args) => {

            });
            Dialog dialog = alert.Create();
            dialog.Show();
        }

        public void calculateNetWorth()
        {
            double? net = 0;
            net += money;
            foreach(Stock s in stockListData)
            {
                net += (double) s.AskingPrice * s.shares;
            }
            Window.SetTitle("Net: " + string.Format("{0:C}", net) + ", Liquid: " + string.Format("{0:C}", money));
        }

        public void UpdateAll()
        {
            OutputLine("Inside update all method");
            bool dothing = true;
            foreach(Stock s in stockListData)
            {
                if (dothing)
                {
                    OutputLine("Updating: " + s.Ticker);
                    Stock newStock = BuyStocksActivity.searchStock(s.Ticker).Result;
                    OutputLine("Stock: " + newStock.Ticker);
                    OutputLine("New Price: " + newStock.AskingPrice);
                    dothing = false;
                }
            }
        }

        public static void OutputLine(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

    }
}

