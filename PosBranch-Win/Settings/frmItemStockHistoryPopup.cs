using Repository.SettingsRepo;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public class frmItemStockHistoryPopup : Form
    {
        private readonly string searchText;
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private DataGridView gridHistory;

        public frmItemStockHistoryPopup(string searchText)
        {
            this.searchText = searchText;
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            Text = "Item Stock History";
            Size = new Size(1120, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(247, 252, 255);
            Font = new Font("Segoe UI", 9F);

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(232, 246, 255) };
            headerPanel.Controls.Add(new Label
            {
                Text = $"Stock History for: '{searchText}'",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = navy,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            });

            gridHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                EnableHeadersVisualStyles = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                GridColor = border,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 30 }
            };
            gridHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 120);
            gridHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridHistory.DefaultCellStyle.ForeColor = Color.FromArgb(30, 62, 120);
            gridHistory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 238, 255);
            gridHistory.DefaultCellStyle.SelectionForeColor = navy;
            gridHistory.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(245, 250, 255) };
            var btnClose = new Button
            {
                Text = "Close",
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(1010, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = navy,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderColor = navy;
            btnClose.Click += (s, e) => Close();
            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Resize += (s, e) => btnClose.Left = bottomPanel.ClientSize.Width - btnClose.Width - 20;

            Controls.Add(gridHistory);
            Controls.Add(headerPanel);
            Controls.Add(bottomPanel);
        }

        private void LoadHistory()
        {
            try
            {
                using (var repo = new ItemStockActivityLogRepository())
                {
                    DataTable data = repo.GetItemStockHistoryLog(searchText);
                    gridHistory.DataSource = BuildDisplayTable(data);
                    ConfigureGrid();
                    ApplyActionColors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load item stock history: " + ex.Message, "Item Stock History", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            if (gridHistory.Columns.Count == 0) return;

            SetColumn("Action", "Action", 150, 0);
            SetColumn("Stock", "Stock", 110, 1);
            SetColumn("Available", "Available", 110, 2);
            SetColumn("Hold", "Hold", 90, 3);
            SetColumn("Qty", "Qty", 100, 4);
            SetColumn("BillNo", "Bill No", 110, 5);
            SetColumn("PurchaseNo", "Purchase No", 120, 6);
            SetColumn("Counter", "Counter", 180, 7);

            foreach (string numericColumn in new[] { "Qty", "Stock", "Available", "Hold" })
            {
                if (gridHistory.Columns.Contains(numericColumn))
                {
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Format = "0.####";
                    gridHistory.Columns[numericColumn].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
        }

        private static DataTable BuildDisplayTable(DataTable source)
        {
            DataTable display = new DataTable();
            display.Columns.Add("Action", typeof(string));
            display.Columns.Add("Stock", typeof(decimal));
            display.Columns.Add("Available", typeof(decimal));
            display.Columns.Add("Hold", typeof(decimal));
            display.Columns.Add("Qty", typeof(decimal));
            display.Columns.Add("BillNo", typeof(string));
            display.Columns.Add("PurchaseNo", typeof(string));
            display.Columns.Add("Counter", typeof(string));

            if (source == null)
            {
                return display;
            }

            foreach (DataRow row in source.Rows)
            {
                display.Rows.Add(
                    Convert.ToString(row["Action"]),
                    ToDecimal(row, "Stock"),
                    ToDecimal(row, "Available"),
                    ToDecimal(row, "Hold"),
                    ToDecimal(row, "Qty"),
                    ToText(row, "SalesBillNo"),
                    FormatPurchaseNo(ToText(row, "PurchaseNo")),
                    Convert.ToString(row["CounterName"]));
            }

            return display;
        }

        private static string ToText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]);
        }

        private static string FormatPurchaseNo(string purchaseNo)
        {
            if (string.IsNullOrWhiteSpace(purchaseNo))
            {
                return string.Empty;
            }

            return purchaseNo.StartsWith("GRN-", StringComparison.OrdinalIgnoreCase)
                ? purchaseNo
                : "GRN-" + purchaseNo;
        }

        private static decimal ToDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return 0m;
            }

            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private void SetColumn(string name, string header, int width, int displayIndex)
        {
            if (!gridHistory.Columns.Contains(name)) return;
            var column = gridHistory.Columns[name];
            column.HeaderText = header;
            column.Width = width;
            column.DisplayIndex = displayIndex;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        }

        private void ApplyActionColors()
        {
            if (!gridHistory.Columns.Contains("Action") || !gridHistory.Columns.Contains("Stock")) return;

            foreach (DataGridViewRow row in gridHistory.Rows)
            {
                string action = Convert.ToString(row.Cells["Action"].Value);
                Color color = GetActionColor(action);
                if (color == Color.Empty) continue;

                row.Cells["Stock"].Style.ForeColor = color;
                row.Cells["Stock"].Style.SelectionForeColor = color;
                row.Cells["Stock"].Style.BackColor = GetActionBackColor(action);
                row.Cells["Stock"].Style.SelectionBackColor = GetActionBackColor(action);
                row.Cells["Action"].Style.Font = new Font(gridHistory.Font, FontStyle.Bold);
            }
        }

        private static Color GetActionColor(string action)
        {
            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(190, 35, 35);
            }

            if (string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(24, 128, 70);
            }

            if (string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(204, 112, 0);
            }

            if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(35, 95, 190);
            }

            return Color.Empty;
        }

        private static Color GetActionBackColor(string action)
        {
            if (string.Equals(action, "Sales", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(255, 238, 238);
            }

            if (string.Equals(action, "Purchase", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(235, 250, 241);
            }

            if (string.Equals(action, "Sales Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(255, 244, 229);
            }

            if (string.Equals(action, "Purchase Return", StringComparison.OrdinalIgnoreCase))
            {
                return Color.FromArgb(235, 242, 255);
            }

            return Color.White;
        }
    }
}
