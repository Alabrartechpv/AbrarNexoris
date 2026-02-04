using ModelClass;
using PosBranch_Win.Accounts;
using PosBranch_Win.Master;
using PosBranch_Win.Reports.SalesReports;
using PosBranch_Win.Transaction;
using PosBranch_Win.Utilities;
using PosBranch_Win.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Linq;

namespace PosBranch_Win
{
    public partial class Home : Form
    {
        private int childFormNumber = 0;
        private bool isCtrlPressed = false;
        private bool isF2Pressed = false;
        private bool isShiftPressed = false;
        // Track close button rectangles for each tab
        private Dictionary<int, Rectangle> closeButtonRects = new Dictionary<int, Rectangle>();
        private int closeHotTrackTab = -1; // No hot-tracking initially
        private int tabHoverIndex = -1;    // Track which tab is being hovered
        private bool enableTabScrolling = true; // Enable tab scrolling feature
        private Timer scrollTimer = new Timer(); // Timer for smooth scrolling
        private int scrollDirection = 0; // 0 = none, -1 = left, 1 = right


        public Home()
        {
            InitializeComponent();

            // UltraTabControl doesn't need DrawItem handler as it has built-in styling

            // Wire up mouse events for tabs
            tabControlMain.MouseClick += tabControlMain_MouseClick;
            tabControlMain.MouseMove += tabControlMain_MouseMove;
            tabControlMain.MouseUp += tabControlMain_MouseUp;
            tabControlMain.Resize += tabControlMain_Resize;
            tabControlMain.MouseDown += tabControlMain_MouseDown;
            tabControlMain.MouseLeave += tabControlMain_MouseLeave;
            tabControlMain.MouseWheel += tabControlMain_MouseWheel;

            // Set up the timer for smooth scrolling
            scrollTimer.Interval = 50;
            scrollTimer.Tick += ScrollTimer_Tick;

            // Wire up context menu events
            closeTabToolStripMenuItem.Click += closeTabToolStripMenuItem_Click;
            closeAllTabsToolStripMenuItem.Click += closeAllTabsToolStripMenuItem_Click;



            // Configure specific properties for UltraTabControl v22.2
            // Note: Some properties may not exist in v22.2, so we use try-catch
            try
            {
                // Try to set scroll buttons if the property exists
                var scrollButtonsProp = this.tabControlMain.GetType().GetProperty("ScrollButtons");
                if (scrollButtonsProp != null)
                {
                    scrollButtonsProp.SetValue(this.tabControlMain, true);
                }
            }
            catch { /* Ignore if property doesn't exist */ }

            try
            {
                // Try to set tab orientation if the property exists
                var tabOrientationProp = this.tabControlMain.GetType().GetProperty("TabOrientation");
                if (tabOrientationProp != null)
                {
                    var enumType = tabOrientationProp.PropertyType;
                    try
                    {
                        var topLeftValue = System.Enum.Parse(enumType, "TopLeft");
                        tabOrientationProp.SetValue(this.tabControlMain, topLeftValue);
                    }
                    catch { /* Ignore if enum value doesn't exist */ }
                }
            }
            catch { /* Ignore if property doesn't exist */ }

            // Set appearance - will be themed dynamically

            // Try to enable close buttons (these properties may or may not exist in v22.2)
            try
            {
                // Using reflection to safely set properties that might exist
                var tabControlType = this.tabControlMain.GetType();

                var closeButtonLocationProp = tabControlType.GetProperty("CloseButtonLocation");
                if (closeButtonLocationProp != null)
                {
                    var enumType = closeButtonLocationProp.PropertyType;
                    try
                    {
                        var tabValue = System.Enum.Parse(enumType, "Tab");
                        closeButtonLocationProp.SetValue(this.tabControlMain, tabValue);
                    }
                    catch { /* Ignore if enum value doesn't exist */ }
                }

                var allowTabClosingProp = tabControlType.GetProperty("AllowTabClosing");
                if (allowTabClosingProp != null)
                {
                    allowTabClosingProp.SetValue(this.tabControlMain, true);
                }
            }
            catch
            {
                // If reflection fails, continue without close buttons
            }
        }

        // Process mouse wheel for tab scrolling
        private void tabControlMain_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!enableTabScrolling || tabControlMain.Tabs.Count <= 1)
                return;

            // Calculate how many tabs to scroll based on delta
            int scrollCount = (e.Delta > 0) ? -1 : 1;
            int newIndex = Math.Min(Math.Max(0, tabControlMain.ActiveTab.Index + scrollCount),
                tabControlMain.Tabs.Count - 1);

            tabControlMain.ActiveTab = tabControlMain.Tabs[newIndex];
        }

        // Timer callback for smooth scrolling animation
        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (scrollDirection == 0 || tabControlMain.Tabs.Count <= 1)
            {
                scrollTimer.Stop();
                return;
            }

            int newIndex = tabControlMain.ActiveTab.Index + scrollDirection;
            if (newIndex >= 0 && newIndex < tabControlMain.Tabs.Count)
            {
                tabControlMain.ActiveTab = tabControlMain.Tabs[newIndex];
            }
            else
            {
                scrollTimer.Stop();
                scrollDirection = 0;
            }
        }

        // Track mouse leave to reset hover states
        private void tabControlMain_MouseLeave(object sender, EventArgs e)
        {
            if (tabHoverIndex != -1)
            {
                tabHoverIndex = -1;
                tabControlMain.Invalidate();
            }

            // Stop scrolling when mouse leaves the control
            scrollDirection = 0;
            scrollTimer.Stop();
        }

        // Process mouse down events including middle-click for close
        private void tabControlMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // Middle click to close tab
                // Find the tab under the mouse cursor and close it
                for (int i = 0; i < tabControlMain.Tabs.Count; i++)
                {
                    var tab = tabControlMain.Tabs[i];
                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                    {
                        Form form = (Form)tab.TabPage.Controls[0];
                        form.Close();
                        break;
                    }
                }
            }
        }

        // (removed unused UpdateCloseButtonRects)

        // UltraTabControl handles custom drawing automatically - no need for custom DrawItem code

        private void tabControlMain_MouseMove(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles mouse movement automatically
            // No need for custom mouse tracking with UltraTabControl
        }

        private void tabControlMain_MouseClick(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles tab clicking automatically
            // No need for custom click handling with UltraTabControl
        }

        private void tabControlMain_MouseUp(object sender, MouseEventArgs e)
        {
            // UltraTabControl handles context menu automatically
            // No need for custom right-click handling with UltraTabControl
        }

        private void tabControlMain_Resize(object sender, EventArgs e)
        {
            // Force redraw to update close button positions
            tabControlMain.Invalidate();

            // CRITICAL FIX: Ensure all forms in tabs are properly resized
            EnsureAllTabFormsAreProperlySized();
        }

        // CRITICAL FIX: Method to ensure all forms in tabs are properly sized
        private void EnsureAllTabFormsAreProperlySized()
        {
            try
            {
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form form)
                    {
                        if (!form.IsDisposed && form.Visible)
                        {
                            // Force form to fill the tab page
                            form.Dock = DockStyle.Fill;
                            form.Size = tab.TabPage.Size;
                            form.Location = new Point(0, 0);

                            // Force layout updates
                            form.PerformLayout();
                            form.Invalidate();
                            form.Update();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring tab forms are properly sized: {ex.Message}");
            }
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCurrentTab();
        }

        private void closeAllTabsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseAllTabs();
        }

        private void CloseCurrentTab()
        {
            if (tabControlMain.ActiveTab != null && tabControlMain.Tabs.Count > 0)
            {
                var activeTab = tabControlMain.ActiveTab;

                // Close the form in the tab
                if (activeTab.TabPage.Controls.Count > 0 && activeTab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)activeTab.TabPage.Controls[0];

                    // CRITICAL FIX: Properly dispose form to prevent memory leaks
                    try
                    {
                        // Unsubscribe from events first
                        form.FormClosed -= null; // Remove any event handlers

                        // Close the form properly
                        form.Close();

                        // Force disposal
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing form: {ex.Message}");
                    }
                }

                // Remove the tab after closing the form
                tabControlMain.Tabs.Remove(activeTab);
            }
        }

        private void CloseAllTabs()
        {
            // Close all forms in tabs
            for (int i = tabControlMain.Tabs.Count - 1; i >= 0; i--)
            {
                var tab = tabControlMain.Tabs[i];
                if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)tab.TabPage.Controls[0];

                    // CRITICAL FIX: Properly dispose each form
                    try
                    {
                        // Unsubscribe from events first
                        form.FormClosed -= null; // Remove any event handlers

                        // Close the form properly
                        form.Close();

                        // Force disposal
                        form.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing form in CloseAllTabs: {ex.Message}");
                    }
                }
            }

            // UltraTabControl will handle tab removal automatically
            // Clear all close button rectangles
            closeButtonRects.Clear();
        }

        /// <summary>
        /// Opens a form in a new tab or switches to its tab if already open
        /// </summary>
        private void OpenFormInTab(Form formToOpen, string tabName)
        {
            try
            {
                // Check if tab already exists
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    if (tab.Text == tabName)
                    {
                        tabControlMain.SelectedTab = tab; // Use SelectedTab instead of ActiveTab
                        return;
                    }
                }

                // Create new tab
                string uniqueKey = $"Tab_{DateTime.Now.Ticks}_{tabName}";
                var newTab = tabControlMain.Tabs.Add(uniqueKey, tabName);

                // CRITICAL FIX: Ensure form is properly initialized BEFORE setting properties
                EnsureFormIsReady(formToOpen);

                // IMPORTANT: Set form properties BEFORE adding to tab
                formToOpen.TopLevel = false;
                formToOpen.FormBorderStyle = FormBorderStyle.None;
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.Visible = true;
                formToOpen.WindowState = FormWindowState.Normal;

                // CRITICAL FIX: Set form size to match tab page size
                formToOpen.Size = newTab.TabPage.Size;
                formToOpen.Location = new Point(0, 0);

                // Apply current theme color to the new form
                formToOpen.BackColor = Color.FromArgb(
                    Math.Min(255, currentThemeColor.R + 5),
                    Math.Min(255, currentThemeColor.G + 5),
                    Math.Min(255, currentThemeColor.B + 5)
                );

                // Apply theme to all controls in the new form
                ApplyThemeToControls(formToOpen.Controls, currentThemeColor);

                // Add the form to the tab page
                newTab.TabPage.Controls.Add(formToOpen);

                // CRITICAL FIX: Force layout updates after adding to tab
                newTab.TabPage.SuspendLayout();
                formToOpen.SuspendLayout();

                // Show the form AFTER adding to tab page
                formToOpen.Show();
                formToOpen.BringToFront();

                // CRITICAL FIX: Resume layout and force proper sizing
                formToOpen.ResumeLayout(false);
                newTab.TabPage.ResumeLayout(false);

                // CRITICAL FIX: Force form to fill the entire tab page
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.PerformLayout();
                newTab.TabPage.PerformLayout();

                // Set the new tab as active/selected
                tabControlMain.SelectedTab = newTab;

                // Apply active styling to the new tab
                ApplyTabStyling(newTab, true);

                // CRITICAL FIX: Force multiple refresh cycles to ensure proper layout
                Application.DoEvents();
                newTab.TabPage.Refresh();
                formToOpen.Refresh();
                tabControlMain.Refresh();

                // CRITICAL FIX: Additional layout enforcement
                formToOpen.Invalidate();
                formToOpen.Update();
                newTab.TabPage.Invalidate();
                newTab.TabPage.Update();

                // Wire up the form's FormClosed event to remove the tab
                formToOpen.FormClosed += (sender, e) =>
                {
                    try
                    {
                        if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                        {
                            tabControlMain.Tabs.Remove(newTab);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Successfully created tab '{tabName}' with {formToOpen.Controls.Count} controls");
                System.Diagnostics.Debug.WriteLine($"Form visible: {formToOpen.Visible}, Handle created: {formToOpen.IsHandleCreated}");
                System.Diagnostics.Debug.WriteLine($"Form size: {formToOpen.Size}, Tab page size: {newTab.TabPage.Size}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening form in tab: {ex.Message}");
                MessageBox.Show($"Error opening form: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Clean up on error
                if (formToOpen != null && !formToOpen.IsDisposed)
                {
                    formToOpen.Dispose();
                }
            }
        }

        // Additional helper method to ensure proper form initialization
        private void EnsureFormIsReady(Form form)
        {
            try
            {
                // CRITICAL FIX: Ensure form is not disposed
                if (form.IsDisposed)
                {
                    throw new ObjectDisposedException("Form is already disposed");
                }

                // Force handle creation if not already created
                if (!form.IsHandleCreated)
                {
                    var handle = form.Handle; // This forces handle creation
                }

                // CRITICAL FIX: Ensure form is in proper state for embedding
                form.WindowState = FormWindowState.Normal;
                form.StartPosition = FormStartPosition.Manual;

                // Ensure all child controls are created
                foreach (Control control in form.Controls)
                {
                    if (!control.IsHandleCreated)
                    {
                        var handle = control.Handle;
                    }
                }

                // CRITICAL FIX: Call PerformLayout to ensure proper layout
                form.SuspendLayout();
                form.PerformLayout();
                form.ResumeLayout(false);

                // CRITICAL FIX: Force form to be ready for embedding
                form.Visible = false; // Will be set to true later

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring form is ready: {ex.Message}");
                throw; // Re-throw to prevent embedding a broken form
            }
        }

        // Modified version with additional safety checks
        private void OpenFormInTabSafe(Form formToOpen, string tabName)
        {
            try
            {
                // Check if tab already exists
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    if (tab.Text == tabName)
                    {
                        // If form exists but is disposed, remove the tab and recreate
                        if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form existingForm)
                        {
                            if (existingForm.IsDisposed)
                            {
                                tabControlMain.Tabs.Remove(tab);
                                break; // Exit loop and create new tab below
                            }
                            else
                            {
                                tabControlMain.SelectedTab = tab;
                                existingForm.BringToFront();
                                existingForm.Focus();
                                return;
                            }
                        }
                        else
                        {
                            tabControlMain.SelectedTab = tab;
                            return;
                        }
                    }
                }

                // Ensure form is properly initialized before embedding
                EnsureFormIsReady(formToOpen);

                // Set form properties for embedding
                formToOpen.TopLevel = false;
                formToOpen.FormBorderStyle = FormBorderStyle.None;
                formToOpen.Dock = DockStyle.Fill;
                formToOpen.Visible = true;
                formToOpen.WindowState = FormWindowState.Normal;

                // Apply current theme color to the new form
                formToOpen.BackColor = Color.FromArgb(
                    Math.Min(255, currentThemeColor.R + 5),
                    Math.Min(255, currentThemeColor.G + 5),
                    Math.Min(255, currentThemeColor.B + 5)
                );

                // Apply theme to all controls in the new form
                ApplyThemeToControls(formToOpen.Controls, currentThemeColor);

                // Create new tab
                string uniqueKey = $"Tab_{DateTime.Now.Ticks}_{tabName}";
                var newTab = tabControlMain.Tabs.Add(uniqueKey, tabName);

                // Add form to tab page
                newTab.TabPage.Controls.Add(formToOpen);

                // Activate the tab
                tabControlMain.SelectedTab = newTab;

                // Apply active styling to the new tab
                ApplyTabStyling(newTab, true);

                // Show and focus the form
                formToOpen.Show();
                formToOpen.BringToFront();
                formToOpen.Focus();

                // Force layout and refresh
                newTab.TabPage.PerformLayout();
                formToOpen.PerformLayout();
                Application.DoEvents(); // Process any pending messages

                // Wire up cleanup event
                formToOpen.FormClosed += (sender, e) =>
                {
                    try
                    {
                        if (newTab != null && tabControlMain.Tabs.Contains(newTab))
                        {
                            // Remove all controls first, then the tab
                            newTab.TabPage.Controls.Clear();
                            tabControlMain.Tabs.Remove(newTab);
                            // Force garbage collection of the closed form sooner
                            GC.Collect();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing tab: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine($"Successfully opened '{tabName}' in new tab");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OpenFormInTabSafe: {ex.Message}");
                MessageBox.Show($"Error opening form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (formToOpen != null && !formToOpen.IsDisposed)
                {
                    formToOpen.Dispose();
                }
            }
        }

        // (removed legacy template menu handlers and test button handlers)

        private void Home_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = DataBase.Branch;
            toolStripStatusUserValLabel3.Text = DataBase.UserName;
            ultraRadialMenu1.Show(this, new Point(Bounds.Right, Bounds.Top));

            // Fix tab control appearance
            AdjustTabControlAppearance();

            // Apply role-based permissions to toolbar buttons
            ApplyRolePermissions();

            // Apply watermark logo to background
            ApplyWatermark();

            // Set tabControlMain BackColor programmatically - will be themed dynamically

            // Apply initial theme color
        }

        private void ApplyWatermark()
        {
            try
            {
                string logoPath = null;
                string[] possiblePaths = new string[]
                {
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "splash_logo.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", "splash_logo.png"),
                    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", "splash_logo.png")
                };

                foreach (string path in possiblePaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        logoPath = path;
                        break;
                    }
                }

                if (logoPath != null)
                {
                    using (Image original = Image.FromFile(logoPath))
                    {
                        // Create a watermark with 15% opacity
                        Bitmap watermark = new Bitmap(original.Width, original.Height);
                        using (Graphics g = Graphics.FromImage(watermark))
                        {
                            System.Drawing.Imaging.ColorMatrix matrix = new System.Drawing.Imaging.ColorMatrix();
                            matrix.Matrix33 = 0.15f; // 15% Opacity for "lil minimal blend in"
                            System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
                            attributes.SetColorMatrix(matrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
                            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                        }

                        // Set as background
                        this.tabControlMain.Appearance.ImageBackground = watermark;
                        this.tabControlMain.Appearance.ImageBackgroundStyle = Infragistics.Win.ImageBackgroundStyle.Centered;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying watermark: {ex.Message}");
            }
        }

        /// <summary>
        /// Adjusts the tab control appearance
        /// </summary>
        private void AdjustTabControlAppearance()
        {
            try
            {
                // Disable OS themes for custom styling
                tabControlMain.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;

                // Set matching light blue background
                tabControlMain.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);

                // Configure tab appearance for custom styling
                ConfigureTabAppearance();

                // Wire up tab closing events
                tabControlMain.TabClosed += TabControlMain_TabClosed;
                tabControlMain.TabClosing += TabControlMain_TabClosing;

                tabControlMain.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adjusting tab control appearance: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures tab appearance and handles selection changes
        /// </summary>
        private void ConfigureTabAppearance()
        {
            try
            {
                // Set tab appearance using reflection with matching colors
                SetTabAppearanceProperty("Appearance", "BackColor", System.Drawing.Color.FromArgb(240, 248, 255)); // Light blue background
                SetTabAppearanceProperty("Appearance", "BorderColor", System.Drawing.Color.FromArgb(0, 120, 215)); // Matching blue border
                SetTabAppearanceProperty("ActiveTabAppearance", "BackColor", System.Drawing.Color.FromArgb(0, 120, 215)); // Bright blue active
                SetTabAppearanceProperty("ActiveTabAppearance", "ForeColor", System.Drawing.Color.White); // White text
                SetTabAppearanceProperty("HotTrackAppearance", "BackColor", System.Drawing.Color.FromArgb(173, 216, 230)); // Light blue hover
                SetTabAppearanceProperty("TabHeaderAppearance", "BackColor", System.Drawing.Color.FromArgb(240, 248, 255)); // Light blue tab area

                // Wire up tab selection event
                tabControlMain.SelectedTabChanged += (sender, e) =>
                {
                    if (e.Tab != null) ApplyTabStyling(e.Tab, true);
                    foreach (var tab in tabControlMain.Tabs) if (tab != e.Tab) ApplyTabStyling(tab, false);

                    // CRITICAL FIX: Ensure form in selected tab is properly sized
                    if (e.Tab != null && e.Tab.TabPage.Controls.Count > 0 && e.Tab.TabPage.Controls[0] is Form form)
                    {
                        if (!form.IsDisposed && form.Visible)
                        {
                            // Force form to fill the tab page when tab is selected
                            form.Dock = DockStyle.Fill;
                            form.Size = e.Tab.TabPage.Size;
                            form.Location = new Point(0, 0);

                            // Force layout updates
                            form.PerformLayout();
                            form.Invalidate();
                            form.Update();

                            // Bring form to front
                            form.BringToFront();
                        }
                    }
                };
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Tab config error: {ex.Message}"); }
        }

        /// <summary>
        /// Helper method to set tab appearance properties
        /// </summary>
        private void SetTabAppearanceProperty(string propertyName, string subProperty, object value)
        {
            try
            {
                var prop = tabControlMain.GetType().GetProperty(propertyName);
                var obj = prop?.GetValue(tabControlMain);
                var subProp = obj?.GetType().GetProperty(subProperty);
                subProp?.SetValue(obj, value);
            }
            catch { /* Ignore reflection errors */ }
        }

        /// <summary>
        /// Applies styling to a tab (active or inactive)
        /// </summary>
        private void ApplyTabStyling(Infragistics.Win.UltraWinTabControl.UltraTab tab, bool isActive)
        {
            try
            {
                var appearance = tab.GetType().GetProperty("Appearance")?.GetValue(tab);
                if (appearance != null)
                {
                    appearance.GetType().GetProperty("BackColor")?.SetValue(appearance,
                        isActive ? System.Drawing.Color.FromArgb(0, 120, 215) : System.Drawing.SystemColors.Control);
                    appearance.GetType().GetProperty("ForeColor")?.SetValue(appearance,
                        isActive ? System.Drawing.Color.White : System.Drawing.SystemColors.ControlText);
                }
            }
            catch { /* Ignore reflection errors */ }
        }

        /// <summary>
        /// Tries to enable close buttons using reflection for UltraTabControl v22.2
        /// </summary>
        private void EnableCloseButtons()
        {
            try
            {
                // Use reflection to access properties that might exist in v22.2
                var type = tabControlMain.GetType();

                // Try to set CloseButtonVisibility
                var closeButtonProp = type.GetProperty("CloseButtonVisibility");
                if (closeButtonProp != null)
                {
                    // Try to find the enum value
                    var enumType = closeButtonProp.PropertyType;
                    var alwaysValue = Enum.Parse(enumType, "Always");
                    closeButtonProp.SetValue(tabControlMain, alwaysValue);
                }

                // Try to set AllowTabClosing
                var allowClosingProp = type.GetProperty("AllowTabClosing");
                if (allowClosingProp != null)
                {
                    allowClosingProp.SetValue(tabControlMain, true);
                }

                // Try alternative property names
                var closeButtonProp2 = type.GetProperty("ShowCloseButton");
                if (closeButtonProp2 != null)
                {
                    closeButtonProp2.SetValue(tabControlMain, true);
                }

                var closeButtonProp3 = type.GetProperty("CloseButton");
                if (closeButtonProp3 != null)
                {
                    closeButtonProp3.SetValue(tabControlMain, true);
                }
            }
            catch (Exception ex)
            {
                // If reflection fails, just continue without close buttons
                System.Diagnostics.Debug.WriteLine($"Could not enable close buttons: {ex.Message}");
            }
        }


        /// <summary>
        /// Handles tab closing event
        /// </summary>
        private void TabControlMain_TabClosed(object sender, Infragistics.Win.UltraWinTabControl.TabClosedEventArgs e)
        {
            try
            {
                // Clean up any resources when a tab is closed
                if (e.Tab.TabPage.Controls.Count > 0 && e.Tab.TabPage.Controls[0] is Form)
                {
                    Form form = (Form)e.Tab.TabPage.Controls[0];
                    if (!form.IsDisposed)
                    {
                        try
                        {
                            // Attempt to unsubscribe generic handlers
                            form.FormClosed -= null;
                        }
                        catch { }
                        form.Close();
                        form.Dispose();
                    }
                    // Clear the tab page to drop references
                    e.Tab.TabPage.Controls.Clear();
                    e.Tab.TabPage.Dispose();
                }
                System.Diagnostics.Debug.WriteLine($"Tab closed: {e.Tab.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TabClosed event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles tab closing event (before closing)
        /// </summary>
        private void TabControlMain_TabClosing(object sender, Infragistics.Win.UltraWinTabControl.TabClosingEventArgs e)
        {
            try
            {
                // You can cancel the closing here if needed
                // e.Cancel = true;
                System.Diagnostics.Debug.WriteLine($"Tab closing: {e.Tab.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TabClosing event: {ex.Message}");
            }
        }








        // (test button click handlers removed)

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        // (removed unused barcode menu handler)

        // (removed unused POS menu handlers)

        // (removed obsolete ToolClick handler, using ultraToolbarsManager1_ToolClick_1)

        /// <summary>
        /// Applies role-based permissions to toolbar buttons.
        /// Disables buttons that the user does not have CanView permission for.
        /// </summary>
        private void ApplyRolePermissions()
        {
            try
            {
                // Get all tools from the toolbar manager
                foreach (Infragistics.Win.UltraWinToolbars.ToolBase tool in ultraToolbarsManager1.Tools)
                {
                    // Check if user has CanView permission for this tool
                    bool hasPermission = SessionContext.CanView(tool.Key);
                    tool.SharedProps.Enabled = hasPermission;

                    System.Diagnostics.Debug.WriteLine($"Tool '{tool.Key}': Enabled={hasPermission}");
                }

                System.Diagnostics.Debug.WriteLine($"Applied permissions to {ultraToolbarsManager1.Tools.Count} toolbar items");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying role permissions: {ex.Message}");
                // Continue without disabling buttons on error
            }
        }

        /// <summary>
        /// Checks if user has CanView permission for the specified form key.
        /// Shows access denied message if not permitted.
        /// </summary>
        /// <param name="formKey">Form key to check</param>
        /// <returns>True if permitted, false if denied</returns>
        private bool CheckViewPermission(string formKey)
        {
            if (!SessionContext.CanView(formKey))
            {
                MessageBox.Show($"You do not have permission to access this module.\n\nRequired permission: View access to '{formKey}'",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ultraToolbarsManager1_ToolClick_1(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            // Debug the clicked tool key
            System.Diagnostics.Debug.WriteLine($"Tool Clicked: {e.Tool.Key}");

            // Map aliases to permission keys
            string permissionKey = e.Tool.Key;
            if (e.Tool.Key == "Roles") permissionKey = "RolePermissions";

            // Check permission before opening any form
            if (!CheckViewPermission(permissionKey))
                return;

            if (e.Tool.Key == "Pos")
            {
                frmPos frmPos = new frmPos();
                OpenFormInTab(frmPos, "POS");
            }

            if (e.Tool.Key == "Sales")
            {
                // Prevent duplicate instances by using the safe opener
                frmSalesInvoice frmsales = new frmSalesInvoice();
                OpenFormInTabSafe(frmsales, "Sales Invoice");
            }
            if (e.Tool.Key == "Sales Return")
            {
                frmSalesReturn frmsalesreturn = new frmSalesReturn();
                OpenFormInTab(frmsalesreturn, "Sales Return");
            }
            if (e.Tool.Key == "Branch")
            {
                FrmBranch frmbranch = new FrmBranch();
                OpenFormInTab(frmbranch, "Branch");
            }
            if (e.Tool.Key == "Category")
            {
                Master.FrmCategory frmcategory = new Master.FrmCategory();
                OpenFormInTab(frmcategory, "Category");
            }
            if (e.Tool.Key == "Group")
            {
                Master.FrmGroup frmcategory = new Master.FrmGroup();
                OpenFormInTab(frmcategory, "Group");
            }
            if (e.Tool.Key == "ItemMaster")
            {
                // FrmItemMaster ItemMaster = new FrmItemMaster();
                frmItemMasterNew ItemMaster = new frmItemMasterNew();
                OpenFormInTab(ItemMaster, "Item Master");
            }
            if (e.Tool.Key == "Line")
            {
                frmLine line = new frmLine();
                OpenFormInTab(line, "Line");
            }
            if (e.Tool.Key == "Company")
            {
                frmCompany compa = new frmCompany();
                OpenFormInTab(compa, "Company");
            }
            if (e.Tool.Key == "Rack")
            {
                frmRack rack = new frmRack();
                OpenFormInTab(rack, "Rack");
            }
            if (e.Tool.Key == "Row")
            {
                FrmRow Row = new FrmRow();
                OpenFormInTab(Row, "Row");
            }
            if (e.Tool.Key == "State")
            {
                FrmState state = new FrmState();
                OpenFormInTab(state, "State");
            }
            if (e.Tool.Key == "Brand")
            {
                FrmBrand Brand = new FrmBrand();
                OpenFormInTab(Brand, "Brand");
            }
            if (e.Tool.Key == "Customer")
            {
                FrmCustomer Customer = new FrmCustomer();
                OpenFormInTab(Customer, "Customer");
            }
            if (e.Tool.Key == "Vendor")
            {
                Accounts.FrmVendor vendor = new FrmVendor();
                OpenFormInTab(vendor, "Vendor");
            }
            if (e.Tool.Key == "Ledger")
            {
                Accounts.FrmLedgers ledgers = new FrmLedgers();
                OpenFormInTab(ledgers, "Ledger");
            }
            if (e.Tool.Key == "AccountGroup")
            {
                Accounts.FrmAccountGroup AccountGroup = new FrmAccountGroup();
                OpenFormInTab(AccountGroup, "AccountGroup");
            }
            if (e.Tool.Key == "Receipt")
            {
                Accounts.FrmReceipt receipt = new FrmReceipt();
                OpenFormInTab(receipt, "Receipt");
            }
            if (e.Tool.Key == "Payment")
            {
                Accounts.FrmPayment payment = new FrmPayment();
                OpenFormInTab(payment, "Payment");
            }
            if (e.Tool.Key == "Contra")
            {
                Accounts.FrmContra contra = new FrmContra();
                OpenFormInTab(contra, "Contra");
            }
            if (e.Tool.Key == "Journal")
            {
                Accounts.FrmJournal journal = new FrmJournal();
                OpenFormInTab(journal, "Journal");
            }
            if (e.Tool.Key == "ChartOfAccount")
            {
                //ChartOfAccount.FrmChartOfAccount chartOfAcc = new ChartOfAccount.FrmChartOfAccount();
                ChartOfAccount.FrmChartOfAcc chartOfAcc = new ChartOfAccount.FrmChartOfAcc();
                OpenFormInTab(chartOfAcc, "ChartOfAccount");
            }
            if (e.Tool.Key == "DebitNote")
            {
                Accounts.FrmDebitNote debitNote = new FrmDebitNote();
                OpenFormInTab(debitNote, "DebitNote");
            }
            if (e.Tool.Key == "CreditNote")
            {
                Accounts.FrmCreditNote creditNote = new FrmCreditNote();
                OpenFormInTab(creditNote, "CreditNote");
            }
            if (e.Tool.Key == "Users")
            {
                FrmUsers users = new FrmUsers();
                OpenFormInTab(users, "Users");
            }
            if (e.Tool.Key == "Purchase")
            {
                FrmPurchase purchase = new FrmPurchase();
                OpenFormInTab(purchase, "Purchase");
            }
            if (e.Tool.Key == "Purchase R/n")
            {
                frmPurchaseReturn preturn = new frmPurchaseReturn();
                OpenFormInTab(preturn, "Purchase Return");
            }
            if (e.Tool.Key == "Country")
            {
                FrmCountry country = new FrmCountry();
                OpenFormInTab(country, "Country");
            }
            if (e.Tool.Key == "stockadjustment")
            {
                FrmStockAdjustment stokadj = new FrmStockAdjustment();
                OpenFormInTab(stokadj, "Stock Adjustment");
            }
            if (e.Tool.Key == "frmvendor")
            {
                FrmVendor vendor = new FrmVendor();
                OpenFormInTab(vendor, "Vendor");
            }
            if (e.Tool.Key == "Goods Received")
            {
                FrmPurchase purchase = new FrmPurchase();
                OpenFormInTab(purchase, "Goods Received");
            }

            // ADD THIS NEW CONDITION FOR PRINT BARCODE
            if (e.Tool.Key == "Print Barcode")
            {
                Utilities.frmBarcode barcodeForm = new Utilities.frmBarcode();
                OpenFormInTab(barcodeForm, "Print Barcode");
            }

            if (e.Tool.Key == "PLU Weighing")
            {
                Utilities.FrmPlu frmPlu = new Utilities.FrmPlu();
                OpenFormInTab(frmPlu, "PLU Weighing");
            }
            if (e.Tool.Key == "OpeningStock")
            {
                Utilities.frmOpeningStock frmOpnstk = new Utilities.frmOpeningStock();
                OpenFormInTab(frmOpnstk, "OpeningStock");
            }
            if (e.Tool.Key == "BtnClosing")
            {
                Utilities.frmClosing frmClosing = new Utilities.frmClosing();
                OpenFormInTab(frmClosing, "Closing");
            }


            if (e.Tool.Key == "Sales Details")
            {
                Reports.SalesReports.frmSalesReportMasterDetail frmSalesDetails = new Reports.SalesReports.frmSalesReportMasterDetail();
                OpenFormInTab(frmSalesDetails, "Sales Details");
            }
            if (e.Tool.Key == "SalesReturn")
            {
                Reports.SalesReports.SalesReturnReport frmSalesRetun = new Reports.SalesReports.SalesReturnReport();
                OpenFormInTab(frmSalesRetun, "SalesReturn");
            }
            if (e.Tool.Key == "Purchase Details")
            {

                Reports.PurchaseReports.frmPurchaseReportDetails frmPurchaseReport = new Reports.PurchaseReports.frmPurchaseReportDetails();
                OpenFormInTab(frmPurchaseReport, "Purchase Details");
            }
            if (e.Tool.Key == "PurchaseReturn")
            {

                Reports.PurchaseReports.PurchaseReturnReport frmPurchaseReturnReport = new Reports.PurchaseReports.PurchaseReturnReport();
                OpenFormInTab(frmPurchaseReturnReport, "PurchaseReturn");
            }
            if (e.Tool.Key == "ItemReport")
            {
                Reports.InventoryReport.frmItemReport frmitemRpt = new Reports.InventoryReport.frmItemReport();
                OpenFormInTab(frmitemRpt, "ItemReport");

            }
            if (e.Tool.Key == "StockReport")
            {
                Reports.InventoryReport.frmStockReportAdvanced frmStkRptAdv = new Reports.InventoryReport.frmStockReportAdvanced();
                OpenFormInTab(frmStkRptAdv, "StockReport");
            }

            // ADD THIS NEW CONDITION FOR UNIT MASTER
            if (e.Tool.Key == "UnitMaster")
            {
                Master.FrmUnitMaster unitMasterForm = new Master.FrmUnitMaster();
                OpenFormInTab(unitMasterForm, "Unit Master");
            }

            // ADD THIS NEW CONDITION FOR TAX MANAGEMENT
            if (e.Tool.Key == "TaxManagement")
            {
                Master.frmTaxManagement taxManagementForm = new Master.frmTaxManagement();
                if (taxManagementForm.ShowDialog() == DialogResult.OK)
                {
                    // Refresh tax status display after settings change
                    UpdateTaxStatusDisplay();
                }
            }
            #region here for reports sections menu
            if (e.Tool.Key == "DSales")
            {
                frmSales_RPT salesRptv = new frmSales_RPT();
                OpenFormInTab(salesRptv, "Daily Sales Report");
            }

            // POS Settings
            if (e.Tool.Key == "POSSettings" || e.Tool.Key == "Settings")
            {
                frmPOSSettings posSettings = new frmPOSSettings();
                OpenFormInTab(posSettings, "Sale Settings");
            }

            // Role Permissions (Admin only)
            // Role Permissions (Admin only)
            if (e.Tool.Key == "RolePermissions" || e.Tool.Key == "Roles")
            {
                FrmRolePermissions rolePerms = new FrmRolePermissions();
                OpenFormInTab(rolePerms, "Role Permissions");
            }

            #endregion
        }

        private void Home_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        /// <summary>
        /// Updates the tax status display in the status bar or form
        /// </summary>
        private void UpdateTaxStatusDisplay()
        {
            try
            {
                // You can add a status label or status bar item to show tax status
                // For now, we'll use debug output and could add a status bar item
                System.Diagnostics.Debug.WriteLine($"Tax Status: {DataBase.TaxToggleMessage}");

                // If you have a status bar, you could update it here:
                // statusStrip1.Items["taxStatus"].Text = DataBase.TaxToggleMessage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating tax status display: {ex.Message}");
            }
        }

        private void Home_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                //frmSalesInvoice frmsales = new frmSalesInvoice();
                //OpenFormInTab(frmsales, "Sales Invoice");
            }
            if (e.KeyCode == Keys.F5)
            {
                //Accounts.FrmCustomer ObjCustomer = new FrmCustomer();
                //OpenFormInTab(ObjCustomer, "Customer");
            }
            if (e.KeyCode == Keys.F6)
            {
                //Accounts.FrmVendor Objvendor = new FrmVendor();
                //OpenFormInTab(Objvendor, "Vendor");
            }

            if (e.KeyCode == Keys.ControlKey)
            {
                isCtrlPressed = true;
            }
            if (e.KeyCode == Keys.F2)
            {
                isF2Pressed = true;
            }
            if (isCtrlPressed && isF2Pressed)
            {

            }
        }



        /// <summary>
        /// Current application theme color
        /// </summary>
        private Color currentThemeColor = Color.FromArgb(240, 248, 255);

        /// <summary>
        /// Applies the selected color to the entire application like IRS POS
        /// </summary>

        private void ApplyThemeToControls(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Apply theme based on control type (like IRS POS - theme EVERYTHING!)
                    if (control is Panel panel && !panel.Name.Contains("color") && !panel.Name.Contains("language"))
                    {
                        panel.BackColor = themeColor; // Use exact color like IRS POS
                    }
                    else if (control is Button button && !button.Name.StartsWith("color") && !button.Name.StartsWith("colorButton"))
                    {
                        button.BackColor = Color.FromArgb(
                            Math.Min(255, themeColor.R + 20),
                            Math.Min(255, themeColor.G + 20),
                            Math.Min(255, themeColor.B + 20)
                        );
                    }
                    else if (control is TextBox || control is ComboBox)
                    {
                        control.BackColor = Color.FromArgb(
                            Math.Min(255, themeColor.R + 30),
                            Math.Min(255, themeColor.G + 30),
                            Math.Min(255, themeColor.B + 30)
                        );
                    }
                    else if (control is DataGridView grid)
                    {
                        grid.BackgroundColor = themeColor; // Use exact color like IRS POS
                        grid.DefaultCellStyle.BackColor = Color.FromArgb(
                            Math.Min(255, themeColor.R + 25),
                            Math.Min(255, themeColor.G + 25),
                            Math.Min(255, themeColor.B + 25)
                        );
                    }
                    else if (control is Label || control is GroupBox)
                    {
                        // Theme labels and group boxes too (like IRS POS)
                        control.BackColor = Color.FromArgb(
                            Math.Min(255, themeColor.R + 5),
                            Math.Min(255, themeColor.G + 5),
                            Math.Min(255, themeColor.B + 5)
                        );
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToControls(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to controls: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to UltraPanel controls and their child controls
        /// </summary>
        /// <param name="ultraPanel">UltraPanel to theme</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToUltraPanelControls(Infragistics.Win.Misc.UltraPanel ultraPanel, Color themeColor)
        {
            try
            {
                if (ultraPanel != null)
                {
                    // Apply theme to the UltraPanel itself
                    ultraPanel.Appearance.BackColor = themeColor;

                    // Apply theme to all controls in the UltraPanel's ClientArea
                    ApplyThemeToControls(ultraPanel.ClientArea.Controls, themeColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to UltraPanel: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to all open MDI child forms (Sales, Purchase, etc.)
        /// </summary>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToMDIChildren(Color themeColor)
        {
            try
            {
                foreach (Form childForm in this.MdiChildren)
                {
                    childForm.BackColor = Color.FromArgb(
                        Math.Min(255, themeColor.R + 5),
                        Math.Min(255, themeColor.G + 5),
                        Math.Min(255, themeColor.B + 5)
                    );

                    // Apply to all controls in the child form
                    ApplyThemeToControls(childForm.Controls, themeColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to MDI children: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies theme to all open tabbed forms (Sales, Purchase, etc.)
        /// </summary>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToTabbedForms(Color themeColor)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting to theme {tabControlMain.Tabs.Count} tabbed forms...");

                // Apply to all forms in tabs
                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControlMain.Tabs)
                {
                    System.Diagnostics.Debug.WriteLine($"Processing tab: {tab.Text}");

                    if (tab.TabPage.Controls.Count > 0 && tab.TabPage.Controls[0] is Form)
                    {
                        Form tabbedForm = (Form)tab.TabPage.Controls[0];
                        System.Diagnostics.Debug.WriteLine($"Found form: {tabbedForm.GetType().Name}");

                        // Use the comprehensive form theming method
                        ApplyThemeToForm(tabbedForm, themeColor);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Tab {tab.Text} has no form or {tab.TabPage.Controls.Count} controls");
                    }

                    // Also apply theme to the tab page itself
                    tab.TabPage.BackColor = Color.FromArgb(
                        Math.Min(255, themeColor.R + 10),
                        Math.Min(255, themeColor.G + 10),
                        Math.Min(255, themeColor.B + 10)
                    );
                }

                // Apply theme to the tab control itself
                tabControlMain.BackColor = themeColor;

                System.Diagnostics.Debug.WriteLine($"Applied theme to {tabControlMain.Tabs.Count} tabbed forms");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying theme to tabbed forms: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Applies comprehensive theming to any form, overriding hardcoded Designer colors
        /// </summary>
        /// <param name="form">Form to theme</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplyThemeToForm(Form form, Color themeColor)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== THEMING FORM: {form.GetType().Name} ===");

                // Apply theme color to the form background
                form.BackColor = Color.FromArgb(
                    Math.Min(255, themeColor.R + 5),
                    Math.Min(255, themeColor.G + 5),
                    Math.Min(255, themeColor.B + 5)
                );
                System.Diagnostics.Debug.WriteLine($"Set form BackColor to: {form.BackColor.Name}");

                // Apply comprehensive theming to all controls
                ApplyThemeToControls(form.Controls, themeColor);

                // Special handling for specific form types
                ApplySpecialFormTheming(form, themeColor);

                // Force refresh to show changes immediately
                form.Refresh();

                System.Diagnostics.Debug.WriteLine($"=== COMPLETED THEMING: {form.GetType().Name} ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming form {form.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies special theming for specific form types that need extra attention
        /// </summary>
        /// <param name="form">Form to apply special theming to</param>
        /// <param name="themeColor">Theme color to apply</param>
        private void ApplySpecialFormTheming(Form form, Color themeColor)
        {
            try
            {
                // Handle Infragistics UltraPanel controls (common in forms like frmBarcode)
                ApplyThemeToUltraPanels(form.Controls, themeColor);

                // Handle Infragistics UltraGrid controls
                ApplyThemeToUltraGrids(form.Controls, themeColor);

                // Handle Infragistics UltraButton controls
                ApplyThemeToUltraButtons(form.Controls, themeColor);

                // Handle Infragistics UltraLabel controls
                ApplyThemeToUltraLabels(form.Controls, themeColor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in special form theming: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraPanel controls
        /// </summary>
        private void ApplyThemeToUltraPanels(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraPanel controls
                    if (control.GetType().Name == "UltraPanel")
                    {
                        // Use reflection to set appearance properties
                        var appearanceProp = control.GetType().GetProperty("Appearance");
                        if (appearanceProp != null)
                        {
                            var appearance = appearanceProp.GetValue(control);
                            if (appearance != null)
                            {
                                var backColorProp = appearance.GetType().GetProperty("BackColor");
                                if (backColorProp != null)
                                {
                                    backColorProp.SetValue(appearance, themeColor);
                                    System.Diagnostics.Debug.WriteLine($"Set UltraPanel appearance BackColor: {control.Name}");
                                }
                            }
                        }

                        // Also set regular BackColor as fallback
                        control.BackColor = themeColor;
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraPanels(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraPanels: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraGrid controls
        /// </summary>
        private void ApplyThemeToUltraGrids(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraGrid controls
                    if (control.GetType().Name == "UltraGrid")
                    {
                        // Theme the grid background
                        control.BackColor = Color.FromArgb(
                            Math.Min(255, themeColor.R + 20),
                            Math.Min(255, themeColor.G + 20),
                            Math.Min(255, themeColor.B + 20)
                        );

                        // Use reflection to theme grid appearance properties
                        try
                        {
                            var displayLayoutProp = control.GetType().GetProperty("DisplayLayout");
                            if (displayLayoutProp != null)
                            {
                                var displayLayout = displayLayoutProp.GetValue(control);
                                if (displayLayout != null)
                                {
                                    // Theme header appearance
                                    var overrideProp = displayLayout.GetType().GetProperty("Override");
                                    if (overrideProp != null)
                                    {
                                        var overrideObj = overrideProp.GetValue(displayLayout);
                                        if (overrideObj != null)
                                        {
                                            var headerAppearanceProp = overrideObj.GetType().GetProperty("HeaderAppearance");
                                            if (headerAppearanceProp != null)
                                            {
                                                var headerAppearance = headerAppearanceProp.GetValue(overrideObj);
                                                if (headerAppearance != null)
                                                {
                                                    var backColorProp = headerAppearance.GetType().GetProperty("BackColor");
                                                    if (backColorProp != null)
                                                    {
                                                        backColorProp.SetValue(headerAppearance, themeColor);
                                                        System.Diagnostics.Debug.WriteLine($"Set UltraGrid header BackColor: {control.Name}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception gridEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error theming UltraGrid details: {gridEx.Message}");
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraGrids(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraGrids: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraButton controls (preserve functional colors)
        /// </summary>
        private void ApplyThemeToUltraButtons(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraButton controls - but preserve functional button colors
                    if (control.GetType().Name == "UltraButton")
                    {
                        // Only theme buttons that don't have special functional colors
                        // Skip buttons with names containing: print, delete, clear, save, cancel
                        string buttonName = control.Name?.ToLower() ?? "";
                        if (!buttonName.Contains("print") && !buttonName.Contains("delete") &&
                            !buttonName.Contains("clear") && !buttonName.Contains("save") &&
                            !buttonName.Contains("cancel"))
                        {
                            // Use reflection to set appearance properties for neutral buttons
                            var appearanceProp = control.GetType().GetProperty("Appearance");
                            if (appearanceProp != null)
                            {
                                var appearance = appearanceProp.GetValue(control);
                                if (appearance != null)
                                {
                                    var backColorProp = appearance.GetType().GetProperty("BackColor");
                                    if (backColorProp != null)
                                    {
                                        backColorProp.SetValue(appearance, Color.FromArgb(
                                            Math.Min(255, themeColor.R + 30),
                                            Math.Min(255, themeColor.G + 30),
                                            Math.Min(255, themeColor.B + 30)
                                        ));
                                        System.Diagnostics.Debug.WriteLine($"Set UltraButton appearance BackColor: {control.Name}");
                                    }
                                }
                            }
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraButtons(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraButtons: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively applies theming to Infragistics UltraLabel controls
        /// </summary>
        private void ApplyThemeToUltraLabels(Control.ControlCollection controls, Color themeColor)
        {
            try
            {
                foreach (Control control in controls)
                {
                    // Handle UltraLabel controls
                    if (control.GetType().Name == "UltraLabel")
                    {
                        // Only theme background labels, not text labels
                        if (control.BackColor != Color.Transparent)
                        {
                            control.BackColor = Color.FromArgb(
                                Math.Min(255, themeColor.R + 10),
                                Math.Min(255, themeColor.G + 10),
                                Math.Min(255, themeColor.B + 10)
                            );
                            System.Diagnostics.Debug.WriteLine($"Set UltraLabel BackColor: {control.Name}");
                        }
                    }

                    // Recursively apply to child controls
                    if (control.HasChildren)
                    {
                        ApplyThemeToUltraLabels(control.Controls, themeColor);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error theming UltraLabels: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies the selected language
        /// </summary>
        private void ApplyLanguage(string language)
        {
            try
            {
                // Here you would implement language switching logic
                System.Diagnostics.Debug.WriteLine($"Language changed to: {language}");
                MessageBox.Show($"Language changed to: {language}", "Language Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying language: {ex.Message}");
            }
        }
    }

}
