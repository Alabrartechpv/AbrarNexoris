using ModelClass;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.PurchaseReports
{
    public class frmvendorpurchasereport : Form
    {
        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color cardBack = Color.FromArgb(250, 253, 255);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color muted = Color.FromArgb(72, 98, 138);
        private readonly Color accent = Color.FromArgb(42, 121, 232);
        private readonly CultureInfo culture = new CultureInfo("en-IN");

        private ComboBox cmbQuickDate;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private TextBox txtVendor;
        private TextBox txtItem;
        private Button btnVendor;
        private Button btnItem;
        private Button btnClearVendor;
        private Button btnClearItem;
        private Button btnApply;
        private Button btnReset;
        private Button btnExport;
        private Label lblTotalRows;
        private Label lblTotalPurchases;
        private Label lblTotalQty;
        private Label lblTotalAmount;
        private Label lblShowing;
        private DataGridView gridReport;

        private int selectedVendorId;
        private string selectedVendorName = string.Empty;
        private int selectedItemId;
        private string selectedItemName = string.Empty;
        private bool suppressQuickDateChange;
        private DataTable currentData = new DataTable();

        public frmvendorpurchasereport()
        {
            Text = "Vendor Purchase Report";
            BackColor = pageBack;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(980, 620);
            Load += frmvendorpurchasereport_Load;
            BuildLayout();
        }

        private void frmvendorpurchasereport_Load(object sender, EventArgs e)
        {
            cmbQuickDate.SelectedItem = "Today";
            ApplyQuickDate();
            LoadReport();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Panel page = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 14, 18, 16),
                BackColor = pageBack
            };

            Label title = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 34,
                Text = "Vendor Purchase Report",
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel filters = CreateCardPanel(112);
            BuildFilters(filters);

            TableLayoutPanel summary = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 78,
                ColumnCount = 4,
                Padding = new Padding(0, 10, 0, 8)
            };
            for (int i = 0; i < 4; i++)
                summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            lblTotalRows = AddSummaryCard(summary, 0, "Rows", "0");
            lblTotalPurchases = AddSummaryCard(summary, 1, "Purchase Bills", "0");
            lblTotalQty = AddSummaryCard(summary, 2, "Quantity", "0.00");
            lblTotalAmount = AddSummaryCard(summary, 3, "Amount", "Rs 0.00");

            Panel gridCard = CreateCardPanel(0);
            gridCard.Dock = DockStyle.Fill;
            gridCard.Padding = new Padding(10, 36, 10, 10);

            lblShowing = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Showing 0 record(s)",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = muted,
                Padding = new Padding(3, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            gridReport = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = cardBack,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(218, 232, 247),
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            StyleGrid();

            gridCard.Controls.Add(gridReport);
            gridCard.Controls.Add(lblShowing);

            page.Controls.Add(gridCard);
            page.Controls.Add(summary);
            page.Controls.Add(filters);
            page.Controls.Add(title);
            Controls.Add(page);

            ResumeLayout(false);
        }

        private void BuildFilters(Panel parent)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 12,
                RowCount = 2,
                Padding = new Padding(12, 10, 12, 10)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 12; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333F));

            cmbQuickDate = CreateCombo();
            cmbQuickDate.Items.AddRange(new object[] { "Today", "Yesterday", "Last 7 Days", "This Month", "Custom" });
            cmbQuickDate.SelectedIndexChanged += (s, e) =>
            {
                if (Convert.ToString(cmbQuickDate.SelectedItem) != "Custom")
                    ApplyQuickDate();
            };

            dtpFrom = CreateDatePicker();
            dtpTo = CreateDatePicker();
            dtpFrom.ValueChanged += (s, e) => SetCustomQuickDate();
            dtpTo.ValueChanged += (s, e) => SetCustomQuickDate();

            txtVendor = CreateReadonlyTextBox();
            txtItem = CreateReadonlyTextBox();
            btnVendor = CreateButton("Vendor");
            btnItem = CreateButton("Item");
            btnClearVendor = CreateButton("Clear");
            btnClearItem = CreateButton("Clear");
            btnApply = CreatePrimaryButton("Apply");
            btnReset = CreateButton("Reset");
            btnExport = CreateButton("Export");

            btnVendor.Click += (s, e) => SelectVendor();
            btnItem.Click += (s, e) => SelectItem();
            btnClearVendor.Click += (s, e) => ClearVendor();
            btnClearItem.Click += (s, e) => ClearItem();
            btnApply.Click += (s, e) => LoadReport();
            btnReset.Click += (s, e) => ResetFilters();
            btnExport.Click += (s, e) => ExportCurrentData();

            AddLabeledControl(layout, "Quick", cmbQuickDate, 0, 0, 2);
            AddLabeledControl(layout, "From", dtpFrom, 2, 0, 2);
            AddLabeledControl(layout, "To", dtpTo, 4, 0, 2);
            AddLabeledControl(layout, "Vendor", txtVendor, 0, 1, 4);
            AddLabeledControl(layout, "Item", txtItem, 4, 1, 4);

            layout.Controls.Add(btnApply, 6, 0);
            layout.SetColumnSpan(btnApply, 1);
            layout.Controls.Add(btnReset, 7, 0);
            layout.Controls.Add(btnExport, 8, 0);
            layout.Controls.Add(btnVendor, 8, 1);
            layout.Controls.Add(btnClearVendor, 9, 1);
            layout.Controls.Add(btnItem, 10, 1);
            layout.Controls.Add(btnClearItem, 11, 1);

            parent.Controls.Add(layout);
        }

        private void AddLabeledControl(TableLayoutPanel layout, string caption, Control control, int column, int row, int columnSpan)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 2) };
            Label label = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                Text = caption,
                ForeColor = muted,
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold)
            };
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control);
            panel.Controls.Add(label);
            layout.Controls.Add(panel, column, row);
            layout.SetColumnSpan(panel, columnSpan);
        }

        private Panel CreateCardPanel(int height)
        {
            Panel panel = new Panel
            {
                BackColor = cardBack,
                Height = height,
                Dock = height > 0 ? DockStyle.Top : DockStyle.None,
                Margin = new Padding(0, 0, 0, 8)
            };
            panel.Paint += Card_Paint;
            return panel;
        }

        private Label AddSummaryCard(TableLayoutPanel parent, int column, string title, string value)
        {
            Panel card = CreateCardPanel(0);
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(column == 0 ? 0 : 6, 0, column == 3 ? 0 : 6, 0);
            card.Padding = new Padding(14, 8, 12, 8);

            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = title,
                ForeColor = muted,
                Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold)
            };
            Label valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = value,
                ForeColor = navy,
                Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);
            parent.Controls.Add(card, column, 0);
            return valueLabel;
        }

        private ComboBox CreateCombo()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
        }

        private DateTimePicker CreateDatePicker()
        {
            return new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MMM-yyyy",
                Font = new Font("Segoe UI", 9F)
            };
        }

        private TextBox CreateReadonlyTextBox()
        {
            return new TextBox
            {
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };
        }

        private Button CreateButton(string text)
        {
            Button button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(236, 246, 255),
                ForeColor = navy,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                Margin = new Padding(4, 20, 4, 3)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(169, 209, 240);
            return button;
        }

        private Button CreatePrimaryButton(string text)
        {
            Button button = CreateButton(text);
            button.BackColor = accent;
            button.ForeColor = Color.White;
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void StyleGrid()
        {
            gridReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 239, 252);
            gridReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridReport.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            gridReport.ColumnHeadersHeight = 32;
            gridReport.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            gridReport.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            gridReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(211, 229, 248);
            gridReport.DefaultCellStyle.SelectionForeColor = navy;
            gridReport.RowTemplate.Height = 28;
        }

        private void SelectVendor()
        {
            using (frmVendorDig dialog = new frmVendorDig())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedVendorId = dialog.SelectedVendorId;
                    selectedVendorName = dialog.SelectedVendorName ?? string.Empty;
                    txtVendor.Text = selectedVendorName;
                    LoadReport();
                }
            }
        }

        private void SelectItem()
        {
            using (frmdialForItemMaster dialog = new frmdialForItemMaster("frmvendorpurchasereport"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var data = dialog.GetSelectedItemData();
                    selectedItemId = GetDictionaryInt(data, "ItemId");
                    selectedItemName = GetDictionaryString(data, "Description");
                    if (string.IsNullOrWhiteSpace(selectedItemName))
                        selectedItemName = GetDictionaryString(data, "ItemName");
                    txtItem.Text = selectedItemName;
                    LoadReport();
                }
            }
        }

        private void ClearVendor()
        {
            selectedVendorId = 0;
            selectedVendorName = string.Empty;
            txtVendor.Clear();
            LoadReport();
        }

        private void ClearItem()
        {
            selectedItemId = 0;
            selectedItemName = string.Empty;
            txtItem.Clear();
            LoadReport();
        }

        private void ResetFilters()
        {
            selectedVendorId = 0;
            selectedItemId = 0;
            selectedVendorName = string.Empty;
            selectedItemName = string.Empty;
            txtVendor.Clear();
            txtItem.Clear();
            cmbQuickDate.SelectedItem = "Today";
            ApplyQuickDate();
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                using (VendorPurchaseReportRepository repo = new VendorPurchaseReportRepository())
                {
                    currentData = selectedItemId > 0
                        ? repo.GetItemVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo), selectedItemId, GetCompanyId(), GetBranchId(), GetFinYearId())
                        : repo.GetVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo), selectedVendorId, 0, GetCompanyId(), GetBranchId(), GetFinYearId());
                }

                gridReport.DataSource = currentData;
                ConfigureGridColumns();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor purchase report: " + ex.Message,
                    "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridReport.Columns.Count == 0)
                return;

            SetColumn("Rank", "#", 55);
            SetColumn("Vendor", "Vendor", 190);
            SetColumn("PurchaseDate", "Purchase Date", 115, "dd-MMM-yyyy");
            SetColumn("InvoiceDate", "Invoice Date", 115, "dd-MMM-yyyy");
            SetColumn("PurchaseNo", "Purchase No", 90);
            SetColumn("GRNNumber", "GRN No", 90);
            SetColumn("InvoiceNo", "Invoice No", 105);
            SetColumn("ItemName", "Item Name", 230);
            SetColumn("Qty", "Qty", 80, "N2", true);
            SetColumn("Amount", "Amount", 110, "N2", true);
            SetColumn("TotalAmount", "Total Amount", 120, "N2", true);

            foreach (DataGridViewColumn column in gridReport.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
                if (column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    column.Visible = false;
            }
        }

        private void SetColumn(string name, string caption, int width, string format = null, bool alignRight = false)
        {
            if (!gridReport.Columns.Contains(name))
                return;

            DataGridViewColumn column = gridReport.Columns[name];
            column.HeaderText = caption;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 80);
            column.FillWeight = Math.Max(50, width);
            if (!string.IsNullOrWhiteSpace(format))
                column.DefaultCellStyle.Format = format;
            if (alignRight)
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void UpdateSummary()
        {
            int rows = currentData == null ? 0 : currentData.Rows.Count;
            lblTotalRows.Text = rows.ToString("N0", culture);
            lblShowing.Text = "Showing " + rows.ToString("N0", culture) + " record(s)";

            decimal qty = 0;
            decimal amount = 0;
            DataView view = currentData == null ? null : currentData.DefaultView;
            if (view != null)
            {
                foreach (DataRowView rowView in view)
                {
                    qty += ToDecimal(rowView.Row, "Qty");
                    amount += ToDecimal(rowView.Row, currentData.Columns.Contains("TotalAmount") ? "TotalAmount" : "Amount");
                }
            }

            lblTotalQty.Text = qty.ToString("N2", culture);
            lblTotalAmount.Text = Money(amount);

            if (currentData != null && currentData.Columns.Contains("PurchaseNo"))
            {
                DataView distinct = new DataView(currentData);
                DataTable bills = distinct.ToTable(true, "PurchaseNo");
                lblTotalPurchases.Text = bills.Rows.Count.ToString("N0", culture);
            }
            else
            {
                lblTotalPurchases.Text = "0";
            }
        }

        private void ApplyQuickDate()
        {
            suppressQuickDateChange = true;
            DateTime today = DateTime.Today;
            string selected = Convert.ToString(cmbQuickDate.SelectedItem);
            DateTime from = today;
            DateTime to = today;

            if (selected == "Yesterday")
            {
                from = today.AddDays(-1);
                to = from;
            }
            else if (selected == "Last 7 Days")
            {
                from = today.AddDays(-6);
            }
            else if (selected == "This Month")
            {
                from = new DateTime(today.Year, today.Month, 1);
            }

            dtpFrom.Value = from;
            dtpTo.Value = to;
            suppressQuickDateChange = false;
        }

        private void SetCustomQuickDate()
        {
            if (suppressQuickDateChange)
                return;

            if (cmbQuickDate != null && Convert.ToString(cmbQuickDate.SelectedItem) != "Custom")
                cmbQuickDate.SelectedItem = "Custom";
        }

        private DateTime GetDateValue(DateTimePicker picker)
        {
            return picker.Value.Date;
        }

        private int GetCompanyId()
        {
            if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                return SessionContext.CompanyId;
            int value;
            return int.TryParse(DataBase.CompanyId, out value) && value > 0 ? value : 0;
        }

        private int GetBranchId()
        {
            if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
                return SessionContext.BranchId;
            int value;
            return int.TryParse(DataBase.BranchId, out value) && value > 0 ? value : 0;
        }

        private int GetFinYearId()
        {
            if (SessionContext.IsInitialized && SessionContext.FinYearId > 0)
                return SessionContext.FinYearId;
            int value;
            return int.TryParse(DataBase.FinyearId, out value) && value > 0 ? value : 0;
        }

        private int GetDictionaryInt(System.Collections.Generic.Dictionary<string, object> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null)
                return 0;
            int value;
            return int.TryParse(Convert.ToString(data[key]), out value) ? value : 0;
        }

        private string GetDictionaryString(System.Collections.Generic.Dictionary<string, object> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null)
                return string.Empty;
            return Convert.ToString(data[key]);
        }

        private decimal ToDecimal(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return 0;
            decimal value;
            return decimal.TryParse(Convert.ToString(row[column]), NumberStyles.Any, culture, out value) ? value : 0;
        }

        private string Money(decimal value)
        {
            return "Rs " + value.ToString("N2", culture);
        }

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No rows to export.", "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = "VendorPurchaseReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllText(dialog.FileName, BuildCsv(currentData), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string BuildCsv(DataTable table)
        {
            StringBuilder csv = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0) csv.Append(',');
                csv.Append(EscapeCsv(table.Columns[i].ColumnName));
            }
            csv.AppendLine();

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0) csv.Append(',');
                    csv.Append(EscapeCsv(Convert.ToString(row[i])));
                }
                csv.AppendLine();
            }

            return csv.ToString();
        }

        private string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (GraphicsPath path = RoundedRect(rect, 7))
            using (SolidBrush brush = new SolidBrush(panel.BackColor))
            using (Pen pen = new Pen(border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
