using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUELTRIP_Logger
{
    class FuelCalcualtionFormulas
    {
        /// <summary>
        /// Density of Fuel in gram/cc.
        /// </summary>
        public static double DensityOfFuel = 0.73;

        /// <summary>
        /// Default A/F ratio.
        /// </summary>
        public static double DefaultAFRatio = 14.7;

        /// <summary>
        /// Calculate and return momentary fuel consumption by rev and injection pulse width method.
        /// </summary>
        /// <param name="elaspedMillisecond">Elapsed time in millisecond.</param>
        /// <param name="engineRev">Engine rpm.</param>
        /// <param name="injetctioPulseWidth">Injection pulse width</param>
        /// <param name="calculatorOption">Calculator option</param>
        /// <returns></returns>
        public static double FuelCalcByRevInjPW(double elaspedMillisecond, double engineRev, double injectionPulseWidth, FuelTripCalculatorOption calculatorOption)
        {
            double GasConsumptionCoefficient = calculatorOption.FuelCorrectionFactor;
            int NumCylinder = calculatorOption.NumCylinder;
            double InjectorCapacity = calculatorOption.InjectorCapacity;
            double InjectionLatency = calculatorOption.InjectionLatency;

            double momentaryFuelConsumption;
            if (engineRev > 500) //Calculate fuel consumption only if the rev is over idling rev.
                momentaryFuelConsumption = GasConsumptionCoefficient * (double)NumCylinder * (double)engineRev * InjectorCapacity * (injectionPulseWidth - InjectionLatency) / (7.2E9) * elaspedMillisecond / 1000;
            else
                momentaryFuelConsumption = 0;

            //If the calculated momentary fuel consumption is negative, override by zero. 
            if (momentaryFuelConsumption < 0)
                momentaryFuelConsumption = 0;

            return momentaryFuelConsumption;
        }

        /// <summary>
        /// Calulate fuel consumption by mass air flow and AF ratio.
        /// </summary>
        /// <param name="elapsedMillisecond">Elapsed time</param>
        /// <param name="massAirFlow">Mass air flow (g/s)</param>
        /// <param name="AFRatio">AF ratio</param>
        /// <param name="calculatorOption">Calculator option</param>
        /// <returns></returns>
        public static double FuelCalcByAFAndMassAir(double elapsedMillisecond, double massAirFlow, double AFRatio, FuelTripCalculatorOption calculatorOption)
        {
            double GasConsumptionCoefficient = calculatorOption.FuelCorrectionFactor;
            double momentaryFuelConsumption = massAirFlow / AFRatio / DensityOfFuel / 1000;

            return momentaryFuelConsumption * GasConsumptionCoefficient * elapsedMillisecond;
        }

        /// <summary>
        /// Calculate fuel consumption by mass air flow.
        /// </summary>
        /// <param name="elapsedMillisecond">Elapsed time</param>
        /// <param name="massAirFlow">Mass air flow</param>
        /// <param name="calculatorOption">AF ratio</param>
        /// <returns></returns>
        public static double FuelCalcByMassAir(double elapsedMillisecond, double massAirFlow, FuelTripCalculatorOption calculatorOption)
        {
            return FuelCalcByAFAndMassAir(elapsedMillisecond, massAirFlow, DefaultAFRatio, calculatorOption);
        }
    }
}
