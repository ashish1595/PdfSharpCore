using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharpCore.Charting
{
    /// <summary>
 /// Represents a formatted value on the data series.
 /// </summary>
    public class PointX : ChartObject
    {
        /// <summary>
        /// Initializes a new instance of the Point class.
        /// </summary>
        internal PointX()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Point class with a real value.
        /// </summary>
        public PointX(double x, double y) : this()
        {
            xvalue = x;
            yvalue = y;

        }


        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new PointX Clone()
        {
            return (PointX)DeepCopy();
        }

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            PointX point = (PointX)base.DeepCopy();
            if (point.lineFormat != null)
            {
                point.lineFormat = point.lineFormat.Clone();
                point.lineFormat.parent = point;
            }
            if (point.fillFormat != null)
            {
                point.fillFormat = point.fillFormat.Clone();
                point.fillFormat.parent = point;
            }
            return point;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the line format of the data point's border.
        /// </summary>
        public LineFormat LineFormat
        {
            get
            {
                if (this.lineFormat == null)
                    this.lineFormat = new LineFormat(this);

                return this.lineFormat;
            }
        }
        internal LineFormat lineFormat;

        /// <summary>
        /// Gets the filling format of the data point.
        /// </summary>
        public FillFormat FillFormat
        {
            get
            {
                if (this.fillFormat == null)
                    this.fillFormat = new FillFormat(this);

                return this.fillFormat;
            }
        }
        internal FillFormat fillFormat;

        /// <summary>
        /// The actual value of the data point.
        /// </summary>
        public double XValue
        {
            get { return this.xvalue; }
            set { this.xvalue = value; }
        }
        /// <summary>
        /// The actual value of the data point.
        /// </summary>
        public double YValue
        {
            get { return this.yvalue; }
            set { this.yvalue = value; }
        }

        internal double xvalue;
        internal double yvalue;
        #endregion
    }
}
