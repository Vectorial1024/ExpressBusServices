using System;

namespace ExpressBusServices
{
    public class VehicleBAInfo
    {
        public int Alighted { get; set; }

        [Obsolete("Use PassengersBeforeBoarding, PassengersAfterBoarding, ActualBoarded instead.")]
        public int Boarded { get; set; }

        public ushort PassengersBeforeBoarding { get; set; }

        public ushort PassengersAfterBoarding { get; set; }

        public int ActualBoarded => PassengersAfterBoarding - PassengersBeforeBoarding;
    }
}
