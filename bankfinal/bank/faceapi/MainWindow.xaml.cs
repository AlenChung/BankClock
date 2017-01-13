using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

using System.Windows.Threading;
using Emgu.CV;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Kinect.Face;

namespace faceapi
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array to store bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Number of bodies tracked
        /// </summary>
        private int bodyCount;

        /// <summary>
        /// Face frame sources
        /// </summary>
        private FaceFrameSource[] faceFrameSources = null;

        /// <summary>
        /// Face frame readers
        /// </summary>
        private FaceFrameReader[] faceFrameReaders = null;

        /// <summary>
        /// Storage for face frame results
        /// </summary>
        private FaceFrameResult[] faceFrameResults = null;

        /// <summary>
        /// Width of display (color space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (color space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Display rectangle
        /// </summary>
        private Rect displayRect;

        /// <summary>
        /// List of brushes for each face tracked
        /// </summary>
        private List<Brush> faceBrush;

        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;


        DispatcherTimer timernow = new DispatcherTimer();
        DispatcherTimer timer;
        private FaceServiceClient faceServiceClient = new FaceServiceClient("8f7a031e5133417aa8b1f1ab525efec1");  //key開始
        string m_primaryOrSecondaryKey = ConfigurationManager.AppSettings["8f7a031e5133417aa8b1f1ab525efec1"];
    
           
        BitmapImage[] bmM_C = new BitmapImage[4];

        public int ed = 1;
        
        public MainWindow()
        {

           
            this.kinectSensor = KinectSensor.GetDefault();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                
            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            
            // get the color frame details
            FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            // set the display specifics
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            this.displayRect = new Rect(0.0, 0.0, this.displayWidth, this.displayHeight);

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // wire handler for body frame arrival
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // set the maximum number of bodies that would be tracked by Kinect
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // allocate storage to store body objects
            this.bodies = new Body[this.bodyCount];

            // specify the required face frame results
            FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.BoundingBoxInColorSpace
                | FaceFrameFeatures.PointsInColorSpace
                | FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.Glasses
                | FaceFrameFeatures.Happy
                | FaceFrameFeatures.LeftEyeClosed
                | FaceFrameFeatures.RightEyeClosed
                | FaceFrameFeatures.LookingAway
                | FaceFrameFeatures.MouthMoved
                | FaceFrameFeatures.MouthOpen;

            // create a face frame source + reader to track each face in the FOV
            this.faceFrameSources = new FaceFrameSource[this.bodyCount];
            this.faceFrameReaders = new FaceFrameReader[this.bodyCount];
            for (int i = 0; i < this.bodyCount; i++)
            {
                // create the face frame source with the required face frame features and an initial tracking Id of 0
                this.faceFrameSources[i] = new FaceFrameSource(this.kinectSensor, 0, faceFrameFeatures);

                // open the corresponding reader
                this.faceFrameReaders[i] = this.faceFrameSources[i].OpenReader();
            }

            // allocate storage to store face frame results for each face in the FOV
            this.faceFrameResults = new FaceFrameResult[this.bodyCount];

            // populate face result colors - one for each face index
            this.faceBrush = new List<Brush>()
            {
                Brushes.White, 
                Brushes.Orange,
                Brushes.Green,
                Brushes.Red,
                Brushes.LightBlue,
                Brushes.Yellow
            };
            
            // open the sensor
            this.kinectSensor.Open();
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            timer.Start();

         
          

        }
      
        void timer_Tick(object sender, EventArgs e)
        {
               FacePhoto.Source = colorBitmap;
        }


        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            ed = 0;        
            clockin.Visibility = Visibility.Visible;
            btn2.IsEnabled = false;
            btn.IsEnabled = false;
            await AutoPhotoClockin();
            
        }

        private async void btn2_Click(object sender, RoutedEventArgs e)
        {
            ed = 0;
            clockout.Visibility = Visibility.Visible;
            btn2.IsEnabled = false;
            btn.IsEnabled = false;
            await AutoPhotoClockout();
        }

        //Clock in 上班
        private async Task AutoPhotoClockin()
        {
            BitmapSource bitmapSource = colorBitmap;
            string photolocation = "faceapiPhoto.jpg";
            FileStream filestream = new FileStream(photolocation, FileMode.Create);//存檔
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(filestream);
            filestream.Close();
            FaceAttributes[] faceattrs = await UploadAndDetectFacesAttributes(photolocation);
            List<String> faceIdentity = await UploadAndIdentityFaces(photolocation);


            Console.WriteLine(faceattrs.Length + "length");//
            if (faceattrs.Length > 0 && faceattrs.Length < 2)
            {
                //九點半前上班
                String aa="09:30";
           
                clockin.Visibility = Visibility.Collapsed;
                clockout.Visibility = Visibility.Collapsed;
                for (int i = 0; i < faceattrs.Length; i++)
                {
                    if (faceIdentity[i].ToString() == "Allen")　　　//是Allen,上傳完整資訊到資料庫
                    {


                        Console.WriteLine(faceattrs.Length + "HERERERERER");
                        yes.Visibility = Visibility.Visible;
                        sh.Visibility = Visibility.Visible;
                        sh.Text = "Clock in:\n"+System.DateTime.Now.ToString();
                        using (var conn = new SqlConnection("Server=tcp:jdp9xu6epe.database.windows.net,1433;Database=mtcsqldb;User ID=mtcsqldb@jdp9xu6epe;Password=Password.1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                        {
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                                        INSERT dbo.PunchClock(Name,Late,Date,ClockTime,Status)
                                        VALUES (@Name,@Late,@Date,@ClockTime,@Status)";




                            cmd.Parameters.AddWithValue("@Name", faceIdentity[i].ToString());
                            if (System.DateTime.Now > DateTime.Parse(aa))
                            {
                                cmd.Parameters.AddWithValue("@Late", "Late");
                            }
                            else { cmd.Parameters.AddWithValue("@Late", "OnTime"); }

                            cmd.Parameters.AddWithValue("@Date", System.DateTime.Now.ToString("d"));
                            cmd.Parameters.AddWithValue("@ClockTime", System.DateTime.Now.ToString("t"));
                            cmd.Parameters.AddWithValue("@Status", "Clockin");

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (faceIdentity[i].ToString() == "Not Employee")　　//不是Allen,只上傳資訊時間資訊到資料庫做紀錄
                    {
                        nobtn.Visibility = Visibility.Visible;
                        no.Visibility = Visibility.Visible;
                        using (var conn = new SqlConnection("Server=tcp:jdp9xu6epe.database.windows.net,1433;Database=mtcsqldb;User ID=mtcsqldb@jdp9xu6epe;Password=Password.1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                        {
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                                        INSERT dbo.PunchClock(Name,Date,ClockTime)
                                        VALUES (@Name,@Date,@ClockTime)";
                            cmd.Parameters.AddWithValue("@Name", faceIdentity[i].ToString());
                            cmd.Parameters.AddWithValue("@Date", System.DateTime.Now.ToString("d"));
                            cmd.Parameters.AddWithValue("@ClockTime", System.DateTime.Now.ToString("t"));
                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }


            }
            else if (faceattrs.Length==0) 
            {
              
                clockin.Visibility = Visibility.Collapsed;
                btn.IsEnabled = true;
                btn2.IsEnabled = true;
                ed = 0;

            }
            else if (faceattrs.Length > 1)
            {
                oneperson.Visibility = Visibility.Visible;
                clockin.Visibility = Visibility.Collapsed;
                btn.IsEnabled = true;
                btn2.IsEnabled = true;
                ed = 0;
                onepersonbtn.Visibility = Visibility.Visible;
            }
         
           
           
        }

        //Clock in 上班
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameReaders[i] != null)
                {
                   
                    // wire handler for face frame arrival
                    this.faceFrameReaders[i].FrameArrived += this.Reader_FaceFrameArrived;
                    
                    
                }
             
            }

            if (this.bodyFrameReader != null)
            {
                // wire handler for body frame arrival
                this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            }
        }

        //Clock out下班
        private async Task AutoPhotoClockout()
        {

            //Image<Bgr, Byte> currentFrame = capture.QueryFrame();
            BitmapSource bitmapSource = colorBitmap;

            string photolocation = "faceapiPhoto.jpg";

            FileStream filestream = new FileStream(photolocation, FileMode.Create);//存檔
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(filestream);
            filestream.Close();
            FaceAttributes[] faceattrs = await UploadAndDetectFacesAttributes(photolocation);
            List<String> faceIdentity = await UploadAndIdentityFaces(photolocation);
            if (faceattrs.Length > 0&& faceattrs.Length<2)
            {
                //六點半下班
                String aa = "18:30";

                clockin.Visibility = Visibility.Collapsed;
                clockout.Visibility = Visibility.Collapsed; //

                for (int i = 0; i < faceattrs.Length; i++)
                {
                    if (faceIdentity[i].ToString() == "Allen")　　　//是Allen,上傳完整資訊到資料庫
                    {
                        
                        yes.Visibility = Visibility.Visible;
                        sh.Visibility = Visibility.Visible;
                        sh.Text = "Clock out:\n"+System.DateTime.Now.ToString();
                        using (var conn = new SqlConnection("Server=tcp:jdp9xu6epe.database.windows.net,1433;Database=mtcsqldb;User ID=mtcsqldb@jdp9xu6epe;Password=Password.1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                        {
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                                        INSERT dbo.PunchClock(Name,Late,Date,ClockTime,Status)
                                        VALUES (@Name,@Late,@Date,@ClockTime,@Status)";




                            cmd.Parameters.AddWithValue("@Name", faceIdentity[i].ToString());
                            if (System.DateTime.Now < DateTime.Parse(aa))
                            {
                                cmd.Parameters.AddWithValue("@Late", "EarlyLeave");
                            }
                            else { cmd.Parameters.AddWithValue("@Late", "OnTime"); }

                            cmd.Parameters.AddWithValue("@Date", System.DateTime.Now.ToString("d"));
                            cmd.Parameters.AddWithValue("@ClockTime", System.DateTime.Now.ToString("t"));
                            cmd.Parameters.AddWithValue("@Status", "Clockout");

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (faceIdentity[i].ToString() == "Not Employee")　　//不是Allen,只上傳資訊時間資訊到資料庫做紀錄
                    {
                        nobtn.Visibility = Visibility.Visible;
                        no.Visibility = Visibility.Visible;
                        using (var conn = new SqlConnection("Server=tcp:jdp9xu6epe.database.windows.net,1433;Database=mtcsqldb;User ID=mtcsqldb@jdp9xu6epe;Password=Password.1;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
                        {
                            var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                                        INSERT dbo.PunchClock(Name,Date,ClockTime)
                                        VALUES (@Name,@Date,@ClockTime)";




                            cmd.Parameters.AddWithValue("@Name", faceIdentity[i].ToString());
                            cmd.Parameters.AddWithValue("@Date", System.DateTime.Now.ToString("d"));
                            cmd.Parameters.AddWithValue("@ClockTime", System.DateTime.Now.ToString("t"));
                        

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    
                }


            }
            else if (faceattrs.Length==0)
            {
                clockout.Visibility = Visibility.Collapsed;
                ed = 0;
                btn.IsEnabled = true;
                btn2.IsEnabled = true;
            }
            else if (faceattrs.Length > 1)
            {
                btn.IsEnabled = true;
                btn2.IsEnabled = true;
                clockout.Visibility = Visibility.Collapsed;//
                oneperson.Visibility = Visibility.Visible;
                onepersonbtn.Visibility = Visibility.Visible;
                ed = 0;
            }



        }
        //Clock out下班

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)   //image to bitmap
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
        private async Task<List<String>> UploadAndIdentityFaces(string imageFilePath) //上傳照片以偵測是否為員工Allen
        {//
            Console.WriteLine("12121212121212"+imageFilePath);

            try
            {

                Console.WriteLine("11232323232322" + imageFilePath);
                List<String> list = new List<string>();
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var faceIds = faces.Select(face => face.FaceId).ToArray();

                    Console.WriteLine(faceIds);
                    var results = await faceServiceClient.IdentifyAsync("mtcbotdemo", faceIds); //Identification Key ID 

                    foreach (var identifyResult in results)
                    {
                        Console.WriteLine("Result of face: {0}", identifyResult.FaceId);

                        if (identifyResult.Candidates.Length == 0)   //不是Allen
                        {

                            list.Add("Not Employee");
                        }
                        else                                        //是Allen
                        {
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            Console.WriteLine("got ittttttttttttttttttttttttttttttttttt");
                            //  var person = await faceServiceClient.GetPersonAsync("2b687813-c478-4874-bbe3-dc28893128fc", candidateId);

                            //  Console.WriteLine("Identified as {0}", person.Name);
                            list.Add("Allen");

                        }
                    }
                    return list;

                }

            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<FaceRectangle[]> UploadAndDetectFaces(string imageFilePath) //上傳照片以偵測人臉
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
                }
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }
        private async Task<FaceLandmarks[]> UploadAndDetectFacesDetail(string imageFilePath) //上傳照片以偵測眼睛鼻子嘴巴
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var facelands = faces.Select(face => face.FaceLandmarks);
                    return facelands.ToArray();
                }
            }
            catch (Exception)
            {
                return new FaceLandmarks[0];
            }
        }


        private async Task<FaceAttributes[]> UploadAndDetectFacesAttributes(string imageFilePath) //上傳照片以偵測性別和年齡
        {
            try
            {
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    var faceAttributes = faces.Select(face => face.FaceAttributes);
                    return faceAttributes.ToArray();
                }
            }
            catch (Exception)
            {

                return new FaceAttributes[0];
            }
        }


        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                    }
                }
            }
        }

        private void yesbtn_Click(object sender, RoutedEventArgs e)
        {
            yes.Visibility = Visibility.Collapsed;
            sh.Visibility = Visibility.Collapsed;
            btn.IsEnabled = true;
            btn2.IsEnabled = true;
            ed = 1;
        }

        private void nobtn_Click(object sender, RoutedEventArgs e)
        {
            
            no.Visibility = Visibility.Collapsed;
            sh.Visibility = Visibility.Collapsed;
            btn.IsEnabled = true;
            btn2.IsEnabled = true;
            ed = 1;
            nobtn.Visibility = Visibility.Collapsed;
        }
        private void onepersonbtn_Click(object sender, RoutedEventArgs e)
        {

            oneperson.Visibility = Visibility.Collapsed;
            onepersonbtn.Visibility = Visibility.Collapsed;
            ed = 1;
        }
        private void Reader_FaceFrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
            {
                if (faceFrame != null)
                {
                    
                    // get the index of the face source from the face source array
                    int index = this.GetFaceSourceIndex(faceFrame.FaceFrameSource);

                    // check if this face frame has valid face frame results
                    if (this.ValidateFaceBoxAndPoints(faceFrame.FaceFrameResult))
                    {
                        // store this face frame result to draw later
                        this.faceFrameResults[index] = faceFrame.FaceFrameResult;
                       // Console.WriteLine("faceFrameResults[index]");
                    }
                    else
                    {   
                        // indicates that the latest face frame result from this reader is invalid
                        this.faceFrameResults[index] = null;
                     //   Console.WriteLine("faceFrameResults[index] = null");
                    }
                }
                //else {  }

              
            }
        }
        private int GetFaceSourceIndex(FaceFrameSource faceFrameSource)
        {
            int index = -1;

            for (int i = 0; i < this.bodyCount; i++)
            {
                if (this.faceFrameSources[i] == faceFrameSource)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    // update body data
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        // draw the dark background
                        dc.DrawRectangle(Brushes.Black, null, this.displayRect);

                        bool drawFaceResult = false;

                        // iterate through each face source
                        for (int i = 0; i < this.bodyCount; i++)
                        {
                            // check if a valid face is tracked in this face source
                            if (this.faceFrameSources[i].IsTrackingIdValid&&ed==1)
                            {
                                // check if we have valid face frame results
                                if (this.faceFrameResults[i] != null)
                                {
                                    // draw face frame results
                                    this.DrawFaceFrameResults(i, this.faceFrameResults[i], dc);

                                    if (!drawFaceResult)
                                    {
                                        //oneperson.Visibility = Visibility.Collapsed;
                                        notdetect.Visibility = Visibility.Collapsed;
                                        drawFaceResult = true;
                                        
                                    }
                                }
                            }
                            else
                            {
                                // check if the corresponding body is tracked 
                                if (this.bodies[i].IsTracked&&ed==1)
                                {
                                    // update the face frame source to track this body
                                    this.faceFrameSources[i].TrackingId = this.bodies[i].TrackingId;
                                    //oneperson.Visibility = Visibility.Collapsed;
                                    notdetect.Visibility = Visibility.Visible;
                                    
                                }
                            }
                        }

                        if (!drawFaceResult && ed == 1)
                        {
                            // if no faces were drawn then this indicates one of the following:
                            // a body was not tracked 
                            // a body was tracked but the corresponding face was not tracked
                            // a body and the corresponding face was tracked though the face box or the face points were not valid

                            //oneperson.Visibility = Visibility.Collapsed;
                            notdetect.Visibility = Visibility.Visible;
                        }

                        //this.drawingGroup.ClipGeometry = new RectangleGeometry(this.displayRect);
                    }
                }
            }
        }
        private void DrawFaceFrameResults(int faceIndex, FaceFrameResult faceResult, DrawingContext drawingContext)
        {
            // choose the brush based on the face index
            double DrawFaceShapeThickness = 8;
            Brush drawingBrush = this.faceBrush[0];
            if (faceIndex < this.bodyCount)
            {
                drawingBrush = this.faceBrush[faceIndex];
            }

            Pen drawingPen = new Pen(drawingBrush, DrawFaceShapeThickness);

            // draw the face bounding box
            var faceBoxSource = faceResult.FaceBoundingBoxInColorSpace;
            Rect faceBox = new Rect(faceBoxSource.Left, faceBoxSource.Top, faceBoxSource.Right - faceBoxSource.Left, faceBoxSource.Bottom - faceBoxSource.Top);
            drawingContext.DrawRectangle(null, drawingPen, faceBox);

            if (faceResult.FacePointsInColorSpace != null)
            {
                double FacePointRadius = 1.0;
                // draw each face point
                foreach (PointF pointF in faceResult.FacePointsInColorSpace.Values)
                {
                    drawingContext.DrawEllipse(null, drawingPen, new Point(pointF.X, pointF.Y), FacePointRadius, FacePointRadius);
                }
            }

            string faceText = string.Empty;

            // extract each face property information and store it in faceText
            if (faceResult.FaceProperties != null)
            {
                foreach (var item in faceResult.FaceProperties)
                {
                    faceText += item.Key.ToString() + " : ";

                    // consider a "maybe" as a "no" to restrict 
                    // the detection result refresh rate
                    if (item.Value == DetectionResult.Maybe)
                    {
                        faceText += DetectionResult.No + "\n";
                    }
                    else
                    {
                        faceText += item.Value.ToString() + "\n";
                    }
                }
            }

            // extract face rotation in degrees as Euler angles
            if (faceResult.FaceRotationQuaternion != null)
            {
                int pitch, yaw, roll;
                ExtractFaceRotationInDegrees(faceResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
                faceText += "FaceYaw : " + yaw + "\n" +
                            "FacePitch : " + pitch + "\n" +
                            "FacenRoll : " + roll + "\n";
            }

            // render the face property and face rotation information
            Point faceTextLayout;
            if (this.GetFaceTextPositionInColorSpace(faceIndex, out faceTextLayout))
            {
                //drawingContext.DrawText(
                //        new FormattedText(
                //            faceText,
                //            CultureInfo.GetCultureInfo("en-us"),
                //            FlowDirection.LeftToRight,
                //            new Typeface("Georgia"),
                //           // DrawTextFontSize,
                //            drawingBrush),
                //        faceTextLayout);
            }
        }
        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;
            int FaceRotationIncrementInDegrees = 5;
            // convert face rotation quaternion to Euler angles in degrees
            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            // clamp the values to a multiple of the specified increment to control the refresh rate
            double increment = FaceRotationIncrementInDegrees;
            pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
            yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
            roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
        }
        private bool GetFaceTextPositionInColorSpace(int faceIndex, out Point faceTextLayout)
        {
            faceTextLayout = new Point();
            bool isLayoutValid = false;

            Body body = this.bodies[faceIndex];
            if (body.IsTracked)
            {
                var headJoint = body.Joints[JointType.Head].Position;

                CameraSpacePoint textPoint = new CameraSpacePoint()
                {
                   // X = headJoint.X + TextLayoutOffsetX,
                   // Y = headJoint.Y + TextLayoutOffsetY,
                    Z = headJoint.Z
                };

                ColorSpacePoint textPointInColor = this.coordinateMapper.MapCameraPointToColorSpace(textPoint);

                faceTextLayout.X = textPointInColor.X;
                faceTextLayout.Y = textPointInColor.Y;
                isLayoutValid = true;
            }

            return isLayoutValid;
        }
        private bool ValidateFaceBoxAndPoints(FaceFrameResult faceResult)
        {
            bool isFaceValid = faceResult != null;

            if (isFaceValid)
            {
                var faceBox = faceResult.FaceBoundingBoxInColorSpace;
                if (faceBox != null)
                {
                    // check if we have a valid rectangle within the bounds of the screen space
                    isFaceValid = (faceBox.Right - faceBox.Left) > 0 &&
                                  (faceBox.Bottom - faceBox.Top) > 0 &&
                                  faceBox.Right <= this.displayWidth &&
                                  faceBox.Bottom <= this.displayHeight;

                    if (isFaceValid)
                    {
                        var facePoints = faceResult.FacePointsInColorSpace;
                        if (facePoints != null)
                        {
                            foreach (PointF pointF in facePoints.Values)
                            {
                                // check if we have a valid face point within the bounds of the screen space
                                bool isFacePointValid = pointF.X > 0.0f &&
                                                        pointF.Y > 0.0f &&
                                                        pointF.X < this.displayWidth &&
                                                        pointF.Y < this.displayHeight;

                                if (!isFacePointValid)
                                {
                                    isFaceValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isFaceValid;
        }
    }
}
