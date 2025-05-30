using System.Collections.Generic;

namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// An object that contains all vehicle progresses in this transport line for easy analysis.
    /// </summary>
    public class TransportLineVehicleProgress
    {
        private VehicleLineProgress[] progressArray;

        private Dictionary<ushort, int> progressIndex = new Dictionary<ushort, int>();

        private int targetVehicleIndex = -1;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="vehicleLineProgress">The (unsorted) list of individual vehicle progresses.</param>
        public TransportLineVehicleProgress(List<VehicleLineProgress> vehicleLineProgress)
        {
            // sort the list by their normalized progress and put them into an array
            vehicleLineProgress.Sort((left, right) => left.percentProgress.CompareTo(right.percentProgress));
            progressArray = vehicleLineProgress.ToArray();

            // also index the array-index for quick lookup
            int arrSize = progressArray.Length;
            for (int i = 0; i < arrSize; i++)
            {
                VehicleLineProgress currentProgress = progressArray[i];
                progressIndex.Add(currentProgress.vehicleID, i);
            }
        }

        public void ResetVehicleFocus()
        {
            targetVehicleIndex = -1;
        }

        public bool SetVehicleFocus(ushort vehicleID)
        {
            if (!progressIndex.TryGetValue(vehicleID, out int indexedPos))
            {
                // not in our list!
                return false;
            }
            targetVehicleIndex = indexedPos;
            return true;
        }
    }
}
