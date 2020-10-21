using System;
using System.Collections.Generic;
using System.Text;

namespace DatafeedClient.DataModels
{
    public class Stock
    {
        public string Symbol { get; set; }
        public decimal RefPrice { get; set; }
        public decimal CeilPrice { get; set; }
        public decimal FloorPrice { get; set; }
    }
}
