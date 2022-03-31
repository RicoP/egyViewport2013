using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

public static class Logger {
    static StreamWriter log = new StreamWriter(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\blablub.log");

    public static void Write(string str) {
        log.Write(DateTime.Now.ToShortTimeString());
        log.Write(" >");
        log.WriteLine(str);
        log.Flush();
    }

    public static string GetPath(this IWpfTextView textView) {
        textView.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);

        if (!(bufferAdapter is IPersistFileFormat persistFileFormat)) {
            return null;
        }
        persistFileFormat.GetCurFile(out string filePath, out _);
        return filePath;
    }

}


namespace egyViewport2013
{
    #region Adornment Factory
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class PurpleBoxAdornmentFactory : IWpfTextViewCreationListener
    {
        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("egyViewport2013")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;
        
        
        /// <summary>
        /// Instantiates a egyViewport2013 manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            Logger.Write("TextView Created");

            new egyViewport2013(textView);
        }
    }
    #endregion //Adornment Factory
}
