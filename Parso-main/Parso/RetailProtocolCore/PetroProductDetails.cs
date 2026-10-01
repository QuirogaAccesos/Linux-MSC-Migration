using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCI.Globalcom.GlobalcomRetailProtocol;


namespace CCI.Globalcom.GlobalcomRetailProtocol
{

    /// <summary>
    /// Enumeration of the product detail tags
    /// (TLV of Product Details)
    /// </summary>
    public enum EPetroProductDetailTags
    {
        UnitPrice   = 201,
        QuantityNBS = 202,
        ProductCode = 203,
        Amount      = 204,
        PumpNumber  = 200,
        GSTAmount   = 205,
        PSTAmount   = 206,
        ProductName = 207,
        QuantityCFN = 208,
        AmountChase = 209,
        FleetUniqueId = 210
    }

    public class PetroProductDetails
    {
        public decimal? UnitPrice { get; set; }
        public decimal? Quantity { get; set; }
        public string ProductCode { get; set; }
        public decimal Amount { get; set; }
        public int Pump { get; set; }
        public decimal? GSTAmount { get; set; }
        public decimal? PSTAmount { get; set; }
        public string ProductName { get; set; }
        public string FleetUniqueId { get; set; }
    }

    public class PetroProductDetailSyntax
    {
        public EPetroProductDetailTags Tag;
        public string syntax;
    }
}
