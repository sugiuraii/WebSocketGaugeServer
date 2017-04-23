using System;
using System.Collections.Generic;

namespace DefiSSMCOM.Defi
{
    public class DefiContentTable : ContentTableCommon<DefiParameterCode, DefiNumericContent>
    {
        //コンストラクタ
        public DefiContentTable()
        {
        }

        protected override void setNumericContentTable()
        {
            _numeric_content_table.Add(DefiParameterCode.Manifold_Absolute_Pressure, new DefiNumericContent(0x01, x=>1.27e-3*x -1.0, "kgf/cm2"));
            _numeric_content_table.Add(DefiParameterCode.Engine_Speed, new DefiNumericContent(0x02, x=>4.77*x -121.98, "rpm"));  // 500-9000rpmにて線形回帰 500rpm以下では誤差大（120rpm程度)
            _numeric_content_table.Add(DefiParameterCode.Oil_Pressure, new DefiNumericContent(0x03, x=>4.23e-3*x + 0.0, "kgf/cm2")); //以下実物合わせしていないので、変換係数は適当
            _numeric_content_table.Add(DefiParameterCode.Fuel_Rail_Pressure, new DefiNumericContent(0x04, x=>2.54e-3*x + 0.0, "kgf/cm2"));
            _numeric_content_table.Add(DefiParameterCode.Exhaust_Gas_Temperature, new DefiNumericContent(0x05, x=>3.81e-1*x + 200.0, "C"));
            _numeric_content_table.Add(DefiParameterCode.Oil_Temperature, new DefiNumericContent(0x07, x=>4.23e-2*x + 50.0, "C"));
            _numeric_content_table.Add(DefiParameterCode.Coolant_Temperature, new DefiNumericContent(0x0f, x=>4.23e-2*x + 20.0, "C"));
        }
    }

    public class DefiNumericContent : NumericContent
    {
        private byte _reciever_id;

        public DefiNumericContent(byte receiver_id, Func<double, double> conversion_function, String unit)
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
        Manifold_Absolute_Pressure,
        Engine_Speed,
        Oil_Pressure,
        Fuel_Rail_Pressure,
        Exhaust_Gas_Temperature,
        Oil_Temperature,
        Coolant_Temperature
    };
}
