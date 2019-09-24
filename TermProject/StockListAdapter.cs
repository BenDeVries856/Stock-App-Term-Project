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
using Android.Graphics;

namespace TermProject
{
    class StockListAdapter : BaseAdapter<Stock>
    {

        private readonly Activity context;
        private List<Stock> stockListData;

        public StockListAdapter(Activity _context, List<Stock> _list) : base()
        {
            this.context = _context;
            this.stockListData = _list;
        }

        public override int Count
        {
            get { return stockListData.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Stock this[int index]
        {
            get { return stockListData[index]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            // re-use an existing view, if one is available otherwise create a new one
            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.StockListItem, null, false);
            }
            Stock stock = this[position];
            view.FindViewById<TextView>(Resource.Id.txtStockName).Text = stock.Name;
            view.FindViewById<TextView>(Resource.Id.txtStockShares).Text = stock.shares.ToString() + " shares";
            // bottom textviews are as follows -> price per share -> total value of all shares owned
            view.FindViewById<TextView>(Resource.Id.txtStockAskingPrice).Text = string.Format("{0:C}", stock.AskingPrice * stock.shares);
            double? original =  stock.OriginalPrice;
            double? current =  stock.AskingPrice;
            if(current >= original)
            {
                view.FindViewById<TextView>(Resource.Id.txtStockOldPrice).Text = "+" + string.Format("{0:C}", stock.AskingPrice);
                view.FindViewById<TextView>(Resource.Id.txtStockOldPrice).SetTextColor(Color.ParseColor("#00cc00"));
            } else {
                view.FindViewById<TextView>(Resource.Id.txtStockOldPrice).Text = "-" + string.Format("{0:C}", stock.AskingPrice);
                view.FindViewById<TextView>(Resource.Id.txtStockOldPrice).SetTextColor(Color.ParseColor("#ff0000"));
            }
            return view;
        }

    }
}