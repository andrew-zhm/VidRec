using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Win32;


namespace vidRec
{
    class Program
    {
        private FilterInfo _currentDevice;

        private BitmapImage _image;

        private IVideoSource _videoSource;
        private VideoFileWriter _writer;
        private bool _recording;
        private DateTime? _firstFrameTime;
        private string FileName = "C:\\Users\\CISL\\Desktop\\1.avi";

        private static CommHandler receive;

        public Program()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            GetVideoDevices();
        }

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }


        private void GetVideoDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                VideoDevices.Add(device);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
            else
            {
                Console.WriteLine("No webcam found");
            }
        }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; }
        }

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime != null)
                        {
                            System.Console.SetOut(new System.IO.StreamWriter(System.IO.Stream.Null));
                            _writer.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime.Value);
                        }
                        else
                        {
                            System.Console.SetOut(new System.IO.StreamWriter(System.IO.Stream.Null));
                            _writer.WriteVideoFrame(bitmap);
                            _firstFrameTime = DateTime.Now;
                        }
                    }
                }
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc){ }
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
            else
            {
                Console.WriteLine("Current device can't be null");
            }
        }

        private void StartRecording()
        {
            _firstFrameTime = null;
            try
            {
                _writer = new VideoFileWriter();
                _writer.Open(FileName, 480, 320, 40, VideoCodec.MPEG4);
            }
            catch (Exception e)
            {
                Console.WriteLine("Open error:", e);
            }
            _recording = true;
        }

        private void commandCallback(string msg)
        {
            var rec = JObject.Parse(msg);
            if (rec.Value<string>("command").CompareTo("start") == 0)
            {
                StartCamera();
                StartRecording();
            }
            if (rec.Value<string>("command").CompareTo("end") == 0)
            {
                Console.WriteLine("Recording stopped");
                _recording = false;
                _writer.Close();
                _writer.Dispose();
                if (_videoSource != null && _videoSource.IsRunning)
                {
                    _videoSource.SignalToStop();
                    _videoSource.NewFrame -= video_NewFrame;
                }
                Image = null;
            }
        }

        public void commandHelper()
        {
            receive = new CommHandler("129.161.106.25");
            Action<string> commandCallbackAction = new Action<string>(commandCallback);
            receive.listen("amq.topic", "commandMaster", commandCallbackAction);
        }

        static void Main()
        {
            Program p = new Program();
            p.commandHelper();
        }
    }
}
