using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Watched_hkws.Camera;

namespace Watched_hkws.MeWebSocket
{
    public static class WebSocketExtensions
    {
        public static IApplicationBuilder UseCustomWebSocketManager(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomWebSocketManager>();
        }
    }

    public class CustomWebSocketManager
    {
        private readonly RequestDelegate _next;
        private readonly ICameraFactory _cameraFactory;

        public CustomWebSocketManager(RequestDelegate next, ICameraFactory cameraFactory)
        {
            _next = next;
            _cameraFactory = cameraFactory;
        }

        public async Task Invoke(HttpContext context, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    string user = context.Request.Query["uid"];
                    string camera = context.Request.Query["cid"];
                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(camera))
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        CustomWebSocket userWebSocket = new CustomWebSocket()
                        {
                            WebSocket = webSocket,
                            Username = user,
                            Camera = camera
                        };
                        await wsmHandler.SendInitialMessages(userWebSocket);
                        wsFactory.Add(userWebSocket);
                        CreateRealStream(wsFactory, camera);
                        await Listen(context, userWebSocket, wsFactory, wsmHandler);
                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else 
            {
                await _next(context);
            }
        }

        private async Task Listen(HttpContext context, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
        {
            WebSocket webSocket = userWebSocket.WebSocket;
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await wsmHandler.HandleMessage(result, buffer, userWebSocket, wsFactory);
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            wsFactory.Remove(userWebSocket.Username);
            List<CustomWebSocket> customs = wsFactory.Part(userWebSocket.Camera);
            if (customs.Count<=0)
            {
                _cameraFactory.rst(userWebSocket.Camera).StopPreview();
                _cameraFactory.rst(userWebSocket.Camera).LoginOut();
                _cameraFactory.Remove(userWebSocket.Camera);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private void CreateRealStream(ICustomWebSocketFactory wsFactory, string camera)
        {
            if (_cameraFactory.rst(camera) == null)
            {
                RealStearm realStearm = new RealStearm(wsFactory, camera);
                realStearm.Login();
                realStearm.Preview();
                _cameraFactory.Add(realStearm);
            }
            else 
            {
                if (_cameraFactory.rst(camera).m_lRealHandle<0)
                {
                    _cameraFactory.rst(camera).Preview();
                }
            }
        }
    }
}
