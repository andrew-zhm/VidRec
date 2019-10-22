using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private string FileName = "C:\\Users\\Andrew_ZHM\\Desktop\\1.avi";

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
                            _writer.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime.Value);
                        }
                        else
                        {
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
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
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

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Image = null;
        }

        private void StopRecording()
        {
            _recording = false;
            _writer.Close();
            _writer.Dispose();
        }

        private void StartRecording()
        {
            _firstFrameTime = null;
            try
            {
                _writer = new VideoFileWriter();
                _writer.Open(FileName, 720, 480);
            }
            catch (Exception e)
            {
                Console.WriteLine("Open error:", e);
            }
            _recording = true;
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            string command = Console.ReadLine();
            if (command == "start")
            {
                p.StartCamera();
                p.StartRecording();
            }
            else if (command == "stop")
            {
                p.StopCamera();
                p.StopRecording();
            }
        }

    }
}
