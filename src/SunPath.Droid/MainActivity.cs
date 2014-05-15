using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace SunPath.Droid
{
	// http://docs.xamarin.com/recipes/android/other_ux/textureview/display_a_stream_from_the_camera/

	[Activity (Label = "SunPath.Droid", MainLauncher = true)]
	public class MainActivity : Activity
	{
		//int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// TODO: Remove MainActivity if it is not needed

			var camera = new Intent(this, typeof(CameraActivity));
			StartActivity(camera);

			// Get our button from the layout resource,
			// and attach an event to it
			/*Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};*/
		}
	}
}


