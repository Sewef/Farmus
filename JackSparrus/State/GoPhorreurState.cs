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

            bool isTherePhorreur = false;
            do
            {
                this.ClickOnNextArea(row.Direction);
                Thread.Sleep(8000 + rand.Next(0, 500));

                Bitmap areaScreenshot1 = WindowManager.CreateScreenBitmap();
                WindowManager.OpenClosePlaneBag();
                Bitmap areaScreenshot2 = WindowManager.CreateScreenBitmap();
                //this.ClickOnNextArea(TreasureHub.GetReverseDirection(row.Direction));
                //Thread.Sleep(2000 + rand.Next(0, 500));
                //this.ClickOnNextArea(row.Direction);
                //Thread.Sleep(2000 + rand.Next(0, 500));
                //Bitmap bitmap2 = WindowManager.CreateScreenBitmap();

                List<RECT> interestPoints =  WindowManager.FindInterestPoints(areaScreenshot1, areaScreenshot2);
                foreach(RECT interestPoint in interestPoints)
                {
                    Point point = new Point((interestPoint.right + interestPoint.left) / 2, (interestPoint.bottom + interestPoint.top) / 2);

                    WindowManager.MoveMouseTo(point.X, point.Y);

                    RECT areaPhorreur = new RECT();
                    areaPhorreur.left = interestPoint.left - 202;
                    areaPhorreur.right = interestPoint.right + 202;
                    areaPhorreur.top = interestPoint.top - 45;
                    areaPhorreur.bottom = interestPoint.bottom + 45;

                    if (isTherePhorreur == false)
                    {
                        Bitmap phorreurScreenshot = WindowManager.CreateScreenBitmap(areaPhorreur);

                        if (this.IsTherePhorreur(phorreurScreenshot))
                        {
                            //phorreurScreenshot.Save("Screenshot3.png", ImageFormat.Png);
                            //Console.WriteLine();

                            //string text = window.TreasureHub.GetTextIn(phorreurScreenshot);

                            //Console.WriteLine(text);

                            window.TreasureHub.ValidateRow(row.Direction, i + 1);

                            Thread.Sleep(2000);

                            this.NextStateId = "getHint";

                            isTherePhorreur = true;
                        }

                        phorreurScreenshot.Dispose();
                    }
                }

                areaScreenshot1.Dispose();
                areaScreenshot2.Dispose();

                i++;
            }
            while (isTherePhorreur == false && i < 10);
        }

        private bool IsTherePhorreur(Bitmap phorreurScreenshot)
        {
            Color colorPhorreur = System.Drawing.ColorTranslator.FromHtml("#FEAD00");

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
