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
    }
}
