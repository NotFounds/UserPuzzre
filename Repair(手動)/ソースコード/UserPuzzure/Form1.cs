using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserPuzzure
{
    public partial class Form1 : Form
    {    
        // 画像に関するデータ変数
        private string  filePath;
        private int     div_x;
        private int     div_y;
        private int     panel_x;
        private int     panel_y;
        private int     img_width;
        private int     img_height;
        private int[,]  imgArray = new int[16,16];

        // フォーカス用の変数
        private Point   forcusPoint = new Point(0, 0);

        // 画像交換等に必要な関数
        private Point   change1;
        private Point   change2;
        private bool    isSelect = false;

        // 画像の境界線を表示する関数
        private bool    isLine = true;

        // パズドラエンジンのための変数
        private bool    isPuzzdora = false;
        private Point   lastPoint;

        public Form1()
        {
            InitializeComponent();

            // コマンドライン引数を受け取る
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 4)
            {
                filePath = args[1];             // 画像のパス
                div_x = int.Parse(args[2]);     // 断片数 横
                div_y = int.Parse(args[3]);     // 断片数 縦

                // 画像の配列
                for (int i = 4; i < args.Length; i++)
                {
                    int x = (i - 4) % div_x;
                    int y = (i - 4) / div_x;
                    imgArray[x, y] = int.Parse(args[i]);
                }

                // 画像読み込み
                Image Img = Image.FromFile(filePath);
                this.pictureBox1.Size = Img.Size;       // pictureBoxのサイズを画像のサイズに合わせる
                this.ClientSize = pictureBox1.Size;     // FormのサイズをpictureBoxに合わせる
                this.pictureBox1.Image = Img;           // 画像を表示 
                this.pictureBox1.MaximumSize = Img.Size;
                // 断片画像のサイズを取得
                panel_x = Img.Width / div_x;
                panel_y = Img.Height / div_y;

                // 画像サイズ取得
                img_width = Img.Width;
                img_height = Img.Height;
            }
            else
            {
                // ファイルパス取得
                Console.WriteLine("FilePath >> ");
                filePath = Console.ReadLine();

                // 断片数 横 取得
                Console.WriteLine("div_x    >> ");
                div_x = int.Parse(Console.ReadLine());

                // 断片数 縦 取得
                Console.WriteLine("div_y    >> ");
                div_y = int.Parse(Console.ReadLine());

                // 画像の配列取得
                Console.WriteLine("Array    >> ");
                string str = Console.ReadLine();
                string[] ary = str.Split(' ');

                for (int i = 0; i < ary.Length; i++)
                {
                    int x = i % div_x;
                    int y = i / div_x;
                    imgArray[x, y] = int.Parse(ary[i]);
                }

                // 画像読み込み
                Image Img = Image.FromFile(filePath);
                this.pictureBox1.Size = Img.Size;       // pictureBoxのサイズを画像のサイズに合わせる
                this.ClientSize = pictureBox1.Size;     // FormのサイズをpictureBoxに合わせる
                this.pictureBox1.Image = Img;           // 画像を表示 

                // 断片画像のサイズを取得
                panel_x = Img.Width / div_x;
                panel_y = Img.Height / div_y;

                // 画像サイズ取得
                img_width = Img.Width;
                img_height = Img.Height;
            }
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            // デバッグ用
#if DEBUG
            debug();
#endif
        }

#if DEBUG
        private void debug()
        { 
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FilePath     : " + filePath);
            Console.WriteLine("ImageWidth   : " + img_width);
            Console.WriteLine("ImageHeight  : " + img_height);
            Console.WriteLine("div_x        : " + div_x.ToString());
            Console.WriteLine("div_y        : " + div_y.ToString());
            Console.WriteLine("panel_x      : " + panel_x);
            Console.WriteLine("panel_y      : " + panel_y);

            for (int y = 0; y < div_y; y++)
            {
                for (int x = 0; x < div_x; x++)
                {
                    Console.Write(imgArray[x, y]);
                    if (x < div_x - 1)
                    {
                        Console.Write(",");
                    }
                }
                Console.WriteLine("");
            }
        }
#endif
        
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            { 
                case System.Windows.Forms.MouseButtons.Left:
                    Point pos = PixelToPoint(e.Location);
                    if (pos.X >= 0 && pos.Y >= 0)
                    {
                        forcusPoint = pos;
                        SelectPanel();
                    }
                    break;

                case System.Windows.Forms.MouseButtons.Right:
                    unSelectPanel();
                    break;
            }

            // pictureBoxの更新
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // フォーカスを移動
            lastPoint = forcusPoint;

            Point pos = PixelToPoint(e.Location);
            if (pos.X >= 0 && pos.Y >= 0)
            {
                forcusPoint = pos;

                // パズドラエンジン
                if (lastPoint != forcusPoint) // 以前居たパネルと違う場合はパズドラ処理を適用（これの処理なしだと無駄な処理が増える）
                {
                    puzzdora();
                }
            }

            // pictureBoxの更新
            pictureBox1.Invalidate();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // 仕事しないサボりイベント
        }

        private Point PixelToPoint(Point pos)
        {
            // 画面外なら(-1,-1)を返す
            if (pos.X > pictureBox1.Width || pos.X < 0)
            {
                return new Point(-1, -1);
            }

            if (pos.Y > pictureBox1.Height || pos.Y < 0)
            {
                return new Point(-1, -1);
            }

            // 画面座標を変換
            return new Point(pos.X / panel_x, pos.Y / panel_y);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // フォーカス移動
            switch (e.KeyCode)
            {
                case Keys.Left:
                    ForcusLeft();
                    puzzdora();
                    break;
                
                case Keys.Right:
                    ForcusRight();
                    puzzdora();
                    break;
                
                case Keys.Up:
                    ForcusUp();
                    puzzdora();
                    break;
                
                case Keys.Down:
                    ForcusDown();
                    puzzdora();
                    break;
                
                case Keys.Space:
                    SelectPanel();
                    break;

                case Keys.Back:
                    unSelectPanel();
                    break;

                case Keys.Enter:
                    this.Close();
                    break;

                case Keys.L:
                    isLine = !isLine;
                    break;

                case Keys.P:
                    isPuzzdora = true;
                    SelectPanel();
                    break;
            }

            // pictureBoxの更新
            pictureBox1.Invalidate();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                isPuzzdora = false;
                isSelect = false;
            }
            
            // pictureBoxの更新
            pictureBox1.Invalidate();
        }

        // フォーカスを左に移動
        private void ForcusLeft()
        {
            Point p = forcusPoint;
            if (p.X == 0)
            {
                forcusPoint = new Point(div_x - 1, p.Y);
            }
            else
            {
                forcusPoint = new Point(p.X - 1, p.Y);
            }
        }
        
        // フォーカスを右に移動
        private void ForcusRight()
        {
            Point p = forcusPoint;
            if (p.X == div_x - 1)
            {
                forcusPoint = new Point(0, p.Y);
            }
            else
            {
                forcusPoint = new Point(p.X + 1, p.Y);
            }
        }

        // フォーカスを上に移動
        private void ForcusUp()
        {
            Point p = forcusPoint;
            if (p.Y == 0)
            {
                forcusPoint = new Point(p.X, div_y - 1);
            }
            else
            {
                forcusPoint = new Point(p.X, p.Y - 1);
            }
        }

        // フォーカスを下に移動
        private void ForcusDown() 
        {
            Point p = forcusPoint;
            if (p.Y == div_y - 1)
            {
                forcusPoint = new Point(p.X, 0);
            }
            else
            {
                forcusPoint = new Point(p.X, p.Y + 1);
            }
        }

        // 選択
        private void SelectPanel()
        {
            if (!isSelect)
            {
                change1 = forcusPoint;
                isSelect = true;
            }
            else 
            {
                change2 = forcusPoint;
                
                ChangeArray();
                ChangePanel();
            }
        }

        // 選択解除
        private void unSelectPanel()
        {
            isSelect = false;  
        }

        // 交換
        private void ChangePanel()
        {
            Bitmap original = new Bitmap(pictureBox1.Image);
            Bitmap result = new Bitmap(pictureBox1.Image);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(original, panel_x * change1.X, panel_y * change1.Y, new Rectangle(panel_x * change2.X, panel_y * change2.Y, panel_x, panel_y), GraphicsUnit.Pixel);
                g.DrawImage(original, panel_x * change2.X, panel_y * change2.Y, new Rectangle(panel_x * change1.X, panel_y * change1.Y, panel_x, panel_y), GraphicsUnit.Pixel);
            }

            pictureBox1.Image = result;

            if (isPuzzdora)
            {
                change1 = change2;
            }
            else
            { 
                unSelectPanel();  
            }
        }

        private void ChangeArray()
        {
            if (change1.X < 0 || change1.Y < 0)
            {
                isSelect = false;
                return;
            }
            
            int tmp = imgArray[change1.X, change1.Y];   
            imgArray[change1.X, change1.Y] = imgArray[change2.X, change2.Y];
            imgArray[change2.X, change2.Y] = tmp;
        }
        
        private void puzzdora()
        {
            if (isPuzzdora && isSelect)
            {
                SelectPanel();
                ChangeArray();
                ChangePanel();
            }
        }
               

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Pen p = new Pen(Brushes.Red, 1);

            if (isLine)
            { 
                // 縦線を描画
                for (int x = 1; x < div_x; x++)
                {
                    g.DrawLine(p, new Point(panel_x * x, 0), new Point(panel_x * x, img_height));
                }
            
                // 横線を描画
                for (int y = 1; y < div_y; y++)
                {
                    g.DrawLine(p, new Point(0, panel_y * y), new Point(img_width, panel_y * y));
                }
            }
            

            // フォーカスを描画
            Rectangle forcusRect = new Rectangle(panel_x * forcusPoint.X, panel_y * forcusPoint.Y, panel_x, panel_y);
            g.DrawRectangle(new Pen(Brushes.GreenYellow, 2), forcusRect);
        
            // 選択画像のフォーカスを表示
            if (isSelect)
            { 
                Rectangle changeRect1 = new Rectangle(panel_x * change1.X, panel_y * change1.Y, panel_x, panel_y);
                g.DrawRectangle(new Pen(Brushes.Blue, 2), changeRect1);
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int y = 0; y < div_y; y++)
            {
                for (int x = 0; x < div_x; x++)
                {
                    Console.Write(imgArray[x, y] + " ");
                }
            }
            Console.ResetColor();
        }


    }
}
