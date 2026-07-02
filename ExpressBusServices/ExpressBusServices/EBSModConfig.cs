namespace ExpressBusServices
{
    public class EBSModConfig
    {
        public enum ExpressMode
        {
            PRUDENTIAL = 0,
            AGGRESSIVE,
            PROTOCOL_424,
        }

        // this is mainly used to determine the kind of unbunching that should be enabled
        public static ExpressMode CurrentExpressBusMode { get; set; }

        // this is used to determine whether service self-balancing is enabled
        public static bool UseServiceSelfBalancing { get; set; }

        // this is used to determine whether service self-balancing can target middle stops
        public static bool ServiceSelfBalancingCanDoMiddleStop { get; set; }

        // this is used to determine whether minibus mode is enabled: fast board/depart for minibus vehicles
        public static bool CanUseMinibusMode { get; set; }

        // section break for express tram

        public enum ExpressTramMode
        {
            NONE = 0,
            LIGHT_RAIL,
            TRAM,
            STREET_CAR,
        }

        public static ExpressTramMode CurrentExpressTramMode { get; set; }

        #region Express Railway Mode(s)

        public enum ExpressRailwayMode
        {
            NONE = 0,
            TIGHT,
        }

        /*
         * important note:
         * MetroTrainAI extends PassengerTrainAI! if want to detect in-game "trains" then need to detect accurately.
         */

        public static ExpressRailwayMode CurrentExpressMetroMode { get; set; }

        #endregion
    }
}
