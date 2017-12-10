using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;


namespace FUELTRIP_Logger
{
    /// <summary>
    /// Option class for FuelTripCalculator
    /// </summary>
    public class FuelTripCalculatorOption
    {
        /// <summary>
        /// Trip coefficient
        /// </summary>
        public double TripCorrectionFactor;
        /// <summary>
        /// Gas consumption coefficient
        /// </summary>
        public double FuelCorrectionFactor;
        /// <summary>
        /// Number of cylinder
        /// </summary>
        public int NumCylinder;
        /// <summary>
        /// Injection latency in ms.
        /// </summary>
        public double InjectionLatency;
        /// <summary>
        /// Injector capacity in cc.
        /// </summary>
        public double InjectorCapacity;
    }

    /// <summary>
    /// Fuel and trip calculation class.
    /// </summary>
	public class FuelTripCalculator
	{
		private double currentEngineRev;
		private double currentVehicleSpeed;
		private double currentInjectionPulseWidth;

        private FuelTripCalculatorOption calculatorOption;
		
		private Stopwatch stopWatch;
		private const long stopwatchTimeout = 3000;

		/// <summary>
        /// Time span (in ms) to save fuel and trip to file.
		/// </summary>
        private const int saveSpan = 5000;
		private double saveElapsedTime;
		
        /// <summary>
        /// File path to save fuel and trip data
        /// </summary>
		private string filePath;
		/// <summary>
		/// Folder path to save fuel and trip data
		/// </summary>
		private string folderPath;

		/// <summary>
		/// Span time of section fuel and trip
		/// </summary>
		private long sectSpan;
		private long sectElapsed;
		
        /// <summary>
        /// Number of store to save section fuel and trip
        /// </summary>
		private int sectStoreMax;
		private TripFuelContent sectTripFuelTemporary;
		private TripFuelContent sectTripFuelLatest;
		private Queue<TripFuelContent> sectTripFuelQueue;

		public int SectStoreMax
		{
			get
			{
				return sectStoreMax;
			}
			set
			{
				sectStoreMax = value;
				resetSectTripFuel ();
			}
		}


		/// <summary>
		/// Event handler called when section fuel/trip is updated.
		/// </summary>
		public event EventHandler SectFUELTRIPUpdated;

		public long SectSpan
		{
			get
			{
				return sectSpan;
			}
			set
			{
				sectSpan = value;
			}
		}

		//Total fuel consumption and trip
		private TripFuelContent totalTripFuel;
		public double TotalTrip
		{
			get
			{
				return totalTripFuel.trip;
			}
		}
		public double TotalFuelConsumption
		{
			get
			{
				return totalTripFuel.fuelConsumption;
			}
		}

		// Total km/L
		public double TotalTripPerFuel
		{
			get
			{
				return totalTripFuel.TripPerFuel;
			
			}
		}

		//区間トリップ、燃料消費量
		public TripFuelContent[] SectTripFuelArray
		{
			get 
			{
				TripFuelContent[] sect_trip_gas_array = sectTripFuelQueue.ToArray ();
				return sect_trip_gas_array;
			}
		}

		public double[] SectTripArray
		{
			get
			{
				int i;
				TripFuelContent[] sect_trip_gas_array = sectTripFuelQueue.ToArray ();

				double[] trip_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					trip_array [i] = sect_trip_gas_array [i].trip;
				}

				return trip_array;
			}
		}

		public double[] SectFuelArray
		{
			get
			{
				int i;
				TripFuelContent[] sect_trip_gas_array = sectTripFuelQueue.ToArray ();

				double[] gas_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					gas_array [i] = sect_trip_gas_array [i].fuelConsumption;
				}

				return gas_array;
			}
		}

		public double[] SectTripPerFuelArray
		{
			get
			{
				int i;
				TripFuelContent[] sect_trip_gas_array = sectTripFuelQueue.ToArray ();

				double[] gasmilage_array = new double[sect_trip_gas_array.Length];

				for (i = 0; i < sect_trip_gas_array.Length; i++) {
					gasmilage_array [i] = sect_trip_gas_array [i].TripPerFuel;
				}

				return gasmilage_array;
			}
		}


		public double SectTripLatest
		{
			get
			{
				return sectTripFuelLatest.trip;
			}
		}
		public double SectFuelLatest
		{
			get
			{
				return sectTripFuelLatest.fuelConsumption;
			}
		}
		public double SectTripPerFuelLatest
		{
			get
			{
				return sectTripFuelLatest.TripPerFuel;
			}
		}

		//瞬間トリップ、燃料消費量
		private double momentaryTrip;
		private double momentaryFuelConsumption;
		public double Momentary_Trip
		{
			get
			{
				return momentaryTrip;
			}
		}
		public double MomentaryFuelConsumption
		{
			get
			{
				return momentaryFuelConsumption;
			}
		}
		public double MomentaryTripPerFuel
		{
			get
			{
				if (momentaryFuelConsumption != 0)
				{
					return momentaryTrip / momentaryFuelConsumption;
				}
				else
				{
					return 0;
				}
			}
		}

		public FuelTripCalculator(FuelTripCalculatorOption option)
		{
            this.calculatorOption = option;

			totalTripFuel = new TripFuelContent ();

			sectElapsed = 0;
			sectStoreMax = 60;
			sectSpan = 60*1000;
			sectTripFuelTemporary = new TripFuelContent ();
			sectTripFuelQueue = new Queue<TripFuelContent>();
			sectTripFuelLatest = new TripFuelContent();

			saveElapsedTime = 0;

			//データ格納しているファイルパスの指定
			folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			filePath = Path.Combine( folderPath, "." + "FUELTRIP_Logger");

			loadTripFuel();

			stopWatch = new Stopwatch ();
			stopWatch.Reset ();
		}

		//デストラクタ
		~FuelTripCalculator()
		{
			saveTripFuel();
		}

		public void update(double rev, double speed, double injpulseWdth)
		{
            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
                return;
            }
                
			stopWatch.Stop ();

            //get elasped time
            long stopwatch_elasped = stopWatch.ElapsedMilliseconds;

			// Set current value
			currentVehicleSpeed = speed;
			currentEngineRev = rev;
			currentInjectionPulseWidth = injpulseWdth;

			// elaspedが長すぎる場合,タイムアウトを発生
			if (stopwatch_elasped > stopwatchTimeout) {
				stopWatch.Reset ();
                stopWatch.Start();
				throw new TimeoutException ("tacho/speed/injpulse update span is too large (Timeout).");
			}
            stopWatch.Reset();
			stopWatch.Start ();

			momentaryTrip = getMomentaryTrip(stopwatch_elasped);
			momentaryFuelConsumption = getMomentaryFuelComsumption(stopwatch_elasped);

			totalTripFuel.trip += momentaryTrip;
			totalTripFuel.fuelConsumption += momentaryFuelConsumption;

			sectElapsed += stopwatch_elasped;

			//区間データアップデート
			if (sectElapsed < sectSpan)
			{
				sectTripFuelTemporary.trip += momentaryTrip;
				sectTripFuelTemporary.fuelConsumption += momentaryFuelConsumption;
			}
			else
			{
				//Section履歴に追加
				enqueueSectTripFuel (sectTripFuelTemporary);

				sectTripFuelTemporary = new TripFuelContent();
				sectElapsed = 0;

				//区間データ更新イベント発生
				SectFUELTRIPUpdated (this, EventArgs.Empty);
			}

			//総燃費、総距離を5sごとに保存
			if (saveElapsedTime < saveSpan)
			{
				saveElapsedTime += stopwatch_elasped;
			}
			else
			{
				saveTripFuel();
				saveElapsedTime = 0;
			}				
		}

		private double getMomentaryTrip(long elasped_millisecond)
		{
			double speed = currentVehicleSpeed;
            double TripCoefficient = calculatorOption.TripCorrectionFactor;
			double monentary_trip = TripCoefficient * (speed) / 3600 / 1000 * elasped_millisecond;

			return monentary_trip;
		}

		private double getMomentaryFuelComsumption(long elasped_millisecond)
		{
			//燃料消費量計算
			double inj_pulse_width = currentInjectionPulseWidth;
			double tacho = currentEngineRev;

            double GasConsumptionCoefficient = calculatorOption.FuelCorrectionFactor;
            double NumCylinder = calculatorOption.NumCylinder;
            double InjectorCapacity = calculatorOption.InjectorCapacity;
            double InjectionLatency = calculatorOption.InjectionLatency;

			double momentary_gas_consumption;
			if (tacho > 500) //アイドリング回転以下の場合は、燃料消費量に加えない
				momentary_gas_consumption = GasConsumptionCoefficient * (double)NumCylinder * (double)tacho * InjectorCapacity * (inj_pulse_width - InjectionLatency) / (7.2E9) * elasped_millisecond / 1000;
			else
				momentary_gas_consumption = 0;

            //燃料消費量が負の場合、燃料消費量は0とする
            if (momentary_gas_consumption < 0)
                momentary_gas_consumption = 0;

			return momentary_gas_consumption;
		}

		private void enqueueSectTripFuel(TripFuelContent content)
		{
			sectTripFuelLatest = content;
			sectTripFuelQueue.Enqueue (content);

			if (sectTripFuelQueue.Count > sectStoreMax) {
				sectTripFuelQueue.Dequeue ();
			}

		}

		public void resetTotalTripFuel()
		{
			totalTripFuel = new TripFuelContent ();
		}

		public void resetSectTripFuel()
		{
			sectTripFuelLatest = new TripFuelContent ();
			sectTripFuelQueue = new Queue<TripFuelContent> ();
		}

		public void loadTripFuel()
		{
			//XmlSerializerオブジェクトの作成
			System.Xml.Serialization.XmlSerializer serializer =
				new System.Xml.Serialization.XmlSerializer(typeof(TripFuelContent));

			try
			{
				//ファイルを開く
				System.IO.FileStream fs =
					new System.IO.FileStream(filePath, System.IO.FileMode.Open);

				try
				{
					//XMLファイルから読み込み、逆シリアル化する
					totalTripFuel =
						(TripFuelContent)serializer.Deserialize(fs);

				}
				catch (XmlException ex)
				{
					Console.WriteLine(ex.Message);
					this.resetTotalTripFuel();
				}

				fs.Close();

			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex.Message);
				this.resetTotalTripFuel ();
			}
			catch (DirectoryNotFoundException ex)
			{
				Console.WriteLine(ex.Message);
				System.IO.Directory.CreateDirectory(folderPath);
				this.resetSectTripFuel ();
			}
			catch (System.Security.SecurityException ex)
			{
				Console.WriteLine(ex.Message);
				this.resetTotalTripFuel ();
			}
		}


		public void saveTripFuel()
		{
			//XmlSerializerオブジェクトを作成
			//書き込むオブジェクトの型を指定する
			System.Xml.Serialization.XmlSerializer serializer1 =
				new System.Xml.Serialization.XmlSerializer(typeof(TripFuelContent));
			//ファイルを開く
			System.IO.FileStream fs1 =
				new System.IO.FileStream(filePath, System.IO.FileMode.Create);
			//シリアル化し、XMLファイルに保存する
			serializer1.Serialize(fs1, totalTripFuel);
			//閉じる
			fs1.Close();
		}
	}

	public class TripFuelContent
	{
		public double trip { get; set; }
		public double fuelConsumption { get; set; }

		public TripFuelContent()
		{
			reset ();
		}

		private void reset()
		{
			this.trip = 0;
			this.fuelConsumption = 0;
		}

		public double TripPerFuel
		{
			get
			{
				if (this.fuelConsumption == 0)
					return 0;
				else
					return this.trip / this.fuelConsumption;
			}
		}

	}
}

