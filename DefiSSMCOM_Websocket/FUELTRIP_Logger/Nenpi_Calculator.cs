using System;
using System.IO;
using SuperSocket.SocketBase;
using SuperWebSocket;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace FUELTRIP_Logger
{
	public class Nenpi_Trip_Calculator
	{

		private double _current_tacho;
		private double _current_speed;
		private double _current_injpulse_width;

		//トリップと燃料消費量の補正係数
		private const double trip_coefficient = 1.0;
		private const double gas_consumption_coefficient = 1.0;
		//気筒数,インジェクタ関連定数
		private const int num_cylinder = 4;
		private const double injection_latency = 0.76; //無効噴射時間ms
		private const double injetcer_capacity = 575;

		//タイマスパン(50ms)
		private const int timer_span = 50;
		//タイマオブジェクト
		private System.Threading.Timer _timer;
		//集計しているかどうかをあらわすフラグ
		private bool _calculate_continue;

		//燃料消費量、トリップを保存するスパン(5s)
		private const int save_span = 5000;
		private double _save_elapsed;


		//燃費、トリップデータ保存ファイルパス
		private string _filepath;
		//↑のフォルダパス
		private string _folderpath;

		//区間データ更新頻度
		private long _sect_span;
		private long _sect_elapsed;
		public long Sect_Span
		{
			get
			{
				return _sect_span;
			}
			set
			{
				_sect_span = value;
			}
		}

		//総燃料消費量、トリップ
		private double _total_trip;
		public double Total_Trip
		{
			get
			{
				return _total_trip;
			}
		}
		private double _total_gas_consumption;
		public double Total_Gas_Consumption
		{
			get
			{
				return _total_gas_consumption;
			}
		}

		//総区間燃費(計算)
		public double Total_Gas_Milage
		{
			get
			{
				if (_total_gas_consumption == 0)
					return 0;
				else
					return _total_trip / _total_gas_consumption;
			}
		}

		//区間トリップ、燃料消費量
		private double _sect_trip;
		private double _sect_gas_consumption;
		private double _sect_trip_spooled;
		private double _sect_gas_consumption_spooled;
		public double Sect_Trip_Spooled
		{
			get
			{
				_sect_data_updated = false;
				return _sect_trip_spooled;
			}
		}
		public double Sect_Gas_Consumption_Spooled
		{
			get
			{
				_sect_data_updated = false;
				return _sect_gas_consumption_spooled;
			}
		}
		public double Sect_Gas_Milage_Spooled
		{
			get
			{
				_sect_data_updated = false;
				if (_sect_gas_consumption_spooled == 0)
					return 0;
				else
				{
					return _sect_trip_spooled / _sect_gas_consumption_spooled;
				}
			}
		}

		//区間データを読み出したあと、値更新されたらTrue
		private bool _sect_data_updated;
		public bool Sect_Data_Updated
		{
			get
			{
				return _sect_data_updated;
			}
		}

		//瞬間トリップ、燃料消費量
		private double _momentary_trip;
		private double _momentary_gas_consumption;
		public double Momentary_Trip
		{
			get
			{
				return _momentary_trip;
			}
		}
		public double Momentary_Gas_Consumption
		{
			get
			{
				return _momentary_gas_consumption;
			}
		}
		public double Momentary_Gas_Milage
		{
			get
			{
				if (_momentary_gas_consumption != 0)
				{
					return _momentary_trip / _momentary_gas_consumption;
				}
				else
				{
					return 0;
				}
			}
		}

		//コンストラクタ
		public Nenpi_Trip_Calculator()
		{
			_sect_trip = 0;
			_sect_gas_consumption = 0;

			_sect_gas_consumption_spooled = 0;
			_sect_trip_spooled = 0;
			_sect_data_updated = false;
			_sect_elapsed = 0;
			_sect_span = 300*1000;
			_calculate_continue = false;
			_save_elapsed = 0;

			//データ格納しているファイルパスの指定
			_folderpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_filepath = Path.Combine( _folderpath, "." + "FUELTRIP_Logger");

			load_trip_gas();

		}

		//デストラクタ
		~Nenpi_Trip_Calculator()
		{
			save_trip_gas();
			caculate_stop();
		}

		public void calculate_start()
		{
			if (!_calculate_continue)
			{
				_calculate_continue = true;
				TimerCallback timerDelegate = new TimerCallback(calculate_main);

				_timer = new System.Threading.Timer(timerDelegate, null, 0, timer_span);
			}
		}

		public void caculate_stop()
		{
			if (_calculate_continue)
			{
				_timer.Change(0, Timeout.Infinite);
				_timer.Dispose();
				_calculate_continue = false;
			}
		}

		private void calculate_main(object o)
		{
			_momentary_trip = get_momentary_trip(timer_span);
			_momentary_gas_consumption = get_momentary_gas_comsumption(timer_span);

			_total_trip += _momentary_trip;
			_total_gas_consumption += _momentary_gas_consumption;

			_sect_elapsed += timer_span;

			//区間データアップデート
			if (_sect_elapsed < _sect_span)
			{
				_sect_trip += _momentary_trip;
				_sect_gas_consumption += _momentary_gas_consumption;
			}
			else
			{
				//spooled変数に集計したセクションデータを保管
				_sect_trip_spooled = _sect_trip;
				_sect_gas_consumption_spooled = _sect_gas_consumption;
				//セクションデータ更新フラグON
				_sect_data_updated = true;

				_sect_trip = 0;
				_sect_gas_consumption = 0;
				_sect_elapsed = 0;
			}

			//総燃費、総距離を5sごとに保存
			if (_save_elapsed < save_span)
			{
				_save_elapsed += timer_span;
			}
			else
			{
				save_trip_gas();
				_save_elapsed = 0;
			}


		}

		private double get_momentary_trip(long elasped_millisecond)
		{
			double speed = _current_speed;
			double tacho = _current_tacho;

			double monentary_trip = trip_coefficient * (speed) / 3600 / 1000 * elasped_millisecond;

			return monentary_trip;
		}

		private double get_momentary_gas_comsumption(long elasped_millisecond)
		{
			//燃料消費量計算
			double inj_pulse_width = _current_injpulse_width;
			double tacho = _current_tacho;

			double momentary_gas_consumption;
			if (tacho > 500) //アイドリング回転以下の場合は、燃料消費量に加えない
				momentary_gas_consumption = gas_consumption_coefficient * (double)num_cylinder * (double)tacho * injetcer_capacity * (inj_pulse_width - injection_latency) / (7.2E9) * elasped_millisecond / 1000;
			else
				momentary_gas_consumption = 0;

			return momentary_gas_consumption;

		}


		public void reset_total_trip()
		{
			_total_trip = 0;
		}

		public void reset_total_gas_consumption()
		{
			_total_gas_consumption = 0;
		}

		public void reset_sect_trip()
		{
			_sect_trip = 0;
		}

		public void reset_sect_gas_consumption()
		{
			_sect_gas_consumption = 0;
		}

		public void load_trip_gas()
		{
			//XmlSerializerオブジェクトの作成
			System.Xml.Serialization.XmlSerializer serializer =
				new System.Xml.Serialization.XmlSerializer(typeof(Nenpi_Trip_Data_Content));

			try
			{
				//ファイルを開く
				System.IO.FileStream fs =
					new System.IO.FileStream(_filepath, System.IO.FileMode.Open);

				try
				{
					//燃費、トリップデータ
					Nenpi_Trip_Data_Content nenpi_trip_data_content;

					//XMLファイルから読み込み、逆シリアル化する
					nenpi_trip_data_content =
						(Nenpi_Trip_Data_Content)serializer.Deserialize(fs);

					_total_gas_consumption = nenpi_trip_data_content.gas_consumption;
					_total_trip = nenpi_trip_data_content.trip;
				}
				catch (XmlException ex)
				{
					Console.WriteLine("燃費、トリップデータ読み込みに失敗しました。初期化します。");
					_total_gas_consumption = 0;
					_total_trip = 0;
				}

				fs.Close();

			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine("燃費、トリップデータファイルが見つかりません。初期化します。");
				_total_gas_consumption = 0;
				_total_trip = 0;
			}
			catch (DirectoryNotFoundException ex)
			{
				Console.WriteLine("燃費、トリップデータファイル保存先フォルダがありません。フォルダ作成します。");
				System.IO.Directory.CreateDirectory(@_folderpath);
				_total_gas_consumption = 0;
				_total_trip = 0;
			}
			catch (System.Security.SecurityException ex)
			{
				Console.WriteLine(ex.Message);
				_total_gas_consumption = 0;
				_total_trip = 0;
			}
		}


		public void save_trip_gas()
		{
			Nenpi_Trip_Data_Content nenpi_trip_data_content = new Nenpi_Trip_Data_Content();
			nenpi_trip_data_content.gas_consumption = _total_gas_consumption;
			nenpi_trip_data_content.trip = _total_trip;

			//XmlSerializerオブジェクトを作成
			//書き込むオブジェクトの型を指定する
			System.Xml.Serialization.XmlSerializer serializer1 =
				new System.Xml.Serialization.XmlSerializer(typeof(Nenpi_Trip_Data_Content));
			//ファイルを開く
			System.IO.FileStream fs1 =
				new System.IO.FileStream(_filepath, System.IO.FileMode.Create);
			//シリアル化し、XMLファイルに保存する
			serializer1.Serialize(fs1, nenpi_trip_data_content);
			//閉じる
			fs1.Close();
		}
	}

	public class Nenpi_Trip_Data_Content
	{
		private double _trip;
		private double _gas_consumption;

		public double trip
		{
			get{return _trip;}
			set{_trip = value;}
		}

		public double gas_consumption
		{
			get { return _gas_consumption; }
			set { _gas_consumption = value; }
		}
	}
}

