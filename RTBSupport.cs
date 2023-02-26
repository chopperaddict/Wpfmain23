using System;
using System . Diagnostics;
using System . IO;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;

using Microsoft . Identity . Client;

namespace Wpfmain
{
    /// <summary>
    /// Class to provide support for loading and saving data
    /// to/from a RichTextBox control
    /// Called generically by FileHandling.CS
    /// </summary>
    public partial class RTBSupport : Page
    {
        public static TextRange range;
        public  static FileStream fStream;

        #region LoadFile methods
        /// <summary>
        /// Load XAML into RichTextBox from a file specified by _fileName
        /// </summary>
        /// <param name="rtb">RichTextBox</param>
        /// <param name="_fileName">string</param>
        /// <returns></returns>
        public static bool LoadXamlPackage ( RichTextBox rtb, string _fileName )
        {
            // Working correctly on valid XAML files 24/2/23
            try
            {
                if ( File . Exists ( _fileName ) )
                {
                    range = new TextRange ( rtb . Document . ContentStart, rtb . Document . ContentEnd );
                    Stream fStream = new FileStream ( _fileName, FileMode . Open );
                    range. Load( fStream, DataFormats . Rtf );
                    fStream . Close ( );
                    return true;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Error Reading XamlPackage data\n{ex . Message}" );
                MessageBox . Show ( $"The operation to load {_fileName} failed, error returned was : \n{ex . Message}", "Load of XAML data failed" ); 
                return false;
            }
            return false;
        }

        /// Load RTF into RichTextBox from a file specified by _fileName
        public static bool LoadRTFdata ( RichTextBox rtb, string _fileName )
        {
            try
            {
                if ( File . Exists ( _fileName ) )
                {
                    Mouse . OverrideCursor = Cursors . Wait;
                    range = new TextRange ( rtb . Document . ContentStart, rtb . Document . ContentEnd );
                    fStream = new FileStream ( _fileName, FileMode . OpenOrCreate );
                    range . Load ( fStream, DataFormats . Rtf);
                    fStream . Close ( );
                    Mouse . OverrideCursor = Cursors . Arrow;
                    return true;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Error Reading RTF data\n{ex . Message}" );
                Mouse . OverrideCursor = Cursors . Arrow;
                //           MessageBox.Show("","" )    
                return false;
            }
            return false;
        }

        public static bool LoadTextData ( RichTextBox rtb, string _fileName )
        {
            try
            {
                if ( File . Exists ( _fileName ) )
                {
                    Mouse . OverrideCursor = Cursors . Wait;
                    range = new TextRange ( rtb . Document . ContentStart, rtb . Document . ContentEnd );
//                    string  datat = File . ReadAllText ( _fileName );
                    fStream = new FileStream ( _fileName, FileMode .Open);
                    range . Load ( fStream, DataFormats . Text );
                    fStream . Close ( );
                    Mouse . OverrideCursor = Cursors . Arrow;
                    return true;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( $"Error Reading RTF data\n{ex . Message}" );
                Mouse . OverrideCursor = Cursors . Arrow;
                //           MessageBox.Show("","" )    
                return false;
            }
            return false;
        }

        // Save XAML in RichTextBox to a file specified by _fileName

        #endregion LoadFile methods

        #region SaveFile methods
        public static bool SaveXamlPackage ( RichTextBox rtb, string _fileName )
        {
            try
            {
                range = new TextRange ( rtb . Document . ContentStart, rtb . Document . ContentEnd );
                fStream = new FileStream ( _fileName, FileMode . Create );
                range . Save ( fStream, DataFormats . XamlPackage );
                fStream . Close ( );
                return true;
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"Error Writing XamlPackage data.\n{ex . Message} " ); return false; }
        }
        /// <summary>
        /// Saves RTF data  to disk, including the linefeeds !!!
        /// </summary>
        /// <param name="rtb"></param>
        /// <param name="FullFilePath"></param>
        /// <returns></returns>
        public static bool SaveRtfData ( RichTextBox rtb, string FullFilePath )
        {
            // this seems to save the rtf correctly fter addition of the 3rd arg (true)
            try
            {
                //string output="";
                TextRange range = new TextRange ( rtb . Document . ContentStart, rtb . Document . ContentEnd );
                using ( FileStream file = new FileStream ( FullFilePath, FileMode . Create ) )
                {
                    range . Save ( file, DataFormats . Rtf , true);
                }
                return true;
            }
            catch ( Exception ex ) { Debug . WriteLine ( $"Error Writing RTF Data data \n{ex . Message}" ); return false; }
        }
        // Print RichTextBox content

        #endregion SaveFile methods

        #region Print methods

        public static void PrintCommand ( RichTextBox rtb )
        {
            PrintDialog pd = new PrintDialog();
            if ( ( pd . ShowDialog ( ) == true ) )
            {
                //use either one of the below
                pd . PrintVisual ( rtb as Visual, "printing as visual" );
                pd . PrintDocument ( ( ( ( IDocumentPaginatorSource ) rtb . Document ) . DocumentPaginator ), "printing as paginator" );
            }
        }

        #endregion  Print methods
    }
}


