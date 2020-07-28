using Hikvision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Watched_hkws.MeWebSocket;

namespace Watched_hkws.Camera
{
    public class RealStearm
    {
        private readonly ICustomWebSocketFactory _customWebSocketFactory;

        public RealStearm(ICustomWebSocketFactory customWebSocketFactory, string camera)
        {
            _customWebSocketFactory = customWebSocketFactory;
            this.camera = camera;
        }

        private Stream _stream;
        public string camera;

        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private bool m_bTalk = false;
        private bool m_Std = false;
        public Int32 m_lRealHandle = -1;
        private int lVoiceComHandle = -1;
        private string str;

        CHCNetSDK.REALDATACALLBACK RealData = null;
        CHCNetSDK.STDDATACALLBACK StdData = null;
        CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;
        public CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;
        public void cbLoginCallBack(int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
        {
            string strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

            if (dwResult == 0)
            {
                uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
                strLoginCallBack = strLoginCallBack + "，错误号:" + iErrCode;
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        public void Login()
        {
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                //MessageBox.Show("NET_DVR_Init error!");
                Console.WriteLine("NET_DVR_Init error!");
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "SdkLog", true);
            }
            if (m_lUserID < 0)
            {
                struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

                //设备IP地址或者域名
                byte[] byIP = System.Text.Encoding.Default.GetBytes(camera);
                struLogInfo.sDeviceAddress = new byte[129];
                byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

                //设备用户名
                byte[] byUserName = System.Text.Encoding.Default.GetBytes("admin");
                struLogInfo.sUserName = new byte[64];
                byUserName.CopyTo(struLogInfo.sUserName, 0);

                //设备密码
                byte[] byPassword = System.Text.Encoding.Default.GetBytes("Zyadmin888");
                struLogInfo.sPassword = new byte[64];
                byPassword.CopyTo(struLogInfo.sPassword, 0);

                struLogInfo.wPort = ushort.Parse("8000");//设备服务端口号

                if (LoginCallBack == null)
                {
                    LoginCallBack = new CHCNetSDK.LOGINRESULTCALLBACK(cbLoginCallBack);//注册回调函数                    
                }
                struLogInfo.cbLoginResult = LoginCallBack;
                struLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 

                DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V40 failed, error code= " + iLastErr; //登录失败，输出错误号
                    Console.WriteLine(str);
                    //MessageBox.Show(str);
                }
                else
                {
                    //登录成功
                    //MessageBox.Show("Login Success!");
                    //btnLogin.Text = "Logout";
                    Console.WriteLine("Login Success!");
                }

            }
        }

        /// <summary>
        /// 注销登录 Logout the device
        /// </summary>
        public void LoginOut()
        {
            //注销登录 Logout the device
            if (m_lRealHandle >= 0)
            {
                //MessageBox.Show("Please stop live view firstly");
                return;
            }

            if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_Logout failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                return;
            }
            m_lUserID = -1;
            //btnLogin.Text = "Login";
        }


        /// <summary>
        /// 播放预览
        /// </summary>
        public void Preview()
        {
            if (m_lUserID < 0)
            {
                //MessageBox.Show("Please login the device firstly");
                Console.WriteLine("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.lChannel = 1;//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 1; //播放库播放缓冲区最大缓冲帧数
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;

                //if (textBoxID.Text != "")
                //{
                //    lpPreviewInfo.lChannel = -1;
                //    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                //    lpPreviewInfo.byStreamID = new byte[32];
                //    byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
                //}


                if (RealData == null)
                {
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数
                }

                //if (StdData == null)
                //{
                //    StdData = new CHCNetSDK.STDDATACALLBACK(StandardDataCallBack);//预览实时流回调函数
                //}


                IntPtr pUser = new IntPtr();//用户数据
                uint dwUser = 0;
                //打开预览 Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, /*null*/RealData, pUser);
                //m_Std=CHCNetSDK.NET_DVR_SetStandardDataCallBack(m_lRealHandle, StdData, dwUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                    //MessageBox.Show(str);
                    Console.WriteLine(str);
                    return;
                }
                else
                {
                    //预览成功
                    //btnPreview.Text = "Stop Live View";
                    Console.WriteLine("预览成功");
                }
            }
            return;
        }


        /// <summary>
        /// 停止预览
        /// </summary>
        public void StopPreview()
        {
            if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                //MessageBox.Show(str);
                Console.WriteLine(str);
            }
            m_lRealHandle = -1;
            //btnPreview.Text = "Live View";
        }

        /// <summary>
        /// 预览回调
        /// </summary>
        /// <param name="lRealHandle"></param>
        /// <param name="dwDataType"></param>
        /// <param name="pBuffer"></param>
        /// <param name="dwBufSize"></param>
        /// <param name="pUser"></param>
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0 && dwDataType == 2)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                //string str = "实时流数据.ps";
                //FileStream fs = new FileStream(str, FileMode.Append);
                //int iLen = (int)dwBufSize;
                //fs.Write(sData, 0, iLen);
                //fs.Close();
                List<CustomWebSocket> customs = _customWebSocketFactory.Part(this.camera);
                _ = Send(sData, customs);
                //if (customs.Count > 0)
                //{
                //    _ = Send(sData, customs);
                //}
                //else
                //{
                //    StopPreview();
                //    LoginOut();
                //}
            }
        }


        /// <summary>
        /// 发送数据流
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task Send(byte[] data, List<CustomWebSocket> customs)
        {
            if (data != null && data.Length > 0)
            {
                //await _stream.WriteAsync(data, 0, data.Length);
                //await _stream.FlushAsync();


                foreach (var item in customs)
                {
                    //await item.WebSocket.SendAsync(data, WebSocketMessageType.Text, false, CancellationToken.None);
                    //byte[] byteArray = System.Text.Encoding.Default.GetBytes("测试");
                    try
                    {
                        //await item.WebSocket.SendAsync(new ArraySegment<byte>(byteArray, 0, byteArray.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                        await item.WebSocket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }
                }

            }
        }
    }
}
