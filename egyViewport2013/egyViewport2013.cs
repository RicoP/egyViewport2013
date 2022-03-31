﻿using Microsoft.VisualStudio.Text.Editor;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace egyViewport2013
{
    [DataContract(Namespace = "egy186/cs/viewport.config")]
    class egyConfig
    {
        [DataMember]
        public string imageFileName = "background.png";
        [DataMember]
        public double imageOpacity = 0.2;
    }

    /// <summary>
    /// Adornment class for background image in right hand of the viewport
    /// </summary>
    class egyViewport2013
    {
        private Image _image;
        private double _imageAspect;
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;

        /// <summary>
        /// Read a image and attaches an event handler to the layout changed event
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public egyViewport2013(IWpfTextView view)
        {
            _view = view;

            string fileNameConfig = "config.json";
            string assemblyLoc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Stream mStream = new MemoryStream();
            FileStream fs = new FileStream(Path.Combine(assemblyLoc, fileNameConfig), FileMode.Open);
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(egyConfig));
            long fLength = fs.Length;
            byte[] fByte = new byte[fLength];
            fs.Read(fByte, 0, (int)fLength);
            mStream.Write(fByte, 0, (int)fLength);
            fs.Dispose();
            mStream.Position = 0;
            egyConfig vConfig = (egyConfig)dcjs.ReadObject(mStream);
            mStream.Dispose();

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(Path.Combine(assemblyLoc, vConfig.imageFileName), UriKind.Absolute);
            bitmapImage.EndInit();

            _image = new Image();
            _image.MaxWidth = bitmapImage.PixelWidth;
            _image.MaxHeight = bitmapImage.PixelHeight;
            _image.Opacity = vConfig.imageOpacity;
            _image.Source = bitmapImage;
            _image.Stretch = Stretch.Uniform;

            _imageAspect = bitmapImage.Height / bitmapImage.Width;

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("egyViewport2013");

            this.onSizeChange();
            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };
        }

        public void onSizeChange()
        {
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
