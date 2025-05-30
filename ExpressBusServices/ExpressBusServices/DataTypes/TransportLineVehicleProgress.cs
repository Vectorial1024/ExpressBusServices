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
        private readonly VehicleLineProgress[] _progressArray;

        private readonly Dictionary<ushort, int> _progressIndex = new Dictionary<ushort, int>();

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="vehicleLineProgress">The (unsorted) list of individual vehicle progresses.</param>
        public TransportLineVehicleProgress(List<VehicleLineProgress> vehicleLineProgress)
        {
            // sort the list by their normalized progress and put them into an array
            vehicleLineProgress.Sort((left, right) => left.percentProgress.CompareTo(right.percentProgress));
            _progressArray = vehicleLineProgress.ToArray();

            // also index the array-index for quick lookup
            int arrSize = _progressArray.Length;
            for (int i = 0; i < arrSize; i++)
            {
                VehicleLineProgress currentProgress = _progressArray[i];
                _progressIndex.Add(currentProgress.vehicleID, i);
            }
        }

        /// <summary>
        /// Returns the vehicle line progress of the vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOf(ushort vehicleID)
        {
            if (!_progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that
            return _progressArray[indexPos];
        }

        /// <summary>
        /// Returns the vehicle line progress of the vehicle "in front of" the given vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOfFrontOf(ushort vehicleID)
        {
            if (!_progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that; find its front-next value
            int frontNextIndexPos = indexPos == _progressArray.Length - 1 ? 0 : indexPos + 1;
            return _progressArray[frontNextIndexPos];
        }

        /// <summary>
        /// Returns the vehicle line progress of the vehicle "behind" the given vehicle.
        /// </summary>
        /// <param name="vehicleID"></param>
        /// <returns></returns>
        public VehicleLineProgress? GetProgressOfBackOf(ushort vehicleID)
        {
            if (!_progressIndex.TryGetValue(vehicleID, out int indexPos))
            {
                // we do not have that
                return null;
            }
            // we do have that; find its back-next value
            int backNextIndexPos = indexPos == 0 ? _progressArray.Length - 1 : indexPos - 1;
            return _progressArray[backNextIndexPos];
        }
    }
}
