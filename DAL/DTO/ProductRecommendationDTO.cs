/*************************************************
* ERP System Version 1.0
* Copyright (c) 2024
*
* Data Transfer Object for Product Recommendation
*************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp_V1.DAL.DTO
{
    /// <summary>
    /// Data Transfer Object (DTO) representing a product recommendation
    /// </summary>
    public class ProductRecommendationDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the product
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// Gets or sets the name of the product
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the name of the category the product belongs to
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the price of the product
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the predicted rating for the product based on user reviews or algorithms
        /// </summary>
        public double PredictedRating { get; set; }
    }
}
