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
        private FuelCalculationMethod calculationMethod;

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

        /// <summary>
        /// Consturctor of FuelTripCalculator.
        /// </summary>
        /// <param name="option">Calculation option.</param>
        /// <param name="calculationMethod">Calclation method</param>
		public FuelTripCalculator(FuelTripCalculatorOption option, FuelCalculationMethod calculationMethod)
		{
            this.calculatorOption = option;
            this.calculationMethod = calculationMethod;

			totalTripFuel = new TripFuelContent ();

			sectElapsed = 0;
			sectStoreMax = 60;
			sectSpan = 60*1000;
			sectTripFuelTemporary = new TripFuelContent ();
			sectTripFuelQueue = new Queue<TripFuelContent>();
			sectTripFuelLatest = new TripFuelContent();

			saveElapsedTime = 0;

			//Set data folder and file path.
			folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			filePath = Path.Combine( folderPath, "." + "FUELTRIP_Logger");

			loadTripFuel();

			stopWatch = new Stopwatch ();
			stopWatch.Reset ();
		}

		//Destructor
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
            long stopwatch_elapsed = stopWatch.ElapsedMilliseconds;

			// Set current value
			currentVehicleSpeed = speed;
			currentEngineRev = rev;
			currentInjectionPulseWidth = injpulseWdth;

			// Invoke timeout exception if the elapsed time over timeout
			if (stopwatch_elapsed > stopwatchTimeout) {
				stopWatch.Reset ();
                stopWatch.Start();
				throw new TimeoutException ("tacho/speed/injpulse update span is too large (Timeout).");
			}
            stopWatch.Reset();
			stopWatch.Start ();

			momentaryTrip = getMomentaryTrip(stopwatch_elapsed);
			momentaryFuelConsumption = getMomentaryFuelComsumption(stopwatch_elapsed);

			totalTripFuel.trip += momentaryTrip;
			totalTripFuel.fuelConsumption += momentaryFuelConsumption;

			sectElapsed += stopwatch_elapsed;

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
				saveElapsedTime += stopwatch_elapsed;
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
			System.Xml.Serialization.XmlSerializer serializer =
				new System.Xml.Serialization.XmlSerializer(typeof(TripFuelContent));

			try
			{
				System.IO.FileStream fs =
					new System.IO.FileStream(filePath, System.IO.FileMode.Open);

				try
				{
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

