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
    }
}
