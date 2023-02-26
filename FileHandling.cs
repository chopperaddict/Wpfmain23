using System;
using System . IO;
using System . Security . Cryptography;
using System . Text;
using System . Windows . Controls;
using System . Windows . Documents;
using System . Windows . Markup;

using Microsoft . Win32;

namespace Wpfmain
{
    public static class FileHandling
    {
        #region file Saving methods
        public static void SaveXamlData ( RichTextBox rtb, string filepath = "" )
        {
            // How to save save XAML data
            if ( filepath == "" )
                FileHandling . GetSaveFileName ( "*.rtf", filepath );
            RTBSupport . SaveXamlPackage ( rtb, filepath );
        }
        public static void SaveAnyData ( RichTextBox rtb, string filepath = "", string filename = "" )
        {
            string fullpath ="";
            string  defDirectory = Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments );
            string data = "";

            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            data = textRange . Text;

            if ( filepath == "" )
                filepath = defDirectory;

            if ( filepath == "" || filepath == "*.*" || filepath . Length < 5 )
                return;

            if ( filename == "" )
            {
                if ( filepath . EndsWith ( '\\' ) == false )
                    fullpath = @$"{filepath}\*.*";
                else
                    fullpath = @$"{filepath}*.*";
            }
            if ( fullpath == "" )
                filename = FileHandling . GetSaveFileName ( "*.*", fullpath );
            else
                filename = FileHandling . GetSaveFileName ( "*.*", fullpath );
            if ( filename == "" )
                return;
           // if(filepath.Substring(filepath.Length - 1, 1) == "\\")
                filepath = filename; 
//            else
                //filepath +=@$"{filename}\{filename}";

            string suffix = GetFileSuffix ( filename.ToUpper());

            if ( suffix == "XAML" )
            {
                RTBSupport . SaveXamlPackage ( rtb, filepath );
                // Write data  to RTF file(EXCLUDES \r\n !!)
                //using ( FileStream file = new FileStream ( fullpath, FileMode . Create ) )
                //{
                //    textRange . Save ( file, System . Windows . DataFormats . Rtf );
                //}
            }
            else if ( suffix == "RTF" )
                RTBSupport . SaveRtfData ( rtb, filepath );
            else if ( suffix == ".SQL" )
                SaveSqlData ( textRange . Text, filepath, filename );
            else
                SaveTextData ( data, filepath  );
        }
        public static void SaveSqlData ( string data, string filepath = "", string filename = "" )
        {
            if ( filepath == "" )
                FileHandling . GetSaveFileName ( "*.sql", filepath );

            if ( filepath == "" || filepath == "*.*" || filepath . Length < 5 ) return;
            File . WriteAllText ( filepath, data );
        }
        public static void SaveTextData ( string data, string filepath = "" )
        {
            if ( filepath == "" )
                FileHandling . GetSaveFileName ( "*.txt", filepath );

            if ( filepath == "" || filepath == "*.*" || filepath . Length < 5 ) return;
            File . WriteAllText ( filepath, data );
        }

        #endregion file Saving methods

        #region SaveFileDialog methods
        public static string GetSaveFileName ( string filespec = "*.*", string filepath = @"C:\user\ianch\Documents\", string filename = "" )
        // opens  the common file SAVE dialog
        {
            SaveFileDialog ofd = new SaveFileDialog ( );
            if ( filepath . Trim ( ) != "" )
                ofd . InitialDirectory = filepath;
            else
                ofd . InitialDirectory = Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments );
            ofd . FileName = filename;
            ofd . CheckFileExists = false;  // we are saving, so it doesn't matter
            ofd . AddExtension = true;
            ofd . Title = "Select name for file to be saved.";
            if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
            {
                ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
                ofd . DefaultExt = ".xl*";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
            {
                ofd . Filter = "Comma seperated data (*.csv) | *.csv";
                ofd . DefaultExt = ".csv";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "PNG" ) )
            {
                ofd . Filter = "Image (*.png*) | *.pb*";
                ofd . DefaultExt = ".png";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "RTF" ) )
            {
                ofd . Filter = "Rich Text (*.rtf*) | *.rtf";
                ofd . DefaultExt = ".rtf";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "TXT" ) )
            {
                ofd . Filter = "Text (*.txt*) | *.txt | Rich Text (*.rtf) | *.rtf ";
                ofd . DefaultExt = ".txt";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "SQL" ) )
            {
                ofd . Filter = "SQL script (*.sql*) | *.sql";
                ofd . DefaultExt = ".sql";
            }
            else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) )
            {
                ofd . Filter = "All Files (*.*) | *.* | Text Files (*.txt) | *.txt | Sql script (*.sql) |*.sql | Rich Text (*.rtf) | *.rtf |.Snippet (*.snippet) | *.snippet | C# Source (*.cs) | *.cs| Xaml (*.xaml) | *.xaml";
                ofd . DefaultExt = "*.*";
            }
            else if ( filespec == "" )
            {
                ofd . Filter = "All Files (*.*) | *.* | Text Files (*.txt) | *.txt | Sql script (*.sql) |*.sql | Rich Text (*.rtf) | *.rtf | Snippet (*.snippet) | *.snippet | C# Source (*.cs) | *.cs| Xaml (*.xaml) | *.xaml";
                ofd . DefaultExt = ".txt";
            }
            ofd . FileName = filespec;
            ofd . ShowDialog ( );
            string fnameonly = ofd . SafeFileName;
            if ( fnameonly == "" )
                return "";      // unsafe filename
            // return full path/name
            return ofd . FileName;
        }
        public static string GetOpenFileName ( string filespec, string filepath = @"C:\user\ianch\Documents\", string filename = "" )
        // opens  the common file OPEN dialog
        {
            OpenFileDialog ofd = new OpenFileDialog ( );
            if ( filepath != "" )
                ofd . InitialDirectory = filepath;
            else
                ofd . InitialDirectory = Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments );
            ofd . FileName = filename;
            ofd . InitialDirectory = @"C:\Users\ianch\Documents\";
            ofd . CheckFileExists = false;
            if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
                ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
            else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
                ofd . Filter = "Comma seperated data (*.csv) | *.csv";
            else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) || filespec == "" )
                ofd . Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt|RTF Text (*.rtf)|*.rtf|Sql script (*.sql)|*.sql|Snippet (*.snippet)|*.snippet|C# Source (*.cs)|*.cs| Xaml (*.xaml) | *.xaml";
            ofd . AddExtension = true;
            ofd . ShowDialog ( );
            return ofd . FileName;
        }
        #endregion SaveFileDialog methods

        #region LOAD data methods


        public static string LoadAnyFile ( RichTextBox rtb, string filespec, string filepath = @"C:\user\ianch\Documents\", string filename = "" )
        {
            if ( filename == "" && filepath == ""  &&    filespec == "")
                filespec= "*.*";
            string CurrentPath= FileHandling.GetOpenFileName(filespec, filepath,filename);

            if ( CurrentPath == "" ) return "";
            string CurrentFile = $" {Utils2 . GetFilenameFromPath ( CurrentPath, out string fullpath ) . Trim ( )} ";
            CurrentFile = CurrentFile . Trim ( );
            CurrentPath = fullpath;
            fullpath = $"{CurrentPath}{CurrentFile . Trim ( )}";
            string suffix = FileHandling.GetFileSuffix ( CurrentFile.ToUpper());

            if ( suffix == "XAML" )
            {
                RTBSupport . LoadXamlPackage ( rtb, fullpath );
            }
            if ( suffix == "RTF" )
            {
                RTBSupport . LoadRTFdata ( rtb, fullpath );
            }
           else
            {
                RTBSupport . LoadTextData ( rtb, fullpath );
            }
            return fullpath;
        }
        
        #endregion LOAD data methods


        #region utilities

        public static string GetFileSuffix ( string fullfilepath )
        {
            string output="";
            string rev = Utils2.ReverseString(fullfilepath);
            byte[] barr = Encoding.ASCII.GetBytes(rev.ToString());
            string chstring = @".";
            byte[] ch = Encoding.ASCII.GetBytes(chstring);
            for ( int x = 0 ; x < barr . Length ; x++ )
            {
                // work down to last period
                if ( barr [ x ] == ch [0] )
                {
                    output = rev . Substring (0, x);
                    output = Utils2 . ReverseString ( output );
                    break;
                }
            }
            return output.Trim();
        }
        
        #endregion utilities
    }
}
