using System.Collections.Generic;

namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// An object that contains all vehicle progresses in this transport line for easy analysis.
    /// </summary>
    public class TransportLineVehicleProgress
    {
        /// <summary>
        /// The progress array in ascending order of progress.
        /// </summary>
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

        /// <summary>
        /// Returns the vehicle line progress of the vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOf(ushort vehicleID)
        {
            if (!progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that
            return progressArray[indexPos];
        }

        /// <summary>
        /// Returns the vehicle line progress of the vehicle "in front of" the given vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOfFrontOf(ushort vehicleID)
        {
            if (!progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that; find its front-next value
            int frontNextIndexPos = indexPos == progressArray.Length - 1 ? 0 : indexPos + 1;
            return progressArray[frontNextIndexPos];
        }

        /// <summary>
        /// Returns the vehicle line progress of the vehicle "behind" the given vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOfBackOf(ushort vehicleID)
        {
            if (!progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that; find its back-next value
            int backNextIndexPos = indexPos == 0 ? progressArray.Length - 1 : indexPos - 1;
            return progressArray[backNextIndexPos];
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
