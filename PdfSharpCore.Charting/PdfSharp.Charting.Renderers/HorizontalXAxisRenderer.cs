#region PDFsharp Charting - A .NET charting library based on PDFsharp
//
// Authors:
//   Niklas Schneider (mailto:Niklas.Schneider@PdfSharpCore.com)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.PdfSharpCore.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using PdfSharpCore.Drawing;

namespace PdfSharpCore.Charting.Renderers
{
    /// <summary>
    /// Represents an axis renderer used for charts of type Column2D or Line.
    /// </summary>
    internal class HorizontalXAxisRenderer : XAxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the HorizontalXAxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal HorizontalXAxisRenderer(RendererParameters parms) : base(parms)
        {
        }

        /// <summary>
        /// Returns an initialized rendererInfo based on the X axis.
        /// </summary>
        internal override RendererInfo Init()
        {
            Chart chart = (Chart)this.rendererParms.DrawingItem;

            AxisRendererInfo xari = new AxisRendererInfo();
            xari.axis = chart.xAxis;

            if (xari.axis != null)
            {
                ChartRendererInfo cri = (ChartRendererInfo)this.rendererParms.RendererInfo;
                bool isxy = false;
                Series s = chart.seriesCollection[0];
                if (s != null && s.Elements.getPointX(0) != null)
                {
                    isxy = true;
                    InitScale(xari);
                    if (xari.axis != null)
                    {
                        InitTickLabels(xari, cri.DefaultFont);
                    }
                }

                if (!isxy)
                {
                    CalculateXAxisValues(xari);
                    InitTickLabels(xari, cri.DefaultFont);
                    InitXValues(xari);
                }
                InitAxisTitle(xari, cri.DefaultFont);
                InitAxisLineFormat(xari);
                InitGridlines(xari);
            }
            return xari;
        }

        /// <summary>
        /// Calculates the space used for the X axis.
        /// </summary>
        internal override void Format()
        {
            AxisRendererInfo xari = ((ChartRendererInfo)this.rendererParms.RendererInfo).xAxisRendererInfo;
            if (xari.axis != null)
            {
                AxisTitleRendererInfo atri = xari.axisTitleRendererInfo;

                // Calculate space used for axis title.
                XSize titleSize = new XSize(0, 0);
                if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
                {
                    titleSize = this.rendererParms.Graphics.MeasureString(atri.AxisTitleText, atri.AxisTitleFont);
                    atri.AxisTitleSize = titleSize;
                }
                Chart chart = (Chart)this.rendererParms.DrawingItem;

                XSize size = new XSize(0, 0);

                bool isxy = false;
                Series s = chart.seriesCollection[0];
                if (s != null && s.Elements.getPointX(0) != null)
                {
                    isxy = true;
                    // width of all ticklabels
                    double xMin = xari.MinimumScale;
                    double xMax = xari.MaximumScale;
                    double xMajorTick = xari.MajorTick;
                    double lineHeight = Double.MinValue;
                    XSize labelSize = new XSize(0, 0);
                    XGraphics gfx = this.rendererParms.Graphics;

                    for (double x = xMin; x <= xMax; x += xMajorTick)
                    {
                        string str = x.ToString(xari.TickLabelsFormat);
                        labelSize = gfx.MeasureString(str, xari.TickLabelsFont);
                        size.Width += labelSize.Width;
                        size.Height = Math.Max(size.Height, labelSize.Height);
                        lineHeight = Math.Max(lineHeight, labelSize.Height);
                    }

                    // add space for tickmarks
                    size.Width += xari.MajorTickMarkWidth * 1.5;

                }

                if (!isxy)
                {
                    // Calculate space used for tick labels.
                    if (xari.XValues.Count > 0)
                    {
                        XSeries xs = xari.XValues[0];
                        foreach (XValue xv in xs)
                        {
                            if (xv != null)
                            {
                                string tickLabel = xv.Value;
                                XSize valueSize = this.rendererParms.Graphics.MeasureString(tickLabel, xari.TickLabelsFont);
                                size.Height = Math.Max(valueSize.Height, size.Height);
                                size.Width += valueSize.Width;
                            }
                        }
                    }

                    // Remember space for later drawing.
                    xari.TickLabelsHeight = size.Height;
                    xari.Height = titleSize.Height + size.Height + xari.MajorTickMarkWidth;
                    xari.Width = Math.Max(titleSize.Width, size.Width);
                }
            }
        }

        /// <summary>
        /// Draws the horizontal X axis.
        /// </summary>
        internal override void Draw()
        {
            XGraphics gfx = this.rendererParms.Graphics;
            ChartRendererInfo cri = (ChartRendererInfo)this.rendererParms.RendererInfo;
            AxisRendererInfo xari = cri.xAxisRendererInfo;
            LineFormatRenderer lineFormatRenderer = new LineFormatRenderer(gfx, xari.LineFormat);

            double xMin = xari.MinimumScale;
            double xMax = xari.MaximumScale;
            double xMajorTick = xari.MajorTick;
            double xMinorTick = xari.MinorTick;
            double xMaxExtension = xari.MajorTick;

            bool isxy = false;

            double majorTickMarkStart = 0, majorTickMarkEnd = 0,
                   minorTickMarkStart = 0, minorTickMarkEnd = 0;
            GetTickMarkPos(xari, ref majorTickMarkStart, ref majorTickMarkEnd, ref minorTickMarkStart, ref minorTickMarkEnd);

            XPoint[] points = new XPoint[2];

            Chart chart = (Chart)this.rendererParms.DrawingItem;
            Series s = chart.seriesCollection[0];
            double tickLabelStep = xari.Width;
            int countTickLabels = (int)xMax;  // for non-xy

            if (countTickLabels != 0)
                tickLabelStep = xari.Width / countTickLabels;

            XPoint startPos = new XPoint(xari.X + tickLabelStep / 2, xari.Y + xari.TickLabelsHeight);

            if (s != null && s.Elements.getPointX(0) != null)
            {
                isxy = true;
                countTickLabels = (int)((xMax - xMin) / xMajorTick) + 1;
                if (countTickLabels != 0)
                    tickLabelStep = xari.Width / countTickLabels;
                startPos = new XPoint(0, xari.Y + xari.TickLabelsHeight);

                XMatrix matrix = new XMatrix();  //XMatrix.Identity;
                                                 //matrix.TranslatePrepend(xari.X, xari.InnerRect.Y ); //-xari.InnerRect.X, xMax);
                matrix.Scale(xari.InnerRect.Width / (xMax - xMin), 1, XMatrixOrder.Append);
                //matrix.ScalePrepend( 1, 1); // mirror Vertical
                matrix.Translate(xari.InnerRect.X, xari.InnerRect.Y, XMatrixOrder.Append);


                if (xari.MajorTickMark != TickMarkType.None)
                    startPos.Y += xari.MajorTickMarkWidth;

                //matrix.OffsetX = startPos.X;

                // Draw axis.
                // First draw tick marks, second draw axis.
                LineFormatRenderer minorTickMarkLineFormat = new LineFormatRenderer(gfx, xari.MinorTickMarkLineFormat);
                LineFormatRenderer majorTickMarkLineFormat = new LineFormatRenderer(gfx, xari.MajorTickMarkLineFormat);

                // Draw minor tick marks.
                if (xari.MinorTickMark != TickMarkType.None)
                {
                    for (double x = xMin + xMinorTick; x < xMax; x += xMinorTick)
                    {
                        points[0].X = x;
                        points[0].Y = minorTickMarkStart;
                        points[1].X = x;
                        points[1].Y = minorTickMarkEnd;
                        matrix.TransformPoints(points);
                        minorTickMarkLineFormat.DrawLine(points[0], points[1]);
                    }
                }

                double lineSpace = xari.TickLabelsFont.GetHeight();
                int cellSpace = xari.TickLabelsFont.FontFamily.GetLineSpacing(xari.TickLabelsFont.Style);
                double xHeight = xari.TickLabelsFont.Metrics.XHeight;

                XSize labelSize = new XSize(0, 0);
                labelSize.Height = lineSpace * xHeight / cellSpace;

                for (int i = 0; i < countTickLabels; ++i)
                {
                    double x = xMin + xMajorTick * i;
                    string str = x.ToString(xari.TickLabelsFormat);
                    labelSize = gfx.MeasureString(str, xari.TickLabelsFont);

                    // Draw major tick marks.
                    if (xari.MajorTickMark != TickMarkType.None)
                    {
                        labelSize.Width += xari.MajorTickMarkWidth * 1.5;
                        points[0].X = x;
                        points[0].Y = 0; // majorTickMarkStart;
                        points[1].X = x;
                        points[1].Y = 0; // majorTickMarkEnd;
                        matrix.TransformPoints(points);
                        points[1].Y += xari.MajorTickMarkWidth;

                        majorTickMarkLineFormat.DrawLine(points[0], points[1]);
                    }
                    else
                        labelSize.Height += SpaceBetweenLabelAndTickmark;

                    // Draw label text.
                    XPoint[] layoutText = new XPoint[1];
                    layoutText[0].X = x;
                    layoutText[0].Y = 0;
                    matrix.TransformPoints(layoutText);
                    layoutText[0].Y += labelSize.Height;
                    layoutText[0].X -= labelSize.Width / 2; // Center text horizontally
                    gfx.DrawString(str, xari.TickLabelsFont, xari.TickLabelsBrush, layoutText[0]);
                }
            }


            if (!isxy)
            {
                // Draw tick labels. Each tick label will be aligned centered.

                //XPoint startPos = new XPoint(xari.X + tickLabelStep / 2, xari.Y + /*xari.TickLabelsHeight +*/ xari.MajorTickMarkWidth);
                if (xari.MajorTickMark != TickMarkType.None)
                    startPos.Y += xari.MajorTickMarkWidth;

                foreach (XSeries xs in xari.XValues)
                {
                    for (int idx = 0; idx < countTickLabels && idx < xs.Count; ++idx)
                    {
                        XValue xv = xs[idx];
                        if (xv != null)
                        {
                            string tickLabel = xv.Value;
                            XSize size = gfx.MeasureString(tickLabel, xari.TickLabelsFont);
                            gfx.DrawString(tickLabel, xari.TickLabelsFont, xari.TickLabelsBrush, startPos.X - size.Width / 2, startPos.Y);
                        }
                        startPos.X += tickLabelStep;
                    }
                }

                // Minor ticks.
                if (xari.MinorTickMark != TickMarkType.None)
                {
                    int countMinorTickMarks = (int)(xMax / xMinorTick);
                    double minorTickMarkStep = xari.Width / countMinorTickMarks;
                    startPos.X = xari.X;
                    for (int x = 0; x <= countMinorTickMarks; x++)
                    {
                        points[0].X = startPos.X + minorTickMarkStep * x;
                        points[0].Y = minorTickMarkStart;
                        points[1].X = points[0].X;
                        points[1].Y = minorTickMarkEnd;
                        lineFormatRenderer.DrawLine(points[0], points[1]);
                    }
                }

                // Major ticks.
                if (xari.MajorTickMark != TickMarkType.None)
                {
                    int countMajorTickMarks = (int)(xMax / xMajorTick);
                    double majorTickMarkStep = xari.Width;
                    if (countMajorTickMarks != 0)
                        majorTickMarkStep = xari.Width / countMajorTickMarks;
                    startPos.X = xari.X;
                    for (int x = 0; x <= countMajorTickMarks; x++)
                    {
                        points[0].X = startPos.X + majorTickMarkStep * x;
                        points[0].Y = majorTickMarkStart;
                        points[1].X = points[0].X;
                        points[1].Y = majorTickMarkEnd;
                        lineFormatRenderer.DrawLine(points[0], points[1]);
                    }
                }

            }

            // Axis.
            if (xari.LineFormat != null)
            {
                points[0].X = xari.X;
                points[0].Y = xari.Y;
                points[1].X = xari.X + xari.Width;
                points[1].Y = xari.Y;
                if (xari.MajorTickMark != TickMarkType.None)
                {
                    points[0].X -= xari.LineFormat.Width / 2;
                    points[1].X += xari.LineFormat.Width / 2;
                }
                lineFormatRenderer.DrawLine(points[0], points[1]);
            }

            // Draw axis title.
            AxisTitleRendererInfo atri = xari.axisTitleRendererInfo;
            if (atri != null && atri.AxisTitleText != null && atri.AxisTitleText.Length > 0)
            {
                XSize size = gfx.MeasureString(atri.AxisTitleText, atri.AxisTitleFont);
                XRect rect = new XRect(xari.Rect.Right / 2 - atri.AxisTitleSize.Width / 2,
                      xari.Rect.Bottom + size.Height * 2, atri.AxisTitleSize.Width, 0);

                gfx.DrawString(atri.AxisTitleText, atri.AxisTitleFont, atri.AxisTitleBrush, rect);
            }

        }

        /// <summary>
        /// Calculates the X axis describing values like minimum/maximum scale, major/minor tick and
        /// major/minor tick mark width.
        /// </summary>
        private void CalculateXAxisValues(AxisRendererInfo rendererInfo)
        {
            // Calculates the maximum number of data points over all series.
            SeriesCollection seriesCollection = ((Chart)rendererInfo.axis.parent).seriesCollection;
            int count = 0;
            foreach (Series series in seriesCollection)
                count = Math.Max(count, series.Count);

            rendererInfo.MinimumScale = 0;
            rendererInfo.MaximumScale = count; // At least 0
            rendererInfo.MajorTick = 1;
            rendererInfo.MinorTick = 0.5;
            rendererInfo.MajorTickMarkWidth = DefaultMajorTickMarkWidth;
            rendererInfo.MinorTickMarkWidth = DefaultMinorTickMarkWidth;
        }

        /// <summary>
        /// Initializes the rendererInfo's xvalues. If not set by the user xvalues will be simply numbers
        /// from minimum scale + 1 to maximum scale.
        /// </summary>
        private void InitXValues(AxisRendererInfo rendererInfo)
        {
            rendererInfo.XValues = ((Chart)rendererInfo.axis.parent).xValues;
            if (rendererInfo.XValues == null)
            {
                rendererInfo.XValues = new XValues();
                XSeries xs = rendererInfo.XValues.AddXSeries();
                for (double i = rendererInfo.MinimumScale + 1; i <= rendererInfo.MaximumScale; ++i)
                    xs.Add(i.ToString(rendererInfo.TickLabelsFormat));
            }
        }

        /// <summary>
        /// Calculates the starting and ending y position for the minor and major tick marks.
        /// </summary>
        private void GetTickMarkPos(AxisRendererInfo rendererInfo,
                                    ref double majorTickMarkStart, ref double majorTickMarkEnd,
                                    ref double minorTickMarkStart, ref double minorTickMarkEnd)
        {
            double majorTickMarkWidth = rendererInfo.MajorTickMarkWidth;
            double minorTickMarkWidth = rendererInfo.MinorTickMarkWidth;
            XRect rect = rendererInfo.Rect;

            switch (rendererInfo.MajorTickMark)
            {
                case TickMarkType.Inside:
                    majorTickMarkStart = rect.Y;
                    majorTickMarkEnd = rect.Y - majorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    majorTickMarkStart = rect.Y;
                    majorTickMarkEnd = rect.Y + majorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    majorTickMarkStart = rect.Y + majorTickMarkWidth;
                    majorTickMarkEnd = rect.Y - majorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    majorTickMarkStart = 0;
                    majorTickMarkEnd = 0;
                    break;
            }

            switch (rendererInfo.MinorTickMark)
            {
                case TickMarkType.Inside:
                    minorTickMarkStart = rect.Y;
                    minorTickMarkEnd = rect.Y - minorTickMarkWidth;
                    break;

                case TickMarkType.Outside:
                    minorTickMarkStart = rect.Y;
                    minorTickMarkEnd = rect.Y + minorTickMarkWidth;
                    break;

                case TickMarkType.Cross:
                    minorTickMarkStart = rect.Y + minorTickMarkWidth;
                    minorTickMarkEnd = rect.Y - minorTickMarkWidth;
                    break;

                case TickMarkType.None:
                    minorTickMarkStart = 0;
                    minorTickMarkEnd = 0;
                    break;
            }
        }

        /// <summary>
        /// Calculates all values necessary for scaling the axis like minimum/maximum scale or
        /// minor/major tick.
        /// </summary>
        private void InitScale(AxisRendererInfo rendererInfo)
        {
            double xMin, xMax;
            CalcXAxis(out xMin, out xMax);
            FineTuneXAxis(rendererInfo, xMin, xMax);

            rendererInfo.MajorTickMarkWidth = DefaultMajorTickMarkWidth;
            rendererInfo.MinorTickMarkWidth = DefaultMinorTickMarkWidth;
        }

        protected virtual void CalcXAxis(out double xMin, out double xMax)
        {
            xMin = double.MaxValue;
            xMax = double.MinValue;

            foreach (Series series in ((Chart)this.rendererParms.DrawingItem).SeriesCollection)
            {
                if (series.Elements.Count > 0)
                {
                    if ((series.Elements.getPointX(0)) != null)
                    {
                        foreach (PointX xp in series.Elements)
                        {
                            if (!double.IsNaN(xp.yvalue))
                            {
                                xMin = Math.Min(xMin, xp.xvalue);
                                xMax = Math.Max(xMax, xp.xvalue);
                            }
                        }
                    }
                }
            }
        }


    }
}