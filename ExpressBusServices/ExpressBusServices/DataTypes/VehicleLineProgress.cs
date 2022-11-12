using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBusServices.DataTypes
{
    public struct VehicleLineProgress
    {
        public ushort vehicleID;
        public float percentProgress;

        public VehicleLineProgress(ushort vehicleID, float progress)
        {
            this.vehicleID = vehicleID;
            this.percentProgress = progress;
        }
    }
}
