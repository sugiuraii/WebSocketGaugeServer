using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;


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

		//時間計測
		private Stopwatch _stopwatch;
		private const long stopwatch_timeout = 3000; //タイムアウト

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
		//区間データ保存キュー
		private int _sect_store_max;
		private Trip_gas_Content _sect_trip_gas_temporary;
		private Trip_gas_Content _sect_trip_gas_latest;
		private Queue<Trip_gas_Content> _sect_trip_gas_queue;

		public int Sect_Store_Max
		{
			get
			{
				return _sect_store_max;
			}
			set
			{
				_sect_store_max = value;
				reset_sect_trip_gas ();
			}
		}


		//区間データ更新時に発生するイベント
		public event EventHandler SectFUELTRIPUpdated;

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
		private Trip_gas_Content _total_trip_gas;
		public double Total_Trip
		{
			get
			{
				return _total_trip_gas.trip;
			}
		}
		public double Total_Gas_Consumption
		{
			get
			{
				return _total_trip_gas.gas_consumption;
			}
		}

		//総区間燃費(計算)
		public double Total_Gas_Milage
		{
			get
			{
				return _total_trip_gas.gas_milage;
			
			}
		}

		//区間トリップ、燃料消費量
		public Trip_gas_Content[] Sect_trip_gas_Array
		{
			get 
			{
				Trip_gas_Content[] sect_trip_gas_array = _sect_trip_gas_queue.ToArray ();
				return sect_trip_gas_array;
			}
		}

		public double[] Sect_trip_array
		{
			get
			{
				int i;
				Trip_gas_Content[] sect_trip_gas_array = _sect_trip_gas_queue.ToArray ();

				double[] trip_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					trip_array [i] = sect_trip_gas_array [i].trip;
				}

				return trip_array;
			}
		}

		public double[] Sect_gas_array
		{
			get
			{
				int i;
				Trip_gas_Content[] sect_trip_gas_array = _sect_trip_gas_queue.ToArray ();

				double[] gas_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					gas_array [i] = sect_trip_gas_array [i].gas_consumption;
				}

				return gas_array;
			}
		}

		public double[] Sect_gasmilage_array
		{
			get
			{
				int i;
				Trip_gas_Content[] sect_trip_gas_array = _sect_trip_gas_queue.ToArray ();

				double[] gasmilage_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					gasmilage_array [i] = sect_trip_gas_array [i].gas_milage;
				}

				return gasmilage_array;
			}
		}


		public double Sect_Trip_Latest
		{
			get
			{
				return _sect_trip_gas_latest.trip;
			}
		}
		public double Sect_Gas_Consumption_Latest
		{
			get
			{
				return _sect_trip_gas_latest.gas_consumption;
			}
		}
		public double Sect_Gas_Milage_Latest
		{
			get
			{
				return _sect_trip_gas_latest.gas_milage;
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
			_total_trip_gas = new Trip_gas_Content ();

			_sect_elapsed = 0;
			_sect_store_max = 60;
			_sect_span = 10*1000;
			_sect_trip_gas_temporary = new Trip_gas_Content ();
			_sect_trip_gas_queue = new Queue<Trip_gas_Content>();
			_sect_trip_gas_latest = new Trip_gas_Content();

			_save_elapsed = 0;

			//データ格納しているファイルパスの指定
			_folderpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			_filepath = Path.Combine( _folderpath, "." + "FUELTRIP_Logger");

			load_trip_gas();

			_stopwatch = new Stopwatch ();
			_stopwatch.Reset ();
		}

		//デストラクタ
		~Nenpi_Trip_Calculator()
		{
			save_trip_gas();
		}

		public void update(double tacho, double speed, double injpulse_width)
		{
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                return;
            }
                
			_stopwatch.Stop ();

            //get elasped time
            long stopwatch_elasped = _stopwatch.ElapsedMilliseconds;

			// Set current value
			_current_speed = speed;
			_current_tacho = tacho;
			_current_injpulse_width = injpulse_width;

			// elaspedが長すぎる場合,タイムアウトを発生
			if (stopwatch_elasped > stopwatch_timeout) {
				_stopwatch.Reset ();
                _stopwatch.Start();
				throw new TimeoutException ("tacho/speed/injpulse update span is too large (Timeout).");
			}
            _stopwatch.Reset();
			_stopwatch.Start ();

			_momentary_trip = get_momentary_trip(stopwatch_elasped);
			_momentary_gas_consumption = get_momentary_gas_comsumption(stopwatch_elasped);

			_total_trip_gas.trip += _momentary_trip;
			_total_trip_gas.gas_consumption += _momentary_gas_consumption;

			_sect_elapsed += stopwatch_elasped;

			//区間データアップデート
			if (_sect_elapsed < _sect_span)
			{
				_sect_trip_gas_temporary.trip += _momentary_trip;
				_sect_trip_gas_temporary.gas_consumption += _momentary_gas_consumption;
			}
			else
			{
				//Section履歴に追加
				enqueue_sect_trip_gas (_sect_trip_gas_temporary);

				_sect_trip_gas_temporary = new Trip_gas_Content();
				_sect_elapsed = 0;

				//区間データ更新イベント発生
				SectFUELTRIPUpdated (this, EventArgs.Empty);
			}

			//総燃費、総距離を5sごとに保存
			if (_save_elapsed < save_span)
			{
				_save_elapsed += stopwatch_elasped;
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

		private void enqueue_sect_trip_gas(Trip_gas_Content content)
		{
			_sect_trip_gas_latest = content;
			_sect_trip_gas_queue.Enqueue (content);

			if (_sect_trip_gas_queue.Count > _sect_store_max) {
				_sect_trip_gas_queue.Dequeue ();
			}

		}

		public void reset_total_trip_gas()
		{
			_total_trip_gas = new Trip_gas_Content ();
		}

		public void reset_sect_trip_gas()
		{
			_sect_trip_gas_latest = new Trip_gas_Content ();
			_sect_trip_gas_queue = new Queue<Trip_gas_Content> ();
		}

		public void load_trip_gas()
		{
			//XmlSerializerオブジェクトの作成
			System.Xml.Serialization.XmlSerializer serializer =
				new System.Xml.Serialization.XmlSerializer(typeof(Trip_gas_Content));

			try
			{
				//ファイルを開く
				System.IO.FileStream fs =
					new System.IO.FileStream(_filepath, System.IO.FileMode.Open);

				try
				{
					//XMLファイルから読み込み、逆シリアル化する
					_total_trip_gas =
						(Trip_gas_Content)serializer.Deserialize(fs);

				}
				catch (XmlException ex)
				{
					Console.WriteLine(ex.Message);
					this.reset_total_trip_gas();
				}

				fs.Close();

			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex.Message);
				this.reset_total_trip_gas ();
			}
			catch (DirectoryNotFoundException ex)
			{
				Console.WriteLine(ex.Message);
				System.IO.Directory.CreateDirectory(@_folderpath);
				this.reset_sect_trip_gas ();
			}
			catch (System.Security.SecurityException ex)
			{
				Console.WriteLine(ex.Message);
				this.reset_total_trip_gas ();
			}
		}


		public void save_trip_gas()
		{
			//XmlSerializerオブジェクトを作成
			//書き込むオブジェクトの型を指定する
			System.Xml.Serialization.XmlSerializer serializer1 =
				new System.Xml.Serialization.XmlSerializer(typeof(Trip_gas_Content));
			//ファイルを開く
			System.IO.FileStream fs1 =
				new System.IO.FileStream(_filepath, System.IO.FileMode.Create);
			//シリアル化し、XMLファイルに保存する
			serializer1.Serialize(fs1, _total_trip_gas);
			//閉じる
			fs1.Close();
		}
	}

	public class Trip_gas_Content
	{
		public double trip { get; set; }
		public double gas_consumption { get; set; }

		public Trip_gas_Content()
		{
			reset ();
		}

		private void reset()
		{
			this.trip = 0;
			this.gas_consumption = 0;
		}

		public double gas_milage
		{
			get
			{
				if (this.gas_consumption == 0)
					return 0;
				else
					return this.trip / this.gas_consumption;
			}
		}

	}
}

