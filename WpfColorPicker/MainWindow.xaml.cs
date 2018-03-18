using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;

using LusColor;
using System.Reflection;

namespace WpfColorPicker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap bmp_hsv_board;
        private WriteableBitmap bmp_hsv_bar;

        private float Hvalue = 0.0f;
        private LusColor.Color SelectedColor = new LusColor.Color();


        private bool IsBarSelect { get; set; }

        public int dpiX;
        public int dpiY;

        public MainWindow()
        {
            SelectedColor.hsv = new HSV(0, 0, 0);
            
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            dpiX = (int)dpiXProperty.GetValue(null, null);
            dpiY = (int)dpiYProperty.GetValue(null, null);

            InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
        //  if (e.LeftButton == MouseButtonState.Pressed)
        //  {
        //      DragMove();
        //      return;
        //  }

            if (e.RightButton == MouseButtonState.Pressed)
                Application.Current.Shutdown();
        }

        private void CreateHSVBoard()
        {
            HSV hsv = new HSV(Hvalue, 0.0f, 1.0f);

            float stepW = 1.0f / ((float)bmp_hsv_board.Width - 1.0f);
            float stepH = 1.0f / ((float)bmp_hsv_board.Height - 1.0f);

            try
            {
                bmp_hsv_board.Lock();
                unsafe
                {
                    int* pStart = (int*)bmp_hsv_board.BackBuffer;

                    for (int y = 0; y < bmp_hsv_board.PixelHeight; ++y)
                    {
                        hsv.s = 0.0f;
                 
                        for (int x = 0; x < bmp_hsv_board.PixelWidth; ++x)
                        {
                            RGB rgb = LusColor.Color.Convert(hsv);
                            int pixel = 0;
                            pixel |= rgb.r << 16;  // R
                            pixel |= rgb.g << 8;   // G
                            pixel |= rgb.b << 0;   // B

                            *(pStart) = pixel;

                            pStart++;
                            hsv.s += stepW;
                        }
                        hsv.v -= stepH; 
                    }
                    bmp_hsv_board.AddDirtyRect(new Int32Rect(0, 0, bmp_hsv_board.PixelWidth, bmp_hsv_board.PixelHeight));
                }
            }
            finally
            {
                bmp_hsv_board.Unlock();
            }
        }

        private void CreateHSVBar()
        {
            float factor = 360.0f / (float)bmp_hsv_bar.Height;

            unsafe
            {
                bmp_hsv_bar.Lock();
                int* pStart = (int*)bmp_hsv_bar.BackBuffer;

                for (int y = 0; y < bmp_hsv_bar.PixelHeight; ++y)
                {
                    RGB rgb = LusColor.Color.Convert(new HSV((bmp_hsv_bar.PixelHeight - (y + 1)) * factor, 1.0f, 1.0f));
                    int pixel = 0;
                    pixel |= rgb.r << 16;  // R
                    pixel |= rgb.g << 8;   // G
                    pixel |= rgb.b << 0;   // B
                    for (int x = 0; x < bmp_hsv_bar.PixelWidth; ++x)
                    {
                        *(pStart) = pixel;
                        pStart++;
                    }
                }

                bmp_hsv_bar.AddDirtyRect(new Int32Rect(0, 0, bmp_hsv_bar.PixelWidth, bmp_hsv_bar.PixelHeight));
                bmp_hsv_bar.Unlock();
            }
        }


        private void Board_Init(object sender, EventArgs e)
        {
            bmp_hsv_board = new WriteableBitmap((int)image_board.Width, (int)image_board.Height, dpiX, dpiY, PixelFormats.Bgr32, null);
            
            CreateHSVBoard();
            image_board.Source = bmp_hsv_board;
            image_board.UpdateLayout();
        }

        private void Bar_Init(object sender, EventArgs e)
        {
            bmp_hsv_bar = new WriteableBitmap((int)image_bar.Width, (int)image_bar.Height, dpiX, dpiY, PixelFormats.Bgr32, null);

            CreateHSVBar();
            image_bar.Source = bmp_hsv_bar;
        }

        private void BarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            IsBarSelect = true;
            float y = (float)(e.GetPosition(image_bar).Y);

            RefreshHSVBoard(y);
        }

        private void RefreshHSVBoard(float y)
        {
            float old = Hvalue;
            float Height = (float)bmp_hsv_bar.Height;
            y = Math.Max(Math.Min(Height - y, Height), 0);
            Hvalue = y * 360.0f / Height ;

            if (Math.Abs(old - Hvalue) > 1.0f)
            {
                CreateHSVBoard();
                image_board.Source = bmp_hsv_board;
            }
        }

        private void BarMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsBarSelect) return;

            float y = (float)(e.GetPosition(image_bar).Y);
            RefreshHSVBoard(y);
        }

        private void BarMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsBarSelect) return;
            IsBarSelect = false;
        }

    }

}
