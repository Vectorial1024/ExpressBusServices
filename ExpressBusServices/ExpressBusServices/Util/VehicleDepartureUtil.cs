namespace ExpressBusServices.Util
{
    /// <summary>
    /// The util class for handling customized vehicle departure logic
    /// </summary>
    public static class VehicleDepartureUtil
    {
        /// <summary>
        /// The centralized method for reviewing whether the given local public transport vehicle should change its departure status.
        /// This mod may hold or release vehicles according to a list of criteria, essentially overriding vanilla logic.
        /// </summary>
        /// <param name="__result">The result of this operation; if true, signals the game engine to depart the vehicle at the next sim-step.</param>
        /// <param name="vehicleID">The ID of the vehicle in question.</param>
        /// <param name="vehicleData">The data of the vehicle in question.</param>
        public static void ReviewDepartureStatus(ref bool __result, ushort vehicleID, ref Vehicle vehicleData)
        {
            // wip
        }
        // wip
    }
}
