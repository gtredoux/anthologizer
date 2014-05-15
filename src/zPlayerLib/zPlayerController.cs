using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zPlayerLib
{
    public class zPlayerController
    {
        private ZPlay zplayer;
        //private int BufferCounter;
        private BinaryReader br;

        public delegate void EndOfMedia(string uri);
        public event EndOfMedia OnEndOfMedia;
        private const int CHUNKSIZE = 1024*8; // arbitrary 
        private TCallbackFunc CallbackFunc = null;

        public zPlayerController()
        {
            zplayer = new ZPlay();
            CallbackFunc = new TCallbackFunc(zPlayerCallback);
            zplayer.SetCallbackFunc(
                CallbackFunc, (TCallbackMessage)((
                    TCallbackMessage.MsgStreamBufferDoneAsync |
                    TCallbackMessage.MsgStreamBufferDone |
                    //TCallbackMessage.MsgStopAsync |
                    TCallbackMessage.MsgStop |
                    TCallbackMessage.MsgStreamNeedMoreData |
                    TCallbackMessage.MsgStreamNeedMoreDataAsync |
                    TCallbackMessage.MsgNextSong |
                    //TCallbackMessage.MsgNextSongAsync |
                    TCallbackMessage.MsgPlay //|
                    //TCallbackMessage.MsgPlayAsync
                    )
               ), 0
           );
        }

        //private System.IO.Stream theStream = null;

        public void Close()
        {
            if (zplayer != null)
            {
                zplayer.Close();
            }

            if (br != null)
            {
                br.Close();
                br = null;
            }
        }


        public void Open(long contentLength, TStreamFormat format, System.IO.Stream pfStream)
        {
            Close();

            //theStream = pfStream;

            //BufferCounter = 0;

            //var ms = CopyStream(contentLength, theStream);

            br = new BinaryReader(pfStream);
 
            //stream_data = ms.ToArray(); 
            byte[] stream_data = br.ReadBytes(System.Convert.ToInt32((int)(100000)));

            // open stream
            if (!(zplayer.OpenStream(true, true, ref stream_data, System.Convert.ToUInt32(stream_data.Length), format)))
            {
                throw new Exception(zplayer.GetError());
            }

            // read more data and push into stream
            byte[] stream_data2 = br.ReadBytes(System.Convert.ToInt32((int)(100000)));
            zplayer.PushDataToStream(ref stream_data2, System.Convert.ToUInt32(stream_data2.Length));

            zplayer.StartPlayback();  // will call the zPlayerCallback function to get more data
        }


        private MemoryStream CopyStream(long contentLength, Stream instream)
        {   
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] buf = new byte[CHUNKSIZE];
            long total = 0;
            int count;
            while (((count = instream.Read(buf,0,buf.Length)) > 0) && (total < contentLength))
            {
                ms.Write(buf, 0, count);
                total += count;
            }
            return ms;
        }

        private int zPlayerCallback(uint objptr, int userData, TCallbackMessage msg, uint param1, uint param2)
        {
            switch (msg)
            {
                //case TCallbackMessage.MsgStopAsync:
                case TCallbackMessage.MsgStreamNeedMoreDataAsync:

                    // read more data and push into stream
                    byte[] stream_data = br.ReadBytes(System.Convert.ToInt32((int)(CHUNKSIZE)));
                    if (stream_data != null && stream_data.Length > 0)
                    {
                        zplayer.PushDataToStream(ref stream_data, System.Convert.ToUInt32(stream_data.Length));
                    }
                    else // no more data
                    {
                        byte[] tempMemNewData1 = null;
                        zplayer.PushDataToStream(ref tempMemNewData1, 0);

                        if (OnEndOfMedia != null)
                        {
                            OnEndOfMedia(null);
                        }
                    }
                    break;

                default:
                    break;
            }

            return 0;
        }
    }
}
