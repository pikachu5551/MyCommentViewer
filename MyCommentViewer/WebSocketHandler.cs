using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MyCommentViewer
{
    public class WebSocketHandler
    {
        WebSocket _socket;

        WebSocketHandler(WebSocket _socket)
        {
            this._socket = _socket;
        }
    }
}
