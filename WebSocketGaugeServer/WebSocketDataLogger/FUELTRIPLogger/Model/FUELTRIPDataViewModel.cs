using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Model
{
    public class FUELTRIPDataViewModel
    {
        private readonly FUELTRIPService fUELTRIPService;

        public double TotalTrip
        {
            get => fUELTRIPService.FUELTripCalculator.TotalTrip;
        }

        public double TotalFuelConsumption
        {
            get => fUELTRIPService.FUELTripCalculator.TotalFuelConsumption;
        }

        public double TotalTripPerFuel
        {
            get => fUELTRIPService.FUELTripCalculator.TotalTripPerFuel;
        }

        public double[] SectTripPerFuelArray
        {
            get => fUELTRIPService.FUELTripCalculator.SectTripPerFuelArray;
        }

        public double SectSpan
        {
            get => fUELTRIPService.FUELTripCalculator.SectSpan;
        }

        public FUELTRIPDataViewModel(FUELTRIPService service)
        {
            this.fUELTRIPService = service;
        }
    }
}