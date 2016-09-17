using System;

namespace DefiSSMCOM
{
    public class NumericContent
    {
        protected Func<Int32, double> _conversion_function;
        protected Int32 _raw_value;
        protected String _unit;

        public double Value
        {
            get
            {
                return _conversion_function(_raw_value);
            }
        }

        public Int32 RawValue
        {
            get
            {
                return _raw_value;
            }
            set
            {
                _raw_value = value;
            }
        }

        public Func<Int32, double> ConversionFunction
        {
            get
            {
                return _conversion_function;
            }
        }

        public String Unit
        {
            get
            {
                return _unit;
            }
        }

    }
}
