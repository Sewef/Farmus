using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JackSparrus.WindowManager;

namespace JackSparrus.State
{
    public class GoPhorreurState: AState
    {
        public override void RunState(MainWindow window)
        {
            base.RunState(window);

            TreasureRow row = window.TreasureHub.GetCurrentTreasureRow();

            Random rand = new Random();

            int i = 0;

            bool phorreurFound = false;
            do
            {
                this.ClickOnNextArea(row.Direction);
                Thread.Sleep(5000 + rand.Next(0, 500));

                MoveMouseTo(0, 0); // Prevent HUD on screenshot
                Bitmap areaScreenshot1 = WindowManager.CreateScreenBitmap();

                // F5 and find the differences
                WindowManager.ToggleTransparency();

                Bitmap areaScreenshot2 = WindowManager.CreateScreenBitmap();

                WindowManager.ToggleTransparency();

                List<POINT> interestPoints =  WindowManager.FindInterestPoints(areaScreenshot1, areaScreenshot2);
                foreach(POINT point in interestPoints)
                {
                    SetCursorPos(point.X, point.Y); // Need faster move
                    Thread.Sleep(250);

                    RECT areaPhorreur = new RECT
                    {
                        x1 = point.X - 45,
                        y1 = point.X + 45,
                        x2 = point.Y - 202,
                        y2 = point.Y + 202
                    };

                    if (!phorreurFound)
                    {
                        Bitmap phorreurScreenshot = WindowManager.CreateScreenBitmap(areaPhorreur);

                        if (this.IsTherePhorreur(phorreurScreenshot))
                        {
                            // TODO : Check current phorreur with hint

                            //phorreurScreenshot.Save("Screenshot3.png", ImageFormat.Png);
                            //Console.WriteLine();

                            //string text = window.TreasureHub.GetTextIn(phorreurScreenshot);

                            //Console.WriteLine(text);

                            window.TreasureHub.ValidateRow(row.Direction, i + 1);

                            Thread.Sleep(2000);

                            this.NextStateId = "getHint";

                            phorreurFound = true;
                        }

                        phorreurScreenshot.Dispose();
                    }

                    if (phorreurFound) break;
                }

                areaScreenshot1.Dispose();
                areaScreenshot2.Dispose();

                i++;
            }
            while (phorreurFound == false && i < 10);
        }

        private bool IsTherePhorreur(Bitmap phorreurScreenshot)
        {
            // Find that orange font
            Color colorPhorreur = Color.FromArgb(254, 173, 0);

            for (int i = 0; i < phorreurScreenshot.Width; i++)
            {
                for (int j = 0; j < phorreurScreenshot.Height; j++)
                {
                    Color color = phorreurScreenshot.GetPixel(i, j);

                    if(color == colorPhorreur)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
