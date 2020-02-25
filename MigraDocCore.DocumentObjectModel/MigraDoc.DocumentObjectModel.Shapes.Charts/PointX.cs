using MigraDocCore.DocumentObjectModel.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigraDocCore.DocumentObjectModel.Shapes.Charts
{
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
        public PointX(double xvalue, double yvalue)
          : this()
        {
            this.XValue = xvalue;
            this.YValue = yvalue;
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
            set
            {
                SetParent(value);
                this.lineFormat = value;
            }
        }
        [DV]
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
            set
            {
                SetParent(value);
                this.fillFormat = value;
            }
        }
        [DV]
        internal FillFormat fillFormat;

        /// <summary>
        /// The actual value of the data point.
        /// </summary>
        public double XValue
        {
            get { return this.xvalue.Value; }
            set { this.xvalue.Value = value; }
        }

        /// <summary>
        /// The actual value of the data point.
        /// </summary>
        public double YValue
        {
            get { return this.yvalue.Value; }
            set { this.yvalue.Value = value; }
        }

        [DV]
        internal NDouble xvalue = NDouble.NullValue;
        internal NDouble yvalue = NDouble.NullValue;
        #endregion

        #region Internal
        /// <summary>
        /// Converts Point into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            if (!this.IsNull("LineFormat") || !this.IsNull("FillFormat"))
            {
                serializer.WriteLine("");
                serializer.WriteLine("\\pointx");
                int pos = serializer.BeginAttributes();

                if (!this.IsNull("LineFormat"))
                    this.lineFormat.Serialize(serializer);
                if (!this.IsNull("FillFormat"))
                    this.fillFormat.Serialize(serializer);

                serializer.EndAttributes(pos);
                serializer.BeginContent();
                serializer.Write(this.XValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                serializer.Write(", ");
                serializer.WriteLine(this.YValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                serializer.EndContent();
            }
            else
            {
                serializer.Write(this.XValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                serializer.Write(", ");
                serializer.WriteLine(this.YValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            serializer.Write(", ");
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta
        {
            get
            {
                if (meta == null)
                    meta = new Meta(typeof(Point));
                return meta;
            }
        }
        static Meta meta;
        #endregion
    }
}
