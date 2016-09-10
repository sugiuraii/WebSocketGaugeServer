using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DefiSSMCOM
{
    public class Defi_Content_Table
    {
        private Dictionary<Defi_Parameter_Code, Defi_Numeric_Content> _defi_numeric_content_table = new Dictionary<Defi_Parameter_Code, Defi_Numeric_Content>();

        //コンストラクタ
        public Defi_Content_Table()
        {
            set_numeric_content_table();
        }

        //デストラクタ
        ~Defi_Content_Table()
        {
        }

        public Defi_Numeric_Content this[Defi_Parameter_Code code]
        {
            get
            {
                return _defi_numeric_content_table[code];
            }
        }

        private void set_numeric_content_table()
        {
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Boost, new Defi_Numeric_Content(0x01, 1.27e-3, -1.0, "kgf/cm2"));
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Tacho, new Defi_Numeric_Content(0x02, 4.77, -121.98, "rpm"));  // 500-9000rpmにて線形回帰 500rpm以下では誤差大（120rpm程度)
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Oil_Pres, new Defi_Numeric_Content(0x03, 4.23e-3, 0.0, "kgf/cm2")); //以下実物合わせしていないので、変換係数は適当
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Fuel_Pres, new Defi_Numeric_Content(0x04, 2.54e-3, 0.0, "kgf/cm2"));
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Ext_Temp, new Defi_Numeric_Content(0x05, 3.81e-1, 200.0, "C"));
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Oil_Temp, new Defi_Numeric_Content(0x07, 4.23e-2, 50.0, "C"));
            _defi_numeric_content_table.Add(Defi_Parameter_Code.Water_Temp, new Defi_Numeric_Content(0x0f, 4.23e-2, 20.0, "C"));
        }
    }

    public class Defi_Numeric_Content
    {
        private byte _reciever_id;
        private double _conversion_coefficient;
        private double _conversion_offset;
        private Int32 _raw_value;
        private String _unit;

        public Defi_Numeric_Content(byte receiver_id, double conversion_coefficient, double conversion_offset, String unit)
        {
            _reciever_id = receiver_id;
            _conversion_coefficient = conversion_coefficient;
            _conversion_offset = conversion_offset;
            _unit = unit;
        }

        ~Defi_Numeric_Content()
        {
        }

        public double Value
        {
            get
            {
                return _conversion_coefficient * _raw_value + _conversion_offset;
            }
        }

        public byte Receiver_id
        {
            get
            {
                return _reciever_id;
            }
        }

        public Int32 Raw_Value
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

        public double Conversion_Coefficient
        {
            get
            {
                return _conversion_coefficient;
            }
        }

        public double Conversion_Offset
        {
            get
            {
                return _conversion_offset;
            }
        }
        public String Unit
        {
            get
            {
                return _unit;
            }
        }

    };

    public enum Defi_Parameter_Code
    {
        Boost,
        Tacho,
        Oil_Pres,
        Fuel_Pres,
        Ext_Temp,
        Oil_Temp,
        Water_Temp
    };
}
