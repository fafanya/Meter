using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace Meter
{
    [Activity(Label = "ImageActivity")]
    public class ImageActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_image);

            string absolutePath = Intent.GetStringExtra("AbsolutePath") ?? string.Empty;

            File imgFile = new File(absolutePath);

            if (imgFile.Exists())
            {

                Bitmap myBitmap = BitmapFactory.DecodeFile(imgFile.AbsolutePath);

                ImageView myImage = (ImageView)FindViewById(Resource.Id.imageView_Image);

                myImage.SetImageBitmap(myBitmap);

            }
        }
    }
}