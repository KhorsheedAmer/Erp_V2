/*************************************************
* ERP System Version 1.0
* Copyright (c) 2024
* 
* Customer Data Access Layer
*************************************************/
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using Erp_V1.DAL.DTO;
using MathNet.Numerics.Statistics.Mcmc;

namespace Erp_V1.DAL.DAO
{
    /// <summary>
    /// Handles database operations for customer entities
    /// </summary>
    /// <remarks>
    /// Implements IDAO interface for customer management
    /// </remarks>
    public class CustomerDAO : StockContext, IDAO<CUSTOMER, CustomerDetailDTO>
    {
        #region Database Operations

        /// <summary>
        /// Performs soft delete on a customer record.
        /// Marks the customer as deleted and sets the DeletedDate.
        /// </summary>
        /// <param name="entity">Customer to delete</param>
        /// <returns>True if the deletion succeeded</returns>
        public virtual bool Delete(CUSTOMER entity)
        {
            try
            {
                var customer = DbContext.CUSTOMER.First(x => x.ID == entity.ID);
                customer.isDeleted = true;
                customer.DeletedDate = DateTime.Today;
                return DbContext.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                     .SelectMany(e => e.ValidationErrors)
                                     .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                throw new Exception($"Customer deletion failed. Validation errors:\n{string.Join("\n", errorMessages)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Customer deletion failed", ex);
            }
        }

        /// <summary>
        /// Restores a soft-deleted customer.
        /// Sets the isDeleted flag to false and clears the DeletedDate.
        /// </summary>
        /// <param name="ID">Customer ID to restore</param>
        /// <returns>True if the restoration succeeded</returns>
        public bool GetBack(int ID)
        {
            try
            {
                var customer = DbContext.CUSTOMER.First(x => x.ID == ID);
                customer.isDeleted = false;
                customer.DeletedDate = null;
                return DbContext.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                     .SelectMany(e => e.ValidationErrors)
                                     .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                throw new Exception($"Customer restoration failed. Validation errors:\n{string.Join("\n", errorMessages)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Customer restoration failed", ex);
            }
        }

        /// <summary>
        /// Creates a new customer in the database.
        /// </summary>
        /// <param name="entity">Customer to create</param>
        /// <returns>True if the insertion succeeded</returns>
        public virtual bool Insert(CUSTOMER entity)
        {
            try
            {
                DbContext.CUSTOMER.Add(entity);
                return DbContext.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                     .SelectMany(e => e.ValidationErrors)
                                     .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                throw new Exception($"Customer insertion failed. Validation errors:\n{string.Join("\n", errorMessages)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Customer insertion failed", ex);
            }
        }

        /// <summary>
        /// Retrieves customers based on deletion status.
        /// </summary>
        /// <param name="isDeleted">Filter flag for deleted items</param>
        /// <returns>List of customer DTOs</returns>
        protected List<CustomerDetailDTO> GetCustomers(bool isDeleted)
        {
            try
            {
                return DbContext.CUSTOMER
                    .Where(x => x.isDeleted == isDeleted)
                    .Select(item => new CustomerDetailDTO
                    {
                        ID = item.ID,
                        CustomerName = item.CustomerName,
                        Cust_Address = item.Cust_Address,
                        Cust_Phone = item.Cust_Phone,
                        Notes = item.Notes,
                    }).ToList();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                     .SelectMany(e => e.ValidationErrors)
                                     .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                throw new Exception($"Customer retrieval failed. Validation errors:\n{string.Join("\n", errorMessages)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Customer retrieval failed", ex);
            }
        }

        /// <summary>
        /// Retrieves all active (non-deleted) customers.
        /// </summary>
        /// <returns>List of active customer DTOs</returns>
        public virtual List<CustomerDetailDTO> Select()
        {
            return GetCustomers(false);
        }

        /// <summary>
        /// Retrieves customers filtered by deletion status.
        /// </summary>
        /// <param name="isDeleted">Deletion status filter</param>
        /// <returns>List of customer DTOs</returns>
        public virtual List<CustomerDetailDTO> Select(bool isDeleted)
        {
            return GetCustomers(isDeleted);
        }

        /// <summary>
        /// Updates customer information.
        /// </summary>
        /// <param name="entity">Updated customer entity</param>
        /// <returns>True if the update succeeded</returns>
        public virtual bool Update(CUSTOMER entity)
        {
            try
            {
                var customer = DbContext.CUSTOMER.First(x => x.ID == entity.ID);
                customer.CustomerName = entity.CustomerName;
                customer.Cust_Address = entity.Cust_Address;
                customer.Cust_Phone = entity.Cust_Phone;
                customer.Notes = entity.Notes;
                return DbContext.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                                     .SelectMany(e => e.ValidationErrors)
                                     .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                throw new Exception($"Customer update failed. Validation errors:\n{string.Join("\n", errorMessages)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Customer update failed", ex);
            }
        }

        #endregion
    }
}
