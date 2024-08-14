using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NexiumService.Utils;
using SharpAvi;
using SharpAvi.Output;

namespace NexiumService.Clips;

public class Recorder
{
    private int _frameRate = 60;
    private int _clipDuration = 10;
    private Storage _outputPath = new("clips");
    private Bitmap[] _frameBuffer;
    private int _currentFrame = 0;
    private bool _isRecording = false;
    private int _width;
    private int _height;
    
    public Recorder()
    {
        var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        _width = screenBounds.Width;
        _height = screenBounds.Height;
        _frameBuffer = new Bitmap[_frameRate * _clipDuration];
        if (!_outputPath.Exists())
        {
            _outputPath.CreateDir();
        }
    }
    
    public void StartRecording()
    {
        _isRecording = true;
        Thread thread = new Thread(CaptureScreen);
        thread.Start();
    }
    
    private void CaptureScreen()
    {
        var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        while (_isRecording)
        {
            using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
                }
                
                _frameBuffer[_currentFrame]?.Dispose();
                _frameBuffer[_currentFrame] = new Bitmap(bitmap);
                _currentFrame = (_currentFrame + 1) % _frameBuffer.Length;
                
                Thread.Sleep(1000 / _frameRate);
            }
        }
    }

    public void SaveClip()
    {
        string fileName = Path.Combine(_outputPath.GetPath(), $"clip_{DateTime.Now:yyyyMMdd_HHmmss}.avi");
        var screenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        
        using (var writer = new AviWriter(fileName)
               {
                   FramesPerSecond = _frameRate,
                   EmitIndex1 = true
               })
        {
            var stream = writer.AddVideoStream();
            stream.Width = _width;
            stream.Height = _height;
            stream.Codec = CodecIds.MotionJpeg;
            stream.BitsPerPixel = BitsPerPixel.Bpp24;
            
            for (int i = 0; i < _frameBuffer.Length; i++)
            {
                int frameIndex = (_currentFrame + i) % _frameBuffer.Length;
                using (var bitmap = _frameBuffer[frameIndex])
                {
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    byte[] frameData = new byte[bitmapData.Stride * bitmapData.Height];
                    Marshal.Copy(bitmapData.Scan0, frameData, 0, frameData.Length);
                    
                    bitmap.UnlockBits(bitmapData);
                    stream.WriteFrame(true, frameData, bitmap.Width * 3, bitmap.Height);
                }
            }
        }
    }
    
}