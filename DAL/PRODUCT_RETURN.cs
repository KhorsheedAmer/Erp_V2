//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Erp_V1.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class PRODUCT_RETURN
    {
        public int ID { get; set; }
        public int SalesID { get; set; }
        public int ProductID { get; set; }
        public int CustomerID { get; set; }
        public int ReturnQuantity { get; set; }
        public System.DateTime ReturnDate { get; set; }
        public string ReturnReason { get; set; }
        public bool isDeleted { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
    }
}
