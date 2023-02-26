using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Documents;
using System . Windows . Markup;
using System . Windows . Xps . Packaging;
using System . Xml . Serialization;
using System . Xml;
using System . Windows . Media;
using SprocsProcessing;
using System . Collections . ObjectModel;
using ViewModels;
using Microsoft . Data . SqlClient . DataClassification;
using System . Windows . Media . Animation;

namespace Wpfmain
{
    public class PrintingSupport
    {
        public string Pathname { get; set; } = "";
        public string filename { get; set; } = "";
        private static string defprintfilepath = "C:\\MyDocumentation\\TempPrintBuffer.dat";

        public PrintingSupport ( )
        {
        }
        public PrintingSupport GetPrintingSsupport ( )
        {
            return this;
        }

        public static void SpPrintItem ( UIElement caller, string documenttext = "", string filename = "", FlowDocument flowDocument = null )
        {
            // get window pointers
            SProcsHandling sph = SProcsHandling.GetSProcsHandling();
            if ( sph == null )
            {
                MessageBox . Show ( "Sorry, but you must have the SProcsHandling Window to be able to use the printing support", "Print Support" );
                return;
            }
            SProcsSupport processsprocs = new();

            var dialog = new System.Windows.Controls.PrintDialog();
            dialog . PageRangeSelection = System . Windows . Controls . PageRangeSelection . AllPages;
            dialog . UserPageRangeEnabled = true;
            // Show save Print dialog box
            Nullable<Boolean> print = dialog.ShowDialog();

            if ( print == true )
            {
                // Pint the file 0xFFFFFFFF = White, 0xFF000000 = black
                if ( flowDocument != null && documenttext == "" && filename == "" )
                {
                    // print Flowdocument
                    Brush fore=MainWindow . ScrollViewerFground;
                    Brush back=MainWindow . ScrollViewerBground;
                    MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                    MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                    FlowDocument myFlowDocument=flowDocument;
                    myFlowDocument . Blocks . Clear ( );
                    sph . FetchStoredProcedureCode ( sph . SProcsListbox . SelectedItem . ToString ( ), ref documenttext );

                    myFlowDocument = processsprocs . CreatePrintFlowDoc ( myFlowDocument, documenttext, "" );
                    myFlowDocument . Background = MainWindow . ScrollViewerBground;
                    myFlowDocument . Foreground = MainWindow . ScrollViewerFground;
                    IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing Flow Document..." );
                    sph . ExecResult . Background = Application . Current . FindResource ( "Green4" ) as SolidColorBrush;
                    sph . ExecResult . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                    sph . StatusText = "Document has been printed successfully.....";
                    MainWindow . ScrollViewerFground = fore; ;
                    MainWindow . ScrollViewerBground = back;
                }
                else if ( documenttext != "" )
                {
                    if ( flowDocument != null )
                    {
                        // print FlowDocument from it's text
                        Brush fore=MainWindow . ScrollViewerFground;
                        Brush back=MainWindow .ScrollViewerBground;
                        MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                        MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                        FlowDocument myFlowDocument=flowDocument;
                        myFlowDocument = processsprocs . CreateBoldString (sph,  myFlowDocument, documenttext, "" );
                        Table table = new();
                        // Create and add a couple of columns.
                        table . Columns . Add ( new TableColumn ( ) );
                        table . Columns . Add ( new TableColumn ( ) );
                        // Create and add a row group and a couple of rows.
                        table . RowGroups . Add ( new TableRowGroup ( ) );
                        table . RowGroups [ 0 ] . Rows . Add ( new TableRow ( ) );
                        table . RowGroups [ 0 ] . Rows . Add ( new TableRow ( ) );

                        // Create four cells initialized with the sample text paragraph.
                        Paragraph padding = new Paragraph(new Run("       "));
                        Paragraph text= new Paragraph(new Run(myFlowDocument.ToString()));

                        table . RowGroups [ 0 ] . Rows [ 0 ] . Cells . Add ( new TableCell ( padding ) );
                        table . RowGroups [ 0 ] . Rows [ 0 ] . Cells . Add ( new TableCell ( text ) );
                        FlowDocument flowDoc = new FlowDocument(table);

                        IDocumentPaginatorSource idocument = flowDoc as IDocumentPaginatorSource;
                        dialog . PrintDocument ( idocument . DocumentPaginator, "Printing New SQL Script..." );
                        // reset font colors to original colors
                        MainWindow . ScrollViewerBground = back;
                        MainWindow . ScrollViewerFground = fore;
                        sph . ExecResult . Background = Application . Current . FindResource ( "Green4" ) as SolidColorBrush;
                        sph . ExecResult . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                        sph . StatusText = "Document has been printed successfully.....";
                    }
                    else
                    {
                        // print textfile
                        // a standard text input of some sort
                        // convert it to a FlowDocument
                        Brush fore=MainWindow . ScrollViewerFground;
                        Brush back=MainWindow .ScrollViewerBground;
                        MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                        MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                        FlowDocument myFlowDocument=new();
                       myFlowDocument = processsprocs . CreatePrintFlowDoc ( myFlowDocument, documenttext, "" );

                        IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                        dialog . PrintDocument ( idocument . DocumentPaginator, "Printing New SQL Script..." );
                        // reset font colors to original colors
                        MainWindow . ScrollViewerBground = back;
                        MainWindow . ScrollViewerFground = fore;
                        sph . ExecResult . Background = Application . Current . FindResource ( "Green4" ) as SolidColorBrush;
                        sph . ExecResult . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                        sph . StatusText = "Document has been printed successfully.....";
                        sph . ExecResult . UpdateLayout ( );
                    }
                }
            }
        }

        public static bool GeneralPrintItem ( UIElement caller, string documenttext = "", string filename = "", FlowDocument flowDocument = null )
        {
            "" . Track ( 0 );
            // get window pointers
            SProcsSupport processsprocs = new();
            SProcsHandling sph = SProcsHandling.GetSProcsHandling();
            var dialog = new System.Windows.Controls.PrintDialog();
            string filesavename=defprintfilepath;
            dialog . PageRangeSelection = System . Windows . Controls . PageRangeSelection . AllPages;
            dialog . UserPageRangeEnabled = true;

            // Show save Print dialog box
            Nullable<Boolean> print = dialog.ShowDialog();

            if ( print == true )
            {
                // Pint the file 0xFFFFFFFF = White, 0xFF000000 = black
                if ( flowDocument != null && documenttext == "" && filename == "" )
                {
                    // print Flowdocument
                    Brush fore=MainWindow . ScrollViewerFground;
                    Brush back=MainWindow . ScrollViewerBground;
                    MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                    MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                    FlowDocument myFlowDocument=flowDocument;
                    myFlowDocument . Blocks . Clear ( );
                    sph . FetchStoredProcedureCode ( sph . SProcsListbox . SelectedItem . ToString ( ), ref documenttext );
                    myFlowDocument = processsprocs . CreatePrintFlowDoc ( myFlowDocument, documenttext, "" );
                    myFlowDocument . Background = MainWindow . ScrollViewerBground;
                    myFlowDocument . Foreground = MainWindow . ScrollViewerFground;
                    IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing Flow Document..." );
                    MainWindow . ScrollViewerFground = fore; ;
                    MainWindow . ScrollViewerBground = back;
                    return true;
                }
                else if ( documenttext != "" && flowDocument == null && filename == "" )
                {
                    // print Text data from wherever
                    Brush fore=MainWindow . ScrollViewerFground;
                    Brush back=MainWindow .ScrollViewerBground;
                    MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                    MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                    FlowDocument myFlowDocument=flowDocument;
                    myFlowDocument = processsprocs . CreateBoldString (sph,  myFlowDocument, documenttext, "" );
                    Table table = new();
                    // Create and add a couple of columns.
                    table . Columns . Add ( new TableColumn ( ) );
                    table . Columns . Add ( new TableColumn ( ) );
                    // Create and add a row group and a couple of rows.
                    table . RowGroups . Add ( new TableRowGroup ( ) );
                    table . RowGroups [ 0 ] . Rows . Add ( new TableRow ( ) );
                    table . RowGroups [ 0 ] . Rows . Add ( new TableRow ( ) );

                    // Create four cells initialized with the sample text paragraph.
                    Paragraph padding = new Paragraph(new Run("       "));
                    Paragraph text= new Paragraph(new Run(myFlowDocument.ToString()));

                    table . RowGroups [ 0 ] . Rows [ 0 ] . Cells . Add ( new TableCell ( padding ) );
                    table . RowGroups [ 0 ] . Rows [ 0 ] . Cells . Add ( new TableCell ( text ) );
                    FlowDocument flowDoc = new FlowDocument(table);

                    IDocumentPaginatorSource idocument = flowDoc as IDocumentPaginatorSource;
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing New SQL Script..." );
                    // reset font colors to original colors
                    MainWindow . ScrollViewerBground = back;
                    MainWindow . ScrollViewerFground = fore;
                    return true;
                }
                else if ( filename != "" && documenttext == "" && flowDocument == null )
                {
                    // print using filename received 
                    if ( File . Exists ( filename ) == false )
                        return false;
                    documenttext = File . ReadAllText ( filename );
                    Brush fore=MainWindow . ScrollViewerFground;
                    Brush back=MainWindow .ScrollViewerBground;
                    MainWindow . ScrollViewerBground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
                    MainWindow . ScrollViewerFground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
                    FlowDocument myFlowDocument = new();
                    myFlowDocument = processsprocs . CreatePrintFlowDoc ( myFlowDocument, documenttext, "" );
                    myFlowDocument . Background = MainWindow . ScrollViewerBground;
                    myFlowDocument . Foreground = MainWindow . ScrollViewerFground;
                    IDocumentPaginatorSource idocument = myFlowDocument as IDocumentPaginatorSource;
                    //Actually print the data (preview is shown)
                    dialog . PrintDocument ( idocument . DocumentPaginator, "Printing Flow Document..." );
                    MainWindow . ScrollViewerFground = fore;
                    MainWindow . ScrollViewerBground = back;
                    // Remove temporary print file again
                    File . Delete ( filesavename );
                    "" . Track ( 1 );
                    return true;
                }
            }
            "" . Track ( 1 );
            return false;
        }

        public static StringBuilder ParseDbTableToCSV ( DataGrid collection, bool PrintData = true, bool ShowCsv = false )
        {
            // process the table then show the print dialog object and set options
            string output="";
            StringBuilder sb  = new();
            SProcsHandling sph = SProcsHandling . GetSProcsHandling ( );
            string fld="";
            // GenericClass gcc = new();
            //int x = 0;
            "" . Track ( 0 );

            foreach ( GenericClass item in collection . Items )
            {
                GenericClass gc = new();
                gc = item as GenericClass;
                // gcc = gc;
                if ( gc . field1 != null ) { fld = gc . field1 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field2 != null ) { fld = gc . field2 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field3 != null ) { fld = gc . field3 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field4 != null ) { fld = gc . field4 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field5 != null ) { fld = gc . field5 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field6 != null ) { fld = gc . field6 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field7 != null ) { fld = gc . field7 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field8 != null ) { fld = gc . field8 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field9 != null ) { fld = gc . field9 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field10 != null ) { fld = gc . field10 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field11 != null ) { fld = gc . field11 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field12 != null ) { fld = gc . field12 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field13 != null ) { fld = gc . field13 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field14 != null ) { fld = gc . field14 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field15 != null ) { fld = gc . field15 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field16 != null ) { fld = gc . field16 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field17 != null ) { fld = gc . field17 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field18 != null ) { fld = gc . field18 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( gc . field19 != null ) { fld = gc . field19 . ToString ( ) . Trim ( ); output += VerifyValidRowEntry ( fld ); }
                if ( output . Length > 0 )
                    sb . Append ( $"{output . Substring ( 0, output . Length - 1 )}\n" );
                output = "";
            }
            "" . Track ( 1 );
            return sb;
        }

        public static string VerifyValidRowEntry ( string item )
        {
            if ( item . GetType ( ) != typeof ( string ) )
                return ", ";
            else if ( item != "" )
            {
                // remove prompt lines I add to results datagrids
                if ( item . ToUpper ( ) . Contains ( "HIT ESCAPE" ) || item . ToUpper ( ) . Contains ( "RIGHT CLICK" ) )
                    return "";
                else
                    return item . Trim ( ) + ", ";
            }
            else
                return "";
        }


    }
}