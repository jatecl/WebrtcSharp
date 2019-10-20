using System.Collections.Generic;

namespace Relywisdom
{
    public interface IMessageFilter
    {
        void onmessage(Dictionary<string, object> msg);
    }
}
