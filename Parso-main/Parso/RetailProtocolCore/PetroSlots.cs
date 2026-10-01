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
    /// Public Class for pre-auth slots for Petroleum
    /// </summary>
    public class PetroSlots
    {
        //slot id (1 through 10) 
        public int SlotID { get; set; }

        //enum of the status
        public int SlotStatus { get; set; }

        //string of the notify amount
        public string Amount { get; set; }

        //string containing the results
        public string Result { get; set; }

        public string XML { get; set; }

        public string NotifyResult { get; set; }

        public string NotifyXML { get; set; }

        //List of prompts and responses for the transaction in this slot
        public BindingList<Prompting> PromptingList { get; set; }

        //The list of product details - I expect someday this will be a List.
        //public PetroProductDetails PetroProductDetails { get; set; }

        public decimal ProductDetailUnitPrice { get; set; }
        public decimal ProductDetailQuantity { get; set; }
        public string ProductDetailProductCode { get; set; }
        public int ProductDetailPump { get; set; }
        public string ProductDetailProductName { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal PSTAmount { get; set; }
        public string FleetUniqueId { get; set; }

        public bool DataPresent
        {
            get
            {
                return !(String.IsNullOrEmpty(Result));
            }
        }

    }

}
