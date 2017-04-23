using System;
using System.IO.Ports;

namespace DefiSSMCOM.Defi
{
    public class DefiCOM : COMCommon
    {
        private DefiContentTable content_table;

		// Defilink received Event
		public event EventHandler DefiPacketReceived;

        //DefiLinkパケットサイズ
        const int DEFI_PACKET_SIZE = 35;

        //コンストラクタ
        public DefiCOM()
        {
            content_table = new DefiContentTable();

            //DEFIボーレート設定
            DefaultBaudRate = 19200;
            //リセット時のボーレート設定(communticate_reset()参照)
            //FT232RLの場合、許容されるボーレートは3000000/n (nは整数または小数点以下が0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875)
            ResetBaudRate = 9600;

            Parity = Parity.Even;
            ReadTimeout = 500;
        }

        //通信部ルーチン実装
        //この実装ではslowread_flagは無視
        protected override void communicate_main(bool slowread_flag)
        {
            int i, c;
            char[] inbuf = new char[DEFI_PACKET_SIZE];
            //読み込みルーチン

            try
            {
                //デリミタ(ReceiverID)を見つけるまで読み進める
                do
                {
                    c = ReadByte();
                }
                while (c < 0x01 || c > 0x0f);

                //Defiパケットサイズ分だけ読み出す。
                inbuf[0] = (char)c;

                for (i = 1; i < DEFI_PACKET_SIZE; i++)
                {
                    c = ReadByte();
                    inbuf[i] = (char)c;
                }
            }
            catch (TimeoutException ex)
            {
                //読み出しタイムアウト時はエラーフラグを立て、次のサイクルでリセット処理を入れる
                logger.Warn("Defi packet timeout. " + ex.GetType().ToString() + " " + ex.Message);
                communicateRealtimeIsError = true;
                return;
            }

            //バッファの残り分は破棄
            DiscardInBuffer();
                    
            //ReceiverIDを判読し、private変数に格納
            int j;
            for (j = 0; j < DEFI_PACKET_SIZE; j += 5)
            {
                try
                {
                    if (inbuf[j] == (char)content_table[DefiParameterCode.Manifold_Absolute_Pressure].Receiver_id)
                    {
                        String boost_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Manifold_Absolute_Pressure].RawValue = Int32.Parse(boost_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Engine_Speed].Receiver_id)
                    {
                        String tacho_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Engine_Speed].RawValue = Int32.Parse(tacho_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Pressure].Receiver_id)
                    {
                        String oilpres_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Oil_Pressure].RawValue = Int32.Parse(oilpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Fuel_Rail_Pressure].Receiver_id)
                    {
                        String fuelpres_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Fuel_Rail_Pressure].RawValue = Int32.Parse(fuelpres_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Exhaust_Gas_Temperature].Receiver_id)
                    {
                        String exttemp_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Exhaust_Gas_Temperature].RawValue = Int32.Parse(exttemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Oil_Temperature].Receiver_id)
                    {
                        String oiltemp_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Oil_Temperature].RawValue = Int32.Parse(oiltemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                    else if (inbuf[j] == (char)content_table[DefiParameterCode.Coolant_Temperature].Receiver_id)
                    {
                        String watertemp_str = new String(inbuf, j + 2, 3);
                        content_table[DefiParameterCode.Coolant_Temperature].RawValue = Int32.Parse(watertemp_str, System.Globalization.NumberStyles.AllowHexSpecifier);
                    }
                }
                catch (FormatException ex)
                {
                    //DefiPacketが崩れていた場合エラーフラグを立て、次のサイクルでリセット処理を入れる。
                    logger.Warn("Invalid Defi packet. " + ex.GetType().ToString() + " " + ex.Message);
                    communicateRealtimeIsError = true;
                    return;
                }
            }

			// Invoke PacketReceived Event
			DefiPacketReceived(this, EventArgs.Empty);                    
        }

        public double get_value(DefiParameterCode code)
        {
            return content_table[code].Value;
        }

        public Int32 get_raw_value(DefiParameterCode code)
        {
            return content_table[code].RawValue;
        }

        public string get_unit(DefiParameterCode code)
        {
            return content_table[code].Unit;
        }
    }
}