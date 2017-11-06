using HomeControl.Surveillance.Server.Data.Rtsp.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HomeControl.Surveillance.Server.Data.Rtsp
{
    public class RtspCameraConnection: ICameraConnection
    {
        private Object ConnectionSync = new Object();
        private String IpAddress;
        private UInt16 Port;
        private String Url;
        private RtspClient Client;
        private DateTime ClientCreatedDate;
        private String ActiveSession;
        private ReconnectionController ReconnectionController = new ReconnectionController();

        public event TypedEventHandler<ICameraConnection, Byte[]> DataReceived = delegate { };



        public RtspCameraConnection(String ipAddress, UInt16 port, String url)
        {
            IpAddress = ipAddress;
            Port = port;
            Url = url;
            StartConnectionRestorating();
            StartConnectionMaintaining();
        }

        private async void StartConnectionRestorating() => await Task.Run(async () =>
        {
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (Client != null)
                        Monitor.Wait(ConnectionSync);
                }

                try
                {
                    var client = new RtspClient(IpAddress, Port, Url);
                    client.ResponseReceived += OnResponseReceived;
                    client.DataReceived += OnDataReceived;
                    await client.SendMessageAsync(RtspClient.Message.Options).ConfigureAwait(false);
                    App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}", "Connected.");

                    lock (ConnectionSync)
                    {
                        Client = client;
                        ClientCreatedDate = DateTime.Now;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
                catch (Exception exception)
                {
                    App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}.{nameof(StartConnectionRestorating)}", exception);
                }
            }
        });

        private async void StartConnectionMaintaining() => await Task.Run(async () =>
        {
            var lastSendMessageDate = new DateTime();
            while (true)
            {
                lock (ConnectionSync)
                {
                    if (Client == null)
                        Monitor.Wait(ConnectionSync);
                }

                await Task.Delay(2000).ConfigureAwait(false);

                try
                {
                    if (ReconnectionController.IsAllowed())
                    {
                        App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}", "No data captured, reconnecting...");
                        lock (ConnectionSync)
                        {
                            Client = null;
                            ActiveSession = null;
                            Monitor.PulseAll(ConnectionSync);
                            continue;
                        }
                    }

                    var sendMessageTask = (Task)null;
                    lock (ConnectionSync)
                    {
                        if ((Client != null) && (ActiveSession != null) && ((DateTime.Now - ClientCreatedDate) > TimeSpan.FromSeconds(25)) && ((DateTime.Now - lastSendMessageDate) > TimeSpan.FromSeconds(25)))
                            sendMessageTask = Client.SendMessageAsync(RtspClient.Message.GetParameter, new Dictionary<String, String> { ["Session"] = ActiveSession });
                    }
                    if (sendMessageTask != null)
                    {
                        App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}", $"Sending ping message...");
                        await sendMessageTask.ConfigureAwait(false);
                        lastSendMessageDate = DateTime.Now;
                    }
                }
                catch (Exception exception)
                {
                    App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}.{nameof(StartConnectionMaintaining)}", exception);
                    lock (ConnectionSync)
                    {
                        Client = null;
                        ActiveSession = null;
                        Monitor.PulseAll(ConnectionSync);
                    }
                }
            }
        });

        private async void OnResponseReceived(RtspClient sender, IResponse response)
        {
            try
            {
                switch (response)
                {
                    case OptionsResponse optionsResponse:
                        await sender.SendMessageAsync(RtspClient.Message.Describe);
                        break;
                    case UnauthorizedResponse unauthorizedResponse:
                        var username = PrivateData.CameraUsername;
                        var password = PrivateData.CameraPassword;
                        var hashAlgorithm = MD5.Create();
                        var hashString1 = BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes($"{username}:{unauthorizedResponse.DigestRealm}:{password}"))).Replace("-", "").ToLower();
                        var hashString2 = BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes("DESCRIBE:/"))).Replace("-", "").ToLower();
                        var digest = BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes($"{hashString1}:{unauthorizedResponse.Nonce}:{hashString2}"))).Replace("-", "").ToLower();
                        var authorizationHeaderValue = $"Digest username=\"{username}\", realm=\"{unauthorizedResponse.DigestRealm}\", nonce=\"{unauthorizedResponse.Nonce}\", uri=\"/\", response=\"{digest}\"";
                        await sender.SendMessageAsync(RtspClient.Message.Describe, new Dictionary<String, String> { ["Authorization"] = authorizationHeaderValue });
                        break;
                    case DescribeResponse describeResponse:
                        //Rtsp.Sdp.SdpFile sdp_data;
                        //using (StreamReader sdp_stream = new StreamReader(new MemoryStream(message.Data)))
                        //{
                        //    sdp_data = Rtsp.Sdp.SdpFile.Read(sdp_stream);
                        //}
                        //for (int x = 0; x < sdp_data.Medias.Count; x++)
                        //{
                        //    if (sdp_data.Medias[x].GetMediaType() == Rtsp.Sdp.Media.MediaType.video)
                        //    {
                        //String control = "";  
                        //foreach (Rtsp.Sdp.Attribut attrib in sdp_data.Medias[x].Attributs)
                        //    if (attrib.Key.Equals("control"))
                        //        control = attrib.Value; // the "track" or "stream id"
                        //setup_message.RtspUri = new Uri(url + "/" + control);


                        await sender.SendMessageAsync(RtspClient.Message.Setup, new Dictionary<String, String> { ["Transport"] = "RTP/AVP/TCP;interleaved=0" }, "rtsp://admin:admin@192.168.1.168:554/trackID=0");
                        //await sender.SendMessageAsync(RtspClient.Message.Setup, new Dictionary<String, String> { ["Transport"] = "RTP/AVP/TCP;interleaved=0" });
                        break;
                    case SetupResponse setupResponse:
                        App.Diagnostics.Debug.Log($"{nameof(RtspCameraConnection)}", "Sending play message...");
                        await sender.SendMessageAsync(RtspClient.Message.Play, new Dictionary<String, String> { ["Session"] = setupResponse.Session });
                        ActiveSession = setupResponse.Session;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                lock (ConnectionSync)
                {
                    Client = null;
                    ActiveSession = null;
                    Monitor.PulseAll(ConnectionSync);
                }
            }
        }

        private void OnDataReceived(RtspClient sender, List<Byte[]> args)
        {
            ReconnectionController.Reset();
            //System.Diagnostics.Debug.WriteLine("RTP Data comprised of " + args.Count + " rtp packets");
            var stream = new MemoryStream();
            for (int payload_index = 0; payload_index < args.Count; payload_index++)
            {
                // Examine the first rtp_payload and the first byte (the NAL header)
                int nal_header_f_bit = (args[payload_index][0] >> 7) & 0x01;
                int nal_header_nri = (args[payload_index][0] >> 5) & 0x03;
                int nal_header_type = (args[payload_index][0] >> 0) & 0x1F;

                // If the Nal Header Type is in the range 1..23 this is a normal NAL (not fragmented)
                // So write the NAL to the file
                if (nal_header_type >= 1 && nal_header_type <= 23)
                {
                    //System.Diagnostics.Debug.WriteLine("Normal NAL");
                    //norm++;
                    byte[] nal_header = new byte[] { 0x00, 0x00, 0x00, 0x01 };
                    stream.Write(nal_header, 0, nal_header.Length);
                    stream.Write(args[payload_index], 0, args[payload_index].Length);
                }
                // There are 4 types of Aggregation Packet (split over RTP payloads)
                else if (nal_header_type == 24)
                {
                    //System.Diagnostics.Debug.WriteLine("Agg STAP-A");
                    //stap_a++;
                }
                else if (nal_header_type == 25)
                {
                    //System.Diagnostics.Debug.WriteLine("Agg STAP-B");
                    //stap_b++;
                }
                else if (nal_header_type == 26)
                {
                    //System.Diagnostics.Debug.WriteLine("Agg MTAP16");
                    //mtap16++;
                }
                else if (nal_header_type == 27)
                {
                    //System.Diagnostics.Debug.WriteLine("Agg MTAP24");
                    //mtap24++;
                }
                else if (nal_header_type == 28)
                {
                    //System.Diagnostics.Debug.WriteLine("Frag FU-A");
                    //fu_a++;

                    // Parse Fragmentation Unit Header
                    int fu_header_s = (args[payload_index][1] >> 7) & 0x01;  // start marker
                    int fu_header_e = (args[payload_index][1] >> 6) & 0x01;  // end marker
                    int fu_header_r = (args[payload_index][1] >> 5) & 0x01;  // reserved. should be 0
                    int fu_header_type = (args[payload_index][1] >> 0) & 0x1F; // Original NAL unit header

                    //System.Diagnostics.Debug.WriteLine("Frag FU-A s=" + fu_header_s + "e=" + fu_header_e);

                    // Start Flag set
                    if (fu_header_s == 1)
                    {
                        // Write 00 00 00 01 header
                        byte[] nal_header = new byte[] { 0x00, 0x00, 0x00, 0x01 };
                        stream.Write(nal_header, 0, nal_header.Length); // 0x00 0x00 0x00 0x01

                        // Modify the NAL Header that was at the start of the RTP packet
                        // Keep the F and NRI flags but substitute the type field with the fu_header_type
                        byte reconstructed_nal_type = (byte)((nal_header_nri << 5) + fu_header_type);
                        stream.WriteByte(reconstructed_nal_type); // NAL Unit Type
                        stream.Write(args[payload_index], 2, args[payload_index].Length - 2); // start after NAL Unit Type and FU Header byte

                    }

                    if (fu_header_s == 0)
                    {
                        // append this payload to the output NAL stream
                        // Data starts after the NAL Unit Type byte and the FU Header byte

                        stream.Write(args[payload_index], 2, args[payload_index].Length - 2); // start after NAL Unit Type and FU Header byte

                    }
                    // We could check the End marker but the start marker is sufficient
                }


                else if (nal_header_type == 29)
                {
                    //System.Diagnostics.Debug.WriteLine("Frag FU-B");
                    //fu_b++;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("Unknown NAL header " + nal_header_type);
                }

            }
            stream.Flush();

            var completeData = new Byte[stream.Length];
            stream.Position = 0;
            stream.Read(completeData, 0, completeData.Length);
            stream.Dispose();
            DataReceived(this, completeData);
        }
    }
}
