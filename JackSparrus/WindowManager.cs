using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JackSparrus
{
    class WindowManager
    {
        private static IntPtr DOFUSPTR; 

        public const int WM_KEYDOWN = 0x100;
        public const int WM_CHAR = 0x102;
        public const int WM_KEYUP = 0x101;
        public const int WM_COMMAND = 0x111;
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(
        int hWnd,
        int Msg,
        int wParam,
        IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern int FindWindowEx(
        int hwndParent,
        int hwndChildAfter,
        string strClassName,
        string strWindowName);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        public static void InitWindowManager()
        {
            DOFUSPTR = GetDofusPtr();
        }

        private static IntPtr GetDofusPtr()
        {
            IntPtr dofusPtr = IntPtr.Zero;
            Process process = null;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle.Contains("Dofus"))
                {
                    dofusPtr = pList.MainWindowHandle;
                    process = pList;
                }
            }

            return dofusPtr;
        }

        public static Bitmap CreateScreenBitmap()
        {
            SetForegroundWindow(DOFUSPTR);

            // Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                Screen.PrimaryScreen.Bounds.Height,
                                PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        public static Bitmap CreateScreenBitmap(RECT zone)
        {
            zone.left = Math.Max(0, zone.left);
            zone.top = Math.Max(0, zone.top);

            zone.right = Math.Min(zone.right, Screen.PrimaryScreen.Bounds.Size.Width - 1);
            zone.bottom = Math.Min(zone.bottom, Screen.PrimaryScreen.Bounds.Size.Height - 1);

            int width = zone.right - zone.left + 1;
            int height = zone.bottom - zone.top + 1;


            SetForegroundWindow(DOFUSPTR);

            // Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                Screen.PrimaryScreen.Bounds.Height,
                                PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(zone.left,
                                        zone.top,
                                        0,
                                        0,
                                        new Size(width, height),
                                            CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        public static Size GetScreenSize()
        {
            return Screen.PrimaryScreen.Bounds.Size;
        }

        public static void ToggleTransparency()
        {
            IntPtr NULL = new IntPtr(0);

            WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_KEYDOWN, (int)Keys.F5, NULL);
            Thread.Sleep(100);
            WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_KEYUP, (int)Keys.F5, NULL);
            Thread.Sleep(1000);
        }

        public static void PressReleaseButton(int button)
        {
            IntPtr NULL = new IntPtr(0);

            WindowManager.SendMessage((int)DOFUSPTR, button, (int)Keys.D3, NULL);
            Thread.Sleep(100);
            WindowManager.SendMessage((int)DOFUSPTR, button, (int)Keys.D3, NULL);
        }

        public static void ClickOn(int posX, int posY)
        {
            Random rand = new Random();

            MoveMouseTo(posX, posY);
            WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_LBUTTONDOWN, 0x00000001, CreateLParam(posX, posY));
            Thread.Sleep(100);
            WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_LBUTTONUP, 0x00000001, CreateLParam(posX, posY));
        }

        public static void MoveMouseTo(int posX, int posY)
        {

            float time = 0;
            float maxTime = 500;
            Random rand = new Random();

            POINT mousePos;
            do
            {
                GetCursorPos(out mousePos);

                int offsetPosX;
                int offsetPosY;
                if (time < maxTime)
                {
                    offsetPosX = (int)((posX - mousePos.X) * time / maxTime);
                    offsetPosY = (int)((posY - mousePos.Y) * time / maxTime);
                }
                else
                {
                    offsetPosX = (int)(posX - mousePos.X);
                    offsetPosY = (int)(posY - mousePos.Y);
                }

                SetCursorPos(mousePos.X + offsetPosX, mousePos.Y + offsetPosY);

                int sleepTime = rand.Next(20, 40);
                time += sleepTime;
                Thread.Sleep(sleepTime);
            } while (Math.Abs(mousePos.X - posX) > 2 || Math.Abs(mousePos.X - posX) > 2);
        }

        private static IntPtr CreateLParam(int LoWord, int HiWord)
        {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        public static bool IsFightStarted(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);

            // Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                Screen.PrimaryScreen.Bounds.Height,
                                PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
            {

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);

                // Save the screenshot to the specified path that the user has chosen.
                //bmpScreenshot.Save("fightEtalon.png", ImageFormat.Png);

                //return new List<RECT>();

                int nbGreen = 0;
                for (int i = 1352; i < 1486; i++)
                {
                    for (int j = 982; j < 1025; j++)
                    {
                        Color color = bmpScreenshot.GetPixel(i, j);

                        if (color.G > 50 && color.B < 10)
                        {
                            nbGreen++;
                        }
                    }
                }

                bmpScreenshot.Dispose();

                return nbGreen > 2000;
            }
        }

        public static POINT FindOpponent(IntPtr hWnd)
        {
            POINT result = new POINT();

            SetForegroundWindow(hWnd);

            // Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                Screen.PrimaryScreen.Bounds.Height,
                                PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
            {

                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size,
                                            CopyPixelOperation.SourceCopy);
                bool[,] matrix = new bool[bmpScreenshot.Height, bmpScreenshot.Width];

                //bmpScreenshot = new Bitmap("fightEtalon.png");

                int nbPoints = 0;

                result.X = 0;
                result.Y = 0;

                //Bitmap bit = new Bitmap(bmpScreenshot);

                for (int i = 0; i < bmpScreenshot.Width; i++)
                {
                    for (int j = 0; j < bmpScreenshot.Height; j++)
                    {
                        Color color = bmpScreenshot.GetPixel(i, j);

                        matrix[j, i] = false;
                        if (color.B > 200 && color.R == 0 && color.G == 0)
                        {
                            matrix[j, i] = true;

                            result.X += i;
                            result.Y += j;

                            nbPoints++;
                        }
                    }
                }

                if (nbPoints > 0)
                {
                    result.X /= nbPoints;
                    result.Y /= nbPoints;

                    result.X -= 5;
                    result.Y -= 5;
                }

                //using (Graphics graphics = Graphics.FromImage(bit))
                //{
                //    using (System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                //    {
                //        graphics.FillRectangle(myBrush, new Rectangle(result.X - 10 , result.Y - 10, 20, 20));
                //        //graphics.FillRectangle(myBrush, new Rectangle(978, 261, 6, 200));
                //    }
                //}

                //bit.Save("ScreenshotFight.png", ImageFormat.Png);

                bmpScreenshot.Dispose();

                return result;
            }
        }

        private static readonly List<Color> phorreurColors = new List<Color>
        {
                Color.FromArgb(203, 205, 154), //Horn light
                Color.FromArgb(145, 148, 103), //Horn dark
                Color.FromArgb(49, 89, 77), //Skin light
                Color.FromArgb(37, 56, 48), //Skin medium
                Color.FromArgb(34, 66, 59), //Skin dark
                Color.FromArgb(25, 48, 50), //Skin darkest
                Color.FromArgb(230, 179, 133), //Light noze
                Color.FromArgb(166, 107, 53), //Dark noze & belly
                Color.FromArgb(255, 255, 204), //Eye
        };
        public static List<RECT> FindInterestPoints(Bitmap opaqueScreen, Bitmap transpaScreen)
        {
            

            List<RECT> result = new List<RECT>();

            bool[,] matrix = new bool[opaqueScreen.Width, opaqueScreen.Height];
            Bitmap diffBitmap = new Bitmap(opaqueScreen.Width, opaqueScreen.Height);

            // Find all differencies
            Bitmap bit4 = new Bitmap(opaqueScreen.Width, opaqueScreen.Height);
            for (int x = 300; x < 1600; x++)
            {
                for (int y = 20; y < 920; y++)
                {
                    Color color1 = transpaScreen.GetPixel(x, y);
                    Color color2 = opaqueScreen.GetPixel(x, y);

                    Color diff = Color.FromArgb(Math.Abs(color1.R - color2.R), Math.Abs(color1.G - color2.G), Math.Abs(color1.B - color2.B));

                    if (diff.R > 10 || diff.G > 10 || diff.B > 10)
                    {
                        foreach (Color item in phorreurColors)
                        {
                            int distR = Math.Abs(opaqueScreen.GetPixel(x, y).R - item.R);
                            int distG = Math.Abs(opaqueScreen.GetPixel(x, y).G - item.G);
                            int distB = Math.Abs(opaqueScreen.GetPixel(x, y).B - item.B);
                            if (distR + distG + distB < 50)
                            {
                                matrix[x, y] = true;
                                bit4.SetPixel(x, y, Color.Red);
                            }
                        }
                    }
                    
                    matrix[x, y] = true;
                    diffBitmap.SetPixel(x, y, diff);
                }
            }

            /*
                    //Bitmap bit5 = new Bitmap(image1);
                    for (int j = 0; j < image1.Height; j++)
                    {
                        for (int i = 0; i < image1.Width; i++)
                        {
                            bool color = matrix[j, i];

                            if (alreadyComputedMatrix[j, i] == false && color)
                            {
                                int z = 0;
                                while (z + i < image1.Width && matrix[j, i + z])
                                {
                                    z++;
                                }

                                RECT rectangle = FindRect(matrix, alreadyComputedMatrix, i, i + z - 1, j);

                                if(Math.Abs(rectangle.right - rectangle.left + 1 - 61) < 3)
                                {
                                    result.Add(rectangle);
                                }

                                //if (rectangle.right - rectangle.left + 1 > 20 && rectangle.bottom - rectangle.top + 1 > 20)
                                //// && rectangle.right - rectangle.left + 1 < 100 && rectangle.bottom - rectangle.top + 1 < 500)
                                //{
                                //    if (rectangle.right - rectangle.left + 1 < 300 || rectangle.bottom - rectangle.top + 1 < 300)
                                //    {

                                //        if (rectangle.right > 280 && rectangle.left + 1 < 1640
                                //            && rectangle.bottom < 930)
                                //        {
                                //            Rectangle rect = new Rectangle(rectangle.left, rectangle.top, rectangle.right - rectangle.left + 1, rectangle.bottom - rectangle.top + 1);

                                //            Rectangle waypoint = new Rectangle(1373, 660, 1443 - 1373, 719 - 660);
                                //            if (waypoint.IntersectsWith(rect) == false)
                                //            {
                                //                //using (Graphics graphics = Graphics.FromImage(bit5))
                                //                //{
                                //                //    using (System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                                //                //    {
                                //                //        graphics.FillRectangle(myBrush, new Rectangle(rectangle.left, rectangle.top, rectangle.right - rectangle.left + 1, rectangle.bottom - rectangle.top + 1));
                                //                //        //graphics.FillRectangle(myBrush, new Rectangle(978, 261, 6, 200));
                                //                //    }
                                //                //}

                                //                result.Add(rectangle);
                                //            }
                                //        }
                                //    }
                                //    else
                                //    {
                                //        Random rand = new Random();

                                //        int posX = 1373 - (int)rand.NextDouble() * 4;
                                //        int posY = 660 - (int)rand.NextDouble() * 4;

                                //        MoveMouseTo(posX, posY);

                                //        WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_LBUTTONDOWN, 0x00000001, CreateLParam(posX, posY));
                                //        Thread.Sleep(100);
                                //        WindowManager.SendMessage((int)DOFUSPTR, WindowManager.WM_LBUTTONUP, 0x00000001, CreateLParam(posX + (int)(rand.NextDouble() * 4) - 2, posY + (int)(rand.NextDouble() * 4) - 2));
                                //    }
                                //}
                            }
                        }
                    }


                     */
            return result;
        }

        private static RECT FindRect(bool[,] matrix, bool[,] alreadyComputedMatrix, int startIndex, int endIndex, int row)
        {
            RECT result = new RECT();
            while (startIndex - 1 >= 0 && matrix[row, startIndex - 1])
            {
                startIndex--;
            }

            while (endIndex + 1 < matrix.GetLength(1) && matrix[row, endIndex + 1])
            {
                endIndex++;
            }

            int min = startIndex;
            int max = endIndex;
            int minRow = row;
            int maxRow = row;

            int minUnderRow = -1;
            int maxUnderRow = -1;
            for (int z = startIndex; z <= endIndex; z++)
            {
                if (alreadyComputedMatrix[row, z] == false)
                {
                    alreadyComputedMatrix[row, z] = true;
                    if (row + 1 < matrix.GetLength(0))
                    {
                        if (matrix[row + 1, z])
                        {
                            if (minUnderRow < 0)
                            {
                                minUnderRow = z;
                            }
                            maxUnderRow = z;
                        }
                    }
                }
            }

            if (minUnderRow >= 0)
            {
                RECT childRect = FindRect(matrix, alreadyComputedMatrix, minUnderRow, maxUnderRow, row + 1);

                if (childRect.left < min)
                {
                    min = childRect.left;
                }

                if (childRect.right > max)
                {
                    max = childRect.right;
                }

                maxRow = childRect.bottom;
            }

            result.left = min;
            result.right = max;
            result.top = row;
            result.bottom = maxRow;

            return result;
        }
    }
}
