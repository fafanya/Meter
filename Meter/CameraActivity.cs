using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;//or Java.Lang
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Media;
using Android.Hardware;
using Camera = Android.Hardware.Camera;
using Java.Util;
using Java.IO;

namespace Meter
{
    [Activity(Label = "CameraActivity")]
    public class CameraActivity : Activity
    {
        public SurfaceView preview = null;
        public ISurfaceHolder previewHolder = null;
        public Camera camera = null;
        public bool inPreview = false;
        public ImageView image;
        public Bitmap bmp, itembmp;
        public static Bitmap mutableBitmap;
        public PointF start = new PointF();
        public PointF mid = new PointF();
        public float oldDist = 1f;
        public Java.IO.File imageFileName = null;
        public Java.IO.File imageFileFolder = null;
        public MediaScannerConnection msConn;
        public Display d;
        public int screenhgt, screenwdh;
        public ProgressDialog dialog;
        public ImageView fotoButton;

        public class OnClickListener : Java.Lang.Object, View.IOnClickListener
        {
            public CameraActivity Parent { get; set; }

            public void OnClick(View v)
            {
                Parent.onBack();
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_camera);
            // Create your application here
            image = FindViewById<ImageView>(Resource.Id.imageView_Photo);
            fotoButton = FindViewById<ImageView>(Resource.Id.imageView_foto);
            preview = FindViewById<SurfaceView>(Resource.Id.surfaceView_Photo);

            fotoButton.SetOnClickListener(new OnClickListener()
            {
                Parent = this
            });

            previewHolder = preview.Holder;
            previewHolder.AddCallback(
                new SurfaceCalback() { Parent = this
                });

            previewHolder.SetType(SurfaceType.PushBuffers);

            previewHolder.SetFixedSize(Window.WindowManager.DefaultDisplay.Width,
                                       Window.WindowManager.DefaultDisplay.Height);
        }

        protected override void OnResume()
        {
            base.OnResume();
            camera = Camera.Open();
        }

        public void onPause()
        {
            if (inPreview)
            {
                camera.StopPreview();
            }

            camera.Release();
            camera = null;
            inPreview = false;
            base.OnPause();
        }

        public Camera.Size GetBestPreviewSize(int width, int height, Camera.Parameters parameters)
        {
            Camera.Size result = null;
            foreach (Camera.Size size in parameters.SupportedPreviewSizes)
            {
                if (size.Width <= width && size.Height <= height)
                {
                    if (result == null)
                    {
                        result = size;
                    }
                    else
                    {
                        int resultArea = result.Width * result.Height;
                        int newArea = size.Width * size.Height;
                        if (newArea > resultArea)
                        {
                            result = size;
                        }
                    }
                }
            }
            return (result);
        }


        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Menu && e.RepeatCount == 0) {
                onBack();
            }
            return base.OnKeyDown(keyCode, e);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }


        public void onBack()
        {
            camera.TakePicture(null, null, new PictureCallback() { Parent = this });
            inPreview = false;
        }

        public void onPictureTake(byte[] data, Camera camera)
        {
            ContextWrapper cw = new ContextWrapper(ApplicationContext);
            imageFileFolder = cw.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);

            Calendar c = Calendar.Instance;
            imageFileName = new Java.IO.File(imageFileFolder, c.Time.Seconds + ".bmp");
            imageFileName.CreateNewFile();

            using (var os = new FileStream(imageFileName.AbsolutePath, FileMode.Create))
            {
                os.Write(data, 0, data.Length);
            }
            dialog.Dismiss();
        }


        public String fromInt(int val)
        {
            return val.ToString();
        }
    }

    public class PictureCallback : Java.Lang.Object, Camera.IPictureCallback
    {
        public CameraActivity Parent { get; set; }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            Parent.dialog = ProgressDialog.Show(Parent, "", "Saving Photo");
            new Thread(
                new ThreadStart(
                    delegate 
                    {
                        Parent.RunOnUiThread(() =>
                        {
                            try
                            {
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex) { }
                            Parent.onPictureTake(data, camera);
                        });
                    })).Start();
        }
    }

    public class SurfaceCalback : Java.Lang.Object, ISurfaceHolderCallback
    {
        public CameraActivity Parent { get; set; }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            Camera.Parameters parameters = Parent.camera.GetParameters();
            Camera.Size size = Parent.GetBestPreviewSize(width, height,
            parameters);

            if (size != null)
            {
                parameters.SetPreviewSize(size.Width, size.Height);
                parameters.SetPictureSize(size.Width, size.Height);
                parameters.SetRotation(90);
                Parent.camera.SetParameters(parameters);
                Parent.camera.StartPreview();
                Parent.inPreview = true;
            }
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                Parent.camera.SetDisplayOrientation(90);
                Parent.camera.SetPreviewDisplay(Parent.previewHolder);
            }
            catch (Exception t)
            {

            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }
    }
}


/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Media;
using Java.IO;

namespace Meter
{
    [Activity(Label = "CameraActivity")]
    public class CameraActivity : Activity, TextureView.ISurfaceTextureListener
    {

        private Android.Hardware.Camera _camera;
        private TextureView _textureView;
        private SurfaceView _surfaceView;
        private ISurfaceHolder holder;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_camera);

            _textureView = (TextureView)FindViewById(Resource.Id.textureView);
            _textureView.SurfaceTextureListener = this;

            _surfaceView = (SurfaceView)FindViewById(Resource.Id.surfaceView);
            //set to top layer
            _surfaceView.SetZOrderOnTop(true);
            //set the background to transparent
            _surfaceView.Holder.SetFormat(Format.Transparent);
            holder = _surfaceView.Holder;
            _surfaceView.Touch += _surfaceView_Touch;
        }

        private void _surfaceView_Touch(object sender, View.TouchEventArgs e)
        {
            //define the paintbrush
            Paint mpaint = new Paint();
            mpaint.Color = Color.Red;
            mpaint.SetStyle(Paint.Style.Stroke);
            mpaint.StrokeWidth = 2f;

            //draw
            Canvas canvas = holder.LockCanvas();
            //clear the paint of last time
            canvas.DrawColor(Color.Transparent, PorterDuff.Mode.Clear);
            //draw a new one, set your ball's position to the rect here
            var x = e.Event.GetX();
            var y = e.Event.GetY();
            Rect r = new Rect((int)x, (int)y, (int)x + 100, (int)y + 100);
            canvas.DrawRect(r, mpaint);
            holder.UnlockCanvasAndPost(canvas);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _camera.StopPreview();
            _camera.Release();

            return true;
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _camera = Android.Hardware.Camera.Open();

            try
            {
                _camera.SetPreviewTexture(surface);
                _camera.SetDisplayOrientation(90);
                _camera.StartPreview();
            }
            catch (Java.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
    }
}*/

/*<? xml version="1.0" encoding="utf-8"?>
<RelativeLayout
  xmlns:android="http://schemas.android.com/apk/res/android"
  android:layout_width="match_parent"
  android:layout_height="match_parent">

	<TextureView
      android:id="@+id/textureView"
      android:layout_height="match_parent"
      android:layout_width="match_parent" />

	<SurfaceView
      android:id="@+id/surfaceView"
      android:layout_height="match_parent"
      android:layout_width="match_parent" />

</RelativeLayout>*/