namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// Represents the rubberbanding command to be given to the vehicle that asked for it.
    /// Vehicles should act according to this command.
    /// </summary>
    public enum RubberbandingCommand
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
