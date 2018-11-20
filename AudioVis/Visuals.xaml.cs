using LiveCharts;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioVis
{
    /// <summary>
    /// Interaction logic for Visuals.xaml
    /// </summary>
    public partial class Visuals : UserControl
    {
        public ChartValues<double> ValuesBlue { get; set; }
        public ChartValues<double> ValuesYellow { get; set; }
        private const double _maxCount = 100;
        public double MaxCount => _maxCount;
        public byte Count { get; set; }



        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Visuals), new PropertyMetadata(""));



        public Visuals()
        {
            ValuesBlue = new ChartValues<double>();
            ValuesBlue.Add(0);
            Count = 0;
            ValuesYellow = new ChartValues<double>();
            ValuesYellow.Add(0);
            InitializeComponent();
        }

        public void AddValues(ushort blue, ushort yellow)
        {
            var colors = FromTwoValues(blue, yellow);
            ColorRectangle.Fill = new SolidColorBrush(colors.Item3);
            ColorRectangleLeft.Fill = new SolidColorBrush(colors.Item1);
            if (yellow > 0)
            {
                ColorRectangleRight.Fill = new SolidColorBrush(colors.Item2);
            }
            ValuesBlue.Add((byte)blue);
            ValuesYellow.Add((byte)yellow);
            Count++;
            if (Count >= MaxCount)
            {
                ValuesBlue.RemoveAt(0);
                ValuesYellow.RemoveAt(0);
                Count--;
            }
        }

        public static Color HlsToColor(double h, double l, double s)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            return Color.FromRgb((byte)(double_r * 255.0), (byte)(double_g * 255.0), (byte)(double_b * 255.0));
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }

        public static Tuple<Color, Color, Color> FromTwoValues(ushort blue, ushort yellow)
        {
            float bCoeff = (float)(blue / 255.0);
            var bComp = HlsToColor(210, bCoeff, 1);
            float yCoeff = (float)(yellow / 255.0);
            var yComp = HlsToColor(60, yCoeff, 1);
            var outColor = Color.Multiply(bComp, 0.5f) + Color.Multiply(yComp, 0.5f);
            return new Tuple<Color, Color, Color>(bComp, yComp, outColor);
        }
    }
}
