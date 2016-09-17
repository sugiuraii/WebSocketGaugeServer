using System;
using System.Collections.Generic;

namespace DefiSSMCOM
{
    public class DefiContentTable
    {
        private Dictionary<DefiParameterCode, DefiNumericContent> _defi_numeric_content_table = new Dictionary<DefiParameterCode, DefiNumericContent>();

        //コンストラクタ
        public DefiContentTable()
        {
            set_numeric_content_table();
        }

        public DefiNumericContent this[DefiParameterCode code]
        {
            get
            {
                return _defi_numeric_content_table[code];
            }
        }

        private void set_numeric_content_table()
        {
            _defi_numeric_content_table.Add(DefiParameterCode.Boost, new DefiNumericContent(0x01, x=>1.27e-3*x -1.0, "kgf/cm2"));
            _defi_numeric_content_table.Add(DefiParameterCode.Tacho, new DefiNumericContent(0x02, x=>4.77*x -121.98, "rpm"));  // 500-9000rpmにて線形回帰 500rpm以下では誤差大（120rpm程度)
            _defi_numeric_content_table.Add(DefiParameterCode.Oil_Pres, new DefiNumericContent(0x03, x=>4.23e-3*x + 0.0, "kgf/cm2")); //以下実物合わせしていないので、変換係数は適当
            _defi_numeric_content_table.Add(DefiParameterCode.Fuel_Pres, new DefiNumericContent(0x04, x=>2.54e-3*x + 0.0, "kgf/cm2"));
            _defi_numeric_content_table.Add(DefiParameterCode.Ext_Temp, new DefiNumericContent(0x05, x=>3.81e-1*x + 200.0, "C"));
            _defi_numeric_content_table.Add(DefiParameterCode.Oil_Temp, new DefiNumericContent(0x07, x=>4.23e-2*x + 50.0, "C"));
            _defi_numeric_content_table.Add(DefiParameterCode.Water_Temp, new DefiNumericContent(0x0f, x=>4.23e-2*x + 20.0, "C"));
        }
    }

    public class DefiNumericContent : NumericContent
    {
        private byte _reciever_id;

        public DefiNumericContent(byte receiver_id, Func<Int32, double> conversion_function, String unit)
        {
            _reciever_id = receiver_id;
            _conversion_function = conversion_function;
            _unit = unit;
        }

        public byte Receiver_id
        {
            get
            {
                return _reciever_id;
            }
        }


    };

    public enum DefiParameterCode
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
