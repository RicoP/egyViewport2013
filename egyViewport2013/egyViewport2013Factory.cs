using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

public static class Helper {
    static readonly string path = $"{System.IO.Path.GetTempFileName()}.log.imagevs";
    static StreamWriter log = new StreamWriter(path);

    static public void Write(string str) {
        log.Write(DateTime.Now.ToShortTimeString());
        log.Write(" >");
        log.WriteLine(str);
        log.Flush();
    }

    static public string GetPath(IWpfTextView textView) {
        textView.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);

        if (!(bufferAdapter is IPersistFileFormat persistFileFormat)) {
            return null;
        }
        persistFileFormat.GetCurFile(out string filePath, out _);
        return filePath;
    }

    static public void Kill() {
        log.Close();
        File.Delete(path);
    }
}

namespace egyViewport2013 {
    #region Adornment Factory
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class PurpleBoxAdornmentFactory : IWpfTextViewCreationListener, IDisposable {
        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("egyViewport2013")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        public void Dispose() {
            Helper.Kill();
        }

        /// <summary>
        /// Instantiates a egyViewport2013 manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView) {
            new egyViewport2013(textView);
        }
    }
    #endregion //Adornment Factory
}
