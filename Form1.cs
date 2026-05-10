using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Simple_electronic_keyboard
{
    public partial class frmBeepPlayer : Form
    {
        [DllImport("winmm.dll")]
        private static extern int midiOutOpen(out IntPtr handle, int deviceID, IntPtr proc, IntPtr instance, int flags);

        [DllImport("winmm.dll")]
        private static extern int midiOutClose(IntPtr handle);

        [DllImport("winmm.dll")]
        private static extern int midiOutShortMsg(IntPtr handle, int message);

        private IntPtr midiHandle;

        int[] midiNotes = { 72, 74, 76, 77, 79, 81, 83, 84 };

        int initWidth = 0;
        int initHeight = 0;
        Dictionary<string, Rectangle> initControl = new Dictionary<string, Rectangle>();

        // 【新增】一個陣列來儲存按鈕，方便我們用程式控制顏色
        private Button[] pianoKeys;

        public frmBeepPlayer()
        {
            InitializeComponent();
            InitializeButton();

            // 【新增】初始化按鈕陣列 (順序必須對應 midiNotes 的音高順序)
            pianoKeys = new Button[] { btnDo, btnRe, btnMi, btnFa, btnSol, btnLa, btnSi, btnHDo };

            // 開啟系統預設的 MIDI 合成器 (-1 代表預設設備)
            midiOutOpen(out midiHandle, -1, IntPtr.Zero, IntPtr.Zero, 0);

            // 當表單關閉時，記得釋放 MIDI 資源
            this.FormClosing += FrmBeepPlayer_FormClosing;

            this.KeyPreview = true;
            this.KeyDown += frmBeepPlayer_KeyDown;

            // 注意：因為你在設計工具已經綁定過 KeyUp 了，這裡就不需要再寫 this.KeyUp +=... 囉！
        }

        private void InitializeButton()
        {
            btnDo.Click += btnDo_Click;
            btnRe.Click += btnDo_Click;
            btnMi.Click += btnDo_Click;
            btnFa.Click += btnDo_Click;
            btnSol.Click += btnDo_Click;
            btnLa.Click += btnDo_Click;
            btnSi.Click += btnDo_Click;
            btnHDo.Click += btnDo_Click;
        }

        private void btnDo_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (int.TryParse(btn.Tag?.ToString(), out int noteIndex) && noteIndex < midiNotes.Length)
            {
                PlayNote(noteIndex);
            }
            else
            {
                PlayNote(btn.TabIndex % midiNotes.Length);
            }
        }

        // 【修改】加入按鍵變灰色的視覺回饋
        private void frmBeepPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            int noteIndex = GetNoteIndexFromKey(e.KeyCode);

            if (noteIndex != -1)
            {
                PlayNote(noteIndex);

                // 視覺回饋：將按下的琴鍵背景變成淺灰色
                pianoKeys[noteIndex].BackColor = Color.LightGray;

                e.Handled = true;
            }
        }

        // 【新增】這就是編譯器之前找不到的那個方法！手放開時把按鈕變回白色
        private void frmBeepPlayer_KeyUp(object sender, KeyEventArgs e)
        {
            int noteIndex = GetNoteIndexFromKey(e.KeyCode);

            if (noteIndex != -1)
            {
                // 視覺回饋：將放開的琴鍵背景恢復成白色
                pianoKeys[noteIndex].BackColor = Color.White;
            }
        }

        // 【新增】將判斷按鍵的邏輯獨立出來，這樣 KeyDown 和 KeyUp 都可以共用
        private int GetNoteIndexFromKey(Keys key)
        {
            switch (key)
            {
                case Keys.A: return 0; // Do
                case Keys.S: return 1; // Re
                case Keys.D: return 2; // Mi
                case Keys.F: return 3; // Fa
                case Keys.G: return 4; // Sol
                case Keys.H: return 5; // La
                case Keys.J: return 6; // Si
                case Keys.K: return 7; // High Do
                default: return -1;
            }
        }

        private void PlayNote(int index)
        {
            if (index >= 0 && index < midiNotes.Length)
            {
                int note = midiNotes[index];
                int msg = 0x90 | (note << 8) | (127 << 16);
                midiOutShortMsg(midiHandle, msg);
            }
        }

        private void FrmBeepPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (midiHandle != IntPtr.Zero)
            {
                midiOutClose(midiHandle);
            }
        }

        private void frmBeepPlayer_Load(object sender, EventArgs e)
        {
            this.initWidth = this.palMain.Width;
            this.initHeight = this.palMain.Height;

            foreach (Control ctl in this.palMain.Controls)
            {
                this.initControl.Add(ctl.Name, new Rectangle(ctl.Left, ctl.Top, ctl.Width, ctl.Height));
            }
        }

        private void frmBeepPlayer_SizeChanged(object sender, EventArgs e)
        {
            if (this.initWidth == 0 || this.initHeight == 0) return;

            double width = this.palMain.Width;
            double height = this.palMain.Height;
            double iRatioWith = width / this.initWidth;
            double iRatioHeight = height / this.initHeight;

            foreach (Control ctl in this.palMain.Controls)
            {
                if (initControl.ContainsKey(ctl.Name))
                {
                    Rectangle originalRect = initControl[ctl.Name];
                    ctl.Left = (int)(originalRect.Left * iRatioWith);
                    ctl.Top = (int)(originalRect.Top * iRatioHeight);
                    ctl.Width = (int)(originalRect.Width * iRatioWith);
                    ctl.Height = (int)(originalRect.Height * iRatioHeight);
                }
            }
        }
    }
}