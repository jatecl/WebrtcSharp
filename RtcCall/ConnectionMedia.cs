using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relywisdom
{
    internal class ConnectionMedia : IConnectionState
    {
        private Dictionary<string, object> media;

        public ConnectionMedia(Dictionary<string, object> media)
        {
            this.media = media;
        }

        public override void start()
        {
            base.start();
            var query = new Dictionary<string, object>();
            this.call.emitQuery(query, this.remote);
            this.socket.send(new
            {
                kind = "webrtc",
                action = "media",
                to = this.remote.id,
                media = this.media,
                query
            });
        }

        public override void onmessage(Dictionary<string, object> msg)
        {
            if (msg.Get<string>("action") == "offer")
            {
                Task.Factory.StartNew(async () => {
                    var answer = await this.connection.createAnswer(msg.Get<Dictionary<string, object>>("offer"));
                    this.socket.send(new
                    {
                        kind = "webrtc",
                        action = "answer",
                        to = this.remote.id,
                        answer
                    });
                    this.connection.setState(new ConnectionWaitForQuery());
                });
            }
            else if (msg.Get<string>("action") == "require")
            {
                this.connection.setState(new ConnectionOffer());
            }
        }
    }
}