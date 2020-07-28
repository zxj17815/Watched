using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watched_hkws.MeWebSocket
{
    public interface ICustomWebSocketFactory
    {
        void Add(CustomWebSocket uws);
        void Remove(string username);
        List<CustomWebSocket> All();
        List<CustomWebSocket> Others(CustomWebSocket client);
        List<CustomWebSocket> Part(string camera);
        CustomWebSocket Client(string username);
    }

    public class CustomWebSocketFactory : ICustomWebSocketFactory
    {
        List<CustomWebSocket> List;

        public CustomWebSocketFactory()
        {
            List = new List<CustomWebSocket>();
        }

        public void Add(CustomWebSocket uws)
        {
            List.Add(uws);
        }

        //when disconnect
        public void Remove(string username)
        {
            List.Remove(Client(username));
        }

        public List<CustomWebSocket> All()
        {
            return List;
        }

        public List<CustomWebSocket> Others(CustomWebSocket client)
        {
            return List.Where(c => c.Username != client.Username).ToList();
        }

        public List<CustomWebSocket> Part(string camera)
        {
            return List.Where(c => c.Camera == camera).ToList();
        }

        public CustomWebSocket Client(string username)
        {
            return List.First(c => c.Username == username);
        }
    }
}
