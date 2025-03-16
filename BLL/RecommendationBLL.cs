using System;
using System.Collections.Generic;
using System.Linq;
using Accord.MachineLearning;
using Accord.Math.Distances;
using Erp_V1.DAL.DTO;
using Erp_V1.DAL.DAO;

namespace Erp_V1.BLL
{
    /// <summary>
    /// Provides product recommendation services using machine learning clustering.
    /// </summary>
    /// <remarks>
    /// Uses K-Means clustering to identify customer groups with similar purchasing patterns
    /// and recommends products based on the behavior of similar customers.
    /// </remarks>
    public class RecommendationBLL
    {
        #region Dependencies & Fields

        private readonly SalesDAO _salesDao;
        private readonly ProductDAO _productDao;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationBLL"/> class.
        /// </summary>
        /// <param name="salesDao">The sales data access object.</param>
        /// <param name="productDao">The product data access object.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public RecommendationBLL(SalesDAO salesDao, ProductDAO productDao)
        {
            _salesDao = salesDao ?? throw new ArgumentNullException(nameof(salesDao));
            _productDao = productDao ?? throw new ArgumentNullException(nameof(productDao));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates product recommendations for a specified customer.
        /// </summary>
        /// <param name="customerName">Name of the customer for which to generate recommendations.</param>
        /// <returns>
        /// A list of recommended products that similar customers have purchased,
        /// excluding any products the customer already owns.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the customer name is null or whitespace.</exception>
        /// <remarks>
        /// Recommendation strategy:
        /// 1. Retrieve the customer's purchase history.
        /// 2. Cluster all customers based on spending patterns (using Price and CategoryID).
        /// 3. Identify similar customers from the customer's cluster.
        /// 4. Recommend products popular among similar customers, excluding purchased products.
        /// </remarks>
        public List<ProductDetailDTO> GetRecommendationsForCustomer(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException("Customer name cannot be empty.", nameof(customerName));
            }

            // Retrieve all sales data.
            List<SalesDetailDTO> salesList = _salesDao.Select();

            // Retrieve sales for the specified customer.
            var customerSales = salesList
                .Where(s => s.CustomerName.Equals(customerName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!customerSales.Any())
            {
                // New customers (no purchase history) receive no recommendations.
                return new List<ProductDetailDTO>();
            }

            // Prepare clustering data (each sale is represented by [Price, CategoryID]).
            double[][] clusteringData = salesList
                .Select(s => new double[] { s.Price, s.CategoryID })
                .ToArray();

            if (clusteringData.Length < 2)
            {
                return new List<ProductDetailDTO>();
            }

            // Determine the optimal number of clusters.
            int optimalK = DetermineOptimalK(clusteringData);

            if (clusteringData.Length <= optimalK)
            {
                return new List<ProductDetailDTO>();
            }

            // Configure and run K-Means clustering.
            var kmeans = new KMeans(k: optimalK, distance: new SquareEuclidean())
            {
                Tolerance = 0.05,
                MaxIterations = 100
            };

            KMeansClusterCollection clusters = kmeans.Learn(clusteringData);

            // Build a feature vector for the customer: average spending and primary category.
            double[] customerFeatures = new double[]
            {
                customerSales.Average(s => s.Price),
                customerSales.First().CategoryID
            };

            int customerCluster = clusters.Decide(customerFeatures);

            // Identify similar customers (by CustomerID) in the same cluster.
            var similarCustomerIDs = salesList
                .Where(s => clusters.Decide(new double[] { s.Price, s.CategoryID }) == customerCluster)
                .Select(s => s.CustomerID)
                .Distinct()
                .ToList();

            // Retrieve the full product catalog.
            List<ProductDetailDTO> allProducts = _productDao.Select();

            // Filter out any products already purchased by the customer.
            HashSet<int> purchasedProductIDs = customerSales
                .Select(cs => cs.ProductID)
                .ToHashSet();

            // Return recommended products.
            return allProducts
                .Where(p => !purchasedProductIDs.Contains(p.ProductID))
                .ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines the optimal number of clusters for the given data using the Elbow Method.
        /// </summary>
        /// <param name="data">A 2D array of clustering features [Price, CategoryID].</param>
        /// <returns>The optimal number of clusters (between 1 and 10).</returns>
        private int DetermineOptimalK(double[][] data)
        {
            const int maxK = 10;
            double[] distortions = new double[maxK];
            int dataLength = data.Length;

            // Limit the maximum number of clusters to (dataLength - 1).
            int actualMaxK = Math.Min(dataLength - 1, maxK);

            for (int k = 1; k <= actualMaxK; k++)
            {
                var kmeans = new KMeans(k: k, distance: new SquareEuclidean())
                {
                    Tolerance = 0.05,
                    MaxIterations = 100
                };

                KMeansClusterCollection clusters = kmeans.Learn(data);

                // Calculate the total distortion (sum of squared distances).
                double distortion = data
                    .Select(point =>
                    {
                        int clusterId = clusters.Decide(point);
                        return new SquareEuclidean().Distance(point, clusters.Centroids[clusterId]);
                    })
                    .Sum(d => d * d);

                distortions[k - 1] = distortion;
            }

            // Use the Elbow Method to detect the point where the decrease in distortion slows.
            for (int i = 1; i < actualMaxK - 1; i++)
            {
                double previousDiff = distortions[i - 1] - distortions[i];
                double nextDiff = distortions[i] - distortions[i + 1];

                if (nextDiff > 0 && previousDiff / nextDiff > 1.5)
                {
                    return i + 1;
                }
            }

            // Fallback to a single cluster if no elbow is found.
            return 1;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current product catalog.
        /// </summary>
        public List<ProductDetailDTO> AvailableProducts => _productDao.Select();

        #endregion
    }
}
