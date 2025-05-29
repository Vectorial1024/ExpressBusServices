namespace ExpressBusServices.DataTypes
{
    /// <summary>
    /// Represents the rubberbanding command to be given to the vehicle that asked for it.
    /// Vehicles should act according to this command.
    /// </summary>
    public enum RubberbandingCommand
    {
        Release = 1,
        Hold = -1,
        Default = 0,
    }
}
