using System;
using System.Collections.Generic;

namespace DefiSSMCOM
{
    public abstract class ContentTableCommon <parameterCodeType, numericContentType> 
        where parameterCodeType:struct 
        where numericContentType : NumericContent 
    {
        protected Dictionary<parameterCodeType, numericContentType> _numeric_content_table;

        public ContentTableCommon()
        {
            _numeric_content_table = new Dictionary<parameterCodeType, numericContentType>();
            set_numeric_content_table();
        }

        protected abstract void set_numeric_content_table();


        public numericContentType this[parameterCodeType code]
        {
            get
            {
                return _numeric_content_table[code];
            }
        }
    }

    public abstract class NumericContent
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
