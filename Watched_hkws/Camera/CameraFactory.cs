using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Watched_hkws.Camera
{
    public interface ICameraFactory
    {
        void Add(RealStearm realStearm);
        void Remove(string camera);
        List<RealStearm> All();
        RealStearm rst(string camera);
    }
    public class CameraFactory:ICameraFactory
    {
        public List<RealStearm> realStearms=new List<RealStearm>();

        public void Add(RealStearm realStearm)
        {
            realStearms.Add(realStearm);
        }

        //when disconnect
        public void Remove(string camera)
        {
            realStearms.Remove(rst(camera));
        }

        public List<RealStearm> All()
        {
            return realStearms;
        }

        public RealStearm rst(string camera)
        {
            return realStearms.FirstOrDefault(c => c.camera == camera);
        }
    }
}
