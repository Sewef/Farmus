using Patagames.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Farmus
{
    public class TreasureHub
    {
        private Bitmap hubStartBitmap;

        private Bitmap[] arrowBitmaps;

        private Bitmap[] flagBitmaps;

        private Point startFirstRow;

        private OcrApi tEngine;

        public ObservableCollection<TreasureRow> Rows
        {
            get;
            private set;
        }

        public TreasureHub()
        {
            hubStartBitmap = new Bitmap(@"Assets\hubStart.png");

            this.arrowBitmaps = new Bitmap[]
            {
                new Bitmap(@"Assets\arrowUp.png"),
                new Bitmap(@"Assets\arrowRight.png"),
                new Bitmap(@"Assets\arrowDown.png"),
                new Bitmap(@"Assets\arrowLeft.png")
            };

            this.flagBitmaps = new Bitmap[]
            {
                new Bitmap(@"Assets\flagFill.png"),
                new Bitmap(@"Assets\flagEmpty.png")
            };

            this.Rows = new ObservableCollection<TreasureRow>();

            this.tEngine = OcrApi.Create();
            this.tEngine.Init(Patagames.Ocr.Enums.Languages.French);
        }

        public void UpdateHubFrom(Bitmap screen)
        {
            this.Rows.Clear();

            Bitmap startBitmap = screen.Clone(new Rectangle(0, 0, 50, 50), screen.PixelFormat);
            Point hubStartPoint = SearchPattern(startBitmap, this.hubStartBitmap, out float bestResult);
            startBitmap.Dispose();

            this.startFirstRow = new Point(hubStartPoint.X + 23, hubStartPoint.Y + 69);

            Point startRow = this.startFirstRow;

            Bitmap iconBitmap = screen.Clone(new Rectangle(startRow.X, startRow.Y, 22, 31), screen.PixelFormat);
            Bitmap flagBitmap = screen.Clone(new Rectangle(startRow.X + 288, startRow.Y, 19, 31), screen.PixelFormat);
            Bitmap rowBitmap = screen.Clone(new Rectangle(startRow.X + 22, startRow.Y, 285 - 19, 31), screen.PixelFormat);

            int i = 0;
            bool isActiveRow = true;
            while(this.IsEndRow(rowBitmap) == false)
            {
                if (i != 0 && isActiveRow)
                {
                    isActiveRow = this.IsFlagFill(flagBitmap);
                }

                Direction rowDirection = Direction.NONE;
                if (isActiveRow)
                {
                    if (i == 0)
                    {
                        rowDirection = Direction.START;
                    }
                    else
                    {
                        rowDirection = this.SearchDirection(iconBitmap);
                    }
                }

                string rowText = "?";
                if (isActiveRow)
                {
                    int newWidth = this.GetWidthTextRow(rowBitmap, i);
                    Bitmap newTextBitmap = rowBitmap.Clone(new Rectangle(0, 0, newWidth, rowBitmap.Height), screen.PixelFormat);
                    //newTextBitmap.Save("test.png", ImageFormat.Png);
                    rowText = this.tEngine.GetTextFromImage(newTextBitmap).Trim();
                    newTextBitmap.Dispose();
                }

                this.Rows.Add(new TreasureRow(rowDirection, rowText, isActiveRow));

                startRow.Y += 31;

                iconBitmap.Dispose();
                flagBitmap.Dispose();
                rowBitmap.Dispose();
                iconBitmap = screen.Clone(new Rectangle(startRow.X, startRow.Y, 22, 31), screen.PixelFormat);
                flagBitmap = screen.Clone(new Rectangle(startRow.X + 288, startRow.Y, 19, 31), screen.PixelFormat);
                rowBitmap = screen.Clone(new Rectangle(startRow.X + 22, startRow.Y, 285 - 19, 31), screen.PixelFormat);

                i++;
            }

            iconBitmap.Dispose();
            flagBitmap.Dispose();
            rowBitmap.Dispose();
        }

        private int GetWidthTextRow(Bitmap rowBitmap, int offsetJ)
        {
            int i = rowBitmap.Width - 1;

            while(i >= 0 && this.IsThereTextInColumn(rowBitmap, i, rowBitmap.Height / 2 - offsetJ) == false)
            {
                i--;
            }

            return Math.Min(rowBitmap.Width, i + 10);
        }

        private bool IsThereTextInColumn(Bitmap rowBitmap, int i, int j)
        {
            //int minJ = rowBitmap.Height;
            //int maxJ = 0;
            //for (int j = 0; j < rowBitmap.Height; j++)
            //{
            //    Color colorPixel = rowBitmap.GetPixel(row, j);

            //    if (colorPixel.R + colorPixel.G + colorPixel.B < 200)
            //    {
            //        if(j > maxJ)
            //        {
            //            maxJ = j;
            //        }
            //        if (j < minJ)
            //        {
            //            minJ = j;
            //        }
            //    }
            //}

            Color colorPixel = rowBitmap.GetPixel(i, j);

            if (colorPixel.R + colorPixel.G + colorPixel.B < 200)
            {
                return true;
            }

            return false; // maxJ > minJ && rowBitmap.Height - maxJ > 1;
        }

        private bool IsFlagFill(Bitmap image)
        {
            int i = 0;
            int bestDirectionIndex = -1;
            float bestResult = float.MaxValue;
            foreach (Bitmap pattern in this.flagBitmaps)
            {
                SearchPattern(image, pattern, out float result);

                if (result < bestResult)
                {
                    bestDirectionIndex = i;

                    bestResult = result;
                }

                i++;
            }

            return bestDirectionIndex == 0;
        }

        private Direction SearchDirection(Bitmap image)
        {
            int i = 0;
            int bestDirectionIndex = -1;
            float bestResult = float.MaxValue;
            foreach(Bitmap pattern in this.arrowBitmaps)
            {
                SearchPattern(image, pattern, out float result);

                if(result < bestResult)
                {
                    bestDirectionIndex = i;

                    bestResult = result;
                }

                i++;
            }

            return (Direction) bestDirectionIndex;
        }

        private bool IsEndRow(Bitmap row)
        {
            int mean = 0;
            for (int i = 0; i < row.Width; i++)
            {
                Color color = row.GetPixel(i, 16);
                mean += color.R + color.G + color.B;
            }

            mean /= row.Width;

            return mean < 200;
        }

        public Point SearchPattern(Bitmap screen, Bitmap pattern, out float bestResult)
        {
            bestResult = float.MaxValue;

            Point startHubPoint = new Point();

            for (int i = 0; i < screen.Width - pattern.Width + 1; i++)
            {
                for (int j = 0; j < screen.Height - pattern.Height + 1; j++)
                {

                    float comparaisonResult = 0;
                    for (int x = 0; x < pattern.Width; x++)
                    {
                        for (int y = 0; y < pattern.Height; y++)
                        {
                            Color colorScreen = screen.GetPixel(i + x, j + y);
                            Color colorPattern = pattern.GetPixel(x, y);

                            int diffR = colorScreen.R - colorPattern.R;
                            int diffG = colorScreen.G - colorPattern.G;
                            int diffB = colorScreen.B - colorPattern.B;

                            comparaisonResult += diffR * diffR + diffG * diffG + diffB * diffB;
                        }
                    }

                    if(comparaisonResult < bestResult)
                    {
                        startHubPoint.X = i;
                        startHubPoint.Y = j;

                        bestResult = comparaisonResult;
                    }
                }
            }

            return startHubPoint;
        }

    }

    public class TreasureRow
    {
        Direction Direction
        {
            get;
            set;
        }

        string RowText
        {
            get;
            set;
        }

        bool FlagActivated
        {
            get;
            set;
        }

        public TreasureRow(Direction direction, string rowText, bool flagActivated)
        {
            this.Direction = direction;
            this.RowText = rowText;
            this.FlagActivated = flagActivated;
        }
    }

    public enum Direction
    {
        NONE = -2,
        START = -1,
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3
    }
}
