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
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;

        /// <summary>
        /// Read a image and attaches an event handler to the layout changed event
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public egyViewport2013(IWpfTextView view) {

            string location = Helper.GetPath(view);
            Helper.Write("TextView Created in location " + (location ?? "UNKNOWN"));

            _view = view;

            var fileNameConfig = "config.json";
            var assemblyLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config_path = Path.Combine(assemblyLoc, fileNameConfig);
            var vConfig = LoadConfigFile(config_path);
            var image_name = vConfig.imageFileName;
            var fallback_image_path = Path.Combine(assemblyLoc, image_name);
            var imageOpacity = vConfig.imageOpacity;
            var image_path = GetParentImagePath(location) ?? fallback_image_path;
            var bitmapImage = LoadBitmap(image_path);
            var image = CreateImage(bitmapImage, imageOpacity);

            _image = image;

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("egyViewport2013");

            this.onSizeChange();
            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };
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

        private static Image CreateImage(BitmapImage bitmapImage, double imageOpacity) {
            var image = new Image();
            image.MaxWidth = bitmapImage.PixelWidth;
            image.MaxHeight = bitmapImage.PixelHeight;
            image.Opacity = imageOpacity;
            image.Source = bitmapImage;
            image.Stretch = Stretch.Uniform;
            return image;
        }

        private static BitmapImage LoadBitmap(string image_path) {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(image_path, UriKind.Absolute);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private static egyConfig LoadConfigFile(string config_path) {
            egyConfig vConfig;
            FileStream fs = new FileStream(config_path, FileMode.Open);
            Stream mStream = new MemoryStream();
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(egyConfig));
            long fLength = fs.Length;
            byte[] fByte = new byte[fLength];
            fs.Read(fByte, 0, (int)fLength);
            mStream.Write(fByte, 0, (int)fLength);
            fs.Dispose();
            mStream.Position = 0;
            vConfig = (egyConfig)dcjs.ReadObject(mStream);
            mStream.Dispose();
            return vConfig;
        }

        public void onSizeChange() {
            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments();

            //stretch image size
            double width = _view.ViewportWidth;
            double height = _view.ViewportHeight;
            _image.Width = width;
            _image.Height = height;

            //Place the image in the right hand of the Viewport
            Canvas.SetLeft(_image, _view.ViewportRight - _image.Width);
            Canvas.SetTop(_image, _view.ViewportBottom - _image.Height);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _image, null);
        }
    }
}
