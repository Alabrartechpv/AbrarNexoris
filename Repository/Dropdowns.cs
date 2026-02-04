using ModelClass;
using ModelClass.Master;
using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class Dropdowns : BaseRepostitory
    {
        //here geting all branch deteails from database
        public BranchDDlGrid getBanchDDl()
        {
            BranchDDlGrid grid = new BranchDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<BranchDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        //here getting all paymode details from paymode
        public PaymodeDDlGrid GetPaymode()
        {
            PaymodeDDlGrid grid = new PaymodeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PayMode, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PaymodeDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        /// <summary>
        /// here we getting all customer details from database
        /// </summary>
        /// <returns></returns>
        public CustomerDDlGrid CustomerDDl()
        {
            CustomerDDlGrid grid = new CustomerDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_CustomerDDl, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "GETCUSTOMER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CustomerDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public VendorDDLGrids VendorDDL()
        {
            VendorDDLGrids ObjVendorDDLGrid = new VendorDDLGrids();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vendor, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "DDLVendor");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ObjVendorDDLGrid.List = ds.Tables[0].ToListOfObject<VendorDDLG>();
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return ObjVendorDDLGrid;

        }


        public PriceLevelDDlGrid GetPriceLevel()
        {
            PriceLevelDDlGrid grid = new PriceLevelDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", "PriceLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PriceLevelDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }




        /// <summary>
        /// here we are going to get all 
        /// </summary>
        /// <returns></returns>
        public PaymodeDDlGrid PaymodeDDl()
        {
            PaymodeDDlGrid grid = new PaymodeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "PAYMODE");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<PaymodeDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        /// <summary>
        /// Gets all units for a specific item using ItemId.
        /// Useful when different units have different barcodes.
        /// </summary>
        public ItemDDlGrid GetItemUnits(int itemId)
        {
            ItemDDlGrid grid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@Barcode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Operation", "ItemUnit");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<ItemDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        /// <summary>
        /// here getting Item Details
        /// </summary>
        /// <param name="Barcode"></param>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public ItemDDlGrid itemDDlGrid(string Barcode = null, string ItemName = null)
        {
            ItemDDlGrid grid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemDetalisDDL, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    //  cmd.Parameters.AddWithValue("@FinyearId", Convert.ToInt64(DataBase.));
                    cmd.Parameters.AddWithValue("@Barcode", Barcode);
                    cmd.Parameters.AddWithValue("@ItemName", ItemName);

                    cmd.Parameters.AddWithValue("@Operation", DataBase.Operations);

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<ItemDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }


        public StateDDlGrid getStateDDl()
        {
            StateDDlGrid grid = new StateDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_State, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<StateDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        /// <summary>
        /// here getting users details
        /// </summary>
        /// <param name="Barcode"></param>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public SalesPersonDDlGrid GetSalesPerson(string Barcode = null, string ItemName = null)
        {
            SalesPersonDDlGrid grid = new SalesPersonDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@FinyearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@Barcode", Barcode);

                    cmd.Parameters.AddWithValue("@Operation", "SALESPERSON");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<SalesPersonDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public GroupDDlGrid getGroupDDl()
        {
            GroupDDlGrid grid = new GroupDDlGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Group, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<GroupDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public GroupDDlGrid GroupDDl()
        {
            GroupDDlGrid grid = new GroupDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Group");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<GroupDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public CompanyDDlGrid CompanyDDl()
        {
            CompanyDDlGrid grid = new CompanyDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "Company");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CompanyDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public CategoryDDlGrid getCategoryDDl(string search)
        {
            CategoryDDlGrid grid = new CategoryDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", search);

                    cmd.Parameters.AddWithValue("@Operation", "Category");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CategoryDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public BrandDDLGrid getBrandDDl()
        {
            BrandDDLGrid grid = new BrandDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Brand");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<BrandDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public ItemTypeDDlGrid getItemTypeDDl()
        {
            ItemTypeDDlGrid itemtypeddl = new ItemTypeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "ItemType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            itemtypeddl.List = ds.Tables[0].ToListOfObject<ItemTypeDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return itemtypeddl;
        }

        public CustomerTypeDDlGrid getCustomerTypeDDl()
        {
            CustomerTypeDDlGrid custtypeddl = new CustomerTypeDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "PriceLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            custtypeddl.List = ds.Tables[0].ToListOfObject<CustomerTypeDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return custtypeddl;
        }

        public UnitDDlGrid getUnitDDl()
        {
            UnitDDlGrid unitgrid = new UnitDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "Unit");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            unitgrid.List = ds.Tables[0].ToListOfObject<UnitDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return unitgrid;
        }

        public ItemDDlGrid ListItemDDl()
        {
            ItemDDlGrid igrid = new ItemDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "ITEM");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            igrid.List = ds.Tables[0].ToListOfObject<ItemDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return igrid;
        }

        public ItemlistDDlGrid ItemlistgridDDl()
        {
            ItemlistDDlGrid ilistgrid = new ItemlistDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@ItemId", 0);
                    cmd.Parameters.AddWithValue("@description", "");
                    cmd.Parameters.AddWithValue("@Operation", "ITEMLIST");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            ilistgrid.List = ds.Tables[0].ToListOfObject<ItemlistDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return ilistgrid;
        }

        public ItemlistDDlGrid SearchItem(ItemlistDDl itemddl)
        {
            ItemlistDDlGrid searchdesc = new ItemlistDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", itemddl.Barcode);
                    cmd.Parameters.AddWithValue("@description", itemddl.Description);
                    cmd.Parameters.AddWithValue("@Operation", "ITEMLIST");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            searchdesc.List = ds.Tables[0].ToListOfObject<ItemlistDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return searchdesc;
        }

        public BrandDDLGrid getBrandDDL()
        {
            BrandDDLGrid grid = new BrandDDLGrid();
            Brand brd = new Brand();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Brand, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", brd.Id);
                    cmd.Parameters.AddWithValue("@BrandName", brd.BrandName);
                    cmd.Parameters.AddWithValue("@Photo", brd.Photo);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds.Tables.Count > 0) && (ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {

                            grid.List = ds.Tables[0].ToListOfObject<BrandDDL>();
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return grid;
        }
        public UserLevelDDlGrid UserLevelDDl()
        {
            UserLevelDDlGrid usergrid = new UserLevelDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "UserLevel");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            usergrid.List = ds.Tables[0].ToListOfObject<UserLevelDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return usergrid;
        }
        public UserDDlGrid getUsersDDl()
        {
            UserDDlGrid grid = new UserDDlGrid();
            Users us = new Users();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_User, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", us.UserID);
                    cmd.Parameters.AddWithValue("@CompanyID", us.CompanyID);
                    cmd.Parameters.AddWithValue("@BranchID", us.BranchID);
                    cmd.Parameters.AddWithValue("@UserLevelID", us.UserLevelID);
                    cmd.Parameters.AddWithValue("@UserName", us.UserName);
                    cmd.Parameters.AddWithValue("@Password", us.Password);
                    cmd.Parameters.AddWithValue("@Email", us.Email);
                    cmd.Parameters.AddWithValue("@IsDelete", 0);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds.Tables.Count > 0) && (ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {

                            grid.List = ds.Tables[0].ToListOfObject<UsersDDl>();
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return grid;
        }
        public SalesDDlGrid SalesDDl()
        {
            SalesDDlGrid usergrid = new SalesDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "SalesInvoice");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            usergrid.List = ds.Tables[0].ToListOfObject<SalesDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return usergrid;
        }

        public TaxTypeDDLGrid GetTaxType()
        {
            TaxTypeDDLGrid TaxType = new TaxTypeDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "TaxType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            TaxType.List = ds.Tables[0].ToListOfObject<TaxTypeDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return TaxType;
        }

        public TaxPerDDlGrid GetTaxPer()
        {
            TaxPerDDlGrid taxper = new TaxPerDDlGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");
                    cmd.Parameters.AddWithValue("@Operation", "GETTAXPER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            taxper.List = ds.Tables[0].ToListOfObject<TaxPerDDl>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return taxper;
        }

        public CountryDDLGrid CountryDDl()
        {
            CountryDDLGrid grid = new CountryDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "Country");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CountryDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public TaxTypeDDLGrid TaxTypeDDL()
        {
            TaxTypeDDLGrid grid = new TaxTypeDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", 0);
                    cmd.Parameters.AddWithValue("@CompanyId", 0);
                    cmd.Parameters.AddWithValue("@FinyearId", 0);
                    cmd.Parameters.AddWithValue("@Barcode", "");

                    cmd.Parameters.AddWithValue("@Operation", "TaxType");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<TaxTypeDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }


        public CountryDDLGrid getCountryDDl()
        {
            CountryDDLGrid grid = new CountryDDLGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<CountryDDL>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }

        public CurrencyDDLGRID getCurrency()
        {
            CurrencyDDLGRID GridCur = new CurrencyDDLGRID();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmdC = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmdC.CommandType = CommandType.StoredProcedure;
                    cmdC.Parameters.AddWithValue("@BranchId", 11);
                    cmdC.Parameters.AddWithValue("@CompanyId", 1);
                    cmdC.Parameters.AddWithValue("@FinyearId", 1);
                    cmdC.Parameters.AddWithValue("@Operation", "Currency");
                    using (SqlDataAdapter adaptC = new SqlDataAdapter(cmdC))
                    {
                        DataSet dsC = new DataSet();
                        adaptC.Fill(dsC);
                        if ((dsC != null) && (dsC.Tables.Count > 0) && (dsC.Tables[0] != null) && (dsC.Tables[0].Rows.Count > 0))
                        {
                            GridCur.List = dsC.Tables[0].ToListOfObject<CurrencyModel>();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {

                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();

            }
            return GridCur;
        }

        public CountryDDLGrid SearchCountry(string searchTerm)
        {
            CountryDDLGrid records = new CountryDDLGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Country, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CountryName", searchTerm);
                    cmd.Parameters.AddWithValue("@_Operation", "SEARCH");
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            records.List = ds.Tables[0].ToListOfObject<CountryDDL>();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return records;
        }
        //*****************
        public ReasonDDLGrid getReasonDDl()
        {
            ReasonDDLGrid grid = new ReasonDDLGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_dropdown, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@Operation", DataBase.Operations);
                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            grid.List = ds.Tables[0].ToListOfObject<Reason>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return grid;
        }
        //******************

        public PurchaseGrid getAllPurchaseMaster()
        {
            PurchaseGrid PurGrid = new PurchaseGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Purchase, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            PurGrid.ListPurchase = ds.Tables[0].ToListOfObject<PurchaseMaster>();
                        }
                    }


                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return PurGrid;

        }

        public StockGrid getAllDocNo()
        {
            StockGrid stkGrid = new StockGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            stkGrid.ListMaster = ds.Tables[0].ToListOfObject<StockAdjMasterDialog>();
                        }
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            stkGrid.ListDetails = ds.Tables[1].ToListOfObject<StockAdjPriceDetails>();
                        }
                    }


                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return stkGrid;

        }

        public StockGrid getStockAdjustmentById(int id)
        {
            StockGrid stkGrid = new StockGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_StockAdjustemnt, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);

                        // First table contains master record
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            // Map the master record to a list with single item
                            var masterRecord = ds.Tables[0].Rows[0].ToNullableObject<StockAdjMasterDialog>();
                            stkGrid.ListMaster = new List<StockAdjMasterDialog> { masterRecord };
                        }

                        // Second table contains detail records
                        if ((ds != null) && (ds.Tables.Count > 1) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            stkGrid.ListDetails = ds.Tables[1].ToListOfObject<StockAdjPriceDetails>();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return stkGrid;
        }
    }
}
