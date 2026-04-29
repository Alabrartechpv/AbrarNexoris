using System.ComponentModel;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Transaction
{
    partial class frmPurchaseOrder
    {
        private IContainer components = null;
        private UltraPanel ultraPanelHeader;
        private UltraPanel ultraPanelAction;
        private UltraPanel ultraPanelGrid;
        private UltraPanel ultraPanelFooter;
        private UltraLabel lblDraftTitle;
        private UltraLabel lblDraftNote;
        private UltraButton btnUseSuggested;
        private UltraButton btnOpenPurchase;
        private UltraButton btnClose;
        private UltraGrid ultraGridDraft;
        private UltraLabel lblLineCount;
        private UltraLabel lblTotalQty;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.ultraPanelHeader = new UltraPanel();
            this.lblDraftTitle = new UltraLabel();
            this.lblDraftNote = new UltraLabel();
            this.ultraPanelAction = new UltraPanel();
            this.btnUseSuggested = new UltraButton();
            this.btnOpenPurchase = new UltraButton();
            this.btnClose = new UltraButton();
            this.ultraPanelGrid = new UltraPanel();
            this.ultraGridDraft = new UltraGrid();
            this.ultraPanelFooter = new UltraPanel();
            this.lblLineCount = new UltraLabel();
            this.lblTotalQty = new UltraLabel();
            this.ultraPanelHeader.ClientArea.SuspendLayout();
            this.ultraPanelHeader.SuspendLayout();
            this.ultraPanelAction.ClientArea.SuspendLayout();
            this.ultraPanelAction.SuspendLayout();
            this.ultraPanelGrid.ClientArea.SuspendLayout();
            this.ultraPanelGrid.SuspendLayout();
            ((ISupportInitialize)(this.ultraGridDraft)).BeginInit();
            this.ultraPanelFooter.ClientArea.SuspendLayout();
            this.ultraPanelFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultraPanelHeader
            // 
            this.ultraPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.ultraPanelHeader.Name = "ultraPanelHeader";
            this.ultraPanelHeader.Size = new System.Drawing.Size(1000, 68);
            this.ultraPanelHeader.TabIndex = 0;
            // 
            // ultraPanelHeader.ClientArea
            // 
            this.ultraPanelHeader.ClientArea.Controls.Add(this.lblDraftTitle);
            this.ultraPanelHeader.ClientArea.Controls.Add(this.lblDraftNote);
            // 
            // lblDraftTitle
            // 
            this.lblDraftTitle.Location = new System.Drawing.Point(16, 10);
            this.lblDraftTitle.Name = "lblDraftTitle";
            this.lblDraftTitle.Size = new System.Drawing.Size(250, 22);
            this.lblDraftTitle.TabIndex = 0;
            this.lblDraftTitle.Text = "Purchase Order Draft";
            // 
            // lblDraftNote
            // 
            this.lblDraftNote.Location = new System.Drawing.Point(16, 36);
            this.lblDraftNote.Name = "lblDraftNote";
            this.lblDraftNote.Size = new System.Drawing.Size(950, 20);
            this.lblDraftNote.TabIndex = 1;
            this.lblDraftNote.Text = "Review the selected reorder items below.";
            // 
            // ultraPanelAction
            // 
            this.ultraPanelAction.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraPanelAction.Location = new System.Drawing.Point(0, 68);
            this.ultraPanelAction.Name = "ultraPanelAction";
            this.ultraPanelAction.Size = new System.Drawing.Size(1000, 42);
            this.ultraPanelAction.TabIndex = 1;
            // 
            // ultraPanelAction.ClientArea
            // 
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnUseSuggested);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnOpenPurchase);
            this.ultraPanelAction.ClientArea.Controls.Add(this.btnClose);
            // 
            // btnUseSuggested
            // 
            this.btnUseSuggested.Location = new System.Drawing.Point(16, 7);
            this.btnUseSuggested.Name = "btnUseSuggested";
            this.btnUseSuggested.Size = new System.Drawing.Size(100, 28);
            this.btnUseSuggested.TabIndex = 0;
            this.btnUseSuggested.Text = "Reset Qty";
            this.btnUseSuggested.Click += new System.EventHandler(this.btnUseSuggested_Click);
            // 
            // btnOpenPurchase
            // 
            this.btnOpenPurchase.Location = new System.Drawing.Point(124, 7);
            this.btnOpenPurchase.Name = "btnOpenPurchase";
            this.btnOpenPurchase.Size = new System.Drawing.Size(148, 28);
            this.btnOpenPurchase.TabIndex = 1;
            this.btnOpenPurchase.Text = "Continue To Purchase";
            this.btnOpenPurchase.Click += new System.EventHandler(this.btnOpenPurchase_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(872, 7);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(112, 28);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ultraPanelGrid
            // 
            this.ultraPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanelGrid.Location = new System.Drawing.Point(0, 110);
            this.ultraPanelGrid.Name = "ultraPanelGrid";
            this.ultraPanelGrid.Size = new System.Drawing.Size(1000, 410);
            this.ultraPanelGrid.TabIndex = 2;
            // 
            // ultraPanelGrid.ClientArea
            // 
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraGridDraft);
            this.ultraPanelGrid.ClientArea.Controls.Add(this.ultraPanelFooter);
            // 
            // ultraGridDraft
            // 
            this.ultraGridDraft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraGridDraft.Location = new System.Drawing.Point(0, 0);
            this.ultraGridDraft.Name = "ultraGridDraft";
            this.ultraGridDraft.Size = new System.Drawing.Size(1000, 382);
            this.ultraGridDraft.TabIndex = 0;
            this.ultraGridDraft.InitializeLayout += new InitializeLayoutEventHandler(this.ultraGridDraft_InitializeLayout);
            this.ultraGridDraft.AfterCellUpdate += new CellEventHandler(this.ultraGridDraft_AfterCellUpdate);
            // 
            // ultraPanelFooter
            // 
            this.ultraPanelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ultraPanelFooter.Location = new System.Drawing.Point(0, 382);
            this.ultraPanelFooter.Name = "ultraPanelFooter";
            this.ultraPanelFooter.Size = new System.Drawing.Size(1000, 28);
            this.ultraPanelFooter.TabIndex = 1;
            // 
            // ultraPanelFooter.ClientArea
            // 
            this.ultraPanelFooter.ClientArea.Controls.Add(this.lblLineCount);
            this.ultraPanelFooter.ClientArea.Controls.Add(this.lblTotalQty);
            // 
            // lblLineCount
            // 
            this.lblLineCount.Location = new System.Drawing.Point(16, 5);
            this.lblLineCount.Name = "lblLineCount";
            this.lblLineCount.Size = new System.Drawing.Size(120, 18);
            this.lblLineCount.TabIndex = 0;
            this.lblLineCount.Text = "Lines: 0";
            // 
            // lblTotalQty
            // 
            this.lblTotalQty.Location = new System.Drawing.Point(170, 5);
            this.lblTotalQty.Name = "lblTotalQty";
            this.lblTotalQty.Size = new System.Drawing.Size(160, 18);
            this.lblTotalQty.TabIndex = 1;
            this.lblTotalQty.Text = "Total Qty: 0";
            // 
            // frmPurchaseOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 520);
            this.Controls.Add(this.ultraPanelGrid);
            this.Controls.Add(this.ultraPanelAction);
            this.Controls.Add(this.ultraPanelHeader);
            this.Name = "frmPurchaseOrder";
            this.Text = "Purchase Order";
            this.ultraPanelHeader.ClientArea.ResumeLayout(false);
            this.ultraPanelHeader.ClientArea.PerformLayout();
            this.ultraPanelHeader.ResumeLayout(false);
            this.ultraPanelAction.ClientArea.ResumeLayout(false);
            this.ultraPanelAction.ResumeLayout(false);
            this.ultraPanelGrid.ClientArea.ResumeLayout(false);
            this.ultraPanelGrid.ResumeLayout(false);
            ((ISupportInitialize)(this.ultraGridDraft)).EndInit();
            this.ultraPanelFooter.ClientArea.ResumeLayout(false);
            this.ultraPanelFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
