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
        public string HotkeyCombo { get; set; }
        public bool IsStagesInit { get; set; }
        public bool IsStageEnableInit { get; set; }
        public int Stage1DelayMs { get; set; }
        public int Stage2DelayMs { get; set; }
        public int Stage3DelayMs { get; set; }
        public int Stage4DelayMs { get; set; }
        public bool Stage2Enabled { get; set; }
        public bool Stage3Enabled { get; set; }
        public bool Stage4Enabled { get; set; }
        public decimal S2_W1_Down { get; set; }
        public decimal S2_W1_Right { get; set; }
        public decimal S2_W1_Left { get; set; }
        public decimal S2_W2_Down { get; set; }
        public decimal S2_W2_Right { get; set; }
        public decimal S2_W2_Left { get; set; }
        public decimal S3_W1_Down { get; set; }
        public decimal S3_W1_Right { get; set; }
        public decimal S3_W1_Left { get; set; }
        public decimal S3_W2_Down { get; set; }
        public decimal S3_W2_Right { get; set; }
        public decimal S3_W2_Left { get; set; }
        public decimal S4_W1_Down { get; set; }
        public decimal S4_W1_Right { get; set; }
        public decimal S4_W1_Left { get; set; }
        public decimal S4_W2_Down { get; set; }
        public decimal S4_W2_Right { get; set; }
        public decimal S4_W2_Left { get; set; }

        public Profile()
        {
            Name = "Default";
            DelayMs = 0;
            HotkeyCombo = "";
            IsStagesInit = true;
            IsStageEnableInit = true;
            Stage1DelayMs = 2;
            Stage2DelayMs = 300;
            Stage3DelayMs = 600;
            Stage4DelayMs = 2500;
            Stage2Enabled = false;
            Stage3Enabled = false;
            Stage4Enabled = true;
        }
        
        public override string ToString()
        {
            return Name;
        }
    }

    public class GameLoadout
    {
        public string Name { get; set; }
        public List<Profile> Profiles { get; set; }
        public int GlobalVariance { get; set; }
        public bool IsVarianceInit { get; set; }
        public int GlobalDelayMs { get; set; }
        public bool IsDelayInit { get; set; }
        public string TempDisableKey { get; set; }
        public bool LeftClickOnlyMode { get; set; }
        public bool ShowRecoilTimer { get; set; }
        public int RecoilTimerSizePercent { get; set; }

        public GameLoadout()
        {
            Name = "Global Game";
            Profiles = new List<Profile>();
            GlobalVariance = 40;
            IsVarianceInit = true;
            GlobalDelayMs = 0;
            IsDelayInit = false;
            TempDisableKey = "Q";
            LeftClickOnlyMode = false;
            ShowRecoilTimer = false;
            RecoilTimerSizePercent = 100;
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

    public class RecoilTimerOverlay : Form
    {
        private const int BaseWidth = 170;
        private const int BaseHeight = 46;
        private const float BaseFontSize = 12f;
        private Label lblTimer;
        private int currentSizePercent = 100;

        public RecoilTimerOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(BaseWidth, BaseHeight);
            this.BackColor = Color.FromArgb(18, 20, 26);
            this.Opacity = 0.88;

            lblTimer = new Label();
            lblTimer.Dock = DockStyle.Fill;
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            lblTimer.ForeColor = Color.FromArgb(220, 225, 235);
            lblTimer.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTimer.Text = "0 ms | Stage 1";
            this.Controls.Add(lblTimer);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return cp;
            }
        }

        public void SetTimerText(string text)
        {
            lblTimer.Text = text;
        }

        public void SetTimerSize(int sizePercent)
        {
            int clamped = Math.Max(50, Math.Min(300, sizePercent));
            if (clamped == currentSizePercent) return;

            currentSizePercent = clamped;
            double scale = clamped / 100.0;
            this.Size = new Size(Math.Max(90, (int)Math.Round(BaseWidth * scale)), Math.Max(28, (int)Math.Round(BaseHeight * scale)));
            lblTimer.Font = new Font("Segoe UI", Math.Max(7f, BaseFontSize * (float)scale), FontStyle.Bold);
        }
    }

    public class ProfileDatabase
    {
        public List<GameLoadout> Games { get; set; }
        public string LastActiveGame { get; set; }
        public string LastActiveProfile { get; set; }

        public int GlobalVariance { get; set; }
        public bool IsVarianceInit { get; set; }
        public int GlobalDelayMs { get; set; }
        public bool IsDelayInit { get; set; }
        public string TempDisableKey { get; set; }
        public bool LeftClickOnlyMode { get; set; }
        public bool ShowRecoilTimer { get; set; }
        public int RecoilTimerSizePercent { get; set; }

        public ProfileDatabase()
        {
            Games = new List<GameLoadout>();
            LastActiveGame = "";
            LastActiveProfile = "";
            GlobalVariance = 40;
            IsVarianceInit = true;
            GlobalDelayMs = 0;
            IsDelayInit = false;
            TempDisableKey = "Q";
            LeftClickOnlyMode = false;
            ShowRecoilTimer = false;
            RecoilTimerSizePercent = 100;
        }
    }

    public class RecoilessForm : Form
    {
        [DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        static extern short GetAsyncKeyStateNative(int vKey);

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

        [Flags]
        private enum HotkeyModifierFlags
        {
            None = 0,
            Ctrl = 1,
            Alt = 2,
            Shift = 4
        }
        
        private readonly string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.xml");
        private ProfileDatabase db;
        private GameLoadout activeGame;
        private Profile activeProfile;
        private int activeWeapon = 1; // 1 or 2

        // UI
        private TabControl tabCtrl;
        
        // Active preset tab
        private Label lblStatus;
        private Panel pnlStatusDot;
        private Label lblActiveWeapon;
        private Label lblActiveProfile;
        private Panel pnlTopProfileStatus;
        private Panel pnlTopStatusDot;
        private Label lblTopProfileStatus;
        
        private NumericUpDown nW1_D, nW1_R, nW1_L;
        private NumericUpDown nW2_D, nW2_R, nW2_L;
        private NumericUpDown nDelay;
        private NumericUpDown[] nStageDelay = new NumericUpDown[4];
        private NumericUpDown[,] nStageW1 = new NumericUpDown[4, 3];
        private NumericUpDown[,] nStageW2 = new NumericUpDown[4, 3];
        private CheckBox[] chkStageEnabled = new CheckBox[4];
        private NumericUpDown nVariance;
        private NumericUpDown nTimerSize;
        private ComboBox cbTempDisableKey;
        private CheckBox chkLeftClickOnlyMode;
        private CheckBox chkShowRecoilTimer;
        
        // Database tab
        private ComboBox cbGames;
        private TextBox txtNewGame;
        private ListBox listProfiles;
        private TextBox txtNewName;
        private ComboBox cbHotkeyProfile;
        private ComboBox cbHotkeyModifier;
        private ComboBox cbHotkeyKey;
        private Label lblHotkeyCurrent;

        private Thread bgThread;
        private bool isRunning = true;
        private bool isEnabled = false;
        private bool isTemporarilyDisabled = false;
        private bool suppressSync = false;
        private Color statusDotColor = Color.FromArgb(240, 100, 100);
        private RecoilTimerOverlay recoilTimerOverlay;
        private bool queuedTimerOverlayVisible = false;
        private string queuedTimerOverlayText = "";
        private double lastTimerOverlayUpdate = -1000;

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

            foreach (var game in db.Games)
            {
                Profile seedProfile = null;
                if (game == activeGame) seedProfile = activeProfile;
                if (seedProfile == null && game.Profiles != null && game.Profiles.Count > 0) seedProfile = game.Profiles[0];
                EnsureGameGlobals(game, seedProfile);
                if (game.Profiles != null)
                {
                    foreach (Profile p in game.Profiles)
                    {
                        EnsureProfileStages(p, game);
                    }
                }
            }
        }

        private void EnsureGameGlobals(GameLoadout game, Profile seedProfile)
        {
            if (game == null) return;

            if (!game.IsVarianceInit)
            {
                game.GlobalVariance = db != null && db.IsVarianceInit ? db.GlobalVariance : 40;
                game.IsVarianceInit = true;
            }

            if (!game.IsDelayInit)
            {
                if (db != null && db.IsDelayInit)
                    game.GlobalDelayMs = db.GlobalDelayMs;
                else
                    game.GlobalDelayMs = seedProfile != null ? seedProfile.DelayMs : 0;

                game.IsDelayInit = true;
            }

            if (string.IsNullOrEmpty(game.TempDisableKey))
                game.TempDisableKey = db != null && !string.IsNullOrEmpty(db.TempDisableKey) ? db.TempDisableKey : "Q";

            if (game.RecoilTimerSizePercent < 50 || game.RecoilTimerSizePercent > 300)
                game.RecoilTimerSizePercent = db != null && db.RecoilTimerSizePercent >= 50 && db.RecoilTimerSizePercent <= 300 ? db.RecoilTimerSizePercent : 100;
        }

        private void EnsureProfileStages(Profile profile, GameLoadout game)
        {
            if (profile == null) return;

            if (!profile.IsStagesInit)
            {
                int migratedDelay = profile.DelayMs;
                if (migratedDelay == 0 && game != null && game.IsDelayInit)
                    migratedDelay = game.GlobalDelayMs;

                profile.Stage1DelayMs = migratedDelay > 0 ? migratedDelay : 2;
                profile.Stage2DelayMs = 300;
                profile.Stage3DelayMs = 600;
                profile.Stage4DelayMs = 2500;
                profile.IsStagesInit = true;
            }

            if (profile.Stage4DelayMs <= 0)
                profile.Stage4DelayMs = 2500;

            if (profile.Stage1DelayMs == 0 && profile.Stage2DelayMs == 30 && profile.Stage3DelayMs == 60 &&
                !StageHasRecoil(profile, 2, 1) && !StageHasRecoil(profile, 2, 2) &&
                !StageHasRecoil(profile, 3, 1) && !StageHasRecoil(profile, 3, 2))
            {
                profile.Stage1DelayMs = 2;
                profile.Stage2DelayMs = 300;
                profile.Stage3DelayMs = 600;
                profile.Stage4DelayMs = 2500;
            }

            if (!profile.IsStageEnableInit)
            {
                profile.Stage2Enabled = StageHasRecoil(profile, 2, 1) || StageHasRecoil(profile, 2, 2);
                profile.Stage3Enabled = StageHasRecoil(profile, 3, 1) || StageHasRecoil(profile, 3, 2);
                profile.Stage4Enabled = true;
                profile.IsStageEnableInit = true;
            }
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
                SyncActiveProfileFromUI();
            };
            return num;
        }

        private NumericUpDown CreateDelayNum()
        {
            var num = new NumericUpDown() { Width = 70, Maximum = 5000, Minimum = 0, Increment = 30, DecimalPlaces = 0, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            num.ValueChanged += (s, e) => {
                if (suppressSync) return;
                SyncActiveProfileFromUI();
            };
            return num;
        }

        private void SyncActiveProfileFromUI()
        {
            if (activeProfile == null) return;

            activeProfile.W1_Down = nW1_D != null ? nW1_D.Value : 0;
            activeProfile.W1_Right = nW1_R != null ? nW1_R.Value : 0;
            activeProfile.W1_Left = nW1_L != null ? nW1_L.Value : 0;
            activeProfile.W2_Down = nW2_D != null ? nW2_D.Value : 0;
            activeProfile.W2_Right = nW2_R != null ? nW2_R.Value : 0;
            activeProfile.W2_Left = nW2_L != null ? nW2_L.Value : 0;

            if (nStageDelay[0] != null) activeProfile.Stage1DelayMs = (int)nStageDelay[0].Value;
            if (nStageDelay[1] != null) activeProfile.Stage2DelayMs = (int)nStageDelay[1].Value;
            if (nStageDelay[2] != null) activeProfile.Stage3DelayMs = (int)nStageDelay[2].Value;
            if (nStageDelay[3] != null) activeProfile.Stage4DelayMs = (int)nStageDelay[3].Value;
            if (chkStageEnabled[1] != null) activeProfile.Stage2Enabled = chkStageEnabled[1].Checked;
            if (chkStageEnabled[2] != null) activeProfile.Stage3Enabled = chkStageEnabled[2].Checked;
            if (chkStageEnabled[3] != null) activeProfile.Stage4Enabled = chkStageEnabled[3].Checked;

            if (nStageW1[1, 0] != null) activeProfile.S2_W1_Down = nStageW1[1, 0].Value;
            if (nStageW1[1, 1] != null) activeProfile.S2_W1_Right = nStageW1[1, 1].Value;
            if (nStageW1[1, 2] != null) activeProfile.S2_W1_Left = nStageW1[1, 2].Value;
            if (nStageW2[1, 0] != null) activeProfile.S2_W2_Down = nStageW2[1, 0].Value;
            if (nStageW2[1, 1] != null) activeProfile.S2_W2_Right = nStageW2[1, 1].Value;
            if (nStageW2[1, 2] != null) activeProfile.S2_W2_Left = nStageW2[1, 2].Value;

            if (nStageW1[2, 0] != null) activeProfile.S3_W1_Down = nStageW1[2, 0].Value;
            if (nStageW1[2, 1] != null) activeProfile.S3_W1_Right = nStageW1[2, 1].Value;
            if (nStageW1[2, 2] != null) activeProfile.S3_W1_Left = nStageW1[2, 2].Value;
            if (nStageW2[2, 0] != null) activeProfile.S3_W2_Down = nStageW2[2, 0].Value;
            if (nStageW2[2, 1] != null) activeProfile.S3_W2_Right = nStageW2[2, 1].Value;
            if (nStageW2[2, 2] != null) activeProfile.S3_W2_Left = nStageW2[2, 2].Value;

            if (nStageW1[3, 0] != null) activeProfile.S4_W1_Down = nStageW1[3, 0].Value;
            if (nStageW1[3, 1] != null) activeProfile.S4_W1_Right = nStageW1[3, 1].Value;
            if (nStageW1[3, 2] != null) activeProfile.S4_W1_Left = nStageW1[3, 2].Value;
            if (nStageW2[3, 0] != null) activeProfile.S4_W2_Down = nStageW2[3, 0].Value;
            if (nStageW2[3, 1] != null) activeProfile.S4_W2_Right = nStageW2[3, 1].Value;
            if (nStageW2[3, 2] != null) activeProfile.S4_W2_Left = nStageW2[3, 2].Value;
        }

        private Panel CreateStagePanel(int stageIndex, int y)
        {
            Panel pnlStage = new Panel() { Location = new Point(10, y), Size = new Size(355, 172) };
            pnlStage.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlStage.Width - 1, pnlStage.Height - 9); }
            };

            Label lblStage = new Label() { Text = "Stage " + stageIndex, Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlStage.Controls.Add(lblStage);

            Label lblDelay = new Label() { Text = "Delay (ms):", Location = new Point(12, 25), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlStage.Controls.Add(lblDelay);
            NumericUpDown delay = CreateDelayNum();
            delay.Location = new Point(84, 22);
            nStageDelay[stageIndex - 1] = delay;
            if (stageIndex == 1) nDelay = delay;
            pnlStage.Controls.Add(delay);

            if (stageIndex == 1)
            {
                Label lblAlwaysOn = new Label() { Text = "Always on", Location = new Point(180, 25), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f) };
                pnlStage.Controls.Add(lblAlwaysOn);
            }
            else
            {
                CheckBox chkEnabled = new CheckBox() { Text = "Enabled", Location = new Point(180, 23), AutoSize = true, ForeColor = cFg, BackColor = cSurface, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f) };
                chkEnabled.CheckedChanged += (s, e) => {
                    if (!suppressSync) SyncActiveProfileFromUI();
                };
                chkStageEnabled[stageIndex - 1] = chkEnabled;
                pnlStage.Controls.Add(chkEnabled);
            }

            Panel pnlW1 = CreateStageWeaponPanel(stageIndex, 1, new Point(10, 54));
            Panel pnlW2 = CreateStageWeaponPanel(stageIndex, 2, new Point(185, 54));
            pnlStage.Controls.Add(pnlW1);
            pnlStage.Controls.Add(pnlW2);

            return pnlStage;
        }

        private Panel CreateStageWeaponPanel(int stageIndex, int weapon, Point location)
        {
            Panel pnlWeapon = new Panel() { Location = location, Size = new Size(160, 106) };
            pnlWeapon.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlWeapon.Width - 1, pnlWeapon.Height - 9); }
            };

            Label lblWeapon = new Label() { Text = "Weapon " + weapon, Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlWeapon.Controls.Add(lblWeapon);

            string[] labels = new string[] { "Down:", "Right:", "Left:" };
            for (int axis = 0; axis < 3; axis++)
            {
                pnlWeapon.Controls.Add(new Label() { Text = labels[axis], Location = new Point(10, 24 + (axis * 26)), AutoSize = true, ForeColor = cFg });
                NumericUpDown num = CreateNum();
                num.Location = new Point(75, 21 + (axis * 26));
                if (stageIndex > 1)
                {
                    num.ValueChanged += (s, e) => {
                        if (!suppressSync && num.Value != 0 && chkStageEnabled[stageIndex - 1] != null)
                            chkStageEnabled[stageIndex - 1].Checked = true;
                    };
                }
                pnlWeapon.Controls.Add(num);

                if (weapon == 1)
                    nStageW1[stageIndex - 1, axis] = num;
                else
                    nStageW2[stageIndex - 1, axis] = num;

                if (stageIndex == 1 && weapon == 1)
                {
                    if (axis == 0) nW1_D = num;
                    else if (axis == 1) nW1_R = num;
                    else nW1_L = num;
                }
                else if (stageIndex == 1 && weapon == 2)
                {
                    if (axis == 0) nW2_D = num;
                    else if (axis == 1) nW2_R = num;
                    else nW2_L = num;
                }
            }

            return pnlWeapon;
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
            this.ClientSize = new Size(414, 1020);
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

            pnlTopProfileStatus = new Panel() { Location = new Point(152, 5), Size = new Size(Math.Max(90, this.ClientSize.Width - 268), 22), BackColor = Color.FromArgb(24, 26, 33), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            pnlTopProfileStatus.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            pnlTopStatusDot = new Panel() { Location = new Point(7, 7), Size = new Size(8, 8), BackColor = pnlTopProfileStatus.BackColor };
            pnlTopStatusDot.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(statusDotColor))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, pnlTopStatusDot.Width - 1, pnlTopStatusDot.Height - 1);
                }
            };
            pnlTopProfileStatus.Controls.Add(pnlTopStatusDot);

            lblTopProfileStatus = new Label() { Name = "lblTopProfileStatus", Text = "No profile | OFF", Location = new Point(20, 2), Size = new Size(Math.Max(50, pnlTopProfileStatus.Width - 24), 18), AutoSize = false, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, ForeColor = cFg, BackColor = pnlTopProfileStatus.BackColor, Font = new Font("Segoe UI", 8f, FontStyle.Bold) };
            lblTopProfileStatus.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
            pnlTopProfileStatus.Controls.Add(lblTopProfileStatus);
            pnlTopProfileStatus.Resize += (s, e) => {
                if (lblTopProfileStatus != null)
                    lblTopProfileStatus.Width = Math.Max(50, pnlTopProfileStatus.Width - 24);
            };
            topBar.Resize += (s, e) => {
                int rightControlWidth = btnPin.Width + btnMin.Width + btnClose.Width;
                pnlTopProfileStatus.Width = Math.Max(70, topBar.ClientSize.Width - pnlTopProfileStatus.Left - rightControlWidth - 8);
            };
            topBar.Controls.Add(pnlTopProfileStatus);

            this.Controls.Add(topBar);

            // ========= TAB CONTROL (owner-drawn) =========
            tabCtrl = new TabControl() { Location = new Point(1, 33), Size = new Size(this.ClientSize.Width - 2, this.ClientSize.Height - 34) };
            tabCtrl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabCtrl.SizeMode = TabSizeMode.Fixed;
            tabCtrl.Multiline = true;
            tabCtrl.ItemSize = new Size((this.ClientSize.Width / 3) - 4, 28);
            tabCtrl.DrawItem += TabCtrl_DrawItem;
            
            // --- TAB 1: ACTIVE LOADOUT ---
            TabPage tabLoadout = new TabPage("Active Loadout");
            tabLoadout.BackColor = cSurface;
            tabLoadout.ForeColor = cFg;
            tabLoadout.AutoScroll = true;
            
            lblActiveProfile = new Label() { Text = "Profile: None", Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = cFg };
            tabLoadout.Controls.Add(lblActiveProfile);

            lblActiveWeapon = new Label() { Text = "Active Weapon: 1", Location = new Point(200, 10), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = cAccent };
            tabLoadout.Controls.Add(lblActiveWeapon);

            Panel pnlStage1 = CreateStagePanel(1, 40);
            tabLoadout.Controls.Add(pnlStage1);

            Panel pnlGlobal = new Panel() { Location = new Point(10, 764), Size = new Size(355, 96) };
            pnlGlobal.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlGlobal.Width - 1, pnlGlobal.Height - 9); }
            };
            Label lblGlobal = new Label() { Text = "Global", Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlGlobal.Controls.Add(lblGlobal);

            Label lblVarTitle = new Label() { Text = "Variance %:", Location = new Point(10, 24), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlGlobal.Controls.Add(lblVarTitle);
            nVariance = new NumericUpDown() { Location = new Point(90, 21), Width = 70, Maximum = 100, Minimum = 0, Increment = 1, DecimalPlaces = 0, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            nVariance.ValueChanged += (s, e) => {
                if (!suppressSync && activeGame != null) {
                    activeGame.GlobalVariance = (int)nVariance.Value;
                    activeGame.IsVarianceInit = true;
                }
            };
            pnlGlobal.Controls.Add(nVariance);

            Label lblTempDisable = new Label() { Text = "Pause Key:", Location = new Point(175, 24), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlGlobal.Controls.Add(lblTempDisable);
            cbTempDisableKey = new ComboBox() { Location = new Point(245, 20), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = cInput, ForeColor = cFg, FlatStyle = FlatStyle.Flat };
            cbTempDisableKey.Items.AddRange(new object[] { "None", "Q", "E", "R", "F", "G", "X", "C", "V", "ShiftKey", "ControlKey", "Menu" });
            cbTempDisableKey.SelectedIndexChanged += (s, e) => {
                if (!suppressSync && activeGame != null && cbTempDisableKey.SelectedItem != null)
                    activeGame.TempDisableKey = cbTempDisableKey.SelectedItem.ToString();
            };
            pnlGlobal.Controls.Add(cbTempDisableKey);

            chkLeftClickOnlyMode = new CheckBox() { Text = "Left click only", Location = new Point(10, 55), AutoSize = true, ForeColor = cFg, BackColor = cSurface, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f) };
            chkLeftClickOnlyMode.CheckedChanged += (s, e) => {
                if (!suppressSync && activeGame != null)
                {
                    activeGame.LeftClickOnlyMode = chkLeftClickOnlyMode.Checked;
                    RefreshStatus();
                }
            };
            pnlGlobal.Controls.Add(chkLeftClickOnlyMode);

            chkShowRecoilTimer = new CheckBox() { Text = "Show timer", Location = new Point(155, 55), AutoSize = true, ForeColor = cFg, BackColor = cSurface, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5f) };
            chkShowRecoilTimer.CheckedChanged += (s, e) => {
                if (!suppressSync && activeGame != null)
                {
                    activeGame.ShowRecoilTimer = chkShowRecoilTimer.Checked;
                    if (!activeGame.ShowRecoilTimer)
                        QueueTimerOverlayUpdate(false, "", 0, true);
                }
            };
            pnlGlobal.Controls.Add(chkShowRecoilTimer);
            tabLoadout.Controls.Add(pnlGlobal);

            Panel pnlStage2 = CreateStagePanel(2, 220);
            Panel pnlStage3 = CreateStagePanel(3, 400);
            Panel pnlStage4 = CreateStagePanel(4, 580);
            tabLoadout.Controls.Add(pnlStage2);
            tabLoadout.Controls.Add(pnlStage3);
            tabLoadout.Controls.Add(pnlStage4);

            Button btnSave = StyleBtn("Save Changes", new Point(10, 228), 355, 32);
            btnSave.Click += BtnSave_Click;
            tabLoadout.Controls.Add(btnSave);
            btnSave.Location = new Point(10, 872);

            pnlStatusDot = new Panel() { Location = new Point(10, 920), Size = new Size(14, 14), BackColor = cSurface };
            pnlStatusDot.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(statusDotColor))
                {
                    e.Graphics.FillEllipse(brush, 1, 1, pnlStatusDot.Width - 3, pnlStatusDot.Height - 3);
                }
            };
            tabLoadout.Controls.Add(pnlStatusDot);

            lblStatus = new Label() { Text = "DISABLED - Press F1 to toggle", Location = new Point(30, 916), AutoSize = true, ForeColor = Color.FromArgb(240, 100, 100), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            tabLoadout.Controls.Add(lblStatus);

            // Resize handler for Active Loadout tab - center weapon panels + stretch save button
            tabLoadout.Resize += (s, e) => {
                int tw = tabLoadout.ClientSize.Width;
                // Center the two weapon panels as a group (total block width = 355)
                int blockW = 375;
                int offsetX = Math.Max(10, (tw - blockW) / 2);

                lblActiveProfile.Location = new Point(offsetX, 10);
                lblActiveWeapon.Location = new Point(offsetX + 190, 10);

                pnlStage1.Location = new Point(offsetX, 40);
                pnlStage1.Width = blockW;
                pnlStage2.Location = new Point(offsetX, 220);
                pnlStage2.Width = blockW;
                pnlStage3.Location = new Point(offsetX, 400);
                pnlStage3.Width = blockW;
                pnlStage4.Location = new Point(offsetX, 580);
                pnlStage4.Width = blockW;
                pnlGlobal.Location = new Point(offsetX, 764);
                pnlGlobal.Width = blockW;

                btnSave.Location = new Point(offsetX, 872);
                btnSave.Width = blockW;

                pnlStatusDot.Location = new Point(offsetX, 920);
                lblStatus.Location = new Point(offsetX + 20, 916);
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

            // --- TAB 3: SETTINGS ---
            TabPage tabSettings = new TabPage("Settings");
            tabSettings.BackColor = cSurface;
            tabSettings.ForeColor = cFg;

            Panel pnlProfileHotkeys = new Panel() { Location = new Point(10, 12), Size = new Size(355, 150) };
            pnlProfileHotkeys.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlProfileHotkeys.Width - 1, pnlProfileHotkeys.Height - 9); }
            };
            Label lblProfileHotkeys = new Label() { Text = "Profile Hotkeys", Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlProfileHotkeys.Controls.Add(lblProfileHotkeys);

            Label lblHotkeyProfile = new Label() { Text = "Profile:", Location = new Point(12, 30), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlProfileHotkeys.Controls.Add(lblHotkeyProfile);
            cbHotkeyProfile = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(82, 26), Width = 245, FlatStyle = FlatStyle.Flat, BackColor = cInput, ForeColor = cFg };
            cbHotkeyProfile.SelectedIndexChanged += (s, e) => {
                if (!suppressSync) ApplySelectedHotkeyToUI();
            };
            pnlProfileHotkeys.Controls.Add(cbHotkeyProfile);

            Label lblModifier = new Label() { Text = "Combo:", Location = new Point(12, 62), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlProfileHotkeys.Controls.Add(lblModifier);
            cbHotkeyModifier = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(82, 58), Width = 105, FlatStyle = FlatStyle.Flat, BackColor = cInput, ForeColor = cFg };
            cbHotkeyModifier.Items.AddRange(new object[] { "None", "Ctrl", "Alt", "Shift", "Ctrl+Alt", "Ctrl+Shift", "Alt+Shift", "Ctrl+Alt+Shift" });
            pnlProfileHotkeys.Controls.Add(cbHotkeyModifier);
            cbHotkeyKey = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(195, 58), Width = 132, FlatStyle = FlatStyle.Flat, BackColor = cInput, ForeColor = cFg };
            cbHotkeyKey.Items.AddRange(new object[] { "None", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16", "F17", "F18", "F19", "F20", "F21", "F22", "F23", "F24", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "A", "B", "C", "E", "G", "H", "R", "T", "V", "X", "Z" });
            pnlProfileHotkeys.Controls.Add(cbHotkeyKey);

            Button btnAssignHotkey = StyleBtn("Assign Hotkey", new Point(12, 94), 150, 28);
            btnAssignHotkey.Click += BtnAssignHotkey_Click;
            pnlProfileHotkeys.Controls.Add(btnAssignHotkey);
            Button btnClearHotkey = StyleBtn("Clear", new Point(170, 94), 70, 28);
            btnClearHotkey.Click += BtnClearHotkey_Click;
            pnlProfileHotkeys.Controls.Add(btnClearHotkey);

            lblHotkeyCurrent = new Label() { Text = "Current: None", Location = new Point(12, 128), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f) };
            pnlProfileHotkeys.Controls.Add(lblHotkeyCurrent);
            tabSettings.Controls.Add(pnlProfileHotkeys);

            Panel pnlTimerSettings = new Panel() { Location = new Point(10, 174), Size = new Size(355, 72) };
            pnlTimerSettings.Paint += (s, e) => {
                using (Pen pen = new Pen(cBorder)) { e.Graphics.DrawRectangle(pen, 0, 8, pnlTimerSettings.Width - 1, pnlTimerSettings.Height - 9); }
            };
            Label lblTimerSettings = new Label() { Text = "Timer", Location = new Point(8, 0), AutoSize = true, ForeColor = cFgDim, Font = new Font("Segoe UI", 8.5f), BackColor = cSurface };
            pnlTimerSettings.Controls.Add(lblTimerSettings);

            Label lblTimerSize = new Label() { Text = "Size %:", Location = new Point(12, 32), AutoSize = true, ForeColor = cFg, Font = new Font("Segoe UI", 8.5f) };
            pnlTimerSettings.Controls.Add(lblTimerSize);
            nTimerSize = new NumericUpDown() { Location = new Point(82, 29), Width = 70, Maximum = 300, Minimum = 50, Increment = 10, DecimalPlaces = 0, BackColor = cInput, ForeColor = cFg, BorderStyle = BorderStyle.FixedSingle };
            nTimerSize.ValueChanged += (s, e) => {
                if (!suppressSync && activeGame != null)
                {
                    activeGame.RecoilTimerSizePercent = (int)nTimerSize.Value;
                    QueueTimerOverlayUpdate(ShouldShowRecoilTimer(), queuedTimerOverlayText, 0, true);
                }
            };
            pnlTimerSettings.Controls.Add(nTimerSize);
            tabSettings.Controls.Add(pnlTimerSettings);

            tabSettings.Resize += (s, e) => {
                int tw = tabSettings.ClientSize.Width;
                int blockW = Math.Max(355, tw - 20);
                pnlProfileHotkeys.Width = blockW;
                cbHotkeyProfile.Width = Math.Max(120, blockW - 110);
                cbHotkeyKey.Width = Math.Max(100, blockW - 223);
                pnlTimerSettings.Width = blockW;
            };

            tabCtrl.TabPages.Add(tabLoadout);
            tabCtrl.TabPages.Add(tabDB);
            tabCtrl.TabPages.Add(tabSettings);

            this.Controls.Add(tabCtrl);

            // Only anchor the tab control itself to stretch with the form
            tabCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Dynamically resize tab headers to split evenly
            this.Resize += (s2, e2) => {
                int tabW = Math.Max(70, (tabCtrl.Width / 3) - 4);
                tabCtrl.ItemSize = new Size(tabW, 28);
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
                if (!string.IsNullOrEmpty(p.HotkeyCombo))
                {
                    using (SolidBrush hotkeyBrush = new SolidBrush(selected ? cAccent : cFgDim))
                    {
                        StringFormat sf = new StringFormat();
                        sf.Alignment = StringAlignment.Far;
                        sf.LineAlignment = StringAlignment.Center;
                        Rectangle hotkeyRect = new Rectangle(e.Bounds.Left + 5, e.Bounds.Top, e.Bounds.Width - 10, e.Bounds.Height);
                        e.Graphics.DrawString(p.HotkeyCombo, fTabReg, hotkeyBrush, hotkeyRect, sf);
                    }
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
                    if (c != lblStatus && c != lblActiveWeapon && c != lblActiveProfile && c != lblTopProfileStatus && c.Name != "lblTitle" && c.Name != "lblWatermark")
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
                SyncActiveProfileFromUI();
                activeGame.GlobalVariance = (int)nVariance.Value;
                activeGame.IsVarianceInit = true;
                if (cbTempDisableKey != null && cbTempDisableKey.SelectedItem != null)
                    activeGame.TempDisableKey = cbTempDisableKey.SelectedItem.ToString();
                if (chkLeftClickOnlyMode != null)
                    activeGame.LeftClickOnlyMode = chkLeftClickOnlyMode.Checked;
                if (chkShowRecoilTimer != null)
                    activeGame.ShowRecoilTimer = chkShowRecoilTimer.Checked;
                if (nTimerSize != null)
                    activeGame.RecoilTimerSizePercent = (int)nTimerSize.Value;
                
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
                if (activeGame.Profiles == null) activeGame.Profiles = new List<Profile>();
                if (activeGame.Profiles.Count == 0) activeGame.Profiles.Add(new Profile() { Name = "Default Loadout" });
                EnsureGameGlobals(activeGame, activeGame.Profiles != null && activeGame.Profiles.Count > 0 ? activeGame.Profiles[0] : null);
                if (activeGame.Profiles != null && (activeProfile == null || !activeGame.Profiles.Contains(activeProfile)))
                {
                    activeProfile = activeGame.Profiles.Find(p => p.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || p.Name.Equals("Default Loadout", StringComparison.OrdinalIgnoreCase));
                    if (activeProfile == null && activeGame.Profiles.Count > 0) activeProfile = activeGame.Profiles[0];
                }
                RefreshList();
                ApplyProfileToUI();
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

            RefreshHotkeyProfileList();
        }

        private void RefreshHotkeyProfileList()
        {
            if (cbHotkeyProfile == null) return;

            Profile selectedProfile = cbHotkeyProfile.SelectedItem as Profile;
            if (selectedProfile == null) selectedProfile = activeProfile;

            suppressSync = true;
            try
            {
                cbHotkeyProfile.Items.Clear();
                if (activeGame != null && activeGame.Profiles != null)
                {
                    foreach (var p in activeGame.Profiles)
                    {
                        cbHotkeyProfile.Items.Add(p);
                    }
                }

                if (selectedProfile != null && cbHotkeyProfile.Items.Contains(selectedProfile))
                    cbHotkeyProfile.SelectedItem = selectedProfile;
                else if (cbHotkeyProfile.Items.Count > 0)
                    cbHotkeyProfile.SelectedIndex = 0;

                ApplySelectedHotkeyToUI();
            }
            finally
            {
                suppressSync = false;
            }
        }

        private void ApplySelectedHotkeyToUI()
        {
            if (cbHotkeyModifier == null || cbHotkeyKey == null || lblHotkeyCurrent == null) return;

            Profile p = cbHotkeyProfile != null ? cbHotkeyProfile.SelectedItem as Profile : null;
            string combo = p != null && !string.IsNullOrEmpty(p.HotkeyCombo) ? p.HotkeyCombo : "";
            string modifier = "None";
            string key = "None";

            if (!string.IsNullOrEmpty(combo))
            {
                int plusIndex = combo.LastIndexOf('+');
                if (plusIndex >= 0)
                {
                    modifier = combo.Substring(0, plusIndex);
                    key = combo.Substring(plusIndex + 1);
                }
                else
                {
                    key = combo;
                }
            }

            if (!cbHotkeyModifier.Items.Contains(modifier)) cbHotkeyModifier.Items.Add(modifier);
            if (!cbHotkeyKey.Items.Contains(key)) cbHotkeyKey.Items.Add(key);
            cbHotkeyModifier.SelectedItem = modifier;
            cbHotkeyKey.SelectedItem = key;
            lblHotkeyCurrent.Text = "Current: " + (string.IsNullOrEmpty(combo) ? "None" : combo);
        }

        private string BuildHotkeyCombo()
        {
            if (cbHotkeyKey == null || cbHotkeyKey.SelectedItem == null) return "";

            string key = cbHotkeyKey.SelectedItem.ToString();
            if (string.IsNullOrEmpty(key) || key == "None") return "";

            string modifier = cbHotkeyModifier != null && cbHotkeyModifier.SelectedItem != null ? cbHotkeyModifier.SelectedItem.ToString() : "None";
            return modifier == "None" ? key : modifier + "+" + key;
        }

        private void BtnAssignHotkey_Click(object sender, EventArgs e)
        {
            Profile p = cbHotkeyProfile != null ? cbHotkeyProfile.SelectedItem as Profile : null;
            if (p == null) return;

            string combo = BuildHotkeyCombo();
            if (!string.IsNullOrEmpty(combo) && activeGame != null && activeGame.Profiles != null)
            {
                foreach (Profile other in activeGame.Profiles)
                {
                    if (other != p && other.HotkeyCombo == combo)
                        other.HotkeyCombo = "";
                }
            }

            p.HotkeyCombo = combo;
            SaveDatabase();
            RefreshList();
            if (cbHotkeyProfile != null) cbHotkeyProfile.SelectedItem = p;
            ApplySelectedHotkeyToUI();
        }

        private void BtnClearHotkey_Click(object sender, EventArgs e)
        {
            Profile p = cbHotkeyProfile != null ? cbHotkeyProfile.SelectedItem as Profile : null;
            if (p == null) return;

            p.HotkeyCombo = "";
            SaveDatabase();
            RefreshList();
            if (cbHotkeyProfile != null) cbHotkeyProfile.SelectedItem = p;
            ApplySelectedHotkeyToUI();
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
                copy.Stage1DelayMs = p.Stage1DelayMs;
                copy.Stage2DelayMs = p.Stage2DelayMs;
                copy.Stage3DelayMs = p.Stage3DelayMs;
                copy.Stage4DelayMs = p.Stage4DelayMs;
                copy.Stage2Enabled = p.Stage2Enabled;
                copy.Stage3Enabled = p.Stage3Enabled;
                copy.Stage4Enabled = p.Stage4Enabled;
                copy.IsStageEnableInit = true;
                copy.S2_W1_Down = p.S2_W1_Down;
                copy.S2_W1_Right = p.S2_W1_Right;
                copy.S2_W1_Left = p.S2_W1_Left;
                copy.S2_W2_Down = p.S2_W2_Down;
                copy.S2_W2_Right = p.S2_W2_Right;
                copy.S2_W2_Left = p.S2_W2_Left;
                copy.S3_W1_Down = p.S3_W1_Down;
                copy.S3_W1_Right = p.S3_W1_Right;
                copy.S3_W1_Left = p.S3_W1_Left;
                copy.S3_W2_Down = p.S3_W2_Down;
                copy.S3_W2_Right = p.S3_W2_Right;
                copy.S3_W2_Left = p.S3_W2_Left;
                copy.S4_W1_Down = p.S4_W1_Down;
                copy.S4_W1_Right = p.S4_W1_Right;
                copy.S4_W1_Left = p.S4_W1_Left;
                copy.S4_W2_Down = p.S4_W2_Down;
                copy.S4_W2_Right = p.S4_W2_Right;
                copy.S4_W2_Left = p.S4_W2_Left;
                copy.IsStagesInit = true;
                copy.HotkeyCombo = "";
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
                EnsureProfileStages(activeProfile, activeGame);
                ApplyProfileToUI();
                SaveDatabase();
                tabCtrl.SelectedIndex = 0;
            }
        }

        private void ApplyProfileToUI()
        {
            if (activeProfile != null)
            {
                EnsureProfileStages(activeProfile, activeGame);
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
                    EnsureGameGlobals(activeGame, activeProfile);
                    nDelay.Value = activeProfile.Stage1DelayMs;
                    if (nStageDelay[1] != null) nStageDelay[1].Value = activeProfile.Stage2DelayMs;
                    if (nStageDelay[2] != null) nStageDelay[2].Value = activeProfile.Stage3DelayMs;
                    if (nStageDelay[3] != null) nStageDelay[3].Value = activeProfile.Stage4DelayMs;
                    if (chkStageEnabled[1] != null) chkStageEnabled[1].Checked = activeProfile.Stage2Enabled;
                    if (chkStageEnabled[2] != null) chkStageEnabled[2].Checked = activeProfile.Stage3Enabled;
                    if (chkStageEnabled[3] != null) chkStageEnabled[3].Checked = activeProfile.Stage4Enabled;
                    if (nStageW1[1, 0] != null) nStageW1[1, 0].Value = activeProfile.S2_W1_Down;
                    if (nStageW1[1, 1] != null) nStageW1[1, 1].Value = activeProfile.S2_W1_Right;
                    if (nStageW1[1, 2] != null) nStageW1[1, 2].Value = activeProfile.S2_W1_Left;
                    if (nStageW2[1, 0] != null) nStageW2[1, 0].Value = activeProfile.S2_W2_Down;
                    if (nStageW2[1, 1] != null) nStageW2[1, 1].Value = activeProfile.S2_W2_Right;
                    if (nStageW2[1, 2] != null) nStageW2[1, 2].Value = activeProfile.S2_W2_Left;
                    if (nStageW1[2, 0] != null) nStageW1[2, 0].Value = activeProfile.S3_W1_Down;
                    if (nStageW1[2, 1] != null) nStageW1[2, 1].Value = activeProfile.S3_W1_Right;
                    if (nStageW1[2, 2] != null) nStageW1[2, 2].Value = activeProfile.S3_W1_Left;
                    if (nStageW2[2, 0] != null) nStageW2[2, 0].Value = activeProfile.S3_W2_Down;
                    if (nStageW2[2, 1] != null) nStageW2[2, 1].Value = activeProfile.S3_W2_Right;
                    if (nStageW2[2, 2] != null) nStageW2[2, 2].Value = activeProfile.S3_W2_Left;
                    if (nStageW1[3, 0] != null) nStageW1[3, 0].Value = activeProfile.S4_W1_Down;
                    if (nStageW1[3, 1] != null) nStageW1[3, 1].Value = activeProfile.S4_W1_Right;
                    if (nStageW1[3, 2] != null) nStageW1[3, 2].Value = activeProfile.S4_W1_Left;
                    if (nStageW2[3, 0] != null) nStageW2[3, 0].Value = activeProfile.S4_W2_Down;
                    if (nStageW2[3, 1] != null) nStageW2[3, 1].Value = activeProfile.S4_W2_Right;
                    if (nStageW2[3, 2] != null) nStageW2[3, 2].Value = activeProfile.S4_W2_Left;
                    if (activeGame != null && nVariance != null) nVariance.Value = activeGame.GlobalVariance;
                    if (activeGame != null && cbTempDisableKey != null)
                    {
                        string tempKey = string.IsNullOrEmpty(activeGame.TempDisableKey) ? "Q" : activeGame.TempDisableKey;
                        if (!cbTempDisableKey.Items.Contains(tempKey)) cbTempDisableKey.Items.Add(tempKey);
                        cbTempDisableKey.SelectedItem = tempKey;
                    }
                    if (chkLeftClickOnlyMode != null && activeGame != null)
                        chkLeftClickOnlyMode.Checked = activeGame.LeftClickOnlyMode;
                    if (chkShowRecoilTimer != null && activeGame != null)
                        chkShowRecoilTimer.Checked = activeGame.ShowRecoilTimer;
                    if (nTimerSize != null && activeGame != null)
                        nTimerSize.Value = Math.Max(nTimerSize.Minimum, Math.Min(nTimerSize.Maximum, activeGame.RecoilTimerSizePercent));
                    if (cbHotkeyProfile != null && activeProfile != null && cbHotkeyProfile.Items.Contains(activeProfile))
                        cbHotkeyProfile.SelectedItem = activeProfile;
                    ApplySelectedHotkeyToUI();
                    RefreshStatus();
                }
                finally
                {
                    suppressSync = false;
                }
            }
        }

        private bool TryGetConfiguredTempDisableKey(out Keys key)
        {
            key = Keys.None;
            if (activeGame == null || string.IsNullOrEmpty(activeGame.TempDisableKey) || activeGame.TempDisableKey == "None")
                return false;

            try
            {
                key = (Keys)Enum.Parse(typeof(Keys), activeGame.TempDisableKey);
                return key != Keys.None;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseHotkeyKey(string keyName, out Keys key)
        {
            key = Keys.None;
            if (string.IsNullOrEmpty(keyName) || keyName == "None") return false;

            try
            {
                key = (Keys)Enum.Parse(typeof(Keys), keyName.Trim(), true);
                return key != Keys.None;
            }
            catch
            {
                return false;
            }
        }

        private static short GetAsyncKeyState(Keys key)
        {
            Keys keyCode = key & Keys.KeyCode;
            if (keyCode == Keys.None) return 0;
            return GetAsyncKeyStateNative((int)keyCode);
        }

        private bool IsVirtualKeyDown(int vKey)
        {
            return (GetAsyncKeyStateNative(vKey) & 0x8000) != 0;
        }

        private bool IsKeyDown(Keys key)
        {
            Keys keyCode = key & Keys.KeyCode;
            return keyCode != Keys.None && IsVirtualKeyDown((int)keyCode);
        }

        private bool IsAnyKeyDown(params Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if (IsKeyDown(key)) return true;
            }

            return false;
        }

        private string NormalizeModifierName(string modifierPart)
        {
            if (string.IsNullOrEmpty(modifierPart)) return "None";

            string trimmed = modifierPart.Trim();
            string lower = trimmed.ToLowerInvariant();

            if (lower == "none") return "None";
            if (lower == "ctrl" || lower == "control" || lower == "controlkey" || lower == "lcontrolkey" || lower == "rcontrolkey")
                return "Ctrl";
            if (lower == "alt" || lower == "menu" || lower == "lmenu" || lower == "rmenu")
                return "Alt";
            if (lower == "shift" || lower == "shiftkey" || lower == "lshiftkey" || lower == "rshiftkey")
                return "Shift";

            return trimmed;
        }

        private bool TryParseModifierFlags(string modifier, out HotkeyModifierFlags flags)
        {
            flags = HotkeyModifierFlags.None;
            if (string.IsNullOrEmpty(modifier) || modifier == "None") return true;

            string[] parts = modifier.Split('+');
            foreach (string part in parts)
            {
                string normalized = NormalizeModifierName(part);
                if (normalized == "None") continue;
                if (normalized == "Ctrl") flags |= HotkeyModifierFlags.Ctrl;
                else if (normalized == "Alt") flags |= HotkeyModifierFlags.Alt;
                else if (normalized == "Shift") flags |= HotkeyModifierFlags.Shift;
                else return false;
            }

            return true;
        }

        private HotkeyModifierFlags GetCurrentHotkeyModifiers()
        {
            HotkeyModifierFlags current = HotkeyModifierFlags.None;

            if (IsVirtualKeyDown(0x11) || IsVirtualKeyDown(0xA2) || IsVirtualKeyDown(0xA3))
                current |= HotkeyModifierFlags.Ctrl;
            if (IsVirtualKeyDown(0x12) || IsVirtualKeyDown(0xA4) || IsVirtualKeyDown(0xA5))
                current |= HotkeyModifierFlags.Alt;
            if (IsVirtualKeyDown(0x10) || IsVirtualKeyDown(0xA0) || IsVirtualKeyDown(0xA1))
                current |= HotkeyModifierFlags.Shift;

            return current;
        }

        private bool IsModifierPartDown(string modifierPart)
        {
            string modifier = NormalizeModifierName(modifierPart);

            if (modifier == "None") return true;
            if (modifier == "Ctrl") return IsAnyKeyDown(Keys.ControlKey, Keys.LControlKey, Keys.RControlKey);
            if (modifier == "Alt") return IsAnyKeyDown(Keys.Menu, Keys.LMenu, Keys.RMenu);
            if (modifier == "Shift") return IsAnyKeyDown(Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey);

            return false;
        }

        private bool AreModifiersDown(string modifier)
        {
            HotkeyModifierFlags required;
            if (!TryParseModifierFlags(modifier, out required)) return false;
            HotkeyModifierFlags current = GetCurrentHotkeyModifiers();
            return (current & required) == required;
        }

        private int GetModifierScore(HotkeyModifierFlags flags)
        {
            int score = 0;
            if ((flags & HotkeyModifierFlags.Ctrl) != 0) score++;
            if ((flags & HotkeyModifierFlags.Alt) != 0) score++;
            if ((flags & HotkeyModifierFlags.Shift) != 0) score++;
            return score;
        }

        private Profile GetPressedProfileHotkey()
        {
            if (activeGame == null || activeGame.Profiles == null) return null;

            Profile bestMatch = null;
            int bestModifierScore = -1;

            foreach (Profile p in activeGame.Profiles)
            {
                if (p == null || string.IsNullOrEmpty(p.HotkeyCombo)) continue;

                string combo = p.HotkeyCombo;
                string modifier = "None";
                string keyName = combo;
                int plusIndex = combo.LastIndexOf('+');
                if (plusIndex >= 0)
                {
                    modifier = combo.Substring(0, plusIndex);
                    keyName = combo.Substring(plusIndex + 1);
                }

                Keys key;
                if (!TryParseHotkeyKey(keyName, out key)) continue;
                if (!IsKeyDown(key)) continue;

                HotkeyModifierFlags required;
                if (!TryParseModifierFlags(modifier, out required)) continue;
                if (!AreModifiersDown(modifier)) continue;

                int modifierScore = GetModifierScore(required);
                if (modifierScore > bestModifierScore)
                {
                    bestMatch = p;
                    bestModifierScore = modifierScore;
                }
            }

            return bestMatch;
        }

        private void RefreshTopProfileStatus()
        {
            if (lblTopProfileStatus == null) return;

            string profileName = activeProfile != null && !string.IsNullOrEmpty(activeProfile.Name) ? activeProfile.Name : "No profile";
            string stateText = "OFF";
            if (isEnabled && isTemporarilyDisabled)
                stateText = "PAUSED";
            else if (isEnabled)
                stateText = "ON";

            lblTopProfileStatus.Text = profileName + " | " + stateText;
            if (pnlTopStatusDot != null) pnlTopStatusDot.Invalidate();
        }

        private void RefreshStatus()
        {
            if (lblStatus == null) return;

            if (!isEnabled)
            {
                statusDotColor = Color.FromArgb(240, 100, 100);
                lblStatus.Text = "DISABLED - Press F1 to toggle";
                lblStatus.ForeColor = statusDotColor;
            }
            else if (isTemporarilyDisabled)
            {
                statusDotColor = Color.FromArgb(245, 190, 80);
                lblStatus.Text = "PAUSED - 1 or double-tap key resumes";
                lblStatus.ForeColor = statusDotColor;
            }
            else
            {
                string triggerText = activeGame != null && activeGame.LeftClickOnlyMode ? "Hold LClick" : "Hold LClick + RClick";
                statusDotColor = Color.FromArgb(100, 220, 100);
                lblStatus.Text = "ACTIVE - " + triggerText;
                lblStatus.ForeColor = statusDotColor;
            }

            if (pnlStatusDot != null) pnlStatusDot.Invalidate();
            RefreshTopProfileStatus();
        }

        private bool StageHasRecoil(Profile profile, int stage, int weapon)
        {
            if (profile == null) return false;
            decimal down = 0, right = 0, left = 0;
            GetStageRecoil(profile, stage, weapon, out down, out right, out left);
            return down != 0 || right != 0 || left != 0;
        }

        private bool IsStageEnabled(Profile profile, int stage)
        {
            if (profile == null) return false;
            if (stage == 1) return true;
            if (stage == 2) return profile.Stage2Enabled;
            if (stage == 3) return profile.Stage3Enabled;
            return profile.Stage4Enabled;
        }

        private int GetStageDelay(Profile profile, int stage)
        {
            if (profile == null) return 0;
            if (stage == 1) return profile.Stage1DelayMs;
            if (stage == 2) return profile.Stage2DelayMs;
            if (stage == 3) return profile.Stage3DelayMs;
            return profile.Stage4DelayMs;
        }

        private int GetActiveStage(Profile profile, int weapon, double elapsedMs)
        {
            if (profile == null) return 1;

            if (elapsedMs >= profile.Stage4DelayMs && IsStageEnabled(profile, 4))
                return 4;

            if (elapsedMs >= profile.Stage3DelayMs && IsStageEnabled(profile, 3))
                return 3;

            if (elapsedMs >= profile.Stage2DelayMs && IsStageEnabled(profile, 2))
                return 2;

            return 1;
        }

        private void GetStageRecoil(Profile profile, int stage, int weapon, out decimal pullD, out decimal pullR, out decimal pullL)
        {
            pullD = 0;
            pullR = 0;
            pullL = 0;
            if (profile == null) return;

            if (stage == 1)
            {
                if (weapon == 1)
                {
                    pullD = profile.W1_Down;
                    pullR = profile.W1_Right;
                    pullL = profile.W1_Left;
                }
                else
                {
                    pullD = profile.W2_Down;
                    pullR = profile.W2_Right;
                    pullL = profile.W2_Left;
                }
            }
            else if (stage == 2)
            {
                if (weapon == 1)
                {
                    pullD = profile.S2_W1_Down;
                    pullR = profile.S2_W1_Right;
                    pullL = profile.S2_W1_Left;
                }
                else
                {
                    pullD = profile.S2_W2_Down;
                    pullR = profile.S2_W2_Right;
                    pullL = profile.S2_W2_Left;
                }
            }
            else if (stage == 3)
            {
                if (weapon == 1)
                {
                    pullD = profile.S3_W1_Down;
                    pullR = profile.S3_W1_Right;
                    pullL = profile.S3_W1_Left;
                }
                else
                {
                    pullD = profile.S3_W2_Down;
                    pullR = profile.S3_W2_Right;
                    pullL = profile.S3_W2_Left;
                }
            }
            else
            {
                if (weapon == 1)
                {
                    pullD = profile.S4_W1_Down;
                    pullR = profile.S4_W1_Right;
                    pullL = profile.S4_W1_Left;
                }
                else
                {
                    pullD = profile.S4_W2_Down;
                    pullR = profile.S4_W2_Right;
                    pullL = profile.S4_W2_Left;
                }
            }
        }

        private bool GetTimedStageRecoil(Profile profile, int weapon, double elapsedMs, out decimal pullD, out decimal pullR, out decimal pullL)
        {
            pullD = 0;
            pullR = 0;
            pullL = 0;
            if (profile == null) return false;

            if (elapsedMs >= profile.Stage4DelayMs && IsStageEnabled(profile, 4))
            {
                GetStageRecoil(profile, 4, weapon, out pullD, out pullR, out pullL);
                return true;
            }

            if (elapsedMs >= profile.Stage3DelayMs && IsStageEnabled(profile, 3))
            {
                GetStageRecoil(profile, 3, weapon, out pullD, out pullR, out pullL);
                return true;
            }

            if (elapsedMs >= profile.Stage2DelayMs && IsStageEnabled(profile, 2))
            {
                GetStageRecoil(profile, 2, weapon, out pullD, out pullR, out pullL);
                return true;
            }

            if (elapsedMs >= profile.Stage1DelayMs)
            {
                GetStageRecoil(profile, 1, weapon, out pullD, out pullR, out pullL);
                return true;
            }

            return false;
        }

        private bool ShouldShowRecoilTimer()
        {
            return activeGame != null && activeGame.ShowRecoilTimer;
        }

        private string BuildTimerOverlayText(double elapsedMs, int stage)
        {
            return ((int)Math.Max(0, elapsedMs)).ToString() + " ms | Stage " + stage.ToString();
        }

        private void QueueTimerOverlayUpdate(bool visible, string text, double currentTime, bool immediate)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (!immediate && visible == queuedTimerOverlayVisible && text == queuedTimerOverlayText) return;
            if (visible && !immediate && (currentTime - lastTimerOverlayUpdate) < 30) return;

            queuedTimerOverlayVisible = visible;
            queuedTimerOverlayText = text;
            lastTimerOverlayUpdate = currentTime;

            try
            {
                this.BeginInvoke(new Action(() => {
                    UpdateTimerOverlay(visible, text);
                }));
            }
            catch
            {
            }
        }

        private void UpdateTimerOverlay(bool visible, string text)
        {
            if (!visible)
            {
                if (recoilTimerOverlay != null && !recoilTimerOverlay.IsDisposed)
                    recoilTimerOverlay.Hide();
                return;
            }

            if (recoilTimerOverlay == null || recoilTimerOverlay.IsDisposed)
                recoilTimerOverlay = new RecoilTimerOverlay();

            recoilTimerOverlay.SetTimerSize(activeGame != null ? activeGame.RecoilTimerSizePercent : 100);
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            recoilTimerOverlay.Location = new Point(area.Left + ((area.Width - recoilTimerOverlay.Width) / 2), area.Top + 42);
            recoilTimerOverlay.SetTimerText(text);

            if (!recoilTimerOverlay.Visible)
                recoilTimerOverlay.Show(this);
        }

        private void BackgroundLoop()
        {
            bool f1WasDown = false;
            bool f2WasDown = false;
            bool tempDisableKeyWasDown = false;
            bool profileHotkeyWasDown = false;
            double lastPauseKeyTapTime = -10000;
            const double pauseKeyDoubleTapMs = 500;
            
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            double lastTime = timer.Elapsed.TotalMilliseconds;
            Random rnd = new Random();
            
            double remX = 0;
            double remY = 0;
            double triggerStart = -1;
            double timerTriggerStart = -1;
            double lastTimerElapsed = 0;
            int lastTimerStage = 1;
            bool hasTimerMeasurement = false;

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
                    if (!isEnabled) isTemporarilyDisabled = false;
                    this.BeginInvoke(new Action(() => {
                        RefreshStatus();
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

                bool appIsForeground = GetForegroundWindow() == this.Handle;
                if (!appIsForeground)
                {
                    Profile hotkeyProfile = GetPressedProfileHotkey();
                    bool profileHotkeyIsDown = hotkeyProfile != null;
                    if (profileHotkeyIsDown && !profileHotkeyWasDown && hotkeyProfile != activeProfile)
                    {
                        activeProfile = hotkeyProfile;
                        activeWeapon = 1;
                        remX = 0;
                        remY = 0;
                        triggerStart = -1;
                        timerTriggerStart = -1;
                        this.BeginInvoke(new Action(() => { ApplyProfileToUI(); }));
                    }
                    profileHotkeyWasDown = profileHotkeyIsDown;
                }
                else
                {
                    profileHotkeyWasDown = false;
                }

                bool lmbDown = (GetAsyncKeyState(Keys.LButton) < 0);
                bool rmbDown = (GetAsyncKeyState(Keys.RButton) < 0);
                bool leftClickOnlyMode = activeGame != null && activeGame.LeftClickOnlyMode;
                bool triggerDown = leftClickOnlyMode ? !appIsForeground && lmbDown : lmbDown && rmbDown;
                double triggerElapsed = 0;
                int timerStage = 1;

                if (triggerDown)
                {
                    if (timerTriggerStart < 0) timerTriggerStart = currentTime;
                    triggerElapsed = currentTime - timerTriggerStart;
                    timerStage = GetActiveStage(activeProfile, activeWeapon, triggerElapsed);
                    lastTimerElapsed = triggerElapsed;
                    lastTimerStage = timerStage;
                    hasTimerMeasurement = true;
                    if (ShouldShowRecoilTimer())
                        QueueTimerOverlayUpdate(true, BuildTimerOverlayText(triggerElapsed, timerStage), currentTime, false);
                }
                else
                {
                    remX = 0;
                    remY = 0;
                    triggerStart = -1;
                    timerTriggerStart = -1;
                    if (ShouldShowRecoilTimer())
                        QueueTimerOverlayUpdate(true, hasTimerMeasurement ? BuildTimerOverlayText(lastTimerElapsed, lastTimerStage) : BuildTimerOverlayText(0, 1), currentTime, false);
                    else
                        QueueTimerOverlayUpdate(false, "", currentTime, true);
                }

                if (!isEnabled) continue;

                // Monitor 1 and 2 keys for weapon switching
                // Only process hotkeys if our app is NOT the active foreground window
                if (!appIsForeground)
                {
                    if (GetAsyncKeyState(Keys.D1) < 0 || GetAsyncKeyState(Keys.NumPad1) < 0)
                    {
                        if (activeWeapon != 1)
                        {
                            activeWeapon = 1;
                            this.BeginInvoke(new Action(() => { lblActiveWeapon.Text = "Active Weapon: 1"; }));
                        }
                        if (isTemporarilyDisabled)
                        {
                            isTemporarilyDisabled = false;
                            triggerStart = -1;
                            this.BeginInvoke(new Action(() => { RefreshStatus(); }));
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

                    Keys tempDisableKey;
                    if (TryGetConfiguredTempDisableKey(out tempDisableKey))
                    {
                        bool tempDisableKeyIsDown = (GetAsyncKeyState(tempDisableKey) < 0);
                        if (tempDisableKeyIsDown && !tempDisableKeyWasDown)
                        {
                            if (isTemporarilyDisabled && (currentTime - lastPauseKeyTapTime) <= pauseKeyDoubleTapMs)
                                isTemporarilyDisabled = false;
                            else
                                isTemporarilyDisabled = true;

                            lastPauseKeyTapTime = currentTime;
                            remX = 0;
                            remY = 0;
                            triggerStart = -1;
                            this.BeginInvoke(new Action(() => { RefreshStatus(); }));
                        }
                        tempDisableKeyWasDown = tempDisableKeyIsDown;
                    }
                    else
                    {
                        tempDisableKeyWasDown = false;
                    }
                }

                if (!isEnabled || isTemporarilyDisabled) continue;

                if (triggerDown)
                {
                    // Stage delays are absolute timestamps from trigger start. The latest enabled stage is the current movement.
                    if (triggerStart < 0) triggerStart = currentTime;
                    double recoilElapsed = currentTime - triggerStart;

                    decimal pullD = 0, pullR = 0, pullL = 0;
                    if (!GetTimedStageRecoil(activeProfile, activeWeapon, recoilElapsed, out pullD, out pullR, out pullL))
                        continue;

                    tickCount++;

                    // Tightened base scale: value 30 is about 857 px/sec
                    double scaleX = (double)(pullR - pullL) * (dt / 35.0);
                    double scaleY = (double)pullD * (dt / 35.0);

                    double varScale = activeGame != null ? (activeGame.GlobalVariance / 100.0) : 0.40;

                    // --- LAYER 1: Smooth organic absolute drift ---
                    // Drift wandering absolute pixels per tick
                    if (tickCount % (rnd.Next(30, 80)) == 0)
                    {
                        driftX = (rnd.NextDouble() * 0.10 - 0.05) * varScale; // +/-0.05 absolute pixels/tick horizontally
                        driftY = (rnd.NextDouble() * 0.30 - 0.15) * varScale; // +/-0.15 absolute pixels/tick vertically
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
            if (recoilTimerOverlay != null && !recoilTimerOverlay.IsDisposed)
            {
                recoilTimerOverlay.Close();
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
