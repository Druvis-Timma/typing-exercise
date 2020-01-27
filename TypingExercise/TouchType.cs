using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TypingExercise
{
    public partial class TouchType : Form
    {
        private static Timer hundredth;
        
        static string spirit = "The primary surface mission for Spirit was planned to last at least 90 sols. The mission received several " +
            "extensions and lasted about 2,208 sols. On August 11, 2007, Spirit obtained the second longest operational duration on" +
            " the surface of Mars for a lander or rover at 1282 Sols, one sol longer than the Viking 2 lander. Viking 2 was powered" +
            " by a nuclear cell whereas Spirit is powered by solar arrays. Until Opportunity overtook it on May 19, 2010, the Mars" +
            " probe with longest operational period was Viking 1 that lasted for 2245 Sols on the surface of Mars. On March 22, 2010," +
            " Spirit sent its last communication, thus falling just over a month short of surpassing Viking 1's operational record." +
            " An archive of weekly updates on the rover's status can be found at the Spirit Update Archive. Spirit's total odometry" +
            " as of March 22, 2010 (sol 2210) is 7,730.50 meters (4.80 mi). ";

        static string[] filePaths = Directory.GetFiles(@"C:\Typing Resource", "*", SearchOption.AllDirectories);

        static char[] randomFile()
        {
            Random rnd = new Random();
            int r = rnd.Next(filePaths.Length);
            string txt = System.IO.File.ReadAllText(filePaths[r]);
            char[] TextToChar = txt.ToCharArray();

            for (int i = 0; i < TextToChar.Length; i++)// Replace enter, tab, etc. with space
            {
                if (TextToChar[i] < (char)32 || TextToChar[i] == (char)127) TextToChar[i] = (char)32; 
            }

            txt = new string(TextToChar);
            txt = txt.Replace("   ", " "); //Remove multiple spaces
            txt = txt.Replace("  ", " ");
            if (txt.Length > 1320) txt = txt.Substring(0, 1320); // Trim to roughly fit screen
            if (txt[txt.Length - 1] != (char)32) txt += " ";  // Last char needs to be space

            TextToChar = txt.ToCharArray();
            return TextToChar;
        }

        char[] DisplayText = randomFile();

        string typed = null;
        string totype = null;
        int selected = 0;
        public static int errorOpacity = 0;
        public static int errors = 0;
        public static int completedWords = 0;
        public static int WPM = 0;
        public static decimal secondsElapsed =0;
        public static float accuracy = 100;
        private Point diffPoint;
        bool mouseDown = false;
        DateTime Started = DateTime.Now;
        List <Rectangle> charErrors = new List<Rectangle>();
        Rectangle thisChar;

        public TouchType()
        {
            InitializeComponent();
            hundredth = new Timer
            {
                Interval = 10,
                Enabled = true
            };
            hundredth.Tick += TimerEventProcessor;

            
        }
        private void TimerEventProcessor(Object myObject,EventArgs myEventArgs)
        {
            
            if (errorOpacity >= 5)
            {
                errorOpacity -= 5;
                this.Invalidate();
            }
            DateTime Current = DateTime.Now;
            secondsElapsed = (decimal)((Current- Started).TotalSeconds);
        }

        private void TouchType_Load(object sender, EventArgs e)
        {
        
        }
        
        private void TouchType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (selected < DisplayText.Length-1)
            {
                if (e.KeyChar == DisplayText[selected]) 
                {
                    if (DisplayText[selected + 1].ToString() == " " && DisplayText[selected].ToString() != " ")
                    {
                        completedWords += 1;
                        WPM = (int)(completedWords / secondsElapsed * 60);
                    }
                    selected += 1;
                }
                else
                {
                    errors += 1;
                    errorOpacity += (255 - errorOpacity) / 2;
                    charErrors.Add(thisChar);
                }
                accuracy = (float)(selected + 1) * 100 / (selected + 1 + errors);
                this.Invalidate();
            }
        }

        private void TouchType_Paint(object sender, PaintEventArgs e)
        {
            Font FontSelected = new Font("Courier New", 18, FontStyle.Underline | FontStyle.Bold);
            Font RegFont = new Font("Courier New", 18, FontStyle.Regular);
            Font Control = new Font("Sans Seriff", 10, FontStyle.Regular);
            SolidBrush typedRed = new SolidBrush(Color.FromArgb(200, 180, 0, 0));
            SolidBrush Red = new SolidBrush(Color.FromArgb(200, Color.Red));
            SolidBrush White = new SolidBrush(Color.FromArgb(150, Color.White));
            SolidBrush typo = new SolidBrush(Color.FromArgb(120, 120, 25, 25));
            Pen SelectChar = new Pen(Red,2);
            SizeF lineSize = new SizeF();
            typed = null;
            totype = null;
            int y_pos = 39;
            byte forceNewline = 0;

            if (selected == DisplayText.Length-1)  // Highlights mistakes at the end
            {
                foreach (Rectangle r in charErrors)
                {
                    e.Graphics.FillRectangle(typo, r);
                }
            }

            for (int i = 0; i < DisplayText.Length; i++)  // Place each character on a line
            {
                lineSize = e.Graphics.MeasureString(totype, RegFont);
                if (DisplayText[i] == (char)32 && lineSize.Width >= 800 && i < DisplayText.Length - 1)
                {
                    int k = 1;
                    string checkBoundary = totype;
                    while (k+i <= DisplayText.Length && DisplayText[i + k] != (char)32)
                    {
                        checkBoundary += DisplayText[k+i];
                        lineSize = e.Graphics.MeasureString(checkBoundary, RegFont);
                        if (lineSize.Width > 980)
                        {
                            forceNewline = 1;
                            break;
                        }
                        k++;
                    }
                }
                lineSize = e.Graphics.MeasureString(totype, RegFont);

                if (DisplayText[i] == (char)92) totype += @"\"; // Escape special chars
                else if (DisplayText[i] == (char)123) totype += @"{";
                else totype += DisplayText[i];

                if (i < selected) typed = totype;



                if (i==selected && i < DisplayText.Length - 1) // Marks the current character that needs to be pressed
                {
                    //lineSize = e.Graphics.MeasureString(typed, RegFont);
                    int spaceAdjust = -4; 
                    if (selected > 0 && DisplayText[selected - 1].ToString() == " ") spaceAdjust = 10; //Adjusts for ignored space char
                    if (typed==null) spaceAdjust = 3;

                    thisChar = new Rectangle((int)(lineSize.Width + spaceAdjust + 20), y_pos, 15, 27); // Same rectangle passed to charErrors list on keypress
                    e.Graphics.DrawRectangle(SelectChar, thisChar);
                }

                //DisplayText[i].ToString() == " " && lineSize.Width >= 885 || 
                if (i == DisplayText.Length-1 || forceNewline==1) // Draws the current line of text
                {
                    e.Graphics.DrawString(totype, RegFont, White, 20, y_pos);
                    e.Graphics.DrawString(typed, RegFont, typedRed, 20, y_pos);

                    y_pos += 27;
                    typed = null;
                    totype = null;
                    forceNewline = 0;
                }
                
                
            }

            // Draw borders and control string
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, 100, 0, 0)), 0, 0, 1023, 24);
            if (errorOpacity>0)e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(errorOpacity, 90, 0, 0)), 0, 0, 1024, 615);
            Pen Border = new Pen((Color.FromArgb(180, 100, 0, 0)), 1);
            e.Graphics.DrawRectangle(Border, 0, 0, 1023, 614);
            e.Graphics.DrawLine(Border, 0, 23, 1023, 23);
            e.Graphics.DrawString("Time Elapsed: " + secondsElapsed.ToString("0.0") + "  Words per minute: " + WPM + 
                "  Words: " + completedWords + "  Mistakes: " + errors + "  Accuracy: " + 
                accuracy.ToString("0.0") + "%", Control, new SolidBrush(Color.FromArgb(100, Color.White)), 10, 4);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        // Make form movable
        private void TouchType_MouseDown(object sender, MouseEventArgs e)
        {
            diffPoint.X = System.Windows.Forms.Cursor.Position.X - this.Left;
            diffPoint.Y = System.Windows.Forms.Cursor.Position.Y - this.Top;
            mouseDown = true;
        }

        private void TouchType_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void TouchType_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Left = System.Windows.Forms.Cursor.Position.X - diffPoint.X;
                this.Top = System.Windows.Forms.Cursor.Position.Y - diffPoint.Y;
            }
        }
    }
}
