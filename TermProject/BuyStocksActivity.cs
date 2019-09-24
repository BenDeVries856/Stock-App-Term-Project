using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace TermProject
{
    [Activity(Label = "BuyStocksActivity")]
    public class BuyStocksActivity : Activity
    {

        // user data values
        private double money = 45000.0;

        // button variables
        private Button btnSearch;
        private Button btnBuy;
        // text variables
        private TextView txtLiquid;
        private EditText etxtTicker;
        private TextView txtCompany;
        private TextView txtAsking;
        private TextView txtDividends;
        private ProgressBar progressBar;
        private EditText etxtShares;
        // database variable
        private Database db;
        // holds the contents of the currently displayed stock
        private Stock currentStock;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.BuyStocks);
            db = new Database("StocksApp.db");
            btnSearch = FindViewById<Button>(Resource.Id.btnSearch);
            btnBuy = FindViewById<Button>(Resource.Id.btnBuyStock);
            txtLiquid = FindViewById<TextView>(Resource.Id.txtLiquid);
            etxtTicker = FindViewById<EditText>(Resource.Id.etxtTicker);
            txtCompany = FindViewById<TextView>(Resource.Id.txtCompany);
            txtAsking = FindViewById<TextView>(Resource.Id.txtAsking);
            txtDividends = FindViewById<TextView>(Resource.Id.txtDividends);
            progressBar = FindViewById<ProgressBar>(Resource.Id.ProgressBar);
            etxtShares = FindViewById<EditText>(Resource.Id.etxtShares);
            // adding click event
            btnSearch.Click += delegate { SearchButtonClickedAsync(); };
            btnBuy.Click += delegate { BuyStock(); };
            updateMoney();
        }

        public async Task SearchButtonClickedAsync()
        {
            string ticker = etxtTicker.Text;
            ticker.ToUpper();
            currentStock = await searchStock(ticker);
            // check if user already has stock
            List<Stock> stocks = await db.GetStocks();
            foreach (Stock stock in stocks)
            {
                if (stock.Ticker.Equals(currentStock.Ticker))
                {
                    currentStock.shares = stock.shares;
                    currentStock.OriginalPrice = stock.OriginalPrice;
                } else {
                    currentStock.OriginalPrice = currentStock.AskingPrice;
                }
            }
            displayStock(currentStock);
        }

        // started with yahoo finance csv api but they seem to have shut it down
        // switched to alpha vantage json but they dont have dividends or the full company name
        // thanks yahoo...
        public static async Task<Stock> searchStock(string ticker)
        {
            ticker = ticker.ToUpper();
            //string url = "http://finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=nayd";
            //string url = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=" + ticker + "&apikey=ACD1HHQFO95N0P93&datatype=csv";
            string url = "https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=" + ticker + "&interval=1min&apikey=ACD1HHQFO95N0P93";
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await httpClient.GetAsync(url);
            if (response != null || response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject jstring = JObject.Parse(content);
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:00:00");
                JObject price = JObject.Parse(jstring["Time Series (1min)"][date].ToString());
                OutputLine(price["1. open"].ToString());
                //string[] values = content.Split(',');
                Stock result = new Stock();
                result.Ticker = ticker;
                result.Name = ticker;
                result.AskingPrice = Convert.ToDouble(price["1. open"]);
                result.volume = Convert.ToInt32(price["5. volume"]);
                result.Dividends = 0.0;
                /**if (values[2].Equals("N/A", StringComparison.OrdinalIgnoreCase)) {
                    result.Dividends = 0.0;
                } else {
                    result.Dividends = Convert.ToDouble(values[2]);
                }**/
                return result;
            } else {
                return new Stock();
            }
        }

        public async void BuyStock()
        {
            if(!(etxtShares.Text == "") && !(etxtShares == null) &&
                !(txtCompany.Text == "") && !(txtCompany.Text == null))
            {
                int shares = 0;
                try
                {
                    shares = Convert.ToInt32(etxtShares.Text);
                }
                catch (Exception e)
                {
                    shares = 0;
                }
                currentStock.shares += shares;
                double? cost = shares * currentStock.AskingPrice;
                if(cost > money){
                    Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle("Insufficient Funds");
                    alert.SetMessage("You do not have enough money to purchase these shares");
                    alert.SetPositiveButton("Done", (senderAlert, args) => { });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                } else {
                    // prepare and show confirm dialog
                    Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                    alert.SetTitle("Purchase Stock?");
                    alert.SetMessage("Buy " + shares + " shares for " + string.Format("{0:C}", cost) + "?");
                    alert.SetPositiveButton("Buy", (senderAlert, args) => {
                        money -= Convert.ToDouble(cost);
                        db.UpdateStock(currentStock);
                        db.UpdateDataItem(new DataItem("money", money.ToString()));
                        updateMoney();
                    });
                    alert.SetNegativeButton("Cancel", (senderAlert, args) => {

                    });
                    Dialog dialog = alert.Create();
                    dialog.Show();
                }
            } else {
                Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("Invalid");
                alert.SetMessage("You must enter a valid ticker symbol and number of shares before pressing buy");
                alert.SetPositiveButton("Done", (senderAlert, args) => { });
                Dialog dialog = alert.Create();
                dialog.Show();
            }
            
        }

        public void displayStock(Stock stock)
        {
            OutputLine("Displaying stocks");
            txtCompany.Text = "Company: " + stock.Name;
            txtAsking.Text = "Asking: " + stock.AskingPrice.ToString();
            txtDividends.Text = "Volume: " + stock.volume.ToString();
        }

        public async void updateMoney()
        {
            String value = await db.getDataItem("money");
            money = Convert.ToDouble(value);
            txtLiquid.Text = string.Format("{0:C}", money);
        }

        public static void OutputLine(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        /** code from stack overflow 
         * https://stackoverflow.com/questions/42987462/xamarin-android-call-a-confirmation-dialog-popup-from-onoptionsitemselected
         * **/
        private Task<string> ConfirmBuy(double cost, int shares)
        {
            var tcs = new TaskCompletionSource<string>();

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            

            return tcs.Task;
        }

    }
}