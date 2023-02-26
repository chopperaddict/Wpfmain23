using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows;
using System . Security . Cryptography . Pkcs;
using System . Reflection . PortableExecutable;
using System . Diagnostics;
using System . Reflection . Metadata;
using System . Security . Cryptography;
using Wpfmain . UtilWindows;

namespace Wpfmain
{
    public class SphMenuControl
    {
        // Neat constants
        private const Visibility COLLAPSED = Visibility.Collapsed;
        private const Visibility VISIBLE = Visibility.Visible;

        SProcsHandling sph { get; set; }
        ScriptViewerWin svw { get; set; }
        private bool AllowSplitterReset { get; set; }
        public SphMenuControl ( SProcsHandling sp, bool AllowSplitter )
        {
            sph = sp;
            AllowSplitterReset = AllowSplitter;
        }
        public SphMenuControl ( ScriptViewerWin svwin )
        {
            svw = svwin;
            sph = SProcsHandling . GetSProcsHandling ( );
        }

        public ContextMenu SetCurrentEntryMenuHeight ( ContextMenu cm,
            SolidColorBrush bkgrnd,
            SolidColorBrush fgrnd,
            string arg, int height = 0, int boldweight = 1, int padding = -1 )
        {
            var v =  cm . Items ;
            // WE only do ONE iteration here, on the matching item
            for ( int x = 0 ; x < cm . Items . Count ; x++ )
            {
                MenuItem mi = cm . Items [x]  as MenuItem;
                if ( mi == null || mi . Header == null || mi . Header == "" ) continue;
                var vvv = mi.Header.ToString();
                if ( vvv == arg )
                {
                    if ( height == 0 )
                    {
                        mi . Height = 40;
                    }
                    else
                        mi . Height = 35;

                    Thickness th = new();
                    th . Left = 0;
                    if ( padding != -1 )
                        th . Top = padding;
                    else if ( boldweight == 2 )
                        th . Top = 8;
                    else
                        th . Top = 3; ;

                    mi . Padding = th;
                    if ( boldweight == 1 )
                        mi . FontWeight = Utils2 . GetfontWeight ( "DemiBold" );
                    else if ( boldweight == 2 )
                        mi . FontWeight = Utils2 . GetfontWeight ( "Bold" );

                    if ( bkgrnd != null )
                        mi . Background = bkgrnd;
                    if ( fgrnd != null )
                        mi . Foreground = fgrnd;
                    break;
                }
            }
            return cm;
        }

        #region ALL Menu Control methods
        /// <summary>
        /// Handles the presentation of the Maiiin Editor Menu intelligently depenfing
        /// on what type of file we have loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Script Editor menu control

        public void ScriptMenuFileOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . CloseWin, "Close Editor" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . Openfile, "Open Saved File " );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . Openanyfile, "Open File (Any)" );
            if ( svw . FileSuffix == "SCRIPT" || svw . FileSuffix == "" )
                sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . SaveScriptFile, "Save Script to Sql Server" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . SaveDataFile, "Save File..." );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . SaveAsDataFile, "Save File As ..." );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuFileOpening, svw . About, "About Editor" );
        }
        public void ScriptMenuEditOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuEditOpening, svw . Replace3Tabs, "Reset Tabs to 3 spaces" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuEditOpening, svw . Replace4Tabs, "Reset Tabs to 4 spaces" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuEditOpening, svw . Copy2Clipboard, "Copy contents to Clipboard" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuEditOpening, svw . PastefromClipboard, "Add Clipboard data to contents" );
        }
        public void ScriptMenuOptsOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuOptsOpening, svw . IncreaseFontsize, "Increase Font Size" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuOptsOpening, svw . decreaseFontsize, "Decrease Font Size" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuOptsOpening, svw . PrintNewScript, "Print contents of Editor" );
        }
        public void ScriptMenuHelpOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuHelpOpening, svw . OverallInfo, "Overview of  this Editor" );
            sph . Addmainmenu ( this . svw . Mainmenu, this . svw . ScriptMenuHelpOpening, svw . UsingSprocs, "Using the Editor" );
        }

        #endregion Script Editor menu control

        /// <summary>
        /// Handles the presentation of  the Context Menu intelligently depenfing
        /// on what panels are currenty visdible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // Open ContextMenu - conditionally

        #region CONTEXTMENU control methods
        //*****************************************//
        // Handle the s. Procedures Context menu options
        //*****************************************//
        public void ShowSpContextMenu ( object sender, MouseButtonEventArgs e, ContextMenu cm, string menuname )
        {
            // Open ContextMenu - conditionally

            #region clean - remove all

            // collapse menu items
            ContextMenu mi = sph.FindResource ( "SProcsContextmenu" ) as ContextMenu;
            // Hide  all entries initially
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            mi = sph . FindResource ( "DgridContextmenu" ) as ContextMenu;
            // Hide  all entries initially
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }

            #endregion clean - remove all

            // now  do  the direct SProcs menu items
            sph . AddMenuEntry ( menuname, "showdgrid", "           Switch to SQL Table view" );
            sph . AddMenuEntry ( menuname, "spprintscript", "Print current Script (from Script Viewer)" );
            sph . AddMenuEntry ( menuname, "spdeletescript", "Delete currently selected Script ", 1, "Red3", "White0" );
            sph . AddMenuEntry ( menuname, "spshowscriptexternally", "Show current script in Editor Window" );
            SetCurrentEntryMenuHeight ( cm,
                ( SolidColorBrush ) Application . Current . FindResource ( "Red7" ),
                //( SolidColorBrush ) Application . Current . FindResource ( "White0" ),
                null, "Switch to SQL Table view", height: 0, boldweight: 2 );

            if ( sph . EditFileTextbox . Visibility == VISIBLE )
            {
                // Handle  text from file viewer
                sph . AddMenuEntry ( menuname, "spclosefilepanel", "Close File Viewer" );
                sph . AddMenuEntry ( menuname, "spprintfilepanel", "Print contents of File viewer" );
            }

            #region Results            // Handle Results FIRST as it will always be on top if open

            if ( sph . ResultsContainerDatagrid . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "spsaveresultstofile", "Save Datagrid to a CSV file", 1, "Green4", "White0" );
                sph . AddMenuEntry ( menuname, "spprintesultspanel", "Print Datagrid Contents" );
                sph . AddMenuEntry ( menuname, "spcloseresultpanel", "CLOSE Datagrid Results Panel" );
                SetCurrentEntryMenuHeight ( cm,
                ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                ( SolidColorBrush ) Application . Current . FindResource ( "Yellow0" ),
                "Close Execution Execution Results Datagrid", padding: 10 );
            }
            else if ( sph . ResultsListBox . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "spprintesultspanel", "Print contents of Results Listbox" );

                sph . AddMenuEntry ( menuname, "spcloseresultpanel", "Close Execution Results Listbox" );
                SetCurrentEntryMenuHeight ( cm,
                ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                ( SolidColorBrush ) Application . Current . FindResource ( "Yellow0" ),
                "Close Execution Execution Results ListBox", padding: 10 );
                sph . AddMenuEntry ( menuname, "spsaveresultstofile", "Save results from Listbox to a file", 1, "Green4", "White0" );
            }
            else if ( sph . ResultsTextbox . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "spcloseresultpanel", "Close Execution Results Text box" );
                SetCurrentEntryMenuHeight ( cm,
                ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                ( SolidColorBrush ) Application . Current . FindResource ( "Yellow0" ),
                "Close Execution Execution Results TextBox", padding: 10 );
                sph . AddMenuEntry ( menuname, "spsaverresultsTofile", "Save current Text Results to file" );
                sph . AddMenuEntry ( menuname, "spprintesultspanel", "Print current Text results content" );
            }
            #endregion Results            // Handle Results FIRST as it will always be on top if open


            #region scripts
            // Save listbox ?
            if ( sph . ResultsContainerListbox . Visibility == VISIBLE )
                sph . AddMenuEntry ( menuname, "spsaverresultslisttTofile", "Save current Results to file" );


            #endregion scripts

            #region DEFAULT current display

            // Handle (permanent) Topmost setting
            if ( sph . Topmost == true )
                sph . AddMenuEntry ( menuname, "nontopmost", "Remove Topmost status from Window " );
            else
                sph . AddMenuEntry ( menuname, "topmost", "Set Window  status to Topmost" );
            if ( AllowSplitterReset )
                sph . AddMenuEntry ( menuname, "spdisableautosplitter", "Stop Splitter bar resetting it's position" );
            else
                sph . AddMenuEntry ( menuname, "spsetautosplittersition", "Allow splitter bar to reset it's position" );

            //if ( sph . Topmost == false )
            //    sph . AddMenuEntry ( menuname, "spnontopmost", "Set Window visibility to TopMost" );
            //else
            //    sph . AddMenuEntry ( menuname, "sptopmost", "Remove Window TopMost status" );

            #endregion DEFAULT current display  

            cm . Visibility = VISIBLE;
            cm . IsOpen = true;
            cm . UpdateLayout ( );

        }

        //*****************************************//
        // Handle the Datagrid Context menu options
        //*****************************************//
        #region SQL table context menu (dg......)

        public void ShowDgridContextMenu ( object sender, MouseButtonEventArgs e, ContextMenu cm2, string menuname )
        {
            int splittercount=0;
            // Open ContextMenu - conditionally

            #region clean - remove all

            ContextMenu mi = sph.FindResource ( "SProcsContextmenu" ) as ContextMenu;
            // Hide  all entries initially
            foreach ( MenuItem item in mi . Items )
                item . Visibility = COLLAPSED;
            mi = sph . FindResource ( "DgridContextmenu" ) as ContextMenu;
            // Hide  all entries initially
            foreach ( MenuItem item in mi . Items )
                item . Visibility = COLLAPSED;
            if ( sph . ShowSp )
                return;

            #endregion clean - remove all


            sph . AddMenuEntry ( menuname, "dgshowsprocs", "         Switch to SQL Table Viewer", 1, "Red4", "White0" );
            //SetCurrentEntryMenuHeight ( cm2,
            //    ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
            //    ( SolidColorBrush ) Application . Current . FindResource ( "White0" ),
            //    "Switch to SQL Table view", height: 0, boldweight: 2 );

            #region Results panels

            if ( sph . ResultsContainerListbox . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "dgsaveresultstofile", "Save results from Listbox to a text file", 1, "Green4", "White0" );
                sph . AddMenuEntry ( menuname, "dgcloseresultspanel", "      Close Listbox Results Panel", 1, "Red4", "Yellow0" );
                //SetCurrentEntryMenuHeight ( cm2,
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Yellow0" ),
                //    "      Close Execution Results Panel", padding: 10 );
            }
            else if ( sph . ResultsContainerDatagrid . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "dgsaveresultstofile", "Save Datagrid content to CSV file", 1, "Green4", "White0" );
                sph . AddMenuEntry ( menuname, "dgcloseresultspanel", "Close DataGrid Results Panel", 1, "Red4", "Yellow0" );
                //SetCurrentEntryMenuHeight ( cm2,
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Yellow0" ),
                //    "Close Execution Results Panel", padding: 10 );
            }
            else if ( sph . ResultsContainerTextblock . Visibility == VISIBLE )
            {
                sph . AddMenuEntry ( menuname, "dgsaveresultstofile", "Save results from Text to a text file", 1, "Green4", "White0" );
                sph . AddMenuEntry ( menuname, "dgcloseresultspanel", "      Close Text Results Panel", 1, "Red4", "Yellow0" );
            }

            #endregion Results panels

            #region scripts

            if ( sph . EditFileContainerGrid . Visibility == VISIBLE && sph . EditFileTextbox . Text != "" )
            {
                //new Script panel open in window
                sph . AddMenuEntry ( menuname, "dgprintscript", "Print Text from Text Viewer" );
            }

            #endregion scripts


            //By here ALL other options are closed !!!!!

            #region generic items

            if ( AllowSplitterReset == true )
                sph . AddMenuEntry ( menuname, "dgDisableAutoSplitter", "Stop Splitter bar resetting it's position" );
            else
                sph . AddMenuEntry ( menuname, "dgSetAutoSplitterPosition", "Allow splitter bar to reset it's position" );

            // Datagrid is VISIBLE
            sph . AddMenuEntry ( menuname, "dgshowfulldatagrid", "Show Datagrid at full height" );
            if ( sph . EditPanel . Visibility == VISIBLE )
            {
                if ( sph . HaveSecondaryPanelsVisible ( ) )
                    sph . AddMenuEntry ( menuname, "dghideeditpanel", "Hide Table Edit Panel" );
                sph . AddMenuEntry ( menuname, "dgseteditpanelheight", "Fit Edit panel to screen" );
            }
            else if ( sph . EditPanel . Visibility == VISIBLE && sph . RowHeight0 < 50 )
            {
                // Edit panel is NOT VISIBLE as  grid is at full height
                sph . AddMenuEntry ( menuname, "dgseteditpanelheight", "Show/Fit Data Edit panel in Window" );
            }
            else if ( sph . EditPanel . Visibility == Visibility . Collapsed )
            {
                sph . AddMenuEntry ( menuname, "dgseteditpanelheight", "Show/Fit Edit panel to Window" );
                splittercount++;
            }

            sph . AddMenuEntry ( menuname, "dgfullwinheight", "Toggle Window ^/v Max/Normal" );
            sph . AddMenuEntry ( menuname, "dgsmallwin", "Show Small Window" );
            sph . AddMenuEntry ( menuname, "dgmediumwin", "Show Medium Window" );
            sph . AddMenuEntry ( menuname, "dglargewin", "Show Large Window" );

            // Handle (permanent) Topmost setting
            if ( sph . Topmost == false )
                sph . AddMenuEntry ( menuname, "dgnontopmost", "Set Window status to TopMost" );
            else
                sph . AddMenuEntry ( menuname, "dgtopmost", "Remove TopMost status from Window" );

            #endregion generic items

            cm2 . Visibility = VISIBLE;
            cm2 . IsOpen = true;
            cm2 . UpdateLayout ( );
        }

        #endregion SQL table context menu (dg......)


        #endregion CONTEXTMENU control methods

        #region Main Window menu handling methods
        public void ViewsMenuOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            if ( sph . SPDatagrid . Visibility == VISIBLE )
            {
                sph . ShowDg = true;
                sph . ShowSp = false;
            }
            else
            {
                sph . ShowDg = false;
                sph . ShowSp = true;
            }
            if ( sph . ShowDg )
            {
                //  Overall SQL Data view OPEN
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ShowSpmenu, "Switch to S.Procedures View" );
                sph . CurrentMenuitem = sph . ShowSpmenu;
            }
            else if ( sph . ShowSp )
            {
                //  Overall SProcs view OPEN
                sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ShowSpmenu );
                sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ShowDgmenu );
                sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ShowDgmenu, "Switch to SQL Datagrid View" );
                sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ReloadSProcs, "Reload Stored Procs List" );
            }

            // Data file editor
            if ( sph . EditFileTextbox . Visibility == VISIBLE )
            {
                // OPEN
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . CloseSavedData, "Close Data Editor (Discards data)" );
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . SaveDataFile, "Save Data file and close panel" );
            }
            else
            {
                // CLOSED
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . viewsmenu, sph . ShowSavedData, "Open Saved Data file" );
            }
        }

        public void ScriptsMenuOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . scriptsmenu, sph . EditExistingSqlScript );
            sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . scriptsmenu, sph . CreateNewScript );
            sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . scriptsmenu, sph . CreateNewScript, "Open NEW SQL Script in Editor" );
            sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . scriptsmenu, sph . EditExistingSqlScript, "Show current SQL Script in Editor" );
        }

        public void OptionsMenuOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            if ( sph . ShowSp )
            {
                sph . MainWinmenu = sph . Addmainmenu ( sph . MainWinmenu, sph . optsmenu, sph . MainPrintScript, "Print currently selected S.P script" );
            }
            else if ( sph . ShowDg )
            {
                //Datagrid showing 
                if ( sph . SPDatagrid . Visibility == VISIBLE )
                {
                    //Datagrid is VISIBLE
                    sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . optsmenu, sph . ShowSPSchema, "Show schema of currently selected Table" );
                    sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . optsmenu, sph . ShowGridFullheight, "Expand Datagrid to fit window" );
                    sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . optsmenu, sph . UpdateRecord );

                    if ( sph . RowHeight2 < sph . DefEditpanelHeight )
                    {
                        sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . optsmenu, sph . ResetGridheight, "Reset Grid height to show Edit Panel " );
                    }
                    else
                        sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . optsmenu, sph . ShowGridFullheight, "Show Tables Panel at Full Height" );
                }
            }
        }

        public void HelpMenuOpening ( object sender, RoutedEventArgs e, string menuname )
        {
            MenuItem mi = sender as MenuItem;
            foreach ( MenuItem item in mi . Items )
            {
                item . Visibility = COLLAPSED;
            }
            if ( sph . SPDatagrid . Visibility == VISIBLE )
                if ( sph . SPDatagrid . Visibility == VISIBLE )
                {
                    sph . ShowDg = true;
                    sph . ShowSp = false;
                }
                else
                {
                    sph . ShowDg = false;
                    sph . ShowSp = true;
                }
            if ( sph . ShowDg )
            {
                // selective options
                //if ( sph . SPDatagrid . ActualHeight != sph . SPFullDataContainerGrid . ActualHeight )
                //{
                //    sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . Helpmenu, sph . ShowGridFullheight, "Show Tables Panel at Full Height" );
                //}
                //sph . CurrentMenuitem = sph . ShowSpmenu;
            }
            else if ( sph . ShowSp )
            {
                //sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . Helpmenu, sph . ShowGridFullheight );
                //sph . MainWinmenu = sph . Removemainmenu ( this . sph . MainWinmenu, this . sph . Helpmenu, sph . UpdateRecord );
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . Helpmenu, sph . OverallInfo, "Overview of this windows functionality" );
                sph . MainWinmenu = sph . Addmainmenu ( this . sph . MainWinmenu, this . sph . Helpmenu, sph . UsingSqlTable, "Using the SQL Table Access System" );
                //                sph . CurrentMenuitem = sph . ShowDgmenu;
            }
        }

        #endregion Main menu handling methods


        #endregion  ALL Menu Control methods

    }
}

/*
 * 
             // not used, but useful
            {
                //SetCurrentEntryMenuHeight ( cm,
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Green8" ),
                //    ( SolidColorBrush ) Application . Current . FindResource ( "Red4" ),
                //    "Close Execution Results Panel", height: 30 );
            }
* */