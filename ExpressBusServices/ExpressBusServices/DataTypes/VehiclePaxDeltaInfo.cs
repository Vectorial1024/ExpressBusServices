using System.Collections.Generic;

namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// A struct for remembering per-vehicle passenger delta information, assuming the vehicle is a local public transport vehicle now stopped at some stop.
    /// <para/>
    /// It just turns out, all kinds of public transport vehicles share essentially the same flow. 
    /// </summary>
    public struct VehiclePaxDeltaInfo
    {
        private static Dictionary<ushort, VehiclePaxDeltaInfo> paxDeltaTable;

        public static void TouchAndResetTable()
        {
            if (paxDeltaTable == null)
            {
                paxDeltaTable = new Dictionary<ushort, VehiclePaxDeltaInfo>();
            }
        }
        
        public static void WipeTable() => paxDeltaTable?.Clear();
    }
}
