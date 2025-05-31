namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// Represents the departure intention of the vehicle that asked for it.
    /// This can be used for rubberbanding unbunching, or just plan per-stop departure control.
    /// <para/>
    /// Vehicles should act according to this command.
    /// </summary>
    public enum DepartureIntention
    {
        /// <summary>
        /// Vehicle is granted permission to leave the stop.
        /// </summary>
        Go = 1,
        /// <summary>
        /// Vehicle should wait at the station for unbunching.
        /// </summary>
        Hold = -1,
        /// <summary>
        /// No comment; let vanilla decide.
        /// </summary>
        Default = 0,
    }
}
