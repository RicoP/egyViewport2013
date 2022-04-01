using Microsoft.VisualStudio.Text.Editor;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace egyViewport2013 {
    [DataContract(Namespace = "egy186/cs/viewport.config")]
    class egyConfig {
        [DataMember]
        public string imageFileName = "background.png";
        [DataMember]
        public double imageOpacity = 0.2;
    }

    /// <summary>
    /// Adornment class for background image in right hand of the viewport
    /// </summary>
    class egyViewport2013 {
        private Image _image;
        private BitmapImage _bitmap;
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;

        /// <summary>
        /// Read a image and attaches an event handler to the layout changed event
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public egyViewport2013(IWpfTextView view) {

            var fileNameConfig = "config.json";
            var location = Helper.GetPath(view) ?? "UNKNOWN";
            var assemblyLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config_path = Path.Combine(assemblyLoc, fileNameConfig);
            var vConfig = LoadConfigFile(config_path);
            var image_name = vConfig.imageFileName;
            var fallback_image_path = Path.Combine(assemblyLoc, image_name);
            var imageOpacity = vConfig.imageOpacity;
            var image_path = GetParentImagePath(location) ?? fallback_image_path;
            SetupImageAndBitmap(image_path, imageOpacity);

            //Grab a reference to the adornment layer that this adornment should be added to
            _view = view;
            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };
            
            _adornmentLayer = view.GetAdornmentLayer("egyViewport2013");

            Helper.Write($"TextView Created in location {location}.");
            onSizeChange();
        }

        private string GetParentImagePath(string location) {
            Helper.Write("GetParentImagePath " + (location ?? "UNKNOWN"));
            var parent = Directory.GetParent(location);
            for (; ; ) {
                if (parent == null) return null;
                Helper.Write("GetParent " + parent.FullName);
                var bg = Path.Combine(parent.FullName, "my_background.png");
                if (File.Exists(bg)) return bg;
                parent = parent.Parent;
            }
        }

        private void SetupImageAndBitmap(string image_path, double imageOpacity) {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(image_path, UriKind.Absolute);
            bitmapImage.EndInit();

            var image = new Image();
            image.MaxWidth = bitmapImage.PixelWidth;
            image.MaxHeight = bitmapImage.PixelHeight;
            image.Opacity = imageOpacity;
            image.Source = bitmapImage;
            //image.Stretch = Stretch.Uniform;
            image.Stretch = Stretch.None;

            _image = image;
            _bitmap = bitmapImage;

            Helper.Write($"Loading Image {image_path}, PW{bitmapImage.PixelWidth}, PH{bitmapImage.PixelHeight}, Opacity{imageOpacity}");
        }

        private static egyConfig LoadConfigFile(string config_path) {
            FileStream fs = new FileStream(config_path, FileMode.Open);
            Stream mStream = new MemoryStream();
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(egyConfig));
            long fLength = fs.Length;
            byte[] fByte = new byte[fLength];
            fs.Read(fByte, 0, (int)fLength);
            mStream.Write(fByte, 0, (int)fLength);
            fs.Dispose();
            mStream.Position = 0;
            var vConfig = (egyConfig)dcjs.ReadObject(mStream);
            mStream.Dispose();
            return vConfig;
        }

        public void onSizeChange() {
            double zoom = _view.ZoomLevel;
            Helper.Write($"onSizeChange VW{_view.ViewportWidth}, VH{_view.ViewportHeight}, IW{_image.Width}, IH{_image.Height}");

            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments();

            //stretch image size
            //_image.Width = _view.ViewportWidth;
            //_image.Height = _view.ViewportHeight;
            //_image.Width = _bitmap.PixelWidth / zoom;
            //_image.Height = _bitmap.PixelHeight / zoom;
            //var width = _image.Source.Width / zoom;
            //var height = _image.Source.Height / zoom;
            var width = _bitmap.PixelWidth;
            var height = _bitmap.PixelHeight;
            //var width = _image.Width;
            //var height = _image.Height;

            //Place the image in the right hand of the Viewport
            Canvas.SetLeft(_image, _view.ViewportRight - width);
            Canvas.SetTop(_image, _view.ViewportBottom - height);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _image, null);
        }
    }
}
