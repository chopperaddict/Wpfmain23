using System;
using System . ComponentModel;
using System . Data;
using System . IO;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Markup;
using System . Windows . Media;

using Dapper;


using Microsoft . Data . SqlClient;

using SprocsProcessing;


namespace Wpfmain . UtilWindows
{
    /// <summary>
    /// Interaction logic for Script Ediitor window thhat saves scripts to SQL server
    /// </summary>
    public partial class ScriptViewerWin : Window, INotifyPropertyChanged
    {

        #region setup variables

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged ( string propertyName )
        {
            try
            {
                if ( this . PropertyChanged != null )
                {
                    var e = new PropertyChangedEventArgs(propertyName);
                    this . PropertyChanged ( this, e );
                }
            }
            catch ( Exception ex ) { }
        }
        #endregion PropertyChanged

        public static ScriptViewerWin sviewer { get; set; }
        public bool IsScript { get; set; } = false;
        public string DefaultSavePath { get; set; } = "";
        public string DefaultOpenPath { get; set; } = "";
        public static double RTBLineHeight { get; set; } = 1.0; 

        public double PrintHeight=0;
        public double PrintWidth=0;

        private FlowDocument editorFlowDoc ;
        public FlowDocument EditorFlowDoc
        {
            get { return editorFlowDoc; }
            set { editorFlowDoc = value; OnPropertyChanged ( nameof ( EditorFlowDoc ) ); }
        }

        private string fileSuffix;
        public string FileSuffix
        {
            get { return fileSuffix; }
            set { fileSuffix = value; OnPropertyChanged ( nameof ( FileSuffix ) ); }
        }

        private string currentFile ;
        public string CurrentFile
        {
            get { return currentFile; }
            set { currentFile = value; OnPropertyChanged ( nameof ( CurrentFile ) ); }
        }
        private string currentPath;
        public string CurrentPath
        {
            get { return currentPath; }
            set { currentPath = value; OnPropertyChanged ( nameof ( CurrentPath ) ); }
        }

        private int charCount;
        public int CharCount
        {
            get { return charCount; }
            set { charCount = value; OnPropertyChanged ( nameof ( CharCount ) ); }
        }
        private int lineCount;
        public int LineCount
        {
            get { return lineCount; }
            set { lineCount = value; OnPropertyChanged ( nameof ( LineCount ) ); }
        }
        private int tabCount;
        public int TabCount
        {
            get { return tabCount; }
            set { tabCount = value; OnPropertyChanged ( nameof ( TabCount ) ); }
        }


        public ScriptViewerWin ( string filenamepath, string script )
        {
            // This reads an RTF file into our editor successfully. 24/2/23
            string [] tmp =null;
            InitializeComponent ( );
            RTBLineHeight = 1;
            RTextEditor . SetValue ( Paragraph . LineHeightProperty, RTBLineHeight );

            var val = RTextEditor . GetValue ( Paragraph . LineHeightProperty );
            RTextEditor . SetValue ( Paragraph . LineHeightProperty, 0.8 );
            this . DataContext = this;
            string suffix = filenamepath. Substring (filenamepath. Length - 4 ).ToUpper();
            if ( suffix == ".RTF" )
            {
                Mouse . OverrideCursor = Cursors . Wait;
                RTBSupport . LoadRTFdata ( RTextEditor, filenamepath );
                Mouse . OverrideCursor = Cursors . Arrow;
            }
            else if ( suffix == ".TXT" || suffix == ".XAML" || suffix == ".SQL" || suffix == ".CS" || suffix == "PPET" )
            {
                // populate RTf editor
                RTextEditor . Document . Blocks . Clear ( );
                RTextEditor . Document = CreateFlowDocFromText ( script, lineheight: RTBLineHeight );
            }
            GetRichTextEditorStats ( RTextEditor, filenamepath );
        }
        public static ScriptViewerWin GetScriptViewerWin ( )
        {
            return sviewer;
        }
        public ScriptViewerWin ( bool arg = true )
        {
            InitializeComponent ( );
            sviewer = this;
            this . DataContext = this;
            string script = "\n/* Create a new Stored Procedure ....\n\nNB: There MUST be at least one line of SQL code bewtween BEGIN and END\notherwise the script will NOT BE SAVED !!  */\n\nUSE [DBNAME]    -- Set Database to be used\nGO\r\n/* Standard options */\nSET ANSI_NULLS ON\r\nGO\r\nSET QUOTED_IDENTIFIER ON\r\nGO\n/* End of standard options */\n\nCREATE PROCEDURE [dbo].[ NEWProcedureNAME ]\n--@..... NVARCHAR (200) \n--,@.... NVARCHAR (MAX)\n\nAS\nBEGIN\n\n/* Add your procedure code here .... \nThere MUST be at least one line of VALID SQL code here ! eg: */ \n\n   Select @@CONNECTIONS\n\n/* otherwise the script will NOT BE SAVED !! */\n";
            ScriptTextEditor . Text = script;
            IsScript = true;
            Title = "Create / Save new Stored Procedure";
            CurrentFile = " New Script !!! ";
            DisableMenuOptions ( true );
            RTextEditor . Document . Blocks . Clear ( );
            RTextEditor . Document = CreateFlowDocFromText ( script, lineheight: RTBLineHeight );
            GetRichTextEditorStats ( RTextEditor,"", script );
            FileSuffix = "SCRIPT";
        }
        #endregion setup variables
        private void Closewin_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        #region SQL  file type handling
        public static void CreateNewStoredProcedure ( string newscript, out string result, string procname = "" )
        {
            try
            {
                // Save script to server
                using ( SqlConnection connection = new SqlConnection ( Flags . CurrentConnectionString ) )
                {
                    using ( SqlCommand cmd = new SqlCommand ( newscript, connection ) )
                    {
                        connection . Open ( );
                        if ( procname != "" )
                        {
                            cmd . CommandType = CommandType . Text;
                            cmd . ExecuteNonQuery ( );

                        }
                        connection . Close ( );
                    }
                }
            }
            catch ( Exception ex ) { result = $"FAILED : [ {ex . Message . ToUpper ( )} ]"; }
            result = "SUCCESS";
        }

        private void Savescript_Click ( object sender, RoutedEventArgs e )
        {
            // Save as a new script
            string result="", NewProcname="";
            // check to ensure it looks like an SP  script
            string[] test = ScriptTextEditor . Text.Split("CREATE PROCEDURE ");
            if ( test . Length == 1 )
            {
                // not a script
                MessageBoxResult mbr =  MessageBox .Show ("The data in the Editor does NOT contain the required \n'CREATE  PROCEDURE' clause, so it is NOT a valid Stored Procedure file.\n\n" +
                    $"It is strongly recommended that you use the 'Save file' or 'Save File As' menu option instead to save this file to a different destination",
                    "Save File Warning",MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel );
                if ( mbr == MessageBoxResult . Cancel )
                {
                    return;
                }
            }
            WriteScriptToServer (
                script: ScriptTextEditor . Text,
                result: out result,
                out NewProcname,
                isbackup: false,
                procedurename: "" );

            if ( result . Contains ( "SUCCESS" ) )
            {
                SProcsHandling  sph  = SProcsHandling  .GetSProcsHandling();
                int currsel = sph.SProcsListbox.SelectedIndex;
                sph . LoadAllSprocs ( );
                int index = GetScriptIndexInList ( NewProcname , currsel);
                sph . SProcsListbox . SelectedIndex = index;
                sph . SProcsListbox . UpdateLayout ( );
                MessageBox . Show ( result, "Save Script", MessageBoxButton . OK, MessageBoxImage . Exclamation );
            }
        }
        public int GetScriptIndexInList ( string NewProcname, int currsel )
        {
            SProcsHandling  sph  = SProcsHandling  .GetSProcsHandling();
            for ( int x = 0 ; x < sph . SProcsListbox . Items . Count ; x++ )
            {
                string entry = sph . SProcsListbox.Items [x].ToString();
                if ( entry . Trim ( ) == NewProcname . Trim ( ) )
                {
                    return x;
                }
            }
            return currsel;
        }
        public string ParseScriptname ( string procname )
        {
            string output = "";
            int x = 0;
            string [] norightbrace = procname.Split(']');
            foreach ( string item in norightbrace )
            {
                string[]  parts = item.Split('[');
                if ( parts . Length == 1 )
                    return procname;
                if ( x == 0 )
                    output += parts [ 1 ] . Trim ( );
                else if ( parts . Length > 1 )
                    output += "." + parts [ 1 ] . Trim ( );
                x++;
            }
            // Return prc name with no braces etc
            return output;
        }

        public bool WriteScriptToServer ( string script, out string result, out string newscriptname, bool isbackup = false, string procedurename = "" )
        {
            string originalscript = script;
            string procname ="";
            string fullscript ="";
            string[]  nameparts=null;
            string SQL2= "", scriptbody="", newfilename="";
            // get offset position to real start position usng ALL CAPS
            int offset1 = script . ToUpper ( ).IndexOf("CREATE PROCEDURE" );
            // reset entire content to NON CAPITALS
            script = originalscript;
            string nameptr = script.Substring(offset1+17);
            // split file down to line  without \r\n
            string[] newname = nameptr.Split("\r\n");
            int counter = 0;
            foreach ( string item in newname )
            {
                if ( counter > 0 )
                    scriptbody += "\r\n" + item;
                counter++;
            }
            // Scriptbody holds complete script MINUS  procname
            // This is our full orginal procedure name from the script
            procname = newname [ 0 ];
            if ( procname . Contains ( "\r" ) )
                procname = procname . Substring ( 0, procname . Length - 1 );
            int stdnamelength = procname.Length;
            string defaultprocname = procname;
            procname = ParseScriptname ( procname );
            // return new name !!
            newscriptname = procname;
            // We now have a proc name without brackets etc in procname

            if ( procedurename != "" )
            {
                //We are creating a new procname version, probably a backup
                procname = $"{procname}_{procedurename}";
                fullscript = $"CREATE PROCEDURE {procname}{scriptbody}";
                newfilename = procname;
            }
            else if ( isbackup == true )
            {
                procedurename = $"{procname}_backup";
                fullscript = $"CREATE PROCEDURE {procedurename}{scriptbody}";
                newfilename = procedurename;
            }
            else
            {
                procedurename = $"{procname . Trim ( )}";
                fullscript = $"CREATE PROCEDURE {procedurename}{scriptbody}";
                newfilename = procedurename;
            }
            script = "CREATE PROCEDURE " + procedurename;
            SQL2 = string . Format ( fullscript, "" );
            string crresult = "";
            CreateNewStoredProcedure ( SQL2, out crresult, defaultprocname );
            if ( crresult != "SUCCESS" )
            {
                MessageBox . Show ( $"The Script could not be saved...\nThe error returned by SQL server was\n\n{crresult . ToUpper ( )}", "Error Encountered", MessageBoxButton . OK, MessageBoxImage . Exclamation );
                result = "FAILED";
                return false;
            }
            result = $"SUCCESS : The Script {newfilename . ToUpper ( )}\nhas been saved to the Stored Procedures folder on your SQL Server successfully...\n\nThe Procedures list has also been updated to show the new procedure !";
            return true;
        }
        private void Deletescript_Click ( object sender, RoutedEventArgs e )
        {
            string result="";
            DeleteProcedure ( delfile . Text, CurrentFile . Trim ( ), out result );
            return;
        }
        public static bool DeleteProcedure ( string scripttext, string currentScriptName, out string result, string procname = "" )
        {
            string deletefile = scripttext;
            string SQLCommand = $"spDropProcedure";
            if ( deletefile == "" )
            {
                result = "FAILURE";
                return false;
            }
            try
            {
                var Params = new DynamicParameters ( );
                Params . Add ( "@ARG1", $"[dbo].[{currentScriptName}]" );
                using ( IDbConnection db = new SqlConnection ( Flags . CurrentConnectionString ) )
                {
                    var exresult = db .Execute( SQLCommand, Params, commandType: CommandType . StoredProcedure);
                }
            }
            catch ( Exception ex ) { MessageBox . Show ( ex . Message, "SQL processing ERROR" ); result = "FAILURE"; return false; }
            result = "SUCCESS";
            return true;
        }
        public static string CreateCopyTable ( string procname )
        {
            string newscript = $"spCopyDb {procname} , {procname}_backup";
            try
            {
                // Save script to server
                using ( SqlConnection connection = new SqlConnection ( Flags . CurrentConnectionString ) )
                {
                    using ( SqlCommand cmd = new SqlCommand ( newscript, connection ) )
                    {
                        connection . Open ( );
                        if ( procname != "" )
                        {
                            cmd . CommandType = CommandType . Text;
                            cmd . ExecuteNonQuery ( );

                        }
                        connection . Close ( );
                    }
                }
            }
            catch ( Exception ex ) { return $"WARNING : Orignal script was dropped succesfully, but the current script could NOT be saved on server : [{ex . Message . ToUpper ( )}]"; }
            return "SUCCESS";
        }

        #endregion SQL  file type handling
        private void TexthasChanged ( object sender, TextChangedEventArgs e )
        {
            if ( delfile . Text == "" )
            {
                DeleteScript . IsEnabled = false;
                DeleteScript . Opacity = 0.6;
            }
            else
            {
                DeleteScript . IsEnabled = true;
                DeleteScript . Opacity = 1;
            }
        }

        private void Printscript_Click ( object sender, RoutedEventArgs e )
        {
            this . Topmost = false;
            PrintingSupport psupport = new();
            SProcsHandling sph = new();
            TextRange txtrange = new TextRange ( RTextEditor . Document . ContentStart, RTextEditor . Document . ContentEnd );
            int len = txtrange . ToString().Length;
            PrintingSupport . SpPrintItem ( this, documenttext: ScriptTextEditor . Text );
            this . Topmost = true;
        }

        private void Close_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        private void printnewscript_click ( object sender, RoutedEventArgs e )
        {
            // create the flowdocument
            var dialog = new System.Windows.Controls.PrintDialog();
            // set up the printing range & other optoins as required
            dialog . PageRangeSelection = System . Windows . Controls . PageRangeSelection . AllPages;
            dialog . UserPageRangeEnabled = true;
            PrintHeight = dialog . PrintableAreaHeight;
            PrintWidth = dialog . PrintableAreaWidth;
            // Show save Print dialog box
            Nullable<Boolean> print = dialog.ShowDialog();

            if ( print == true )
            {
                // go ahead and print it
                FlowDocument myFlowDocument = CreateFlowDocFromText ( ScriptTextEditor . Text ,lineheight: RTBLineHeight);
                IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                dialog . PrintDocument ( idocument . DocumentPaginator, "Printing New SQL Script..." );
            }
        }
        public static FlowDocument CreateFlowDocFromText ( string inputtext, double  fontsize = 16 , double lineheight=0)
        {
            Brush fore=MainWindow . ScrollViewerFground;
            Brush back=MainWindow .ScrollViewerBground;
            MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
            MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
            
            SProcsSupport processsprocs = new();
            FlowDocument myFlowDocument = processsprocs . CreateFlowDocumentScroll ( inputtext, fontsize: fontsize, fground: "Black0", bground: "White3" );
            
            myFlowDocument . FontSize = fontsize;
            //if( lineheight  != 0)
            //    RTBLineHeight = lineheight;
            //myFlowDocument . LineHeight = RTBLineHeight;
            // reset font colors to original colors
            MainWindow . ScrollViewerBground = back;
            MainWindow . ScrollViewerFground = fore;
            return myFlowDocument;
        }
        private void Overall_Click ( object sender, RoutedEventArgs e )
        {
        }
        private void UsingSprocsPanel ( object sender, RoutedEventArgs e )
        {
        }
        private void About_Click ( object sender, RoutedEventArgs e )
        {
            // show about info
        }
        /// <summary>
        /// Saves data of most formats correctly, 
        /// incuding .txt, .RTF, .XAML <see langword="abstract"/>, .CS and most others
        /// when being saved from RichTextBox control type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveDataFiles_Click ( object sender, RoutedEventArgs e )
        {
            string fullfilepath ="";
            string defsavetype="*.*";
            // set up most relevant file type (so we use RTF if relevant)
            defsavetype = "*.rtf";

            if ( IsScript )
            {
                if ( CurrentFile . Trim ( ) != null && CurrentPath != null && CurrentFile . Trim ( ) != "" && CurrentPath != "" )
                    fullfilepath = $"{CurrentPath}{CurrentFile . Trim ( )}";
                else     // no path/name provided
                    fullfilepath = FileHandling . GetSaveFileName ( "*.*", CurrentFile . Trim ( ) );
            }
            else
            {
                // Not a script file
                if ( CurrentFile . Trim ( ) != null && CurrentPath != null && CurrentFile . Trim ( ) != "" && CurrentPath != "" )
                    fullfilepath = $"{CurrentPath}{CurrentFile . Trim ( )}";
                else     // no path/name provided
                    fullfilepath = FileHandling . GetSaveFileName ( defsavetype, CurrentFile . Trim ( ) );
            }
            if ( fullfilepath . Contains ( "*" ) == true || fullfilepath == "" || fullfilepath == "*.*" || fullfilepath == "*.rtf" || fullfilepath . Length < 6 )
                return;
            // Save path selected to global var
            string pathalone = "";
            Utils2 . GetFilenameFromPath ( fullfilepath, out pathalone );
            if ( pathalone != "" )
                DefaultSavePath = pathalone;

            Mouse . OverrideCursor = Cursors . Wait;
            // Get text from file being edited
            var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
            if ( fullfilepath . ToUpper ( ) . Contains ( ".RTF" ) )
            {
                // This works successfully.... 24/2/23
                // Writes data to an RTF format file
                RTBSupport . SaveXamlPackage ( RTextEditor, fullfilepath );
                //Stream fStream = new FileStream ( fullfilepath, FileMode . Create );
                //textRange . Save ( fStream, DataFormats . Rtf );
                //fStream . Close ( );
            }
            else if ( fullfilepath . ToUpper ( ) . Contains ( ".XAML" ) )
                RTBSupport. SaveXamlPackage ( RTextEditor, fullfilepath );
            else if ( fullfilepath . ToUpper ( ) . Contains ( ".TXT" ) )
                File . WriteAllText ( fullfilepath, textRange . Text );
            else if ( fullfilepath . ToUpper ( ) . Contains ( ".SQL" ) )
                File . WriteAllText ( fullfilepath, textRange . Text );
            else
            {
                Mouse . OverrideCursor = Cursors . Arrow;
                MessageBox . Show ( $"The file suffix (type of document) in [ {fullfilepath} ] has not been recognised, so it could nt be saved\n\n" +
                    $"Try using the File Save As option ?", "File Type not reconized" );
            }
            Mouse . OverrideCursor = Cursors . Arrow;
        }

        private void Open_Click ( object sender, RoutedEventArgs e )
        {
            string script="";
            string filepath = FileHandling . GetOpenFileName ( "*.*" );
            if ( filepath == "" ) return;
            string suffix = filepath. Substring (filepath. Length - 4 ).ToUpper();
            FileSuffix = suffix;

            if ( suffix == ".RTF" )
            {
                Mouse . OverrideCursor = Cursors . Wait;
                if ( RTBSupport . LoadXamlPackage ( RTextEditor, filepath ) == false )
                {
                    MessageBox . Show ( "Failed to load XAML file ...." );
                    Mouse . OverrideCursor = Cursors . Arrow;
                    return;
                }
                Mouse . OverrideCursor = Cursors . Arrow;
            }
            else if ( suffix == ".TXT" || suffix == ".XAML" || suffix == ".SQL" || suffix == ".CS" || suffix == "PPET" )
            {
                if ( filepath != "" && filepath . Length > 5 )
                {
                    Mouse . OverrideCursor = Cursors . Wait;
                    script = File . ReadAllText ( filepath );
                    RTextEditor . Document . Blocks . Clear ( );
                    RTextEditor . Document = CreateFlowDocFromText ( script , lineheight: RTBLineHeight );
                    RTextEditor . UpdateLayout ( ); ;
                }
            }
            GetRichTextEditorStats ( RTextEditor, filepath );
            DisableMenuOptions ( true );
            Mouse . OverrideCursor = Cursors . Arrow;
        }

        private void OpenAny_Click ( object sender, RoutedEventArgs e )
        {
            Mouse . OverrideCursor = Cursors . Wait;
            string fullFilePath = FileHandling . LoadAnyFile (RTextEditor,  "*.*" );
            DisableMenuOptions ( true );
            GetRichTextEditorStats ( RTextEditor, fullFilePath );
            Mouse . OverrideCursor = Cursors . Arrow;
            statusbar . UpdateLayout ( );
        }
        private void Replace3tabs_click ( object sender, RoutedEventArgs e )
        {
            ScriptTextEditor . Text = Utils2 . TabsToSpaces ( ScriptTextEditor . Text, 3 );
            ScriptTextEditor . UpdateLayout ( );
            DisableMenuOptions ( false );
        }
        private void Replace4tabs_click ( object sender, RoutedEventArgs e )
        {
            ScriptTextEditor . Text = Utils2 . TabsToSpaces ( ScriptTextEditor . Text, 4 );
            ScriptTextEditor . UpdateLayout ( );
            DisableMenuOptions ( false );
        }
        private void DisableMenuOptions ( bool direction )
        {
            // true means ENABLE, false = DISABLE
            if ( direction == false )
            {
                Replace3Tabs . IsEnabled = false;
                Replace4Tabs . IsEnabled = false;
            }
            else
            {
                Replace3Tabs . IsEnabled = true;
                Replace4Tabs . IsEnabled = true;
            }
        }

        private void CopyToClipboard ( object sender, RoutedEventArgs e )
        {
            FlowDocument doc =RTextEditor.Document;
            var textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
            //            string[] lines = textRange.Text.Split('\n');
            if ( CurrentFile . Trim ( ) . ToUpper ( ) . Contains ( ".RTF" ) )
                Clipboard . SetText ( textRange . Text, TextDataFormat . Rtf );
            else if ( CurrentFile . Trim ( ) . ToUpper ( ) . Contains ( ".XAML" ) )
                Clipboard . SetText ( textRange . Text, TextDataFormat . Xaml );
            else if ( CurrentFile . Trim ( ) . ToUpper ( ) . Contains ( ".TXT" ) )
                Clipboard . SetText ( textRange . Text, TextDataFormat . Text );
            //else if ( CurrentFile.Trim() . ToUpper ( ) . Contains ( ".RTF" ) )
            //    Clipboard . SetText ( textRange . Text, TextDataFormat .UnicodeText);
            else
                Clipboard . SetText ( textRange . Text );
        }

        private void PasteFromClipboard ( object sender, RoutedEventArgs e )
        {
            if ( Clipboard . ContainsText ( ) == false )
            {
                MessageBox . Show ( "Sorry, the clipboard does NOT have any 'pastable' Text data available", "ClipBoard" );
                return;
            }
            string txt = Clipboard . GetText ( );
            ScriptTextEditor . Text += txt;
            ScriptTextEditor . UpdateLayout ( );
        }

        #region staistics handlers 
        private void CopyseltextToClipboard ( object sender, RoutedEventArgs e )
        {
            string txt = ScriptTextEditor . SelectedText;
            Clipboard . SetText ( txt );
        }

        private void CountChars_click ( object sender, RoutedEventArgs e )
        {
            int len = ScriptTextEditor . Text.Length;
            MessageBox . Show ( $"There are  {len} characters in the  file, (including line feeds etc)", "File Character Count", MessageBoxButton . OK, MessageBoxImage . Information );

        }

        private void CountLines_click ( object sender, RoutedEventArgs e )
        {
            string[] tmp = RTextEditor.Document.ToString() .Split ( "\n");
            int len = tmp.Length;
            LineCount = len;
            MessageBox . Show ( $"There are  {len} lines in the  file", "File line Count", MessageBoxButton . OK, MessageBoxImage . Information );
        }

        private void ShowStats_click ( object sender, RoutedEventArgs e )
        {
            MessageBox . Show ( $"There are  {CharCount} characters including {TabCount} tab stop(s)\nin a total of {LineCount} lines in the  file...", "File Statistics", MessageBoxButton . OK, MessageBoxImage . Information );
        }

        private void EditorTxtChanged ( object sender, TextChangedEventArgs e )
        {
            GetTextEditorStats ( );
            Col1 . UpdateLayout ( );
            Col2 . UpdateLayout ( );
            Col3 . UpdateLayout ( );
            Col4 . UpdateLayout ( );
        }

        public void GetTextEditorStats ( )
        {
            CharCount = ScriptTextEditor . Text . Length;
            string[] tmp = ScriptTextEditor.Text.Split ( "\n");
            int len = tmp.Length;
            LineCount = len;
            string[] tabs = ScriptTextEditor . Text .Split ( '\t' );
            TabCount = tabs . Length;
        }

        public void GetRichTextEditorStats ( RichTextBox rtb, string filenamepath , string textcontent="")
        {
            FlowDocument doc =rtb . Document;
            string path="";
            if ( textcontent == "" )
            {
                CurrentFile = $" {Utils2 . GetFilenameFromPath ( filenamepath, out path )} ";
                CurrentPath = path;
                var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                string[] lines = textRange.Text.Split('\n');
                LineCount = lines . Length;
                lines = textRange . Text . Split ( '\t' );
                TabCount = lines . Length;
                CharCount = textRange . Text . Length;
            }
            else
            {
                string[] lines = textcontent.Split('\n');
                LineCount = lines . Length;
                lines = textcontent .  Split ( '\t' );
                TabCount = lines . Length;
                CharCount = textcontent . Length;
                CurrentFile = "NEWSCRIPT.SQL";
                FileSuffix = "SQL";

            }
        }

        #endregion staistics handlers 

        #region file Saving methods
        public void SaveXamlData ( RichTextBox rtb, string path )
        {
            // How to save save XAML data
            RTBSupport . SaveXamlPackage ( rtb, path );
        }
        private void SaveTextData ( object sender, RoutedEventArgs e )
        {
            string data = ScriptTextEditor . Text ;
            FileHandling . SaveTextData ( data, $@"C:\WpfMain\userdatafiles\" );
            string filepath = FileHandling . GetSaveFileName ( "*.txt" , CurrentFile.Trim());
            if ( filepath == "" || filepath == "*.*" || filepath . Length < 5 ) return;
            if ( filepath . ToUpper ( ) . Contains ( "RTF" ) )
            {
                RTBSupport . SaveXamlPackage ( RTextEditor, filepath );
                // Write data  to RTF file(EXCLUDES \r\n !!)
                using ( FileStream file = new FileStream ( filepath, FileMode . Create ) )
                {
                    var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
                    string strng = textRange . Text;
                    string[] lines = strng.Split("\n");
                    string output="";
                    foreach ( string item in lines )
                    {
                        output += $"{item}\r\n";
                    }
                    // textRange .ContentStart = output;
                    textRange . Save ( file, System . Windows . DataFormats . Rtf );
                }
            }
            else
                File . WriteAllText ( filepath, data );
        }
        private void SaveScriptData ( object sender, RoutedEventArgs e )
        {
            if ( CurrentFile . Trim ( ) != "" && CurrentPath != "" ) return;
            string data = ScriptTextEditor . Text ;
            string fullpath = $"{CurrentPath}{CurrentFile.Trim()}";
            File . WriteAllText ( fullpath, data );
        }

        #endregion file Saving methods
        public void SaveAsStream ( string filepath )
        {
            //System.IO.StreamWriter file = new System.IO.StreamWriter(filepath);
            //file . WriteLine ( RTextEditor . RTF );
            //file . Close ( );
        }

        private void SaveScriptFile_Click ( object sender, RoutedEventArgs e )
        {

        }

        private void SaveAsDataFiles_Click ( object sender, RoutedEventArgs e )
        {
            // just trickle it down to my  specialist libraries
            FileHandling . SaveAnyData ( RTextEditor, CurrentPath, CurrentFile );
        }

        private void IncreaseLineheight_click ( object sender, RoutedEventArgs e )
        {
            var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
            string data = textRange . Text;
            double val = Convert.ToDouble(RTextEditor . GetValue ( Paragraph . LineHeightProperty));
            RTBLineHeight += 2;
            RTextEditor . SetValue ( Paragraph . LineHeightProperty, RTBLineHeight );
            FlowDocument fdoc = CreateFlowDocFromText ( data, fontsize: 16, lineheight: RTBLineHeight );
            RTextEditor . Document = fdoc;
            RTextEditor . UpdateLayout ( );
        }

        private void decreaseLineheight_click ( object sender, RoutedEventArgs e )
        {
            var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
            string data = textRange . Text;
            double val = Convert.ToDouble(RTextEditor . GetValue ( Paragraph . LineHeightProperty));
            if ( RTBLineHeight >= 3 )
                RTBLineHeight -= 2;
            RTextEditor . SetValue ( Paragraph . LineHeightProperty, RTBLineHeight );
            FlowDocument fdoc = CreateFlowDocFromText ( data, fontsize: 16, lineheight: RTBLineHeight );
            RTextEditor . Document = fdoc;
            RTextEditor . UpdateLayout ( );

        }

        private void IncreaseFontsize_click ( object sender, RoutedEventArgs e )
        {
            var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
            string data = textRange . Text;
            double val = Convert.ToDouble(RTextEditor . GetValue ( Paragraph . LineHeightProperty));
            RTBLineHeight += 2;
           double fsize =  Convert.ToInt32(RTextEditor . GetValue ( Paragraph . FontSizeProperty));
            fsize += 1;
            FlowDocument fdoc = CreateFlowDocFromText ( data, fontsize: fsize, lineheight: RTBLineHeight );
            RTextEditor . Document = fdoc;
            RTextEditor.SetValue ( Paragraph . FontSizeProperty, fsize );
            RTextEditor . UpdateLayout ( );

        }

        private void DecreaseFontsize_click ( object sender, RoutedEventArgs e )
        {
            var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
            string data = textRange . Text;
            double val = Convert.ToDouble(RTextEditor . GetValue ( Paragraph . LineHeightProperty));
            RTBLineHeight += 2;
            double fsize =  Convert.ToInt32(RTextEditor . GetValue ( Paragraph . FontSizeProperty));
            fsize -= 1;
            FlowDocument fdoc = CreateFlowDocFromText ( data, fontsize: fsize, lineheight: RTBLineHeight );
            RTextEditor . Document = fdoc;
            RTextEditor . SetValue ( Paragraph . FontSizeProperty, fsize );
            RTextEditor . UpdateLayout ( );
        }

        private void ScriptMenu_MouseEnter ( object sender, MouseEventArgs e )
        {
            MenuItem mi = sender as MenuItem;
            if ( mi == null ) return;

            SphMenuControl spm = new(this );
            if ( mi . Name == "ScriptMenuFileOpening" )
                spm . ScriptMenuFileOpening ( sender, e, "ScriptMenuFileOpening" );
            else if ( mi . Name == "ScriptMenuEditOpening" )
                spm . ScriptMenuEditOpening ( sender, e, "ScriptMenuEditOpening" );
            else if ( mi . Name == "ScriptMenuOptsOpening" )
                spm . ScriptMenuOptsOpening ( sender, e, "ScriptMenuOptsOpening" );
            else if ( mi . Name == "ScriptMenuHelpOpening" )
                spm . ScriptMenuHelpOpening ( sender, e, "ScriptMenuHelpOpening" );

        }
    }
}
//var textRange = new TextRange(RTextEditor.Document.ContentStart, RTextEditor.Document.ContentEnd);
////