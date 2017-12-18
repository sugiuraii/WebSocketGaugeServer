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

		//Section fuel and trip
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

		private double momentaryTrip;
		private double momentaryFuelConsumption;
		public double MomerntaryTrip
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

        /// <summary>
        /// Update fuel and trip by given parameters.
        /// (Unused parameters are ignored for calculation)
        /// </summary>
        /// <param name="rev">Engine rev (only refered on RPM_INJECTION_PW mode)</param>
        /// <param name="speed">Vehicle speed.</param>
        /// <param name="injpulseWdth">Injection pulse width (in ms)(only refered on RPM_INJECTION_PW mode)</param>
        /// <param name="massAirFlow">Mass air flow (g/s) (only refered on MASS_AIR_FLOW or MASS_AIR_FLOW_AF mode)</param>
        /// <param name="AFRatio">AF ration (only refered on MASS_AIR_FLOW_AF mode)</param>
        /// <param name="fuelRate">Fuel rate (L/h) (only refered on FUEL_RATE mode)</param>
		public void update(double rev, double speed, double injpulseWdth, double massAirFlow, double AFRatio, double fuelRate)
		{
            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
                return;
            }
                
			stopWatch.Stop ();

            //get elasped time
            long stopwatch_elapsed = stopWatch.ElapsedMilliseconds;

			// Invoke timeout exception if the elapsed time over timeout
			if (stopwatch_elapsed > stopwatchTimeout) {
				stopWatch.Reset ();
                stopWatch.Start();
				throw new TimeoutException ("tacho/speed/injpulse/massAirFlow/AFRatio update span is too large (Timeout).");
			}
            stopWatch.Reset();
			stopWatch.Start ();

            //Calculate momentary trip from speed.
			momentaryTrip = getMomentaryTrip(stopwatch_elapsed, speed, calculatorOption);

            //Calculate momentart fuel consumption.
            switch(calculationMethod)
            {
                case FuelCalculationMethod.RPM_INJECTION_PW:
                    momentaryFuelConsumption = FuelCalcualtionFormulas.FuelCalcByRevInjPW(stopwatch_elapsed, rev, injpulseWdth, calculatorOption);
                    break;
                case FuelCalculationMethod.MASS_AIR_FLOW:
                    momentaryFuelConsumption = FuelCalcualtionFormulas.FuelCalcByMassAir(stopwatch_elapsed, massAirFlow, calculatorOption);
                    break;
                case FuelCalculationMethod.MASS_AIR_FLOW_AF:
                    momentaryFuelConsumption = FuelCalcualtionFormulas.FuelCalcByAFAndMassAir(stopwatch_elapsed, massAirFlow, AFRatio, calculatorOption);
                    break;
                case FuelCalculationMethod.FUEL_RATE:
                    momentaryFuelConsumption = fuelRate / 3600;
                    break;
            }

            // Add to tolal trip and total fuel.
			totalTripFuel.trip += momentaryTrip;
			totalTripFuel.fuelConsumption += momentaryFuelConsumption;

            // Increase section elapsed timer co
			sectElapsed += stopwatch_elapsed;

			// Update sect data.
			if (sectElapsed < sectSpan)
			{
				sectTripFuelTemporary.trip += momentaryTrip;
				sectTripFuelTemporary.fuelConsumption += momentaryFuelConsumption;
			}
			else
			{
				enqueueSectTripFuel (sectTripFuelTemporary);

				sectTripFuelTemporary = new TripFuelContent();
				sectElapsed = 0;

				//Invoke event
				SectFUELTRIPUpdated (this, EventArgs.Empty);
			}

			//Save total fuel and trip to file by saveSpan(default : 5min)
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

		private double getMomentaryTrip(long elasped_millisecond, double currentVehicleSpeed, FuelTripCalculatorOption calculatorOption)
		{
            double TripCoefficient = calculatorOption.TripCorrectionFactor;
			double monentary_trip = TripCoefficient * (currentVehicleSpeed) / 3600 / 1000 * elasped_millisecond;

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

        /// <summary>
        /// Save fuel and trip data to file.
        /// </summary>
		public void saveTripFuel()
		{
			System.Xml.Serialization.XmlSerializer serializer1 =
				new System.Xml.Serialization.XmlSerializer(typeof(TripFuelContent));
			System.IO.FileStream fs1 =
				new System.IO.FileStream(filePath, System.IO.FileMode.Create);
			serializer1.Serialize(fs1, totalTripFuel);
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

