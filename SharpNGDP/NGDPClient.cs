using SharpNGDP.Ribbit;
using SharpNGDP.TACT;

namespace SharpNGDP
{
    public class NGDPClient
    {
        public NGDPClient(NGDPContext context)
        {
            Context = context;

            RibbitClient = new RibbitClient(Context);
            TACTClient = new TACTClient(Context);
        }

        public NGDPClient()
            : this(NGDPContext.DefaultContext)
        { }

        public NGDPContext Context { get; private set; }

        public RibbitClient RibbitClient { get; private set; }
        public TACTClient TACTClient { get; private set; }
    }
}
