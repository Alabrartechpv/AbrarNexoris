using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmdialForItemMaster : Form
    {
        Dropdowns dp = new Dropdowns();
        ItemMasterRepository ItemREpo = new ItemMasterRepository();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();

        string FormName;
        public frmdialForItemMaster(string Params)
        {
            InitializeComponent();
            FormName = Params;
        }

        private void frmdialForItemMaster_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "GETITEM";
            ItemDialogDDLGrid ItemDDlGrid = ItemREpo.getItemGetAll("");
            ItemDDlGrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = ItemDDlGrid.List;
        }
        public void GetItem(string ItemName)
        {
            DataBase.Operations = "GETITEM";
            ItemDialogDDLGrid ItemDDlGrid = ItemREpo.getItemGetAll(ItemName);
            if(ItemDDlGrid.List != null && ItemDDlGrid.List.Count() > 0)
            {
                ItemDDlGrid.List.ToString();
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
                ts1.GridColumnStyles.Add(datagrid);
                ultraGrid1.DataSource = ItemDDlGrid.List;
            }
           
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {

        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            int count;
            if (FormName == "FromItemMaster")
            {
                ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                UltraGridCell ItemId = this.ultraGrid1.ActiveRow.Cells["ItemId"];
                int ItemID = Convert.ToInt32(ItemId.Value.ToString());
                ItemGet getItem = ItemREpo.GetByIdItem(ItemID);
                ItemMaster.lblItemTypeId.Text = getItem.ItemTypeId.ToString();
                ItemMaster.txt_description.Text = getItem.Description;
                ItemMaster.txt_LocalLanguage.Text = getItem.NameInLocalLanguage;
                ItemMaster.txt_CustomerType.Text = getItem.PriceLevel;
                ItemMaster.txt_Category.Text = getItem.CategoryName;
                ItemMaster.txt_Group.Text = getItem.GroupName;
                ItemMaster.txt_ItemType.Text = getItem.ItemType;
                ItemMaster.txt_BaseUnit.Text = getItem.UnitName;
                ItemMaster.txt_ItemNo.Text = getItem.ItemNo.ToString();
                
                for (int i =0;getItem.List.Count() > i; i++)
                {
                    count = ItemMaster.dgv_Uom.Rows.Add();
                    ItemMaster.dgv_Uom.Rows[count].Cells["Unit"].Value = getItem.List[i].Unit;
                    ItemMaster.dgv_Uom.Rows[count].Cells["Packing"].Value = getItem.List[i].Unit;
                    ItemMaster.dgv_Uom.Rows[count].Cells["BarCode"].Value = getItem.List[i].Unit;
                    ItemMaster.dgv_Uom.Rows[count].Cells["Reorder"].Value = getItem.List[i].Unit;
                    ItemMaster.dgv_Uom.Rows[count].Cells["OpnStk"].Value = getItem.List[i].Unit;

                    count = ItemMaster.dgv_Price.Rows.Add();

                    ItemMaster.dgv_Price.Rows[count].Cells["Unit"].Value = getItem.List[i].Unit;
                    ItemMaster.dgv_Price.Rows[count].Cells["Packing"].Value = getItem.List[i].Packing;
                    ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value = getItem.List[i].Cost;
                    ItemMaster.dgv_Price.Rows[count].Cells["MarginAmt"].Value = getItem.List[i].MarginAmt;
                    ItemMaster.dgv_Price.Rows[count].Cells["MarginPer"].Value = getItem.List[i].MarginPer;
                    ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = getItem.List[i].TaxPer;
                    ItemMaster.dgv_Price.Rows[count].Cells["TaxAmt"].Value = getItem.List[i].TaxAmt;
                    ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value = getItem.List[i].MRP;
                    ItemMaster.dgv_Price.Rows[count].Cells["RetailPrice"].Value = getItem.List[i].RetailPrice;
                    ItemMaster.dgv_Price.Rows[count].Cells["WholeSalePrice"].Value = getItem.List[i].WholeSalePrice;
                    ItemMaster.dgv_Price.Rows[count].Cells["CreditPrice"].Value = getItem.List[i].CreditPrice;
                    ItemMaster.dgv_Price.Rows[count].Cells["CardPrice"].Value = getItem.List[i].CardPrice;
                    ItemMaster.Txt_UnitCost.Text = getItem.List[0].Cost.ToString();
                    ItemMaster.txt_TaxPer.Text = getItem.List[0].TaxPer.ToString();
                    ItemMaster.txt_TaxAmount.Text = getItem.List[0].TaxAmt.ToString();

                    count++;
                }
              // SetImageFromBase64( getItem.List[0].Photo.ToString());
               // byte[] byteArray = StringToByteArray(getItem.List[0].Photo.ToString());
                // BitConverter.ToString(byteArray);
                //this.LoadImageFromByteArray(byteArray);
                //this.SetImageFromByteArray(byteArray);


                this.Close();
            }



        }

        public void SetImageFromBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                MessageBox.Show("Base64 string is null or empty.");
                return;
            }

            // Remove the data URL scheme if present
            if (base64String.StartsWith("data:image/"))
            {
                var commaIndex = base64String.IndexOf(',');
                if (commaIndex >= 0)
                {
                    base64String = base64String.Substring(commaIndex + 1);
                }
            }

            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(ms);
                    ItemMaster.pictureBox1.Image = image; // Set the PictureBox image
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Invalid Base64 string: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        public static byte[] StringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Input string cannot be null or empty.");
            }

            // Use UTF8 encoding (you can choose another encoding if needed)
            return Encoding.UTF8.GetBytes(str);
        }
        public void LoadImageFromByteArray(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                Image image = Image.FromStream(ms);
                //ItemMaster.openFileDialog1.FileName = image;
            }
        }
        public void SetImageFromByteArray(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    ms.Position = 0;
                    Image image = Image.FromStream(ms);
                    ItemMaster.pictureBox1.Image = image; // Set the PictureBox image
                }
            }
            else
            {
                ItemMaster.pictureBox1.Image = null; // Optionally set to null if no image
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.GetItem(textBox1.Text);
            
        }
    }
}
