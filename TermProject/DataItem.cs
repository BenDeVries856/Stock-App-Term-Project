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
using SQLite;

namespace TermProject
{
    // class is used for storing information about the user in sqlite
    class DataItem
    {

        [PrimaryKey]
        public string Name { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return string.Format("[DataItem: Name={0}, Value={1}]", Name, Value);
        }

        public DataItem() { }
        public DataItem(String name, String value)
        {
            this.Name = name;
            this.Value = value;
        }

    }
}