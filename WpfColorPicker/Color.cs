using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LusColor
{
    public struct RGB
    {
        public byte b;
        public byte g;
        public byte r;

        public RGB(byte R, byte G, byte B)
        {
            r = R;
            g = G;
            b = B;
        }
    }

    public struct HSV
    {
        public float h;
        public float s;
        public float v;

        public HSV(float H, float S, float V)
        {
            h = H;
            s = S;
            v = V;
        }
    }

    public class Color
    {

        private HSV _hsv = new HSV();
        private RGB _rgb = new RGB();

        public HSV hsv
        {
            get { return _hsv; }
            set { _hsv = value; _rgb = Convert(value); }
        }

        public RGB rgb
        {
            get { return _rgb; }
            set { _rgb = value; _hsv = Convert(value); }
        }

        public Color()
        {
            rgb = new RGB(0, 0, 0);
            hsv = Convert(rgb);
        }

        static public RGB Convert(HSV hsv)
        {
            RGB rgb = new RGB();

            byte v = (byte)(255 * hsv.v);
            // achromatic (grey)
            if (hsv.s == 0.0f)
            {
                rgb.r = v;
                rgb.g = v;
                rgb.b = v;
                return rgb;
            }

            // sector 0 to 5
            hsv.h /= 60.0f;

            int i = (int)Math.Floor(hsv.h);

            // factorial part of h
            float f = hsv.h - i;
            byte p = (byte)(255 * (hsv.v * (1.0f - hsv.s)));
            byte q = (byte)(255 * (hsv.v * (1.0f - hsv.s * f)));
            byte t = (byte)(255 * (hsv.v * (1.0f - hsv.s * (1.0f - f))));

            switch (i)
            {
                case 0: rgb.r = v; rgb.g = t; rgb.b = p; break;
                case 1: rgb.r = q; rgb.g = v; rgb.b = p; break;
                case 2: rgb.r = p; rgb.g = v; rgb.b = t; break;
                case 3: rgb.r = p; rgb.g = q; rgb.b = v; break;
                case 4: rgb.r = t; rgb.g = p; rgb.b = v; break;
                case 5: rgb.r = v; rgb.g = p; rgb.b = q; break;
            }

            return rgb;
        }

        static public HSV Convert(RGB rgb)
        {
            HSV hsv = new HSV();

            float min = Math.Min(Math.Min(rgb.r, rgb.g), rgb.b) / 255.0f;
            float max = Math.Max(Math.Max(rgb.r, rgb.g), rgb.b) / 255.0f;
            float delta = max - min;

            // between yellow & magenta
            if (rgb.r / 255.0f == max) hsv.h = 0.0f + (float)(rgb.g - rgb.b) / delta;
            // between cyan & yellow
            else if (rgb.g / 255.0f == max) hsv.h = 2.0f + (float)(rgb.b - rgb.r) / delta;
            // between magenta & cyan
            else hsv.h = 4.0f + (float)(rgb.r - rgb.g) / delta;

            // degrees
            hsv.h *= 60.0f;

            if (hsv.h < 0) hsv.h += 360;

            // s
            if (max != 0) hsv.s = delta / max;
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                hsv.s = 0;
                hsv.h = -1.0f;
                return hsv;
            }

            // v
            hsv.v = max;

            return hsv;
        }
    }
}
