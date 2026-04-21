using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace RecoilessApp
{
    public class Profile
    {
        public string Name { get; set; }
        public decimal W1_Down { get; set; }
        public decimal W1_Right { get; set; }
        public decimal W1_Left { get; set; }

        public decimal W2_Down { get; set; }
        public decimal W2_Right { get; set; }
        public decimal W2_Left { get; set; }

        public int DelayMs { get; set; }

        public Profile() { Name = "Default"; DelayMs = 0; }
        
        public override string ToString()
        {
            return Name;
        }
    }

    public class GameLoadout
    {
        public string Name { get; set; }
        public List<Profile> Profiles { get; set; }

        public GameLoadout()
        {
            Name = "Global Game";
            Profiles = new List<Profile>();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class DarkListBox : ListBox
    {
        public DarkListBox()
        {
            this.DoubleBuffered = true;
            this.IntegralHeight = false;
        }
    }

    public class ProfileDatabase
    {
        public List<GameLoadout> Games { get; set; }
        public string LastActiveGame { get; set; }
        public string LastActiveProfile { get; set; }

        public int GlobalVariance { get; set; }
        public bool IsVarianceInit { get; set; }

        public ProfileDatabase()
        {
            Games = new List<GameLoadout>();
            LastActiveGame = "";
            LastActiveProfile = "";
            GlobalVariance = 40;
            IsVarianceInit = true;
        }
    }

    public class RecoilessForm : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
        const uint MOUSEEVENTF_MOVE = 0x0001;
        
        private string dbPath = "profiles.xml";
        private ProfileDatabase db;
        private GameLoadout activeGame;
        private Profile activeProfile;
        private int activeWeapon = 1; // 1 or 2

        // UI
        private TabControl tabCtrl;
        
        // Active preset tab
        private Label lblStatus;
        private Label lblActiveWeapon;
        private Label lblActiveProfile;
        
        private NumericUpDown nW1_D, nW1_R, nW1_L;
        private NumericUpDown nW2_D, nW2_R, nW2_L;
        private NumericUpDown nDelay;
        private NumericUpDown nVariance;
        
        // Database tab
        private ComboBox cbGames;
        private TextBox txtNewGame;
        private ListBox listProfiles;
        private TextBox txtNewName;

        private Thread bgThread;
        private bool isRunning = true;
        private bool isEnabled = false;
        private bool suppressSync = false;

        public RecoilessForm()
        {
            LoadDatabase();
            InitializeComponent();
            ApplyProfileToUI();
            
            bgThread = new Thread(BackgroundLoop);
            bgThread.IsBackground = true;
            bgThread.Start();
        }

        private void LoadDatabase()
        {
            if (File.Exists(dbPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ProfileDatabase));
                    using (FileStream fs = new FileStream(dbPath, FileMode.Open))
                    {
                        db = (ProfileDatabase)serializer.Deserialize(fs);
                        if (!db.IsVarianceInit) {
                            db.GlobalVariance = 40;
                            db.IsVarianceInit = true;
                        }
                    }
                }
                catch { db = new ProfileDatabase(); }
            }
            else
            {
                db = new ProfileDatabase();
            }

            if (db.Games == null) db.Games = new List<GameLoadout>();
            if (db.Games.Count == 0)
            {
                var defGame = new GameLoadout() { Name = "Global Game" };
                defGame.Profiles.Add(new Profile() { Name = "Default Loadout" });
                db.Games.Add(defGame);
            }

            activeGame = db.Games.Find(g => g.Name == db.LastActiveGame);
            if (activeGame == null) activeGame = db.Games[0];

            if (activeGame.Profiles == null) activeGame.Profiles = new List<Profile>();
            if (activeGame.Profiles.Count == 0) activeGame.Profiles.Add(new Profile() { Name = "Default Loadout" });

            // Prioritize "Default" or "Default Loadout" inside the active game
            activeProfile = activeGame.Profiles.Find(p => p.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Default Loadout", StringComparison.OrdinalIgnoreCase));
            
            // Fallback to last active if "Default" doesn't exist
            if (activeProfile == null) activeProfile = activeGame.Profiles.Find(p => p.Name == db.LastActiveProfile);
            
            // Final fallback
            if (activeProfile == null && activeGame.Profiles.Count > 0) activeProfile = activeGame.Profiles[0];
        }

        private void SaveDatabase()
        {
            try
            {
                if (activeGame != null) db.LastActiveGame = activeGame.Name;
                if (activeProfile != null) db.LastActiveProfile = activeProfile.Name;
                XmlSerializer serializer = new XmlSerializer(typeof(ProfileDatabase));
                using (FileStream fs = new FileStream(dbPath, FileMode.Create))
                {
                    serializer.Serialize(fs, db);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save profiles: " + ex.Message);
            }
        }

        private NumericUpDown CreateNum()
        {
            var num = new NumericUpDown() { Width = 60, DecimalPlaces = 2, Maximum = 100, Minimum = 0, Increment = 0.1M, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            num.ValueChanged += (s, e) => {
                if (suppressSync) return;
                if (activeProfile != null)
                {
                    activeProfile.W1_Down = nW1_D != null ? nW1_D.Value : 0;
                    activeProfile.W1_Right = nW1_R != null ? nW1_R.Value : 0;
                    activeProfile.W1_Left = nW1_L != null ? nW1_L.Value : 0;
                    activeProfile.W2_Down = nW2_D != null ? nW2_D.Value : 0;
                    activeProfile.W2_Right = nW2_R != null ? nW2_R.Value : 0;
                    activeProfile.W2_Left = nW2_L != null ? nW2_L.Value : 0;
                }
            };
            return num;
        }

        // -- Font Cache --
        private static readonly Font fTabReg   = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        private static readonly Font fTabBold  = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font fListBold = new Font("Segoe UI", 9f, FontStyle.Bold);

        // -- Color palette --
        private static readonly Color cBg       = Color.FromArgb(24, 26, 33);     // main background
        private static readonly Color cSurface  = Color.FromArgb(32, 35, 44);     // panels / tab pages
        private static readonly Color cTopBar   = Color.FromArgb(18, 20, 26);     // title bar
        private static readonly Color cInput    = Color.FromArgb(40, 43, 54);     // text boxes / lists
        private static readonly Color cBorder   = Color.FromArgb(55, 60, 75);     // subtle borders
        private static readonly Color cBtn      = Color.FromArgb(48, 52, 65);     // buttons
        private static readonly Color cBtnHover = Color.FromArgb(60, 65, 82);     // button hover (unused in code but nice ref)
        private static readonly Color cAccent   = Color.FromArgb(86, 140, 245);   // blue accent
        private static readonly Color cFg       = Color.FromArgb(220, 225, 235);  // primary text
        private static readonly Color cFgDim    = Color.FromArgb(140, 148, 168);  // secondary text

        private void InitializeComponent()
        {
            this.Text = "Recoiless - Accessibility App";
            this.ClientSize = new Size(384, 395);
            this.MinimumSize = new Size(384, 395);
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = cBg;
            this.ForeColor = cFg;
            this.DoubleBuffered = true;
            this.Padding = new Padding(1);

            // 1-pixel border around the whole form + resize grip
            this.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
                
                int gripS = 14; 
                int w = this.Width;
                int h = this.Height;
                using (Pen p = new Pen(cFgDim))
                {
                    e.Graphics.DrawLine(p, w - gripS, h - 3, w - 3, h - gripS);
                    e.Graphics.DrawLine(p, w - (gripS - 4), h - 3, w - 3, h - (gripS - 4));
                    e.Graphics.DrawLine(p, w - (gripS - 8), h - 3, w - 3, h - (gripS - 8));
                }
            };
            this.Resize += (s, e) => { this.Invalidate(); };

            // ========= TITLE BAR =========
            Panel topBar = new Panel() { Dock = DockStyle.Top, Width = this.Width, Height = 32, BackColor = cTopBar };
            topBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };

            Label lblTitle = new Label() { Name = "lblTitle", Text = "  RECOILESS", ForeColor = cAccent, Location = new Point(6, 8), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTitle.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            topBar.Controls.Add(lblTitle);

            Label lblWatermark = new Label() { Name = "lblWatermark", Text = "by nxucs", ForeColor = cFgDim, BackColor = cTopBar, Location = new Point(95, 10), AutoSize = true, Font = new Font("Segoe UI", 7.5f, FontStyle.Regular) };
            lblWatermark.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            topBar.Controls.Add(lblWatermark);
            lblWatermark.BringToFront();


            // Pin button (always-on-top toggle, added first so it docks leftmost)
            Button btnPin = new Button() { Text = "\uD83D\uDCCC", Width = 32, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, ForeColor = cFgDim, Font = new Font("Segoe UI", 8f) };
            btnPin.FlatAppearance.BorderSize = 0;
            btnPin.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 60, 75);
            btnPin.BackColor = cTopBar;
            btnPin.Click += (s, e) => {
                this.TopMost = !this.TopMost;
                btnPin.ForeColor = this.TopMost ? cAccent : cFgDim;
                btnPin.BackColor = this.TopMost ? Color.FromArgb(30, 35, 50) : cTopBar;
            };
            topBar.Controls.Add(btnPin);

            // Minimize button (added second so it docks left of Close)
            Button btnMin = new Button() { Text = "\u2500", Width = 38, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, ForeColor = cFgDim, Font = new Font("Segoe UI", 9f) };
            btnMin.FlatAppearance.BorderSize = 0;
            btnMin.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 60, 75);
            btnMin.BackColor = cTopBar;
            btnMin.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };
            topBar.Controls.Add(btnMin);

            // Close button (added last so it's rightmost with Dock.Right)
            Button btnClose = new Button() { Text = "\u2715", Width = 38, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, ForeColor = cFgDim, Font = new Font("Segoe UI", 9f) };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 50);
            btnClose.BackColor = cTopBar;
            btnClose.Click += (s, e) => { this.Close(); };
            topBar.Controls.Add(btnClose);

            this.Controls.Add(topBar);

            // ========= TAB CONTROL (owner-drawn) =========
            tabCtrl = new TabControl() { Location = new Point(1, 33), Size = new Size(382, 361) };
            tabCtrl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabCtrl.SizeMode = TabSizeMode.Fixed;
            tabCtrl.Multiline = true;
            tabCtrl.ItemSize = new Size((this.ClientSize.Width / 2) - 4, 28);
            tabCtrl.DrawItem += TabCtrl_DrawItem;
            
            // --- TAB 1: ACTIVE LOADOUT ---
            TabPage tabLoadout = new TabPage("Active Loadout");
            tabLoadout.BackColor = cSurface;
            tabLoadout.ForeColor = cFg;
            
            lblActiveProfile = new Label() { Text = "Profile: None", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = cFg };
            tabLoadout.Controls.Add(lblActiveProfile);

            lblActiveWeapon = new Label() { Text = "Active Weapon: 1", Location = new Point(200, 10), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = cAccent };
            tabLoadout.Controls.Add(lblActiveWeapon);

            Panel pnlW1 = new Panel() { Location = new Point(10, 40), Size = new Size(170, 140) };
            pnlW1.Paint += (s, e) => { 
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlW1.Width - 1, pnlW1.Height - 9); }
            };
            Label lblW1 = new Label() { Text = "Weapon 1 (Hotkey: 1)", Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlW1.Controls.Add(lblW1);

            pnlW1.Controls.Add(new Label() { Text = "Down:", Location = new Point(10, 25), AutoSize = true, ForeColor = cFg });
            nW1_D = CreateNum(); nW1_D.Location = new Point(80, 22); pnlW1.Controls.Add(nW1_D);
            
            pnlW1.Controls.Add(new Label() { Text = "Right:", Location = new Point(10, 55), AutoSize = true, ForeColor = cFg });
            nW1_R = CreateNum(); nW1_R.Location = new Point(80, 52); pnlW1.Controls.Add(nW1_R);

            pnlW1.Controls.Add(new Label() { Text = "Left:", Location = new Point(10, 85), AutoSize = true, ForeColor = cFg });
            nW1_L = CreateNum(); nW1_L.Location = new Point(80, 82); pnlW1.Controls.Add(nW1_L);
            tabLoadout.Controls.Add(pnlW1);

            Panel pnlW2 = new Panel() { Location = new Point(195, 40), Size = new Size(170, 140) };
            pnlW2.Paint += (s, e) => { 
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlW2.Width - 1, pnlW2.Height - 9); }
            };
            Label lblW2 = new Label() { Text = "Weapon 2 (Hotkey: 2)", Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlW2.Controls.Add(lblW2);

            pnlW2.Controls.Add(new Label() { Text = "Down:", Location = new Point(10, 25), AutoSize = true, ForeColor = cFg });
            nW2_D = CreateNum(); nW2_D.Location = new Point(80, 22); pnlW2.Controls.Add(nW2_D);
            
            pnlW2.Controls.Add(new Label() { Text = "Right:", Location = new Point(10, 55), AutoSize = true, ForeColor = cFg });
            nW2_R = CreateNum(); nW2_R.Location = new Point(80, 52); pnlW2.Controls.Add(nW2_R);

            pnlW2.Controls.Add(new Label() { Text = "Left:", Location = new Point(10, 85), AutoSize = true, ForeColor = cFg });
            nW2_L = CreateNum(); nW2_L.Location = new Point(80, 82); pnlW2.Controls.Add(nW2_L);
            tabLoadout.Controls.Add(pnlW2);

            Label lblDelay = new Label() { Text = "Delay (ms):", Location = new Point(10, 195), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            tabLoadout.Controls.Add(lblDelay);
            nDelay = new NumericUpDown() { Location = new Point(90, 192), Width = 70, Maximum = 5000, Minimum = 0, Increment = 10, DecimalPlaces = 0, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            nDelay.ValueChanged += (s, e) => {
                if (!suppressSync && activeProfile != null)
                    activeProfile.DelayMs = (int)nDelay.Value;
            };
            tabLoadout.Controls.Add(nDelay);

            Label lblVarTitle = new Label() { Text = "Variance % (Rec. 20+):", Location = new Point(170, 195), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            tabLoadout.Controls.Add(lblVarTitle);
            nVariance = new NumericUpDown() { Location = new Point(295, 192), Width = 70, Maximum = 100, Minimum = 0, Increment = 1, DecimalPlaces = 0, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            nVariance.ValueChanged += (s, e) => {
                if (!suppressSync && db != null) {
                    db.GlobalVariance = (int)nVariance.Value;
                    db.IsVarianceInit = true;
                }
            };
            tabLoadout.Controls.Add(nVariance);

            Button btnSave = StyleBtn("Save Changes to Current Profile", new Point(10, 228), 355, 32);
            btnSave.Click += BtnSave_Click;
            tabLoadout.Controls.Add(btnSave);

            lblStatus = new Label() { Text = "Status: DISABLED (Press F1 to toggle)", Location = new Point(10, 272), AutoSize = true, ForeColor = Color.FromArgb(240, 100, 100), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            tabLoadout.Controls.Add(lblStatus);

            // Resize handler for Active Loadout tab - center weapon panels + stretch save button
            tabLoadout.Resize += (s, e) => {
                int tw = tabLoadout.ClientSize.Width;
                // Center the two weapon panels as a group (total block width = 355)
                int blockW = 375;
                int offsetX = Math.Max(10, (tw - blockW) / 2);

                pnlW1.Location = new Point(offsetX, 40);
                pnlW2.Location = new Point(offsetX + 185, 40);

                lblActiveProfile.Location = new Point(offsetX, 10);
                lblActiveWeapon.Location = new Point(offsetX + 190, 10);

                lblDelay.Location = new Point(offsetX, 195);
                nDelay.Location = new Point(offsetX + 80, 192);
                lblVarTitle.Location = new Point(offsetX + 160, 195);
                nVariance.Location = new Point(offsetX + 285, 192);

                btnSave.Location = new Point(offsetX, 228);
                btnSave.Width = blockW;

                lblStatus.Location = new Point(offsetX, 272);
            };

            // --- TAB 2: DATABASE ---
            TabPage tabDB = new TabPage("Profiles Database");
            tabDB.BackColor = cSurface;
            tabDB.ForeColor = cFg;

            Label lblGame = new Label() { Text = "GAME:", Location = new Point(10, 13), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
            tabDB.Controls.Add(lblGame);
            
            cbGames = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(50, 10), Width = 120, FlatStyle = FlatStyle.Flat, BackColor = cInput, ForeColor = cFg };
            cbGames.SelectedIndexChanged += CbGames_SelectedIndexChanged;
            tabDB.Controls.Add(cbGames);
            
            txtNewGame = new TextBox() { Location = new Point(180, 10), Width = 100 };
            tabDB.Controls.Add(txtNewGame);

            Button btnAddGame = StyleBtn("+", new Point(285, 9), 25, 23);
            btnAddGame.Click += BtnAddGame_Click;
            tabDB.Controls.Add(btnAddGame);

            Button btnRenameGame = StyleBtn("R", new Point(315, 9), 25, 23);
            btnRenameGame.Click += BtnRenameGame_Click;
            tabDB.Controls.Add(btnRenameGame);

            Button btnDelGame = StyleBtn("\u2212", new Point(345, 9), 25, 23);
            btnDelGame.Click += BtnDelGame_Click;
            tabDB.Controls.Add(btnDelGame);

            listProfiles = new DarkListBox() { Location = new Point(10, 40), Size = new Size(355, 170), BorderStyle = BorderStyle.None };
            listProfiles.ItemHeight = 24;
            listProfiles.DrawMode = DrawMode.OwnerDrawFixed;
            listProfiles.MultiColumn = true;
            listProfiles.ColumnWidth = 170;
            listProfiles.DrawItem += ListProfiles_DrawItem;
            listProfiles.DoubleClick += ListProfiles_DoubleClick;
            tabDB.Controls.Add(listProfiles);

            txtNewName = new TextBox() { Location = new Point(10, 220), Width = 100 };
            tabDB.Controls.Add(txtNewName);

            Button btnAdd = StyleBtn("Add", new Point(115, 218), 50, 24);
            btnAdd.Click += BtnAdd_Click;
            tabDB.Controls.Add(btnAdd);

            Button btnCopy = StyleBtn("Copy", new Point(170, 218), 50, 24);
            btnCopy.Click += BtnCopy_Click;
            tabDB.Controls.Add(btnCopy);

            Button btnRename = StyleBtn("Rename", new Point(225, 218), 65, 24);
            btnRename.Click += BtnRename_Click;
            tabDB.Controls.Add(btnRename);

            Button btnDel = StyleBtn("Delete", new Point(295, 218), 70, 24);
            btnDel.Click += BtnDel_Click;
            tabDB.Controls.Add(btnDel);

            Button btnMakeActive = StyleBtn("Set Selected as Active Loadout", new Point(10, 252), 355, 30);
            btnMakeActive.Click += BtnMakeActive_Click;
            tabDB.Controls.Add(btnMakeActive);

            Button btnImport = StyleBtn("Import .ini Files as New Profiles", new Point(10, 288), 355, 30);
            btnImport.Click += BtnImport_Click;
            tabDB.Controls.Add(btnImport);

            // Manual resize handler for Profiles Database tab to guarantee layout
            tabDB.Resize += (s, e) => {
                int tw = tabDB.ClientSize.Width;
                int th = tabDB.ClientSize.Height;

                // Game row: label + combo fixed, text box fills, then 3 buttons pinned right
                int btnAreaW = 85; // 25*3 + 5*2 spacing
                btnDelGame.Location = new Point(tw - 30, 9);
                btnRenameGame.Location = new Point(tw - 60, 9);
                btnAddGame.Location = new Point(tw - 90, 9);
                int txtGameRight = tw - btnAreaW - 10;
                txtNewGame.Location = new Point(180, 10);
                txtNewGame.Width = Math.Max(40, txtGameRight - 180);

                // Bottom buttons layout
                int importY = th - 32;
                int activeY = importY - 36;
                int rowY = activeY - 30;

                btnImport.Location = new Point(10, importY);
                btnImport.Width = tw - 20;
                btnMakeActive.Location = new Point(10, activeY);
                btnMakeActive.Width = tw - 20;

                // Bottom row: txtNewName takes ~30%, then 4 buttons share the rest evenly
                int rowW = tw - 20;
                int nameW = (int)(rowW * 0.28);
                int btnW = (rowW - nameW - 12) / 4; // 12 = 3 gaps of 4px
                txtNewName.Location = new Point(10, rowY + 2);
                txtNewName.Width = nameW;
                int bx = 10 + nameW + 4;
                btnAdd.Location = new Point(bx, rowY);
                btnAdd.Width = btnW;
                bx += btnW + 4;
                btnCopy.Location = new Point(bx, rowY);
                btnCopy.Width = btnW;
                bx += btnW + 4;
                btnRename.Location = new Point(bx, rowY);
                btnRename.Width = btnW;
                bx += btnW + 4;
                btnDel.Location = new Point(bx, rowY);
                btnDel.Width = btnW;

                // List fills the middle
                int listBottom = rowY - 8;
                int listW = tw - 20;
                listProfiles.Size = new Size(listW, Math.Max(20, listBottom - 40));
                listProfiles.ColumnWidth = Math.Max(80, listW / 2);
            };

            tabCtrl.TabPages.Add(tabLoadout);
            tabCtrl.TabPages.Add(tabDB);

            this.Controls.Add(tabCtrl);

            // Only anchor the tab control itself to stretch with the form
            tabCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Dynamically resize tab headers to split 50/50
            this.Resize += (s2, e2) => {
                int halfW = Math.Max(50, (tabCtrl.Width / 2) - 4);
                tabCtrl.ItemSize = new Size(halfW, 28);
                tabCtrl.Invalidate();
            };

            RefreshGameList();
            ApplyDarkTheme(this.Controls);
        }

        private Button StyleBtn(string text, Point loc, int w, int h)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = loc;
            b.Size = new Size(w, h);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = cBorder;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = cBtnHover;
            b.BackColor = cBtn;
            b.ForeColor = cFg;
            b.Font = new Font("Segoe UI", 8f);
            return b;
        }

        private void TabCtrl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabCtrl.TabPages[e.Index];
            bool selected = (tabCtrl.SelectedIndex == e.Index);
            Color tabBg = selected ? cSurface : cTopBar;
            Color tabFg = selected ? cAccent : cFgDim;

            using (SolidBrush bgBrush = new SolidBrush(tabBg))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }
            
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            using (SolidBrush brush = new SolidBrush(tabFg))
            {
                e.Graphics.DrawString(page.Text, selected ? fTabBold : fTabReg, brush, e.Bounds, sf);
            }

            // Draw a small accent line under the selected tab
            if (selected)
            {
                using (Pen pen = new Pen(cAccent, 2))
                {
                    e.Graphics.DrawLine(pen, e.Bounds.Left + 4, e.Bounds.Bottom - 1, e.Bounds.Right - 4, e.Bounds.Bottom - 1);
                }
            }
        }

        private void ListProfiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Profile p = listProfiles.Items[e.Index] as Profile;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            
            Color bg = selected ? cBorder : cInput;
            Color fg = selected ? Color.White : cFg;
            
            using (SolidBrush brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            
            if (p != null)
            {
                using (SolidBrush textBrush = new SolidBrush(fg))
                {
                    e.Graphics.DrawString(p.Name, fListBold, textBrush, new Point(e.Bounds.Left + 5, e.Bounds.Top + ((e.Bounds.Height - 15) / 2)));
                }
            }
        }

        private void ApplyDarkTheme(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c is TabControl)
                {
                    c.BackColor = cBg;
                    c.ForeColor = cFg;
                }
                else if (c is TabPage)
                {
                    c.BackColor = cSurface;
                    c.ForeColor = cFg;
                }
                else if (c is GroupBox)
                {
                    c.BackColor = cSurface;
                    c.ForeColor = cFgDim;
                }
                else if (c is Label)
                {
                    // Don't override special labels
                    if (c != lblStatus && c != lblActiveWeapon && c != lblActiveProfile && c.Name != "lblTitle" && c.Name != "lblWatermark")
                    {
                        c.ForeColor = cFg;
                    }
                }
                else if (c is TextBox)
                {
                    c.BackColor = cInput;
                    c.ForeColor = cFg;
                    ((TextBox)c).BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is ListBox)
                {
                    c.BackColor = cInput;
                    c.ForeColor = cFg;
                }
                else if (c is ComboBox)
                {
                    c.BackColor = cInput;
                    c.ForeColor = cFg;
                }
                else if (c is NumericUpDown)
                {
                    c.BackColor = cInput;
                    c.ForeColor = cFg;
                }

                if (c.HasChildren)
                {
                    ApplyDarkTheme(c.Controls);
                }
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "INI Files (*.ini)|*.ini|All Files (*.*)|*.*";
            ofd.Title = "Import Recoil Configuration";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int importedCount = 0;
                try
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        string[] lines = File.ReadAllLines(fileName);
                        decimal iniDown = 0, iniRight = 0, iniLeft = 0;

                        foreach (string rawLine in lines)
                        {
                            string line = rawLine.Trim();
                            // Usually these headers exist in standard configs to denote specific scopes
                            if (line.StartsWith("[") || string.IsNullOrEmpty(line)) continue;

                            string[] parts = line.Split('=');
                            if (parts.Length != 2) continue;

                            string key = parts[0].Trim();
                            string val = parts[1].Trim();
                            decimal parsed;
                            if (!decimal.TryParse(val, out parsed)) continue;

                            if (key == "Down") iniDown = parsed;
                            else if (key == "Right") iniRight = parsed;
                            else if (key == "Left")
                            {
                                // Normalizing negative convention: Left=-0.5 means pull left
                                if (parsed < 0) parsed = Math.Abs(parsed);
                                iniLeft = parsed;
                            }
                        }

                        // Convert .ini values to our internal scale (~2.35 multiplier)
                        decimal iniScale = 2.35M;
                        iniDown = Math.Round(iniDown * iniScale, 2);
                        iniRight = Math.Round(iniRight * iniScale, 2);
                        iniLeft = Math.Round(iniLeft * iniScale, 2);

                        string profileName = Path.GetFileNameWithoutExtension(fileName);

                        Profile newProfile = new Profile();
                        newProfile.Name = profileName;
                        newProfile.W1_Down = iniDown;
                        newProfile.W1_Right = iniRight;
                        newProfile.W1_Left = iniLeft;

                        if (activeGame == null) activeGame = db.Games[0];
                        activeGame.Profiles.Add(newProfile);
                        importedCount++;
                    }

                    RefreshList();
                    SaveDatabase();

                    MessageBox.Show("Successfully imported " + importedCount + " profile(s).",
                        "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to import: " + ex.Message, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SilentSave()
        {
            if (activeProfile != null)
            {
                activeProfile.W1_Down = nW1_D.Value;
                activeProfile.W1_Right = nW1_R.Value;
                activeProfile.W1_Left = nW1_L.Value;
                
                activeProfile.W2_Down = nW2_D.Value;
                activeProfile.W2_Right = nW2_R.Value;
                activeProfile.W2_Left = nW2_L.Value;

                activeProfile.DelayMs = (int)nDelay.Value;
                
                SaveDatabase();
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SilentSave();
            MessageBox.Show("Saved settings for: " + activeProfile.Name, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshGameList()
        {
            cbGames.Items.Clear();
            foreach (var g in db.Games)
            {
                cbGames.Items.Add(g);
            }
            if (activeGame != null && db.Games.Contains(activeGame))
            {
                cbGames.SelectedItem = activeGame;
            }
            else if (cbGames.Items.Count > 0)
            {
                cbGames.SelectedIndex = 0;
            }
            RefreshList();
        }

        private void CbGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sel = cbGames.SelectedItem as GameLoadout;
            if (sel != null)
            {
                activeGame = sel;
                RefreshList();
            }
        }

        private void BtnAddGame_Click(object sender, EventArgs e)
        {
            string name = txtNewGame.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            var g = new GameLoadout() { Name = name };
            g.Profiles.Add(new Profile() { Name = "Default Loadout" });
            db.Games.Add(g);
            RefreshGameList();
            SaveDatabase();
            txtNewGame.Text = "";
            cbGames.SelectedItem = g;
        }

        private void BtnRenameGame_Click(object sender, EventArgs e)
        {
            string name = txtNewGame.Text.Trim();
            if (string.IsNullOrEmpty(name) || activeGame == null) return;
            activeGame.Name = name;
            RefreshGameList();
            SaveDatabase();
            txtNewGame.Text = "";
        }

        private void BtnDelGame_Click(object sender, EventArgs e)
        {
            if (activeGame != null && db.Games.Count > 1)
            {
                db.Games.Remove(activeGame);
                activeGame = db.Games[0];
                RefreshGameList();
                SaveDatabase();
            }
            else
            {
                MessageBox.Show("Cannot delete the last Game!");
            }
        }

        private void RefreshList()
        {
            listProfiles.Items.Clear();
            if (activeGame == null) return;
            
            // Sort profiles alphabetically, pinning "Default" and "Default Loadout" to the top
            activeGame.Profiles.Sort((a, b) => {
                bool aIsDefault = a.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || a.Name.Equals("Default Loadout", StringComparison.OrdinalIgnoreCase);
                bool bIsDefault = b.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || b.Name.Equals("Default Loadout", StringComparison.OrdinalIgnoreCase);

                if (aIsDefault && !bIsDefault) return -1;
                if (!aIsDefault && bIsDefault) return 1;
                
                return a.Name.CompareTo(b.Name);
            });

            foreach (var p in activeGame.Profiles)
            {
                listProfiles.Items.Add(p);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string name = txtNewName.Text.Trim();
            if (string.IsNullOrEmpty(name) || activeGame == null) return;
            
            activeGame.Profiles.Add(new Profile() { Name = name });
            RefreshList();
            SaveDatabase();
            txtNewName.Text = "";
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            Profile p = listProfiles.SelectedItem as Profile;
            if (p != null && activeGame != null)
            {
                if (activeGame.Profiles.Count > 1)
                {
                    activeGame.Profiles.Remove(p);
                    if (activeProfile == p) activeProfile = activeGame.Profiles[0];
                    RefreshList();
                    ApplyProfileToUI();
                    SaveDatabase();
                }
                else
                {
                    MessageBox.Show("Cannot delete the last profile in a Game!");
                }
            }
        }

        private void BtnRename_Click(object sender, EventArgs e)
        {
            string newName = txtNewName.Text.Trim();
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Please enter a new name in the text box below the list.");
                return;
            }

            Profile p = listProfiles.SelectedItem as Profile;
            if (p != null)
            {
                p.Name = newName;
                RefreshList();
                if (p == activeProfile) ApplyProfileToUI();
                SaveDatabase();
                txtNewName.Text = "";
            }
            else
            {
                MessageBox.Show("Please select a profile to rename from the list.");
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            Profile p = listProfiles.SelectedItem as Profile;
            if (p != null && activeGame != null)
            {
                Profile copy = new Profile();
                copy.Name = p.Name + " Copy";
                copy.W1_Down = p.W1_Down;
                copy.W1_Right = p.W1_Right;
                copy.W1_Left = p.W1_Left;
                copy.W2_Down = p.W2_Down;
                copy.W2_Right = p.W2_Right;
                copy.W2_Left = p.W2_Left;
                copy.DelayMs = p.DelayMs;

                activeGame.Profiles.Add(copy);
                RefreshList();
                SaveDatabase();
                
                // Select the newly copied profile
                listProfiles.SelectedItem = copy;
            }
            else
            {
                MessageBox.Show("Please select a profile to copy from the list.");
            }
        }

        private void BtnMakeActive_Click(object sender, EventArgs e)
        {
            ActivateSelectedProfile();
        }

        private void ListProfiles_DoubleClick(object sender, EventArgs e)
        {
            ActivateSelectedProfile();
        }

        private void ActivateSelectedProfile()
        {
            Profile p = listProfiles.SelectedItem as Profile;
            if (p != null)
            {
                activeProfile = p;
                ApplyProfileToUI();
                SaveDatabase();
                tabCtrl.SelectedIndex = 0;
            }
        }

        private void ApplyProfileToUI()
        {
            if (activeProfile != null)
            {
                suppressSync = true;
                try
                {
                    lblActiveProfile.Text = "Profile: " + activeProfile.Name;
                    
                    activeWeapon = 1;
                    if (lblActiveWeapon != null) lblActiveWeapon.Text = "Active Weapon: 1";
                    nW1_D.Value = activeProfile.W1_Down;
                    nW1_R.Value = activeProfile.W1_Right;
                    nW1_L.Value = activeProfile.W1_Left;
                    
                    nW2_D.Value = activeProfile.W2_Down;
                    nW2_R.Value = activeProfile.W2_Right;
                    nW2_L.Value = activeProfile.W2_Left;
                    nDelay.Value = activeProfile.DelayMs;
                    if (db != null && nVariance != null) nVariance.Value = db.GlobalVariance;
                }
                finally
                {
                    suppressSync = false;
                }
            }
        }

        private void BackgroundLoop()
        {
            bool f1WasDown = false;
            bool f2WasDown = false;
            
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            double lastTime = timer.Elapsed.TotalMilliseconds;
            Random rnd = new Random();
            
            double remX = 0;
            double remY = 0;
            double triggerStart = -1;

            // Humanization: smooth drift noise that wanders over time
            double driftX = 0;
            double driftY = 0;
            int tickCount = 0;

            while (isRunning)
            {
                Thread.Sleep(rnd.Next(7, 14)); 
                
                double currentTime = timer.Elapsed.TotalMilliseconds;
                double dt = currentTime - lastTime;
                lastTime = currentTime;

                bool f1IsDown = (GetAsyncKeyState(Keys.F1) < 0);
                if (f1IsDown && !f1WasDown)
                {
                    isEnabled = !isEnabled;
                    this.BeginInvoke(new Action(() => {
                        if (isEnabled)
                        {
                            lblStatus.Text = "Status: ACTIVE (Hold LClick + RClick)";
                            lblStatus.ForeColor = Color.FromArgb(100, 220, 100);
                        }
                        else
                        {
                            lblStatus.Text = "Status: DISABLED (Press F1 to toggle)";
                            lblStatus.ForeColor = Color.FromArgb(240, 100, 100);
                        }
                    }));
                }
                f1WasDown = f1IsDown;
                
                bool f2IsDown = (GetAsyncKeyState(Keys.F2) < 0);
                if (f2IsDown && !f2WasDown)
                {
                    this.BeginInvoke(new Action(() => {
                        SilentSave();
                    }));
                }
                f2WasDown = f2IsDown;

                if (!isEnabled) continue;

                // Monitor 1 and 2 keys for weapon switching
                // Only process hotkeys if our app is NOT the active foreground window
                if (GetForegroundWindow() != this.Handle)
                {
                    if (GetAsyncKeyState(Keys.D1) < 0 || GetAsyncKeyState(Keys.NumPad1) < 0)
                    {
                        if (activeWeapon != 1)
                        {
                            activeWeapon = 1;
                            this.BeginInvoke(new Action(() => { lblActiveWeapon.Text = "Active Weapon: 1"; }));
                        }
                    }
                    else if (GetAsyncKeyState(Keys.D2) < 0 || GetAsyncKeyState(Keys.NumPad2) < 0)
                    {
                        if (activeWeapon != 2)
                        {
                            activeWeapon = 2;
                            this.BeginInvoke(new Action(() => { lblActiveWeapon.Text = "Active Weapon: 2"; }));
                        }
                    }
                }

                bool lmbDown = (GetAsyncKeyState(Keys.LButton) < 0);
                bool rmbDown = (GetAsyncKeyState(Keys.RButton) < 0);

                if (lmbDown && rmbDown)
                {
                    // Delay logic: wait DelayMs after both buttons are first pressed
                    if (triggerStart < 0) triggerStart = currentTime;
                    int delayMs = activeProfile != null ? activeProfile.DelayMs : 0;
                    if ((currentTime - triggerStart) < delayMs) continue;

                    decimal pullD = 0, pullR = 0, pullL = 0;
                    
                    // Safely grab memory representations
                    if (activeProfile != null)
                    {
                        if (activeWeapon == 1)
                        {
                           pullD = activeProfile.W1_Down;
                           pullR = activeProfile.W1_Right;
                           pullL = activeProfile.W1_Left;
                        }
                        else
                        {
                           pullD = activeProfile.W2_Down;
                           pullR = activeProfile.W2_Right;
                           pullL = activeProfile.W2_Left;
                        }
                    }

                    tickCount++;

                    // Tightened base scale: value 30 ≈ ~857 px/sec
                    double scaleX = (double)(pullR - pullL) * (dt / 35.0);
                    double scaleY = (double)pullD * (dt / 35.0);

                    double varScale = db != null ? (db.GlobalVariance / 100.0) : 0.40;

                    // --- LAYER 1: Smooth organic absolute drift ---
                    // Drift wandering absolute pixels per tick
                    if (tickCount % (rnd.Next(30, 80)) == 0)
                    {
                        driftX = (rnd.NextDouble() * 0.10 - 0.05) * varScale; // ±0.05 absolute pixels/tick horizontally
                        driftY = (rnd.NextDouble() * 0.30 - 0.15) * varScale; // ±0.15 absolute pixels/tick vertically
                    }

                    // --- LAYER 2: High frequency absolute jitter ---
                    double staticNoiseX = (rnd.NextDouble() * 0.10 - 0.05) * varScale;
                    double staticNoiseY = (rnd.NextDouble() * 0.20 - 0.10) * varScale;

                    // --- LAYER 3: Micro-pause (~2% chance per tick) ---
                    if (rnd.Next(100) < (2.0 * varScale)) continue;

                    // Standardize the absolute noise to the actual elapsed dt to keep it perfectly uniform under lag spikes
                    double timeRatio = (dt / 10.0); // 10ms expected standard loop
                    double deltaX = (driftX + staticNoiseX) * timeRatio;
                    double deltaY = (driftY + staticNoiseY) * timeRatio;

                    double moveX = scaleX + deltaX;
                    double moveY = scaleY + deltaY;

                    remX += moveX;
                    remY += moveY;

                    int dx = (int)Math.Truncate(remX);
                    int dy = (int)Math.Truncate(remY);

                    remX -= dx;
                    remY -= dy;

                    if (dx != 0 || dy != 0)
                    {
                        mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, 0);
                    }
                }
                else
                {
                    remX = 0;
                    remY = 0;
                    triggerStart = -1;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            if (bgThread != null && bgThread.IsAlive)
            {
                bgThread.Join(500);
            }
            SaveDatabase(); // Final save
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84) // WM_NCHITTEST
            {
                int resizeArea = 12;
                Point cursor = this.PointToClient(Cursor.Position);
                if (cursor.X >= this.ClientSize.Width - resizeArea && cursor.Y >= this.ClientSize.Height - resizeArea)
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                else if (cursor.X >= this.ClientSize.Width - resizeArea)
                    m.Result = (IntPtr)11; // HTRIGHT
                else if (cursor.Y >= this.ClientSize.Height - resizeArea)
                    m.Result = (IntPtr)15; // HTBOTTOM
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RecoilessForm());
        }
    }
}
