using SQLite;
using System;

namespace TermProject
{
    public class Stock
    {

        [PrimaryKey]
        public string Ticker { get; set; }
        public string Name { get; set; }
        public double? AskingPrice { get; set; }
        public double? Dividends { get; set; }
        public double? OriginalPrice { get; set; }
        public int shares { get; set; }
        public int volume { get; set; }
        public override string ToString()
        {
            return string.Format("[Stock: Ticker={0}, Name={1}, AskingPrice={2}," + 
                                    " Dividends={3}, OriginalPrice={4}, Shares={5}]",
                Ticker, Name, AskingPrice, Dividends, OriginalPrice, shares);
        }
        public Stock()
        {
            shares = 0;
        }
    }
}
