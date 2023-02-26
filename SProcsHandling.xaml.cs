#region usings
using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Printing;
using System . Reflection;
using System . Runtime . ExceptionServices;
using System . Runtime . InteropServices;
using System . Text;
using System . Text . RegularExpressions;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Markup . Localizer;
using System . Windows . Media;
using System . Windows . Media . Animation;
using System . Windows . Shapes;

using Dapper;

using Microsoft . Data . SqlClient;
using Microsoft . VisualBasic;
using Microsoft . Win32;

using Newtonsoft . Json . Linq;

using SprocsProcessing;

using SqlMethods;

using ViewModels;

using Wpfmain . Dapper;
using Wpfmain . Models;
using Wpfmain . UtilWindows;



//using static System . Net . Mime . MediaTypeNames;

//#################################//
#endregion usings
//#################################//

namespace Wpfmain
{
    public partial class SProcsHandling : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Window that hosts datagrid and listboxes used to help process S.Procedures
        /// and manipulalte sqltables in a totally generic manner
        /// </summary>

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

        private string _statusText = "Original text";
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                if ( value != _statusText )
                {
                    _statusText = value;
                    Debug . WriteLine ( $"*** Status = {StatusText}" );
                    OnPropertyChanged ( "StatusText" );
                    IsUpdated ( null, null );
                }
            }
        }
        //#################################//
        #region ALL DECLARATIONS
        //++++++++++++++++++++++++++++++++//

        #region MAJOR SETUP VARIABLES
        private bool SHOWGRIDFIRST = false;

        public FlowDocument flowDoc = new();
        ObservableCollection<GenericClass> DynamicCollection= new();

        // PRIVATE Declarations
        public struct comboclrs
        {
            public string name;
            public SolidColorBrush Bground;
            public SolidColorBrush Fground;
        }
        public static comboclrs cclrs = new();
        public  List<comboclrs> CbColorsList = new ();
        public SphMenuControl sphMenus { get; set; }
        public string SpToUse { get; set; }

        private List<string> genericlist = new();
        private Stopwatch sw = new();
        public bool ShowSp { get; set; } = false;
        public bool ShowDg { get; set; } = false;
        public bool ShowSc { get; set; } = false;  // Script creation panel visibility
        public bool ShowRt { get; set; } = false;  // Results panel visibility
        public List<string> OriginalRecordData { get; set; } = new ( );
        public List<string> UpdatedRecordData { get; set; } = new ( );

        public List<string> DuplicateRecordData { get; set; } = new ( );
        static public bool DragActive { get; set; } = false;
        public static Dictionary<string, SolidColorBrush> ColorsDictionary { get; set; } = new Dictionary<string, SolidColorBrush> ( );
        public static Dictionary<string, SolidColorBrush>ColorsDictionaryOut = new Dictionary<string, SolidColorBrush>();

        public GenericClass gclass = new();
        public GenericClass newgclass = new();
        public bool ScriptEditorOpen { get; set; } = false;


        #endregion MAJOR SETUP VARIABLES

        //#################################//
        #region general full Declarations
        //#################################//
        public bool bdirty { get; set; } = false;
        public int reccount { get; set; } = 0;
        public int currselection { get; set; } = 0;
        public static SProcsHandling sphandling { get; set; }
        public static SprocsHandlingViewmodel sphViewmodel { get; set; }
        public double Dragdistance { get; set; } = 0;

        #endregion general full Declarations

        private bool AllowSplitterReset { get; set; } = true;
        private bool ShowFullGrid { get; set; } = true;
        private int colcount { get; set; } = 0;

        #region  ReSizing  variables
        private double PreEditsplitterheight1 = 0;
        private double PreEditsplitterheight2 = 0;
        private string WinSize = "MED";
        private double DefSmallHeight = 40;
        private double DefMediumHeight = 125;
        private double DefLargeHeight = 275;
        public static int MAXARGSIZE = 6;
        public double TotalWinHeight { get; set; }
        public double TotalDataArea { get; set; }
        public double UpperDataArea { get; set; }
        public double LowerDataArea { get; set; }
        public double splitht { get; set; } = 25.00;
        public double UsablePanelsHeight { get; set; }
        public double UnusedPanelsHeight { get; set; }
        public double SPDatagridHeight { get; set; }
        public bool IsGridFullHeight { get; set; } = false;
        public bool IsEditFullHeight { get; set; } = false;
        public double SpUnusedSpace { get; set; } = 240;
        public double ResultsTextHeight { get; set; } = 1040;
        public int CurrentFontsize { get; set; } = 16;
        public SolidColorBrush ColorCb { get; set; }

        //Container for the splitter height data
        //        public List<RowDefinition> rowlist { get; set; } = new ( );

        //++++++++++++++++++++++++++++++++//
        #endregion resizing

        #region FULL PROPERTY declarations
        //#################################//

        // FULL PROPERTIES
        private double _rowHeights0;
        public double RowHeight0
        {
            get { return _rowHeights0; }
            set { _rowHeights0 = value; OnPropertyChanged ( nameof ( RowHeight0 ) ); }
        }
        private double _rowHeights1;
        public double RowHeight1
        {
            get { return _rowHeights1; }
            set { _rowHeights1 = value; OnPropertyChanged ( nameof ( RowHeight1 ) ); }
        }
        private double _rowHeights2;
        public double RowHeight2
        {
            get { return _rowHeights2; }
            set { _rowHeights2 = value; OnPropertyChanged ( nameof ( RowHeight2 ) ); }
        }
        private double _rowHeights4;
        public double RowHeight4
        {
            get { return _rowHeights4; }
            set { _rowHeights4 = value; OnPropertyChanged ( nameof ( RowHeight4 ) ); }
        }
        private ObservableCollection<GenericClass> _sqlTable;
        public ObservableCollection<GenericClass> SqlTable
        {
            get { return _sqlTable; }
            set { _sqlTable = value; OnPropertyChanged ( nameof ( SqlTable ) ); }
        }
        private bool _WinResizing;
        public bool WinResizing
        {
            get { return _WinResizing; }
            set { _WinResizing = value; OnPropertyChanged ( nameof ( WinResizing ) ); }
        }

        private double _DefEditpanelHeight;
        public double DefEditpanelHeight
        {
            get { return _DefEditpanelHeight; }
            set { _DefEditpanelHeight = value; OnPropertyChanged ( nameof ( DefEditpanelHeight ) ); }
        }

        //#################################//
        #endregion FULL PROPERTY declarations

        #region general public static declarations
        //#################################//

        // PUBLIC Declarations
        static public ObservableCollection<GenericClass> Sprocs = new();
        // Table structure information for currently open SQL table
        public DataGridLayout dglayout = new DataGridLayout();
        public List<DataGridLayout> dglayoutlist = new();
        public List<int> VarCharLength = new();

        // static declarations
        static public GenericClass genclass = new();
        static public SProcsHandling sph { get; set; }
        static public IEnumerable<dynamic> IeSprocs { get; set; }
        static public string ConString = "";
        static public string SqlCommand { get; set; }
        static public string DbDomain { get; set; } = "IAN1";
        static public string CurrentDbName { get; set; } = "BankAccount";
        static public List<SolidColorBrush> Brushcolors = new();
        static public FlowDocument myFlowDocument = new FlowDocument();
        static public string[] arguments = new string[DEFAULTARGSSIZE];
        static public Dictionary<string, string> ConnectionStringsDict = new Dictionary<string, string>();
        static public Dictionary<string, int> SpNamesExecIndex= new();



        //++++++++++++++++++++++++++++++++//
        #endregion general public static declarations

        #region general public declarations
        //#################################//

        public static List<string> ExecCommands = new();
        public List<SolidColorBrush> ColorComboForeground= new();
        public const int DEFAULTARGSSIZE = 6;
        private const Visibility COLLAPSED = Visibility . Collapsed;
        private const Visibility VISIBLE = Visibility . Visible;
        private bool ISGRIDVISIBLE;

        #endregion general public declarations

        #region S.Procs Properties declarations
        public static SProcsHandling spviewer { get; set; }
        public SProcsSupport processsprocs { get; set; } = new SProcsSupport ( );
        public int CurrentSelectionIndex { get; set; } = -1;
        public string CurrentSelectionId { get; set; } = "-1";
        public GenericClass CurrentGenclass { get; set; } = new ( );
        public bool LeftMousePressed { get; set; } = false;
        //public Brush MainWindow . ScrollViewerBground { get; set; }
        //public Brush ScrollViewerFontSize { get; set; }
        public string Searchtext { get; set; } = "ARG";
        public string Searchterm { get; set; }
        public bool CloseArgsViewerOnPaste { get; set; } = false;
        public bool ShowTypesInArgsViewer { get; set; } = true;
        public bool ShowParseDetails { get; set; } = false;
        public bool KeepTypes { get; set; } = false;
        public bool IsLoading { get; set; } = true;
        public bool TableReloading { get; set; } = false;
        public bool LEFTMOUSEDOWN { get; set; } = false;

        #region full propertes


        //store for the full SP text
        private string spTextBuffer;
        // Text in create new Sproc editor
        private string _NewSprocText;
        // store  for SP arguments alone
        private string spArgstext;
        //public double ScrollViewerFontSize
        //{
        //    get { return scrollViewerFontSize; }
        //    set { scrollViewerFontSize = value; OnPropertyChanged ( nameof ( ScrollViewerFontSize ) ); }
        //}

        public string SpTextBuffer
        {
            get { return spTextBuffer; }
            set { spTextBuffer = value; OnPropertyChanged ( nameof ( SpTextBuffer ) ); }
        }

        public string SpArgsText
        {
            get { return spArgstext; }
            set { spArgstext = value; OnPropertyChanged ( nameof ( spArgstext ) ); }
        }

        public string NewSprocText
        {
            get { return _NewSprocText; }
            set { _NewSprocText = value; OnPropertyChanged ( nameof ( NewSprocText ) ); }
        }

        #endregion full propertes

        #endregion S.Procs Properties declarations

        #region Splitter properties

        private bool WinLoaded { get; set; } = false;
        private double Splitterlastpos { get; set; }
        new public bool MouseRightButtonDown { get; set; } = false;
        public Dictionary<string, string> cmContents { get; set; } = new ( );
        public MenuItem CurrentMenuitem { get; set; }


        #endregion Splitter properties

        #region attached proerties
        public static double GetScrollSpeed ( DependencyObject obj )
        {
            return ( double ) obj . GetValue ( ScrollSpeedProperty );
        }

        public static void SetScrollSpeed ( DependencyObject obj, double value )
        {
            obj . SetValue ( ScrollSpeedProperty, value );
        }

        public static readonly DependencyProperty ScrollSpeedProperty =
            DependencyProperty.RegisterAttached("ScrollSpeed",typeof(double),typeof(SProcsHandling),
                new FrameworkPropertyMetadata(1.0,FrameworkPropertyMetadataOptions.Inherits & FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnScrollSpeedChanged)));

        #endregion attached proerties

        private static void OnScrollSpeedChanged ( DependencyObject o, DependencyPropertyChangedEventArgs e )
        {
        }

        #region  DP's

        #endregion  DP's

        #endregion ALL DECLARATIONS

        #region Initialization
        static public  bool startup = true;
        // CONSTRUCTOR
        public SProcsHandling ( bool bl )
        {
        }
        public static SProcsHandling GetSProcsHandling ( )
        {
            return sphandling;
        }

        public SProcsHandling ( )
        {
            int colcount = 0;
            "" . Track ( 0 );
            // how to init multi row array
            //var xse =  new string [150, 2, 2 ];
            Mouse . OverrideCursor = Cursors . Wait;
            InitializeComponent ( );
            this . DataContext = this;   // ?????
            SprocsHandlingViewmodel sphViewmodel = new SprocsHandlingViewmodel ();
            sphMenus = new SphMenuControl ( this, AllowSplitterReset );
            sph = this;
            sphandling = this;
            spviewer = this;
            MainWindow . ScrollViewerFontSize = 15.00;
            SqlTable = new ObservableCollection<GenericClass> ( );
            ISGRIDVISIBLE = SHOWGRIDFIRST;
            if ( ISGRIDVISIBLE )
            {
                ShowDg = true;
                ShowSp = false;
            }
            else
            {
                ShowDg = false;
                ShowSp = true;
            }
            ConString = SqlBasicSupport . SqlSupport . LoadConnectionStrings ( );
            MainWindow . ScrollViewerBground = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
            MainWindow . ScrollViewerFground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
            MainWindow . ScrollViewerHiliteColor = Application . Current . FindResource ( "Red4" ) as SolidColorBrush;

            SPDatagrid . ItemsSource = null;
            SPDatagrid . Items . Clear ( );
            int dbselect = 0;

            if ( ExecCommands . Count == 0 )
            {
                ExecCommands . Add ( "1. SP returning a Table as ObservableCollection" );
                ExecCommands . Add ( "2. SP returning a List<string>" );
                ExecCommands . Add ( "3. SP returning a String" );
                ExecCommands . Add ( "4. SP returning an INT value" );
                ExecCommands . Add ( "5. Execute Stored Procedure with return value" );
                ExecCommands . Add ( "6. Execute Stored Procedure without return value" );
                ExecCommands . Add ( " *** No Execution method information available ***" );
                ExecList . ItemsSource = ExecCommands;
            }

            LoadAllSprocs ( );            // Get IEnumerable<dynamic> collection of the data requested using S.Proc
                                          //IeSprocs = LoadAllSprocs ( ConString, CurrentDbName, "spGetStoredProcs" );
                                          //GenericClass gc = new();
                                          //// Now parse the data received into GenericClass records
                                          //Sprocs = new ObservableCollection<GenericClass> ( );
                                          //// Create list for any sql tble size info (dglayout)
                                          //Dictionary<string, object> dict = new();
                                          //List<string> list = new();
                                          //List<string> Fldnames = new();
                                          //ArrayList SprocsData = new ArrayList();
                                          //SpNamesExecIndex . Clear ( );
                                          //// Create an internal Obscollection of s.Procedure names entries
                                          //if ( IeSprocs != null )
                                          //{
                                          //    foreach ( var item in IeSprocs )
                                          //    {
                                          //        // get data from DapperRow as Dictionary{string>, object>
                                          //        dict = new ( );
                                          //        dict = DapperGeneric . ParseDapperRowGen ( item, dict, out colcount );
                                          //        // return a list of data as Fieldname=Data:"
                                          //        list = GetGenericListFromDictionary ( dict );
                                          //        // Convert List<string> into GenericClass Record with only one column
                                          //        genclass = ConvertListToSingleFieldGenericClass ( list, colcount, out Fldnames );
                                          //        // Add GenericClass to our public Observable collection
                                          //        Sprocs . Add ( genclass );
                                          //        // Add dict to our collection of these dicts (for later indexing)
                                          //        SpNamesExecIndex . Add ( genclass . field1 . ToString ( ), 0 );
                                          //        SProcsListbox . ItemsSource = null;
                                          //        SProcsListbox . Items . Clear ( );
                                          //        // Add sp name to an ArrayList 
                                          //        SprocsData = LoadListBoxData ( Sprocs );
                                          //    }
                                          //    SProcsListbox . ItemsSource = SprocsData;
                                          //    SProcsListbox . SelectedIndex = 0;
                                          //    SetExecutionIndex ( "", 0 );
                                          //}

            LoadColorsComboFromFile ( );
            // Create multi job task
            Task maintask = new  Task (async  ( ) =>
            {
                Debug . WriteLine ( "\nthread running load commands...." );
                this . Dispatcher . Invoke ( async ( ) =>
                {
                    await LoadFontSizes ( );
 //                   Debug . WriteLine ( "\nnRunning LoadSqlTableNames ( );...." );
 //                    dbselect = await LoadSqlTableNames ( );
                } );
                Debug . WriteLine ( "All threads ended...." );
            });     // close off child task
                    //// RUN THE WHOLE TASK combination
            maintask . Start ( );

            // dont need to do this here....
            //CreateNewSprocEditorTemplate ( );

            // Setup the buttons/Combos panel
            ResetOptionsAccessColors ( );
            Mouse . OverrideCursor = Cursors . Arrow;
            // No events  yet 12/2/23
            SetupEvents ( );
            "" . Track ( 1 );
            IsLoading = true;

            // Set default row heights
            RowHeight0 = 300.0;
            RowHeight1 = 20.0;
            RowHeight2 = 250.0;

            // this lets me monitor the height changes of  the RowDefinitions in real time - Yeahhhh !
            var heightDescriptor = DependencyPropertyDescriptor.FromProperty(RowDefinition.HeightProperty, typeof(ItemsControl));
            heightDescriptor . AddValueChanged ( SPFullDataContainerGrid . RowDefinitions [ 0 ], HeightChanged );
            // create  the SProcslistbox index list
            //            GridCombo . SelectedIndex = dbselect;
            Task task = Task . Run (  ( ) =>
            {
                this . Dispatcher . Invoke ( async ( ) =>
                {
                    await LoadSpExecutionIndexing ( );
                    SProcsListbox . SelectedIndex = 0;
                    SetExecutionIndex ( "", 0 );
                } );
            });
            maintask = new Task ( async ( ) =>
            {
                Debug . WriteLine ( "\nthread running load commands...." );
                this . Dispatcher . Invoke ( async ( ) =>
                {
                    dbselect = await LoadSqlTableNames ( );
                } );
            } );
            maintask . Start ( );

            PrintLayout  playout = PrintLayout . A4;
            //Set  the correct A4 document width for our default FlowDocument
            flowDoc . ColumnWidth = playout . Size . Width;
        }
        private void Window_Loaded ( object sender, RoutedEventArgs e )
        {
            double controlheight1 = 0, controlheight2 = 0;
            string Error = "";
            if ( IsLoading == false )
            {
                "IsLoading==false  " . Track ( 1 );
                return;
            }
            "" . Track ( 0 );
            DefEditpanelHeight = 350;
            RowHeight1 = 20.0;

            // FOUND IT - The unusedspace is the correct height to cover all the lower panes below the working area !!!!!
            //           SpUnusedSpace = ArgumentsContainerGrid .Height+ Bottompanel.Height;
            RowHeight4 = SProcsViewer . Height - SpUnusedSpace;
            RowHeight2 = DefEditpanelHeight;
            RowHeight0 = RowHeight4 - ( DefEditpanelHeight );

            //Splitter intial position - Gives  correct proportions
            /*
             *  The missing height has been found, it it made up of ALL the lower panels frm 
             *  the Blanker/Propmpt row down to the bottom (240 pixels is pretty close ot correct anyway)
             */
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight4 - ( DefEditpanelHeight + 25 ) );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( EditPanel . Height );
            PreEditsplitterheight1 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;

            Splitterlastpos = RowHeight0;
            RowHeight1 = 20;
            RowHeight2 = DefEditpanelHeight;

            LeftMousePressed = true;
            IsLoading = false;
            IsLoading = true;
            IsLoading = false;
            Mouse . OverrideCursor = Cursors . Arrow;
            SPDatagrid . SelectedIndex = 0;
            // hide all result panels
            ResultsContainerDatagrid . Visibility = COLLAPSED;
            ResultsContainerListbox . Visibility = COLLAPSED;
            ResultsContainerTextblock . Visibility = COLLAPSED;
            // The  correct reduction - finally.
            RowHeight4 = SPFullDataContainerGrid . Height - SpUnusedSpace;
            // set dgrid as  visible
            if ( SHOWGRIDFIRST )
            {
                //Startup with SQL Db tables controls  visible
                TextResult . Visibility = COLLAPSED;
                // Set flags up
                ShowDg = true;
                ShowSp = false;

                SProcsListbox . Visibility = COLLAPSED;
                SPInfopanelGrid . Visibility = COLLAPSED;
                ExecList . Visibility = COLLAPSED;

                SPDatagrid . Visibility = Visibility . Visible;
                EditPanel . Visibility = Visibility . Visible;

                shrink1 . Text = " Hide Edit";
                shrink2 . Text = "   Panel ";
                ShowEditpanel . Visibility = Visibility . Visible;

                RefreshDatagrid ( "BANKACCOUNT" );
                SPFullDataContainerGrid . UpdateLayout ( );
                SPDatagrid . UpdateLayout ( );

                CurrentMenuitem = ShowDgmenu;
                CurrentMenuitem = ShowSpmenu;
                show_DataGrid ( sender, null );
                ResetPanelControlSizes ( );
                SProcsListbox . SelectedIndex = 0;
                SetExecutionIndex ( "", 0 );
            }
            else
            {
                //Startup with S Procs controls  visible
                // Set flags up
                ShowDg = false;
                ShowSp = true;
                // hide the edit show button in info panels
                ShowEditpanel . Visibility = COLLAPSED;
                ResultsContainerListbox . Visibility = COLLAPSED;
                EditPanel . Visibility = COLLAPSED;
                Blanker . Visibility = COLLAPSED;
                SPInfopanelGrid . Visibility = Visibility . Visible;
                TextResult . Visibility = Visibility . Visible;
                show_Sprocs ( sender, null );
                ResetPanelControlSizes ( );
                SProcsListbox . Focus ( );
                SProcsListbox . SelectedIndex = 0;
                SetExecutionIndex ( "", 0 );
            }
            Splitter_DragCompleted ( sender, null );
            WinLoaded = true;
            SetWindowTitleBar ( );

            PrintLayout  playout = PrintLayout . A4;
            //Set  the correct A4 document width for our default FlowDocument
            flowDoc . ColumnWidth = playout . Size . Width;

            "" . Track ( 1 );
            this . Focus ( );
        }

        private void HeightChanged ( object sender, EventArgs e )
        {
            // KEEP THESE - Show splitter info (RowDefinitions) in real time 
            //Debug . WriteLine ( $"[0] : {SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height}" );
            //Debug . WriteLine ( $"[2] : {SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height}" );
        }
        private void HSplitter_PreviewQueryContinueDrag ( object sender, System . Windows . Controls . Primitives . DragDeltaEventArgs e )
        {
            Debug . WriteLine ( $"QueryDrag : {SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height}" );
        }

        public async Task LoadSpExecutionIndexing ( )
        {
            // Create clever list <string,int>of all sp's, with the exec method index to be used (if avalable)
            string sptext="";
            int execvalue=-1;
            int index=0;
            List<int> allints = new();
            foreach ( string item in SProcsListbox . Items )
            {
                string spname = item . ToString ( );
                this . FetchStoredProcedureCode ( spname, ref sptext );
                bool flagexists = sptext .Contains("Use Execution Option [");
                if ( flagexists )
                {
                    string outbuff= "";
                    string buff = sptext.Substring(25, 5);
                    for ( int x = 0 ; x < 5 ; x++ )
                    {
                        char ch = buff[x];
                        if ( Char . IsDigit ( ch ) == false )
                        {
                            if ( outbuff == "" )
                            {
                                allints . Add ( -1 );
                                break;
                            }
                            execvalue = Convert . ToInt32 ( outbuff );
                            allints . Add ( execvalue );
                            break;
                        }
                        outbuff += ch;
                    }
                }
                else
                    allints . Add ( -1 );
                index++;
            }
            int ndx = 0;
            Dictionary<string, int> tempdict = new();
            foreach ( var item2 in SpNamesExecIndex )
            {
                string str = item2 . Key;
                int result = -1;
                tempdict . Add ( str, allints [ ndx++ ] );
                SpNamesExecIndex . Remove ( item2 . Key, out result );
            }
            SpNamesExecIndex = tempdict;
            // We now have a dictionary with sp name in key and execution # in value field
        }
        private void SetGlobals ( )
        {
            GridLength gl1;
            "" . Track ( 0 );

            // Set Global variables
            TotalWinHeight = spviewer . ActualHeight;
            UnusedPanelsHeight = 292;
            splitht = 25;
            if ( SPFullDataContainerGrid . RowDefinitions [ 2 ] . ActualHeight == 1 )
                gl1 = SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height;
            LowerDataArea = SPFullDataContainerGrid . RowDefinitions [ 2 ] . ActualHeight;
            Debug . WriteLine ( $"SetGlobals : LowerDataArea = {LowerDataArea}" );
            UpperDataArea = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
            TotalDataArea = SPFullDataContainerGrid . ActualHeight;
            "" . Track ( 1 );
        }

        public double GetDoubleHeight ( object gname, out string error )
        {
            double dvalue = -1;
            error = "";
            Type type = gname.GetType();
            Debug . WriteLine ( $"type = {type}" );
            if ( gname . GetType ( ) == typeof ( double ) )
                dvalue = Convert . ToDouble ( gname );
            else if ( gname . GetType ( ) == typeof ( Grid ) )
            {
                Grid grid = new Grid();
                grid = gname as Grid;
                dvalue = grid . ActualHeight;
            }
            else
            {
                if ( gname . GetType ( ) == typeof ( DataGrid ) )
                {
                    // eg SPDatagrid
                    DataGrid dgrid = new DataGrid();
                    dgrid = gname as DataGrid;
                    dvalue = dgrid . Height;
                }
                else if ( gname . GetType ( ) == typeof ( FlowDocument ) )
                {
                    // eg TextResult
                    FlowDocumentScrollViewer fd = new();
                    fd = gname as FlowDocumentScrollViewer;
                    dvalue = fd . Height;
                }
                else if ( gname . GetType ( ) == typeof ( GridLength ) )
                {
                    // eg TextResult
                    GridLength glen = new();
                    glen = ( GridLength ) gname;
                    GridLength gflen = new GridLength(glen.Value, GridUnitType.Pixel);
                    dvalue = glen . Value;
                }
                else if ( dvalue == -1 )
                {
                    error = $"Failed to identify Control type received of {gname . GetType ( )}";
                    Debug . WriteLine ( $"{error}" );
                    Utils . DoErrorBeep ( );
                    return -1;

                }
            }
            return dvalue;
        }

        #endregion Initialization

        private void SetupEvents ( )
        {
        }

        #region utility methods
        //#################################//
        public ObservableCollection<GenericClass> LoadSqlData ( string tablename, bool LoadGrid = true )
        {
            List<Dictionary<string, string>> dict = new List<Dictionary<string, string>>();

            "" . Track ( 0 );
            CurrentDbName = tablename;

            SqlTable = LoadDbAsGenericData ( $"Select * from {tablename}",
                SqlTable,
                ref dict,
                "",
                DbDomain,
                ref dglayoutlist,
                true );

            return SqlTable;
        }

        public void LoadSqlDataIntoDatagrid ( ObservableCollection<GenericClass> SqlTable, DataGrid SPDatagrid, string tablename )
        {
            if ( SqlTable . Count > 0 )
                LoadTableIntoGrid ( SqlTable, SPDatagrid );
        }

        private ArrayList LoadListBoxData ( ObservableCollection<GenericClass> SprocsView )
        {
            ArrayList itemsList = new ArrayList();
            foreach ( var item in SprocsView )
            {
                itemsList . Add ( item . field1 . ToString ( ) );
            }
            return itemsList;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion utility methods

        #region SQL Fetch SProcs Data
        //#################################//

        static IEnumerable<dynamic> LoadAllSprocs ( string constring, string CurrentDbName, string SqlCommand )
        {
            if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == false )
            {
                // probably a stored procedure ?  							
                var IsSuccess = new DynamicParameters();
                IEnumerable<dynamic> reslt;
                Debug . WriteLine ( $"Running SP db.Query" );
                using ( IDbConnection db = new SqlConnection ( ConString ) )
                {
                    // Create our aguments using the Dynamic parameters provided by Dapper
                    var Params = new DynamicParameters();

                    //***************************************************************************************************************//
                    //WORKING  JUST FINE FOR OBSERVABLECOLLECTION<GENERICCLASS>
                    reslt = db . Query ( SqlCommand, Params, commandType: CommandType . StoredProcedure );
                }
                if ( reslt != null )
                {
                    return reslt;
                }
                else
                    return null;
            }
            else if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == true )
            {
                if ( SqlCommand == "" )
                    SqlCommand = $"select * from {CurrentDbName}";

                string Arguments = "";
                string OrderByClause = "";
                string WhereClause = "";
                List<string> strings = new List<string>();
                IEnumerable<dynamic> IeSprocs = DapperGeneric.ExecuteSPGenericClass<GenericClass>(
                   sph.SqlTable,
                   constring,
                   SqlCommand,
                   Arguments,
                   WhereClause,
                   OrderByClause,
                   out List<string> genericlist,
                   out string errormsg);

                if ( IeSprocs != null )
                {
                    return IeSprocs;
                }
                else
                    return null;
            }
            return null;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion SQL Fetch SProcs Data

        #region Support methods
        //#################################//

        public bool LoadShowMatchingSproc ( Window win, FlowDocumentScrollViewer flowdocsv, string spfilename, ref string sptext )
        {
            // Read full script of an SP into memory and display it inFlowdocscrollviewer received
            // This reads the SP into memory in sptext  and displays it in the SpResultsViewer Scrollviewer
            this . FetchStoredProcedureCode ( spfilename, ref sptext );
            if ( sptext == "" )
            {
                return false;
            }
            else
                spTextBuffer = sptext;     // store full sp text in window Property

            // This ensures that both widnows are updated independently
            // depending on which list triggers the reload of the SP.
            if ( win . Name == "SProcsViewer" )
            {
                /// It is Genericgrid that has triggered ths data load, so put the SP details into the ScrollDooc
                TextResult . Document = null;
                myFlowDocument = new FlowDocument ( );
                myFlowDocument . Blocks . Clear ( );
                myFlowDocument = processsprocs . CreateBoldString ( this, myFlowDocument, sptext, Searchtext );
                myFlowDocument . Background = MainWindow . ScrollViewerBground;
                myFlowDocument . Foreground = MainWindow . ScrollViewerFground;
                TextResult . Document = myFlowDocument;
                TextResult . Document . Blocks . FirstBlock .BringIntoView ( );
                TextResult . UpdateLayout ( );

            }
            return true;
        }
        public string FetchStoredProcedureCode ( string spName, ref string stringresult, bool HeaderOnly = false )
        {
            // Load a specified SP file annd show in Scrollviewer
            stringresult = "";
            DataTable dt = new();
            string output = "";
            if ( spName == null )
            {
                stringresult = output;
                return stringresult;
            }
            dt = SProcsSupport . ProcessSqlCommand ( $"spGetNamedSproc  '{spName}'", Flags . CurrentConnectionString );
            List<string> list = new List<string>();
            List<string> headeronlylist = new List<string>();
            foreach ( DataRow row in dt . Rows )
            {
                list . Add ( row . Field<string> ( 0 ) );
            }
            if ( HeaderOnly )
            {
                list [ 0 ] = SProcsSupport . GetSpHeaderTextOnly ( list [ 0 ] );
                // now display the full content of the seleted S.P
                if ( list . Count > 0 )
                    output = list [ 0 ];
                stringresult = output;
                return stringresult;
                //return output;
            }
            // now display the full content of the seleted S.P
            if ( list . Count > 0 )
                output = list [ 0 ];
            stringresult = output;
            return stringresult;
        }

        public static ObservableCollection<GenericClass> GetDbTableColumns (
            ref ObservableCollection<GenericClass> Gencollection,
            ref List<Dictionary<string, string>> ColumntypesList,
             ref List<string> list,
             string dbName,
             string DbDomain,
             ref List<DataGridLayout> dglayoutlist )
        {
            // Make sure we are accessing the correct Db Domain
            DapperSupport . CheckDbDomain ( DbDomain );
            Gencollection = GetSpArgs ( ref Gencollection, ref ColumntypesList, ref list, dbName, DbDomain, ref dglayoutlist );
            return Gencollection;
        }

        public static ObservableCollection<GenericClass> GetSpArgs (
            ref ObservableCollection<GenericClass> Gencollection,
            ref List<Dictionary<string, string>> ColumntypesList,
            ref List<string> list,
            string dbName,
            string DbDomain,
            ref List<DataGridLayout> dglayoutlist )
        {
            DataTable dt = new DataTable();
            GenericClass genclass = new GenericClass();
            try
            {
                // only used by grid2 on initial load cos grid1 uses List for datasource & gets count diffrently.
                //called on initial load to get column name and type (not datagrid data)
                if ( dglayoutlist . Count == 0 )
                    Gencollection = LoadDbAsGenericData ( "spGetTableColumnWithSizes",
                        Gencollection,
                        ref ColumntypesList,
                        dbName,
                        DbDomain,
                        ref dglayoutlist,
                        true );
                // Gencollection now contains FULL schema info on selected table  whereas 
                //dglayoutlist & Columntype contain only columnn name and type
                // list is not effected at all
            }
            catch ( Exception ex )
            {
                MessageBox . Show ( $"SQL ERROR 1125 - {ex . Message}" );
                return Gencollection;
            }
            return Gencollection;
        }

        public static GenericClass ParseDapperRowGen ( dynamic buff,
        Dictionary<string, object> dict, out int colcount )
        {
            GenericClass GenRow = new GenericClass();
            int index = 2;
            colcount = 0;
            foreach ( var item in buff )
            {
                try
                {
                    if ( item . Key == "" || item . Value == null )
                        break;
                    dict . Add ( item . Key, item . Value );
                }
                catch ( Exception ex )
                {
                    MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
                    break;
                }
            }
            colcount = index;
            return GenRow;
        }

        private static bool IsDuplicatecolumnname ( string Columntypes, Dictionary<string, string> ColumntypesList )
        {
            bool success = false;
            foreach ( KeyValuePair<string, string> item in ColumntypesList )
            {
                if ( item . Key == Columntypes )
                {
                    success = true;
                    break;
                }
            }
            return success;
        }
        private static bool IsDuplicateFieldname ( DataGridLayout dglayout, List<DataGridLayout> dglayoutlist )
        {
            //$"Entering " . dcwinfo(0);
            bool success = false;
            //;			int count = 0;
            foreach ( var item in dglayoutlist )
            {
                if ( item . Fieldname == dglayout . Fieldname )
                    return true;
            }
            return success;
        }
        /// <summary>
        /// called to load all types of data using a Stored procedure (only)        /// </summary>
        /// <param name="SqlCommand"></param>
        /// <param name="collection"></param>
        /// <param name="ColumntypesList"></param>
        /// <param name="Arguments"></param>
        /// <param name="DbDomain"></param>
        /// <param name="dglayoutlist"></param>
        /// <param name="GetLengths"></param>
        /// <returns></returns>
        public static ObservableCollection<GenericClass> LoadDbAsGenericData (
            string SqlCommand,
            ObservableCollection<GenericClass> collection,
            ref List<Dictionary<string, string>> ColumntypesList,
            string Arguments,
            string DbDomain,
            ref List<DataGridLayout> dglayoutlist,
            bool GetLengths = false )
        {
            string result = "";
            string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
            // provide a default connection string
            string ConString = "ConnectionString";
            Dictionary<string, dynamic> dict = new Dictionary<string, object>();
            ObservableCollection<GenericClass> GenClass = new ObservableCollection<GenericClass>();

            // Ensure we have the correct connection string for the current Db Doman
            Utils2 . CheckResetDbConnection ( DbDomain, out string constr );
            Flags . CurrentConnectionString = constr;
            ConString = constr;
            collection . Clear ( );
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    // Use DAPPER to run  Stored Procedure
                    // One or No arguments
                    arg1 = Arguments;
                    if ( arg1 . Contains ( "," ) )              // trim comma off
                        arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                    // Create our aguments using the Dynamic parameters provided by Dapper
                    var Params = new DynamicParameters();
                    if ( arg1 != "" )
                        Params . Add ( "Arg1", arg1, DbType . String, ParameterDirection . Input, arg1 . Length );
                    if ( arg2 != "" )
                        Params . Add ( "Arg2", arg2, DbType . String, ParameterDirection . Input, arg2 . Length );
                    if ( arg3 != "" )
                        Params . Add ( "Arg3", arg3, DbType . String, ParameterDirection . Input, arg3 . Length );
                    if ( arg4 != "" )
                        Params . Add ( "Arg4", arg4, DbType . String, ParameterDirection . Input, arg4 . Length );

                    //***************************************************************************************************************//
                    // This returns the data from SP commands (only) in a GenericClass Structured format
                    // FAILS on parsedapper
                    var reslt = db.Query(SqlCommand, Params, commandType: CommandType.Text);
                    //***************************************************************************************************************//

                    if ( reslt != null )
                    {
                        //Although this is duplicated  with the one below we CANNOT make it a method()
                        string errormsg = "DYNAMIC";
                        int dictcount = 0;
                        int fldcount = 0;
                        int colcount = 0;
                        GenericClass gc = new GenericClass();
                        List<int> VarcharList = new List<int>();
                        Dictionary<string, string> outdict = new Dictionary<string, string>();
                        try
                        {
                            foreach ( var item in reslt )
                            {
                                try
                                {
                                    // we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
                                    string buffer = "";
                                    // WORKS OK
                                    ParseDapperRowGen ( item, dict, out colcount );
                                    gc = new GenericClass ( );
                                    dictcount = 1;
                                    int index = 1;
                                    fldcount = dict . Count;
                                    string tmp = "";

                                    // Parse reslt.item into  single dglayout Dictionary record
                                    foreach ( var pair in dict )
                                    {
                                        Dictionary<string, string> Columntypes = new Dictionary<string, string>();
                                        try
                                        {
                                            if ( pair . Key != null )
                                            {
                                                if ( pair . Value != null )
                                                {
                                                    DapperSupport . AddDictPairToGeneric ( gc, pair, dictcount++ );
                                                    tmp = $"field{index++} = {pair . Value . ToString ( )}";
                                                    outdict . Add ( pair . Key, pair . Value . ToString ( ) );
                                                    buffer += tmp + ",";
                                                }
                                                //List<int>
                                                if ( pair . Key == "character_maximum_length" )
                                                    sph . dglayout . Fieldlength = item . character_maximum_length == null ? 0 : item . character_maximum_length;
                                                if ( pair . Key == "data_type" )
                                                    sph . dglayout . Fieldtype = item . data_type == null ? 0 : item . data_type;
                                                if ( pair . Key == "column_name" )
                                                {
                                                    string temp = item.column_name.ToString();
                                                    if ( IsDuplicatecolumnname ( temp, Columntypes ) == false )
                                                        Columntypes . Add ( temp, item . data_type . ToString ( ) );
                                                    sph . dglayout . Fieldname = item . column_name == null ? "UNSPECIFIED" : item . column_name;
                                                    // Add Dictionary <string,string> to List<Dictionary<string,string>
                                                    ColumntypesList . Add ( Columntypes );
                                                }
                                            }
                                        }
                                        catch ( Exception ex )
                                        {
                                            Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                                            result = ex . Message;
                                        }
                                    }
                                    //remove trailing comma
                                    string s = buffer.Substring(0, buffer.Length - 1);
                                    buffer = s;
                                    // We now  have ONE single record, but need to add this  to a GenericClass structure 
                                    int reccount = 1;
                                    foreach ( KeyValuePair<string, string> val in outdict )
                                    {  //
                                        switch ( reccount )
                                        {
                                            case 1:
                                                gc . field1 = val . Value . ToString ( );
                                                break;
                                            case 2:
                                                gc . field2 = val . Value . ToString ( );
                                                break;
                                            case 3:
                                                gc . field3 = val . Value . ToString ( );
                                                break;
                                            case 4:
                                                gc . field4 = val . Value . ToString ( );
                                                break;
                                            case 5:
                                                gc . field5 = val . Value . ToString ( );
                                                break;
                                            case 6:
                                                gc . field6 = val . Value . ToString ( );
                                                break;
                                            case 7:
                                                gc . field7 = val . Value . ToString ( );
                                                break;
                                            case 8:
                                                gc . field8 = val . Value . ToString ( );
                                                break;
                                            case 9:
                                                gc . field9 = val . Value . ToString ( );
                                                break;
                                            case 10:
                                                gc . field10 = val . Value . ToString ( );
                                                break;
                                            case 11:
                                                gc . field11 = val . Value . ToString ( );
                                                break;
                                            case 12:
                                                gc . field12 = val . Value . ToString ( );
                                                break;
                                            case 13:
                                                gc . field13 = val . Value . ToString ( );
                                                break;
                                            case 14:
                                                gc . field14 = val . Value . ToString ( );
                                                break;
                                            case 15:
                                                gc . field15 = val . Value . ToString ( );
                                                break;
                                            case 16:
                                                gc . field16 = val . Value . ToString ( );
                                                break;
                                            case 17:
                                                gc . field17 = val . Value . ToString ( );
                                                break;
                                            case 18:
                                                gc . field18 = val . Value . ToString ( );
                                                break;
                                            case 19:
                                                gc . field19 = val . Value . ToString ( );
                                                break;
                                            case 20:
                                                gc . field20 = val . Value . ToString ( );
                                                break;
                                        }
                                        reccount += 1;
                                    }
                                    collection . Add ( gc );
                                }
                                catch ( Exception ex )
                                {
                                    result = $"SQLERROR : {ex . Message}";
                                    errormsg = result;
                                    Debug . WriteLine ( result );
                                }
                                dict . Clear ( );
                                outdict . Clear ( );
                                dictcount = 1;
                            }
                        }
                        catch ( Exception ex )
                        {
                            Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
                            result = ex . Message;
                            errormsg = result;
                        }
                        if ( errormsg == "" )
                            errormsg = $"DYNAMIC:{fldcount}";
                        return collection;
                    }
                }
                catch ( Exception ex )
                {
                    $"{ex . Message}" . cwerror ( );
                }
            }
            return GenClass;
        }

        static public List<string> GetGenericListFromDictionary ( Dictionary<string, object> dict )
        {
            // called immediately after parsing DapperRow data
            List<string> list = new();
            int count = 0;
            foreach ( var pair in dict )
            {
                try
                {
                    if ( pair . Key != null && pair . Value != null )
                    {
                        genclass = new GenericClass ( );
                        DapperSupport . AddDictPairToGeneric ( genclass, pair, count++ );
                        string tmp = pair.Key.ToString() + "=" + pair.Value.ToString();
                        list . Add ( tmp );
                    }
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                }
            }
            return list;
        }

        static public GenericClass ConvertListToGenericClassRecord ( List<string> Recordslist, int colcount, out List<string> Fldnames )
        {
            // called after a list<string> contaning a single row of GenericClass data
            // to create a GenericClass Record
            int count = 0;
            Fldnames = new List<string> ( );
            while ( count < colcount )
            {
                foreach ( var item in Recordslist )
                {
                    string[] buff = new string[3];
                    buff = item . Split ( "=" );
                    switch ( count )
                    {
                        case 0:
                            genclass . field1 = buff [ 1 ];
                            break;
                        case 1:
                            genclass . field2 = buff [ 1 ];
                            break;
                        case 2:
                            genclass . field3 = buff [ 1 ];
                            break;
                        case 3:
                            genclass . field4 = buff [ 1 ];
                            break;
                        case 4:
                            genclass . field5 = buff [ 1 ];
                            break;
                        case 5:
                            genclass . field6 = buff [ 1 ];
                            break;
                        case 6:
                            genclass . field7 = buff [ 1 ];
                            break;
                        case 7:
                            genclass . field8 = buff [ 1 ];
                            break;
                        case 8:
                            genclass . field9 = buff [ 1 ];
                            break;
                        case 9:
                            genclass . field10 = buff [ 1 ];
                            break;
                        case 10:
                            genclass . field11 = buff [ 1 ];
                            break;
                        case 11:
                            genclass . field12 = buff [ 1 ];
                            break;
                        case 12:
                            genclass . field13 = buff [ 1 ];
                            break;
                        case 13:
                            genclass . field14 = buff [ 1 ];
                            break;
                        case 14:
                            genclass . field15 = buff [ 1 ];
                            break;
                        case 15:
                            genclass . field16 = buff [ 1 ];
                            break;
                        case 16:
                            genclass . field17 = buff [ 1 ];
                            break;
                        case 17:
                            genclass . field18 = buff [ 1 ];
                            break;
                        case 18:
                            genclass . field19 = buff [ 1 ];
                            break;
                        case 19:
                            genclass . field20 = buff [ 1 ];
                            break;
                        default:
                            break;
                    }
                    Fldnames . Add ( buff [ 0 ] );
                    count++;
                }
            }
            return genclass;
        }

        public static GenericClass ConvertListToSingleFieldGenericClass ( List<string> Recordslist, int colcount, out List<string> Fldnames )
        {
            // called after a list<string> contaning a single row of GenericClass data
            // to create a GenericClass Record
            int count = 0;
            // Fldnames is NOT used in this version of sinlge column (S.Procs list) mode
            Fldnames = new List<string> ( );
            while ( count < colcount )
            {
                foreach ( var item in Recordslist )
                {
                    string[] buff = new string[3];
                    buff = item . Split ( "=" );
                    genclass . field1 = buff [ 1 ];
                    count++;
                }
            }
            return genclass;

        }

        static public DynamicParameters ParseNewSqlArgs ( DynamicParameters parameters, List<string [ ]> argsbuffer, out string error )
        {
            DynamicParameters pms = new DynamicParameters();
            error = "";
            try
            {
                /*
                 order is :
                @name
                Argument
                Type
                !!!! Size
                Direction
                 */
                int argcount = 0;
                for ( var i = 0 ; i < argsbuffer . Count ; i++ )
                {
                    string[] args = new string[6];
                    args = PadArgsArray ( args );
                    args = argsbuffer [ i ];
                    PadArgsArrayNulls ( ref args );
                    int[] argindx = new int[5];
                    // set  all to zero to initialise flags
                    string printflags="";
                    for ( int z = 0 ; z < 5 ; z++ )
                    {
                        if ( args [ z ] == null )
                            continue;
                        if ( args [ z ] != "" )
                        {
                            printflags += " 1,";
                            argindx [ z ] = 1;
                        }
                        else
                        {
                            argindx [ z ] = 0;
                            printflags += " 0,";
                        }
                    }
                    printflags = printflags . Substring ( 0, printflags . Length - 1 );

                    Debug . WriteLine ( printflags );
                    // MISSING @Arg name - ERROR
                    if ( argindx [ 0 ] == 0 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        error = $"The mandatory Argument name is missing, processing cannot proceed";
                        break;
                    }
                    // Got (1 = 10000) arg name only 
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ] );
                        continue;
                    }
                    // got (2 = 11000) @arg name and argument name only - valid anywhere
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ] );
                        argcount++;
                        continue;
                    }
                    // Got (2 = 10010) arg name + direction only  2nd or subsequent args only !!
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 0 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        // Got @argument but no arg name ! possible an OUTPUT variable name used without an argument name,
                        // //but with al other of the 5 params (argument  type, Ouput/return etc, allowed size)
                        if ( args [ 4 ] != "" )
                            pms . Add (
                                name: args [ 0 ]
                                , dbType: GetArgType ( args [ 2 ] ) );
                        continue;
                    }
                    // Got (2 = 10100) arg name + arg type + type
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 0 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // got (3 = 11001) @arg name and argument name + size only
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 0 && argindx [ 4 ] == 1 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , size: GetArgSize ( args [ 4 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (3 = 11010) arg name Argumment name + direction
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 0 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                                name: args [ 0 ]
                                , value: args [ 1 ]
                                , dbType: GetArgType ( args [ 2 ] ) );
                        continue;
                    }
                    // Got (3 = 11100) arg name
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 0 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (4 = 11110) arg name+ arg type + arg size only (default direction)
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 1 && argindx [ 4 ] == 0 )
                    {
                        pms . Add (
                           name: args [ 0 ]
                           , value: args [ 1 ]
                           , dbType: GetArgType ( args [ 2 ] )
                           , direction: GetDirection ( args [ 3 ] ) );
                        argcount++;
                        continue;
                    }
                    // Got (5 = 11111) Full House
                    if ( argindx [ 0 ] == 1 && argindx [ 1 ] == 1 && argindx [ 2 ] == 1 && argindx [ 3 ] == 1 && argindx [ 4 ] == 1 )
                    {
                        pms . Add (
                            name: args [ 0 ]
                            , value: args [ 1 ]
                            , dbType: GetArgType ( args [ 2 ] )
                            , direction: GetDirection ( args [ 3 ] )
                            , size: GetArgSize ( args [ 4 ] ) );
                        argcount++;
                        continue;
                    }
                    PrintDebugArgs ( args );
                }
                if ( argcount < argsbuffer . Count )
                    error = "One or more invalid arguments identified";
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( ex . Message );
            }
            return pms;
        }

        public static void PrintDebugArgs ( string [ ] args )
        {
            string tmp="";
            tmp = $"\n@Argument" . PadRight ( 20 );
            string output = $"{tmp}{args[0]}\n";
            tmp = $"Arg. Name" . PadRight ( 20 );
            output += $"{tmp}{args [ 1 ]}\n";
            tmp = $"Arg Type" . PadRight ( 20 );
            output += $"{tmp}{args [ 2 ]}\n";
            tmp = $"Direction" . PadRight ( 20 );
            output += $"{tmp}{args [ 4 ]}\n";
            tmp = $"Arg Size" . PadRight ( 20 );
            output += $"{tmp}{args [ 3 ]}\n";
            Debug . WriteLine ( output + $"\n" );
        }

        public static string [ ] PadArgsArray ( string [ ] content )
        {
            string[] tmp = new string[MAXARGSIZE ];
            for ( int x = 0 ; x < MAXARGSIZE ; x++ )
            {
                tmp [ x ] = "";
            }
            return tmp;
        }
        public static string [ ] PadArgsArrayNulls ( ref string [ ] content )
        {
            string[] tmp = new string[MAXARGSIZE ];
            for ( int x = 0 ; x < MAXARGSIZE ; x++ )
            {
                if ( content [ x ] == null )
                    tmp [ x ] = "";
                else tmp [ x ] = content [ x ];
            }
            content = tmp;
            return tmp;
        }

        static public DbType GetArgType ( string type )
        {
            if ( type == "" )
                return DbType . String;
            if ( type . Contains ( "STR" ) || type . Contains ( "STR" ) )
                return DbType . String;
            if ( type . Contains ( "INT" ) )
                return DbType . Int32;
            if ( type . Contains ( "FLOAT" ) )
                return DbType . Double;
            if ( type . Contains ( "VARCHAR" ) )
                return DbType . String;
            if ( type . Contains ( "VARBIN" ) )
                return DbType . Binary;
            if ( type . Contains ( "TEXT" ) )
                return DbType . String;
            if ( type . Contains ( "BIT" ) )
                return DbType . Boolean;
            if ( type . Contains ( "BOOL" ) )
                return DbType . Boolean;
            if ( type . Contains ( "SMALLINT" ) )
                return DbType . Int16;
            if ( type . Contains ( "BIGINT" ) )
                return DbType . Int64;
            if ( type . Contains ( "DOUBLE" ) )
                return DbType . Double;
            if ( type . Contains ( "DEC" ) )
                return DbType . Decimal;
            if ( type . Contains ( "CURR" ) )
                return DbType . Currency;
            if ( type . Contains ( "DATETIME" ) )
                return DbType . DateTime;
            if ( type . Contains ( "DATE" ) )
                return DbType . Date;
            if ( type . Contains ( "TIMESTAMP" ) )
                return DbType . Time;
            if ( type . Contains ( "TIME" ) )
                return DbType . Time;

            return DbType . Object;
        }
        static public int GetArgSize ( string args )
        {
            int size = 0;
            if ( args == null || args == "" )
                return 0;
            if ( args != "" && args != "MAX" )
            {
                char ch;
                for ( int x = 0 ; x < args . Length ; x++ )
                {
                    ch = args [ x ];
                    if ( Char . IsDigit ( ch ) == false )
                        return 0;
                }
                return ( Convert . ToInt32 ( args ) );
            }
            else if ( args == "MAX" )
                return 64000;
            return 64000;
        }
        static public ParameterDirection GetDirection ( string args )
        {
            if ( args == "" || args . Contains ( "IN" ) )
                return ParameterDirection . Input;
            else if ( args . Contains ( "OUT" ) )
                return ParameterDirection . Output;

            return ParameterDirection . Input;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion Support methods
        private void CloseBtn_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        private void Paste_Click ( object sender, RoutedEventArgs e )
        {
            string output = "";
            string buff = "";
            string interim = "";
            int arraycount = 0;
            string[] arg = null;
            string[] initial = SPHeaderblock.Text.Split("\n");

            for ( int x = 0 ; x < initial . Length ; x++ )
            {   // Check for empty entries after split
                if ( initial [ x ] == null || initial [ x ] . Length != 0 )
                    arraycount++;
            }

            string[] parts = new string[arraycount];
            string fullarg = "";
            // clean up the entries first
            for ( int x = 0 ; x < arraycount ; x++ )
            {
                parts [ x ] = initial [ x ] . TrimEnd ( ) . TrimStart ( );
            }
            for ( int x = 0 ; x < parts . Length ; x++ )
            {
                // strip [  Input : ] presention text from each line in parts[]
                if ( parts [ x ] . ToUpper ( ) . Contains ( "INPUT : " ) )
                    parts [ x ] = parts [ x ] . Substring ( 8 );
                if ( parts [ x ] . ToUpper ( ) . Contains ( "OUTPUT : " ) )
                    parts [ x ] = parts [ x ] . Substring ( 9 );
                parts [ x ] = SProcsSupport . CheckForComment ( parts [ x ] );
            }
            arg = parts;
            for ( int y = 0 ; y < arg . Length ; y++ )
            {
                if ( arg [ y ] . Contains ( "CREATE PROCEDURE" ) )
                    continue;
                // get next argument line
                string input = arg[y];
                parts = arg [ y ] . Split ( ' ' );

                //strip leading/Trailing commas
                for ( int x = 0 ; x < parts . Length ; x++ )
                {
                    parts [ x ] = SProcsSupport . CheckForCommas ( parts [ x ] );
                }
                // now we can parse current phrase out
                for ( int x = 0 ; x < parts . Length ; x++ )
                {
                    // is it an argument name ?
                    if ( parts [ x ] . StartsWith ( "@" ) )
                    {
                        fullarg = parts [ x ];
                        continue;
                    }
                    // check for various data Type identifiers
                    interim = SProcsSupport . CheckForVarchar ( KeepTypes, parts [ x ] . Trim ( ) );
                    fullarg += $" {interim}";
                    continue;
                }
                buff = fullarg;
                if ( output . Length == 0 )
                    output += $"{buff}";
                else
                    output += $", {buff}";
            }

            Clipboard . SetText ( output );
        }

        /// <summary>
        /// Clever method that loads any selected  Stored Procedure into a ScrollViewer
        /// in Genericgrid and SpResultsViewer widows independently of each other.
        /// The Document viewer higlights the current Search term in the SP loaded.
        /// </summary>
        /// <param name="win">Caller window</param>
        /// <param name="spfilename">SP to be loaded</param>
        /// <param name="sptext">Search Text to be highlighted</param>
        /// <returns></returns>	

        private void SProcsListbox_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // ListBox lb = sender as ListBox;
        }

        #region  Generic (Combo) Data Loading methods
        //#################################//

        public void LoadColorsCombo ( )
        {
            "" . Track ( 0 );
            List<string> colors = new();
            spviewer . ColorsCombo . Items . Clear ( );
            foreach ( comboclrs item in ColorsCombo . Items )
            {
                colors . Add ( item . name );
                Brushcolors . Add ( item . Bground );
            }
            spviewer . ColorsCombo . ItemsSource = colors;
            spviewer . ColorsCombo . SelectedIndex = 113;
            "" . Track ( 1 );
        }
        public static Task LoadFontSizes ( )
        {
            "" . Track ( 0 );
            SProcsHandling sp = SProcsHandling.spviewer;
            List<string> sizes = new();
            sizes . Add ( "10" );
            sizes . Add ( "11" );
            sizes . Add ( "12" );
            sizes . Add ( "14" );
            sizes . Add ( "16" );
            sizes . Add ( "17" );
            sizes . Add ( "18" );
            sizes . Add ( "19" );
            sizes . Add ( "20" );
            sizes . Add ( "22" );
            sizes . Add ( "24" );
            sp . FontSizeCombo . ItemsSource = sizes;
            sp . FontSizeCombo . SelectedIndex = 4; // "16"
            "" . Track ( 1 );
            return Task . CompletedTask;
        }
        public Task<int> LoadSqlTableNames ( )
        {
            List<string> list = new List<string>();
            List<string> SqlTablesList = new List<string>();
            // Returns an IEnumerable <dynamic> collection
            IEnumerable<dynamic> Ienum = GenDapperQueries.CallStoredProcedure(list, "spGetTablesList");
            "" . Track ( 0 );

            if ( Ienum != null )
            {
                var ie = Ienum.GetEnumerator();
                if ( ie != null )
                {
                    foreach ( var item in Ienum )
                    {
                        if ( ie . MoveNext ( ) )
                            SqlTablesList . Add ( item );
                    }
                }
            }
            GridCombo . ItemsSource = SqlTablesList;
            int counter = 0;
            foreach ( var item in SqlTablesList )
            {
                if ( item . ToUpper ( ) == "BANKACCOUNT" )
                    break;
                counter++;
            }
            GridCombo . SelectedIndex = counter;
            "" . Track ( 1 );
            return Task . FromResult ( counter );
        }

        #endregion   Generic (Combo) Data Loading methods

        private void ColorsCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            // Actually works !! 6/2/23
            //Changes the HiLite color of the selected search term in FlowDocument TextResult.
            ComboBox cb = sender as ComboBox;
            int selindex = cb.SelectedIndex;
            string currColor = cb.SelectedItem.ToString();
            string CurrProc = SProcsListbox.SelectedItem.ToString();
            int indx = SProcsListbox.SelectedIndex;
            comboclrs cc = CbColorsList[indx];
            SolidColorBrush newcolor = cc.Bground;
            SProcsHandling sp = SProcsHandling.spviewer;
            MainWindow . ScrollViewerHiliteColor = newcolor;
            string sptext = Searchtext;
            FlowDocument currentdoc = TextResult.Document;
            LoadShowMatchingSproc ( this, TextResult, SProcsListbox . SelectedItem . ToString ( ), ref sptext );
            TextResult . UpdateLayout ( );
            return;
        }

        public comboclrs GetCurrentSelectionItem ( )
        {
            comboclrs cc = new();
            cc = ( comboclrs ) ColorsCombo . SelectedItem;
            return cc;
        }

        #region  selection changing
        //#################################//

        private void SPDatagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            DataGrid dg = sender as DataGrid;
            if ( dg . Items . Count == 0 || dg . SelectedIndex == -1 )
                return;
            // Set public properties
            CurrentSelectionIndex = dg . SelectedIndex;
            GenericClass gc = new();
            if ( dg . SelectedItem == null )
                return;
            gc = dg . SelectedItem as GenericClass;
            if ( gc == null )
                return;
            CurrentGenclass = gc;
            CurrentSelectionId = gc . field1;
            DataGridRow dgr = new();
            dgr = dg . SelectedValue as DataGridRow;
            if ( EditPanel . Visibility == VISIBLE )
            {
                if ( bdirty )
                {
                    MessageBoxResult mbr = MessageBox.Show("There are 1 or more unsaved changes to the data in the Edit panel.\n\nDo you want to save those changes first ? or discard them !", "Unsaved Changes", MessageBoxButton.YesNo);
                    if ( mbr == MessageBoxResult . Yes )
                        return;

                }
                LoadSqlEditData ( null, null );
            }
        }

        private void FsizeCombo_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( IsLoading )
                return;
            SProcsHandling sp = SProcsHandling.spviewer;

            ComboBox cb = sender as ComboBox;
            if ( FontSizeCombo . SelectedItem == null )
                return;
            int selindex = cb.SelectedIndex;
            string currSize = cb.SelectedItem.ToString();
            double Fontsize = Convert . ToDouble ( currSize );
            MainWindow . ScrollViewerFontSize = Fontsize;
            string CurrProc = SProcsListbox.SelectedItem.ToString();
            string sptext = "";
            bool result = LoadShowMatchingSproc(this, TextResult, CurrProc, ref sptext);
            if ( TextResult . Visibility == VISIBLE )
            {
                string Arguments = SProcsSupport.GetSpHeaderBlock(sptext, this);
                if ( Arguments . Length == 0 || Arguments . Contains ( "No valid Arguments were found" ) == true
                    || Arguments . Contains ( "Either the \"AS\" or \"BEGIN \" statements are missing" )
                    || Arguments . StartsWith ( "ERROR -" ) )
                {
                    SPArguments . Text = "The Header Block or parameters in the S.Procedure appear to be invalid !";
                    Parameterstop . Text = Arguments;
                }
                else
                {
                    SPArguments . Text = Arguments;
                }
                TextResult . UpdateLayout ( );
            }
            ResultsTextbox . FontSize = Fontsize;
            ResultsTextbox . UpdateLayout ( );
            ResultsListBox . FontSize = Fontsize;
            ResultsListBox . UpdateLayout ( );
            ResultsDatagrid . FontSize = Fontsize;
            ResultsDatagrid . UpdateLayout ( );
            SPDatagrid . FontSize = Fontsize;
            SPDatagrid . RowHeight = Fontsize + 10;
            SPDatagrid . UpdateLayout ( );
            SProcsListbox . FontSize = Fontsize;
            SProcsListbox . UpdateLayout ( );
            ExecList . FontSize = Fontsize;
            ExecList . UpdateLayout ( );
        }

        #endregion selection changing

        private void SProcsViewer_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
        {
            MainWindow . MainWin . SetValue ( TopmostProperty, true );
        }

        public void RefreshDatagrid ( string tablename )
        {
            int currsel = SPDatagrid.SelectedIndex;
            TableReloading = true;
            CurrentDbName = tablename;
            SqlTable . Clear ( );
            SPDatagrid . ItemsSource = null;
            LoadSqlData ( tablename );
            SPDatagrid_Loaded ( null, null );
            SPDatagrid . SelectedIndex = currsel;
            SPDatagrid . ScrollIntoView ( CurrentDbName );
            Utils2 . ScrollRowInGrid ( SPDatagrid, currsel );
            TableReloading = false;
            SPDatagrid . UpdateLayout ( );
            Mouse . OverrideCursor = Cursors . Arrow;
        }

        private void edit_dgItem ( object sender, RoutedEventArgs e )
        {

            if ( SPDatagrid . Visibility == VISIBLE )
            {
                DataGrid dg = SPDatagrid;
                if ( dg == null )
                    return;
                GenericClass selection = dg.SelectedItem as GenericClass;
                if ( selection == null )
                {
                    MessageBox . Show ( "You MUST select a row before you can edit it ?", "No current record selected" );
                    return;
                }// create list of all fields in current record
                List<string> dgData = UnpackDgRecord(selection);
                //We now have all the data for this record in a list
                int totalfields = dgData.Count;
            }
        }

        public static List<string> UnpackDgRecord ( GenericClass selection )
        {
            // selectoin is current record as GenericClass record
            List<string> dgdata = new();
            dgdata . Add ( selection . field1 != null ? selection . field1 . Trim ( ) : "" );
            if ( dgdata [ 0 ] == "" )
                return null;
            else
            {
                while ( true )
                {
                    if ( selection . field2 == null )
                        break;
                    else
                        dgdata . Add ( selection . field2 != null ? selection . field2 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field3 == null )
                        break;
                    else
                        dgdata . Add ( selection . field3 != null ? selection . field3 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field4 == null )
                        break;
                    else
                        dgdata . Add ( selection . field4 != null ? selection . field4 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field5 == null )
                        break;
                    else
                        dgdata . Add ( selection . field5 != null ? selection . field5 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field6 == null )
                        break;
                    else
                        dgdata . Add ( selection . field6 != null ? selection . field6 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field7 == null )
                        break;
                    else
                        dgdata . Add ( selection . field7 != null ? selection . field7 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field8 == null )
                        break;
                    else
                        dgdata . Add ( selection . field8 != null ? selection . field8 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field9 == null )
                        break;
                    else
                        dgdata . Add ( selection . field9 != null ? selection . field9 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field10 == null )
                        break;
                    else
                        dgdata . Add ( selection . field10 != null ? selection . field10 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field11 == null )
                        break;
                    else
                        dgdata . Add ( selection . field11 != null ? selection . field11 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field12 == null )
                        break;
                    else
                        dgdata . Add ( selection . field12 != null ? selection . field12 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field13 == null )
                        break;
                    else
                        dgdata . Add ( selection . field13 != null ? selection . field13 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field14 == null )
                        break;
                    else
                        dgdata . Add ( selection . field14 != null ? selection . field14 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field15 == null )
                        break;
                    else
                        dgdata . Add ( selection . field15 != null ? selection . field15 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field16 == null )
                        break;
                    else
                        dgdata . Add ( selection . field16 != null ? selection . field16 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field17 == null )
                        break;
                    else
                        dgdata . Add ( selection . field17 != null ? selection . field17 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field18 == null )
                        break;
                    else
                        dgdata . Add ( selection . field18 != null ? selection . field18 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field19 == null )
                        break;
                    else
                        dgdata . Add ( selection . field19 != null ? selection . field19 . Trim ( ) . Trim ( ) : "" );
                    if ( selection . field20 == null )
                        break;
                    else
                        dgdata . Add ( selection . field20 != null ? selection . field20 . Trim ( ) . Trim ( ) : "" );
                    break;
                    ;
                }
            }
            return dgdata;
        }

        private void Datagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( IsLoading )
                return;
            ComboBox cb = sender as ComboBox;

            if ( cb == null )
                return;
            string tablename = cb.SelectedItem as string;
            SqlTable . Clear ( );
            SPDatagrid . ItemsSource = null;
            SPDatagrid . Columns . Clear ( );
            CurrentDbName = tablename;
            LoadSqlData ( $"{tablename . ToUpper ( )}", true );

            // stop _loaded method from reloading current table
            TableReloading = true;
            SPDatagrid_Loaded ( sender, null );
            TableReloading = false;
            //  load 1st record into edit panel
            if ( EditPanel . Visibility == VISIBLE )
            {

                SPDatagrid . SelectedIndex = 0;
                Resetdata ( null, null );
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
            }
            SetWindowTitleBar ( );
        }

        public string GetMenuEntry ( string menu, string key )
        {
            string result = "";
            foreach ( KeyValuePair<string, string> entry in cmContents )
            {
                if ( entry . Key == key )
                {
                    result = entry . Value;
                    break;
                }
            }
            return result;
        }

        public void AddMenuEntry ( string menu, string item, string prompt, int fsize = 1, string Bground=null, string Fground = null )
        {
            ContextMenuSupport . AddMenuItem ( ( ContextMenu ) this . FindResource ( menu ), item, prompt, fsize , Bground, Fground);
        }

        public void RemoveMenuEntry ( string menu, string item )
        {
            ContextMenuSupport . RemoveMenuItems ( ( ContextMenu ) this . FindResource ( menu ),
            item, null );
        }

        private void DoHandled ( object sender, MouseButtonEventArgs e )
        {
            MouseRightButtonDown = false;
            e . Handled = true;
        }

        private void SPDatagrid_Loaded ( object sender, RoutedEventArgs e )
        {
            // we  now know the Window is fuly loaded
            List<string> list = new();
            if ( IsLoading == false && TableReloading == false )
                return;
            // get the column info for the current  SQL table
            SqlDataMethods . GetDbTableColumns<GenericClass> (
             SqlTable,
            ref list,
             CurrentDbName,
             DbDomain,
             ref VarCharLength,
            ref this . dglayoutlist );
            "" . Track ( 0 );

            /// create colum headers for datagrid
            if ( TableReloading == false || IsLoading == true )
            {
                // Now we need to get some SQL data
                SqlTable = LoadSqlData ( "BANKACCOUNT", LoadGrid: true );
            }

            LoadTableIntoGrid ( SqlTable, SPDatagrid );

            if ( SPDatagrid . Visibility == VISIBLE )
            {
                SPDatagrid . ItemsSource = SqlTable;
                SPDatagrid . UpdateLayout ( );
            }
            else
            {
                SPDatagrid . ItemsSource = SqlTable;
            }
            SPDatagrid . SelectedIndex = 0;
            if ( SHOWGRIDFIRST )
            {
                show_DataGrid ( null, null );
                SetWindowTitleBar ( );
            }
            IsLoading = false;
            "" . Track ( 1 );
        }

        public bool LoadTableIntoGrid ( ObservableCollection<GenericClass> SqlTable, DataGrid SPDatagrid )
        {
            "" . Track ( 0 );
            // Clear datagrid
            SPDatagrid . ItemsSource = null;
            SPDatagrid . Items . Clear ( );
            // create columns matching current table
            CreateDatagridColumns ( );

            SPDatagrid . UpdateLayout ( );
            "" . Track ( 1 );
            return true;
        }

        public void CreateDatagridColumns ( )
        {
            GenericClass gc = new();
            if ( SqlTable . Count == 0 )
                return;
            if ( SPDatagrid . Columns . Count > 0 )
                SPDatagrid . Columns . Clear ( );
            gc = SqlTable [ 0 ] as GenericClass;
            for ( int x = 0 ; x < dglayoutlist . Count ; x++ )
            {
                DataGridTextColumn c1 = new DataGridTextColumn();
                if ( dglayoutlist [ x ] . Fieldname . ToUpper ( ) == "ID" )
                    c1 . IsReadOnly = true;
                c1 . Header = dglayoutlist [ x ] . Fieldname;
                c1 . Binding = new Binding ( GetGenericFieldNameByIndex ( x ) );
                c1 . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                SPDatagrid . Columns . Add ( c1 );
            }
        }

        public void CreateNewSqlUpdateCommand ( List<string> UpdatedRecordData, GenericClass newgclass )
        {
            /* 
             Update SQL table itself from data in the DataEditWin.
             This is a smart method as it replaces  our generic column header names of field1, field2 ....
             with the valid field names of the SQL table itself via our (magic) dglayoutlist colection.

            This is because our underlying table is always a collection<GenericClass> so all fieldnames
            are fidl1, field2 .... field20, whereas the column headers etc shown are the ACTUAL Sql Table Column names (Clever eh?)
            so we have to massage the SQL statement to match the table column names against our own internal column names 
            of field1, field2 etc.

            This method does exactly that very elegantly.

             IT ACTUALLY WORKS VERY WELL and the table is updated SUCCESSFULLY
            */
            SqlCommand = $"Update {GridCombo . SelectedItem . ToString ( ) . ToUpper ( )} ";
            if ( UpdatedRecordData . Count == 1 )
                return;
            if ( UpdatedRecordData [ 1 ] != "" )
                SqlCommand += $"set {dglayoutlist [ 1 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 1 ] )}";
            if ( UpdatedRecordData . Count == 2 )
                return;
            if ( UpdatedRecordData [ 2 ] != "" )
                SqlCommand += $",{dglayoutlist [ 2 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 2 ] )}";
            if ( UpdatedRecordData . Count == 3 )
                return;
            if ( UpdatedRecordData [ 3 ] != "" )
                SqlCommand += $",{dglayoutlist [ 3 ] . Fieldname}={ConvertDataToSql ( UpdatedRecordData [ 3 ] )}";
            if ( UpdatedRecordData . Count == 4 )
                return;
            if ( UpdatedRecordData [ 4 ] != "" )
                SqlCommand += $", {dglayoutlist [ 4 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 4 ] )}";
            if ( UpdatedRecordData . Count == 5 )
                return;
            if ( UpdatedRecordData [ 5 ] != "" )
                SqlCommand += $", {dglayoutlist [ 5 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 5 ] )}";
            if ( UpdatedRecordData . Count == 6 )
                return;
            if ( UpdatedRecordData [ 6 ] != "" )
                SqlCommand += $", {dglayoutlist [ 6 ] . Fieldname} ={ConvertDataToSql ( UpdatedRecordData [ 6 ] )}";
            if ( UpdatedRecordData . Count == 7 )
                return;
            if ( UpdatedRecordData [ 7 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 7 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 7 ] )}";
            if ( UpdatedRecordData . Count == 8 )
                return;
            if ( UpdatedRecordData [ 8 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 8 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 8 ] )}";
            if ( UpdatedRecordData . Count == 9 )
                return;
            if ( UpdatedRecordData [ 9 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 9 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 9 ] )}";
            if ( UpdatedRecordData . Count == 10 )
                return;
            if ( UpdatedRecordData [ 10 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 10 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 10 ] )}";
            if ( UpdatedRecordData . Count == 11 )
                return;
            if ( UpdatedRecordData [ 11 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 11 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 11 ] )}";
            if ( UpdatedRecordData . Count == 12 )
                return;
            if ( UpdatedRecordData [ 12 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 12 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 12 ] )}";
            if ( UpdatedRecordData . Count == 13 )
                return;
            if ( UpdatedRecordData [ 13 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 13 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 13 ] )}";
            if ( UpdatedRecordData . Count == 14 )
                return;
            if ( UpdatedRecordData [ 14 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 14 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 14 ] )}";
            if ( UpdatedRecordData . Count == 15 )
                return;
            if ( UpdatedRecordData [ 15 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 15 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 15 ] )}";
            if ( UpdatedRecordData . Count == 16 )
                return;
            if ( UpdatedRecordData [ 16 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 16 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 16 ] )}";
            if ( UpdatedRecordData . Count == 17 )
                return;
            if ( UpdatedRecordData [ 17 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 17 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 17 ] )}";
            if ( UpdatedRecordData . Count == 18 )
                return;
            if ( UpdatedRecordData [ 18 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 18 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 18 ] )}";
            if ( UpdatedRecordData . Count == 19 )
                return;
            if ( UpdatedRecordData [ 19 ] != "" )
                SqlCommand += $",  {dglayoutlist [ 19 ] . Fieldname}  ={ConvertDataToSql ( UpdatedRecordData [ 19 ] )}";
            return;
        }

        public bool UpdateSqlTable ( string SqlCommand )
        {
            return DapperSupport . UpdateDbTable ( SqlCommand );
        }

        private void ToggleTopmost ( object sender, RoutedEventArgs e )
        {
            if ( this . Topmost == true )
            {
                this . Topmost = false;
                this . Title = "S.Procs/Sql Tables Window Topmost status is Set to Normal";
            }
            else
            {
                this . Topmost = true;
                this . Title = "S.Procs/Sql Tables Window Topmost status is Set to TOPMOST";
            }
        }

        #region Sprocs mouse/key handlers
        //#################################//
        private void SProcsViewer_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . F1 )
            {
                Debugger . Break ( );
            }
            if ( e . Key == Key . F2 )
            {
                string tmp="";
                Debug . WriteLine ( "Window sizes (ActualHeight):-" );
                Debug . WriteLine ( $"Total Window: {this . ActualHeight}		this height" );
                Debug . WriteLine ( $"Top rows: 60" );
                tmp = "SPFullDataContainerGrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPFullDataContainerGrid . ActualHeight}		Total internal grid" );
                tmp = "SPInfopanelGrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPInfopanelGrid . ActualHeight}		Top info rows" );
                tmp = "SPDatagrid" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {SPDatagrid . ActualHeight}		DataGrid height" );
                tmp = "EditPanel   " . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {EditPanel . ActualHeight}		Editpanel height" );
                tmp = "Bottompanel" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {Bottompanel . ActualHeight}		bottom panel height" );
                tmp = "ExecResult" . PadRight ( 25 );
                Debug . WriteLine ( $"{tmp} : {ExecResult . ActualHeight}\t\tbottom info panel  height" );
                Debug . WriteLine ( $"Grand TOTAL: {SPFullDataContainerGrid . ActualHeight} - {SPDatagrid . ActualHeight} + {EditPanel . ActualHeight} )" );
            }
            if ( e . Key == Key . F4 )
            {
                // show TextResult
                ResultsContainerListbox . Visibility = COLLAPSED;
                ResultsListBox . Visibility = COLLAPSED;
                ResultsContainerTextblock . Visibility = COLLAPSED;
                EditPanel . Visibility = COLLAPSED;
                TextResult . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                TextResult . Visibility = Visibility . Visible;
            }
            if ( e . Key == Key . F6 )
            {
                // show Results box
                ResultsContainerTextblock . Visibility = COLLAPSED;
                EditPanel . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                TextResult . Visibility = COLLAPSED;
            }
            if ( e . Key == Key . F7 )
            {
                // show Results TextBlock
                ResultsContainerListbox . Visibility = COLLAPSED;
                ResultsListBox . Visibility = COLLAPSED;
                EditPanel . Visibility = COLLAPSED;
                TextResult . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                ResultsDatagrid . Visibility = COLLAPSED;
                ResultsContainerTextblock . Visibility = Visibility . Visible;
            }
            if ( e . Key == Key . F8 )
            {
                // show top 10 entries in SQLTABLE and Datagrid !!! - They SHOULD match
                //set current index for both sources
                int currsel = SPDatagrid.SelectedIndex;
                if ( currsel == -1 )
                {
                    SPDatagrid . SelectedIndex = 0;
                    currsel = 0;
                }
                Debug . WriteLine ( $"\nSqlTable values for top 10 items from {currsel}:-" );
                for ( int x = currsel ; x < currsel + 10 ; x++ )
                {
                    GenericClass gc = new();
                    SPDatagrid . SelectedIndex = x;
                    gc = SPDatagrid . SelectedItem as GenericClass;

                    Debug . WriteLine ( $"Datagrid  : {gc . field3} : {gc . field2}" );
                    Debug . Write ( $"SqlTable  : {SqlTable [ x ] . field3} : {SqlTable [ x ] . field2}" );
                    if ( SqlTable [ x ] . field3 != gc . field3 )
                        Debug . WriteLine ( " **Mismatch identified**" );
                    else
                        Debug . WriteLine ( "\n" );
                    SPDatagrid . SelectedItem = x;
                }
            }
            if ( e . Key == Key . F9 )
            {
                string output = $"\nSizing information\n";
                output += $"\n" + "SProcsViewer" . ToString ( ) . PadRight ( 25 ) + $" : {SProcsViewer . Height}  : ActualHeight (Minus 240 pixels for the bottom panels)";
                Debug . WriteLine ( output );
            }
            if ( e . Key == Key . System )
            {
                string output = $"\nColors Combo data information\n";
                for ( int x = 0 ; x < ColorsCombo . Items . Count ; x++ )
                {
                    comboclrs cc = (comboclrs)ColorsCombo.Items[x];
                    output += $"\n" + $"{cc . name . PadRight ( 27 )} : {cc . Bground . ToString ( ) . PadRight ( 12 )}   {cc . Fground}";
                    Debug . WriteLine ( output );
                }
            }
        }
        private void SProcsListbox_KeyDown ( object sender, KeyEventArgs e )
        {
            ListBox lb = sender as ListBox;
            int index = lb . SelectedIndex;
            if ( lb == null )
                return;
            if ( e . Key == Key . Down )
            {
                string currselection = lb.SelectedItem.ToString();
                lb . SelectedIndex++;
            }
            else if ( e . Key == Key . Up )
            {
                index = lb . SelectedIndex;
                string currselection = lb.SelectedItem.ToString();
                if ( lb . SelectedIndex > 0 )
                    lb . SelectedIndex--;
            }
            else if ( e . Key == Key . Escape )
                HideResultsPanel ( sender, null ); // close S procs resuilts  viewer
        }

        private void SProcsListbox_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
            e . Handled = true;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion Sprocs mouse/key handlers

        public string GetGenericFieldNameByIndex ( int x )
        {
            string colname = "";
            colname = $"field{x + 1}";
            return colname;
        }
        private void SPDatagrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
        {
            if ( LEFTMOUSEDOWN == true )
            {
                e . Cancel = true;
                return;
            }
        }

        private void SPDatagrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
        {
        }

        private void dg_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
        {
        }

        #region splitter  handlers
        //++++++++++++++++++++++++++++++++//

        private void Splitter_DragStarted ( object sender, System . Windows . Controls . Primitives . DragStartedEventArgs e )
        {
            Dragdistance = 0;
            DragActive = true;
        }


        private void Splitter_DragCompleted ( object sender, System . Windows . Controls . Primitives . DragCompletedEventArgs e )
        {
            // All working fairly fine 7/2/2022  !!!
            if ( WinLoaded == false )
                return;
            double Totalheight = SPFullDataContainerGrid.ActualHeight - 100;
            RowHeight4 = Totalheight;
            double DivisibleArea = Totalheight;
            UpperDataArea = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
            // save upper height to global
            RowHeight0 = UpperDataArea;

            // save lower height to global
            // TODO  do not know why  we need to add the 175 ???
            RowHeight2 = LowerDataArea = DivisibleArea - ( UpperDataArea + 185 );
            if ( RowHeight2 < 0 ) RowHeight2 = 0;
            LowerDataArea = RowHeight2;
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            // reset lower panel control heights
            TextResult . Height = RowHeight2 - 10;
            ResultsContainerDatagrid . Height = RowHeight2;
            //SprocCreationGrid . Height = RowHeight2;
            //CreateSprocTextbox .Height = RowHeight2 - 10;
            EditFileContainerGrid . Height = RowHeight2;
            EditFileTextbox . Height = RowHeight2 - 10;
            SPFullDataContainerGrid . UpdateLayout ( );
            return;
        }

        private void Splitter_MouseEnter ( object sender, MouseEventArgs e )
        {
            //e.Handled= true;
        }

        private void Splitter_MouseLeave ( object sender, MouseEventArgs e )
        {
        }

        private void Splitter_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
        }

        private void Splitter_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            GridLength gl1 = new(SPFullDataContainerGrid.RowDefinitions[0].ActualHeight, GridUnitType.Pixel);
            GridLength gl2 = new(SPFullDataContainerGrid.RowDefinitions[2].ActualHeight, GridUnitType.Pixel);
            PreEditsplitterheight1 = gl1 . Value;
        }
        private void Togglesplitterreset ( object sender, RoutedEventArgs e )
        {
            AllowSplitterReset = !AllowSplitterReset;
        }

        //#################################//				
        #endregion END of splitter mouse handlers 

        #region SProcs mouse handlers
        //++++++++++++++++++++++++++++++++//

        private void SProcsListbox_MouseLeftButtonDown ( object sender, System . Windows . Input . MouseButtonEventArgs e )
        {
            ListBox lb = sender as ListBox;
            if ( lb == null )
                return;
            string currselection = lb.SelectedItem.ToString();
            SPHeaderblock . Text = currselection;
            LeftMousePressed = true;
            SProcsListbox_SelectionChanged ( sender, null );
            LeftMousePressed = false;
        }

        private void SProcsListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            ListBox lb = sender as ListBox;
            "" . Track ( 0 );
            if ( e == null ) return;
            // Load data into Scrollviewer
            string selname = "";
            if ( SProcsListbox . Items . Count > 0 && e . AddedItems != null )
            {
                // Reset info panel at bottom of Window
                ExecResult . Background = FindResource ( "Green8" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                StatusText = "Execution Results Panel ...";
                ExecResult . UpdateLayout ( );

                selname = e . AddedItems [ 0 ] . ToString ( );
                // Store search term in our dialog for easier access

                if ( TextResult . Document != null )
                {
                    TextResult . Document . Blocks . Clear ( );
                    TextResult . Document = null;
                }
                string sptext = "";
                // Update cosmetics
                if ( IsLoading == false )
                {
                    bool result = LoadShowMatchingSproc(this, TextResult, selname, ref sptext);
                    ShowParseDetails = true;
                    if ( ShowParseDetails )
                    {
                        string Arguments = SProcsSupport.GetSpHeaderBlock(sptext, this);
                        if ( Arguments . Length == 0 || Arguments . Contains ( "No valid Arguments were found" ) == true )
                        {
                            spviewer . SPArguments . Text = @$"No arguments are required.... to execute the Stored Procedure, just dbl-click the relevant Execution Method ...";
                            spviewer . Parameterstop . Text = @$"[No parameters required (or allowed)]";
                        }
                        else if ( Arguments . Contains ( "Either the \"AS\" or \"BEGIN \" statements are missing" )
                            || Arguments . StartsWith ( "ERROR -" ) )
                        {
                            SPArguments . Text = "The Header Block or parameters in the S.Procedure appear to be invalid !";
                            Parameterstop . Text = Arguments;
                        }
                        else
                            SPArguments . Text = Arguments;
                    }
                    if ( SPArguments . Text . Contains ( " STRING" ) == true )
                    {
                        ExecResult . Text = "Arguments are required for this Procedure";
                        ExecResult . Background = FindResource ( "Yellow1" ) as SolidColorBrush;
                        ExecResult . Foreground = FindResource ( "Red3" ) as SolidColorBrush;
                        ExecResult . UpdateLayout ( );
                        ExecuteBtn . IsEnabled = false;
                        ExecuteBtn . Opacity = 0.6;
                        ExecuteBtn . UpdateLayout ( );
                    }
                    else
                    {
                        ExecResult . Text = "NO parameters are required...";
                        ExecResult . Background = FindResource ( "Green8" ) as SolidColorBrush;
                        ExecResult . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
                        ExecResult . UpdateLayout ( );
                        ExecuteBtn . IsEnabled = true;
                        ExecuteBtn . Opacity = 1;
                        ExecuteBtn . UpdateLayout ( );
                    }

                }

                // Now setup the selection in Execution method listbox if we  have an ID in the script !!
                SetExecutionIndex ( selname );
                ExecListbox_SelChanged ( null, null );
            }
            LeftMousePressed = false;
            "" . Track ( 1 );
        }

        public void SetExecutionIndex ( string spname, int index = -1 )
        {
            //setup the selection in Execution method listbox if we  have an ID in the script !!
            int execindex = 0;
            if ( index != -1 )
            {
                string str = SProcsListbox . SelectedItem . ToString ( );
                int indx = SpNamesExecIndex[str];
                ExecList . SelectedIndex = indx;
                ExecList . UpdateLayout ( );
            }
            else if ( spname != "" )
            {
                foreach ( var item in SpNamesExecIndex )
                {
                    if ( item . Key == spname )
                    {
                        if ( item . Value == -1 )
                            execindex = 6;
                        else
                            execindex = item . Value - 1;
                        if ( execindex < 0 ) execindex = 6;
                        ExecList . SelectedIndex = execindex;
                        ExecList . UpdateLayout ( );
                        Debug . WriteLine ( execindex );
                        break;
                    }
                }
            }
        }
        //++++++++++++++++++++++++++++++++//
        #endregion  END of  SProcs mouse handlers

        #region Stored Procedure Execution handers

        #region ExecListBox Commands mouse handlers
        //#################################//

        private void ExecListbox_MouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            // double click on execution methods - go execute Sp.
            string currselection = SProcsListbox.SelectedItem.ToString();
            LeftMousePressed = true;
            ExecListbox_SelectionChanged ( sender, null );
            LeftMousePressed = false;
        }
        private void ExecListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            // Execute the SP using command selected
            int Count = 0;
            string ResultString = "";
            Type Objtype = null;
            object Obj = null;
            string Err = "";
            if ( IsLoading )
                return;
            SpExecution spe = new("IAN1");
            spe . GetSpExecution ( );
            SqlCommand = SProcsListbox . SelectedValue . ToString ( );
            string   execommand = ExecList.SelectedItem.ToString();
            // get command #
            int selvalue = Convert . ToInt32 ( execommand. Substring ( 0, 1 ) );

            //*******************************************************************************************************************//
            // call the execution system from here. the return value is our result
            // First check to see if this is a Scema request, If not, SpToUse will be ""
            if ( SpToUse != "" )
                SqlCommand = "spgetprocedureschema ";
            Debug . WriteLine ( $"\nStarting Execution of Command [ {ExecList . SelectedItem . ToString ( )} ]\nto execute Stored Procedure [ {SProcsListbox . SelectedItem . ToString ( )} ]\n" +
         $"with arguments [ {SPArguments . Text . ToUpper ( )} ] using Method {selvalue}\n" );

            dynamic dyn = spe.Execute_click(SqlCommand, ref Count, ref ResultString, ref Objtype, ref Obj, out Err, selvalue);
            // clear flag again so we  do not do schema every time.
            //*******************************************************************************************************************//
            if( Objtype == null)
            {
                ShowNonErrorPanel ( null, 0, "The execution process completed without errors...." );
                return;
            }
            else if ( Err == "" && Objtype . FullName . Contains ( ".ObservableCollection" ) == true )
            {
                //create a collection of the data returned cos this will capture ALL columns
                // Clever stuff eh ?
                int count = 0;
                DynamicCollection . Clear ( );
                DynamicCollection = CreateCollectionFromIEnumerableCollection ( dyn, ref count );
                Debug . WriteLine ( $"Generic Collection created for dynamic object" );
            }

            if ( Err != "" )
            {
                if ( SPArguments . Text . Contains ( " STRING" ) == true )
                {
                    string errmsg =  $"\nReturned from Execution of Command [ {ExecList . SelectedItem . ToString ( )} ]\nexecuting Stored Procedure " +
                        $"[ {SProcsListbox . SelectedItem . ToString ( )} ]\n" +
                     $"with arguments [ {SPArguments . Text . ToUpper ( )} ]\n" +
                     $"ERROR ENCOUNTERED : The required Arguments do not appear to have been entered !.\n";
                    Debug . WriteLine ( errmsg );
                    StatusText = Err;
                    ExecResult . Background = FindResource ( "Red4" ) as SolidColorBrush;
                    ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ShowErrorPanel ( Obj, Count, errmsg );
                    return;
                }
                else
                {
                    if ( Obj == null )
                        Debug . WriteLine ( $"\nReturned from Execution of Command [ {ExecList . SelectedItem . ToString ( )} ]\nexecuting Stored Procedure [ {SProcsListbox . SelectedItem . ToString ( )} ]\n" +
                         $"with arguments [ {SPArguments . Text . ToUpper ( )} ]\n" +
                         $"ERROR ENCOUNTERED : Mesage={Err},\nOther results were : ExecResult={ResultString}.\n" );
                    else
                        Debug . WriteLine ( $"\nReturned from Execution of Command [ {ExecList . SelectedItem . ToString ( )} ]\nexecuting Stored Procedure [ {SProcsListbox . SelectedItem . ToString ( )} ]\n" +
                         $"with arguments [ {SPArguments . Text . ToUpper ( )} ]\n" +
                         $"ERROR ENCOUNTERED : Mesage={Err},\nOther results were : ExecResult={ResultString}. Method = {selvalue}. Obj={Obj . ToString ( )}\n" );
                    StatusText = Err;
                    ExecResult . Background = FindResource ( "Red4" ) as SolidColorBrush;
                    ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ShowErrorPanel ( Obj, Count, Err );
                    return;
                }
            }
            else
            {
                Debug . WriteLine ( $"\nReturned from Execution of Command [ {ExecList . SelectedItem . ToString ( )} ]\nexecuting Stored Procedure [ {SProcsListbox . SelectedItem . ToString ( )} ]\n" +
                 $"with arguments [ {SPArguments . Text . ToUpper ( )} ]\n" );
                if ( Count != 0 )
                    Debug . WriteLine ( $"Results were : Count = {Count}, ExecResult={ResultString}. Method = {selvalue}. Object returned is {Obj . ToString ( )}\n" );
                else
                {
                    if( Obj  == null)
                        Debug . WriteLine ( $"No actual Results were returned\n" );
                    else
                        Debug . WriteLine ( $"Results were : ExecResult={ResultString}. Method = {selvalue}. Object returned is {Obj . ToString ( )}\n" );
                }
            }
            if ( ResultString == "SUCCESS" && Count == 0 )
            {
                ShowNonErrorPanel ( Obj, Count, Err );
            }
            else
            {
                // Handle a valid result
                string commandtype = ExecList.SelectedItem.ToString();
                string[] cmdstrings = new string[]
                    {
                    "1. SP returning a Table as ObservableCollection",
                    "2. SP returning a List<string> ",
                    "3. SP returning a String",
                    "4. SP returning an INT value",
                    "5. Execute Stored Procedure with return value",
                    "6. Execute Stored Procedure without return value"
            };
                ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                // Clean up receiving controls to avoid exceptions
                ResultsListBox . ItemsSource = null;
                ResultsListBox . Items . Clear ( );
                ResultsDatagrid . ItemsSource = null;
                ResultsDatagrid . Items . Clear ( );

                // Close all previous result panels - JIC
                ResultsContainerTextblock . Visibility = COLLAPSED;
                ResultsTextbox . Visibility = COLLAPSED;
                ResultsContainerListbox . Visibility = COLLAPSED;
                ResultsListBox . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                ResultsDatagrid . Visibility = COLLAPSED;
                //if (SprocCreationGrid . Visibility == VISIBLE )
                //SprocCreationGrid . Visibility = COLLAPSED;

                if ( commandtype == cmdstrings [ 0 ] . Trim ( ) )
                {
                    //Display a Datagrid for results
                    if ( Count > 0 )
                    {
                        // Parse data into OBSCOLLECTION
                        EditPanel . Visibility = COLLAPSED;
                        ResultsDatagrid . ItemsSource = null;
                        ObservableCollection<GenericClass>  querytable = new();
                        querytable = CreateCollectionFromDynamic ( dyn );
                        ResultsContainerDatagrid . Visibility = Visibility . Visible;
                        //// make results Datagrid ViSIBLE
                        ShowRt = true;
                        ResultsContainerDatagrid . Visibility = Visibility . Visible;
                        TextResult . Visibility = COLLAPSED;
                        ResultsDatagrid . Visibility = Visibility . Visible;
                        if ( SpToUse != "" )
                        {
                            SpToUse = "";
                            StatusText = $"Your Db Schema request has completed successfully, Full results are shown above ...";
                        }
                        else
                            StatusText = $"ObservableCollection results from Execution of {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} has completed successfully";
                        // add data into our grid, use dynamicgrid as it has more info (if available)
                        if ( DynamicCollection == null )
                            ResultsDatagrid . ItemsSource = querytable;
                        else
                        {
                            GenericClass gc = new();
                            gc . field1 = "Hit Escape to close this viewer";
                            DynamicCollection . Add ( gc );
                            GenericClass gc2 = new();
                            gc2 . field1 = "or Right Click for other options...";
                            DynamicCollection . Add ( gc2 );
                            ResultsDatagrid . ItemsSource = DynamicCollection;
                        }//if ( EditPanel . Visibility == VISIBLE )
                        //    ExecResult . Background = FindResource ( "Green7" ) as SolidColorBrush;
                        ResultsDatagrid . FontSize = MainWindow . ScrollViewerFontSize;
                        ResultsDatagrid . Focus ( );
                        ResultsDatagrid . SelectedIndex = 0;
                        ResultsDatagrid . SelectedItem = 0;
                        //Utils2 . DoSuccessBeep ( );
                    }
                    return;
                }
                else if ( commandtype == cmdstrings [ 1 ] . Trim ( ) )
                {
                    //Display a ListBox  for results
                    List<string> genericlist = CreateListFromDynamic ( dyn );
                    ResultsListBox . SelectedIndex = 0;
                    ResultsListBox . ScrollIntoView ( 0 );
                    if ( TextResult . Visibility == VISIBLE )
                        TextResult . Visibility = COLLAPSED;
                    EditPanel . IsEnabled = false;
                    ShowRt = true;
                    if ( ResultsListBox . Visibility == VISIBLE )
                        ResultsListBox . Visibility = COLLAPSED;
                    ResultsListBox . Visibility = Visibility . Visible;
                    Splitter_DragCompleted ( null, null );
                    //if ( SprocCreationGrid . Visibility == VISIBLE )
                    {
                        //SprocCreationGrid . Visibility = COLLAPSED;
                        ScriptEditorOpen = true;
                    }
                    ResultsListBox . ItemsSource = null;
                    if ( genericlist != null )
                    {
                        ResultsListBox . ItemsSource = null;
                        ResultsListBox . Items . Clear ( );
                        ResultsListBox . Items . Add ( $"EXECUTION RESULT for {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )}" );
                        ResultsListBox . Items . Add ( $"EXECUTION returned a total of {Count} items : (Hit Escape to return to Script Viewer)" );
                        if ( Count > 0 && ResultString == "SUCCESS" )
                        {
                            foreach ( var item in genericlist )
                            {
                                ResultsListBox . Items . Add ( $"{item}" );
                            }
                        }
                        else
                        {
                            ResultsListBox . Items . Add ( $"\tThe query returned no records, but NO error message was returned..." );
                            ResultsListBox . Items . Add ( $"\tThe parameters you passed of [{SPArguments . Text . ToUpper ( )}] may be 'suspect', or the table queried is actually empty." );
                        }
                    }
                    if ( ResultsContainerListbox . Visibility == COLLAPSED )
                    {
                        ResultsContainerListbox . Visibility = Visibility . Visible;
                        ResultsListBox . Visibility = Visibility . Visible;
                        Splitter_DragCompleted ( null, null );

                    }
                    ResultsListBox . Items . Add ( $"" );
                    ResultsListBox . Items . Add ( $"Dbl-Click this line or hit ESC to Close....\n\n" );
                    ResultsListBox . UpdateLayout ( );
                    StatusText = $"List<string> Results : Execution of {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} has completed successfully\n\n\n\n\n\n\n";
                    //                    ExecResult . Background = FindResource ( "Green7" ) as SolidColorBrush;
                    ResultsListBox . FontSize = MainWindow . ScrollViewerFontSize;
                    ResultsListBox . SelectedIndex = 0;
                    ResultsListBox . SelectedItem = 0;
                    ResultsListBox . ScrollIntoView ( 0 );
                    ResultsListBox . Focus ( );
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 2 ] . Trim ( ) )
                {
                    //Display a single string result
                    if ( TextResult . Visibility == VISIBLE )
                        TextResult . Visibility = COLLAPSED;

                    TextResult . Visibility = Visibility . Visible;
                    ResultsTextbox . Text = dyn . ToString ( );
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 3 ] . Trim ( ) )
                {
                    //Display an int result
                    ShowResultsTextbox ( );
                    ResultsTextbox . Text = $"SUCCESSFUL EXECUTION of Stored procedure [ {SqlCommand . ToUpper ( )} ]\nby execution of the Method [ {commandtype . ToUpper ( )} ] " +
                        $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                        $"\n\nThe RESULT of the Execution returned an integer value of [ {Count . ToString ( )} ] \n\n" +
                        $"Hit Escape to hide this results panel and redisplay the S.P Scripts Viewer.\\n\n\n\n\n";
                    ResultsTextbox . Background = FindResource ( "Green5" ) as SolidColorBrush;
                    ResultsTextbox . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 4 ] . Trim ( ) )
                {
                    //Display an int result
                    ShowResultsTextbox ( );
                    if ( Count > 0 )
                    {
                        ResultsTextbox . Text = $"Execution of Stored procedure [ {SqlCommand . ToUpper ( )} ]\nusing [ {commandtype . ToUpper ( )} ]" +
                            $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                            $"\nwas completed successfull...\n" +
                            $"\nThe Stored Procedure script returned a value of [ {Count . ToString ( )} ] so a table operation appears to have been successful\n\n" +
                            $"Hit Escape to hide this results panel and display the S.Procedure Scripts viewer.\n\n\n\n\n\n";
                    }
                    else
                    {
                        ResultsTextbox . Text = $"Execution of Stored procedure [ {SqlCommand . ToUpper ( )} ]\nusing [ {commandtype . ToUpper ( )} ] \n" +
                            $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                            $"\nwas completed successfully..." +
                            $"\nHowever, the Stored Procedure script did not return a value \n\n" +
                            $"Hit Escape to hide this results panel and display the S.Procedure Scripts viewer.\n\n\n\n\n\n";
                    }
                    ResultsTextbox . Background = FindResource ( "Green5" ) as SolidColorBrush;
                    ResultsTextbox . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
                    Utils2 . DoSuccessBeep ( );
                }
                else if ( commandtype == cmdstrings [ 5 ] . Trim ( ) )
                {
                    //Display an unknown proessing result
                    // try to create a datagrid
                    //ObservableCollection<GenericClass>  querytable = new();
                    //querytable = CreateCollectionFromDynamic ( dyn );
                    //if ( querytable != null )
                    //{ }

                    ShowResultsTextbox ( );
                    if ( Count > 0 )
                    {
                        ResultsTextbox . Text = $"Execution of Stored procedure [ {SqlCommand . ToUpper ( )} \nusing [ {commandtype . ToUpper ( )} ]" +
                            $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                            $"\nhas been processed successfully...\n" +
                            $"\nThe Execution did not appear to return any data, but it did return a count of {Count} which indicates a table was processed successfully.\n\n" +
                            $"Hit Escape to hide this results panel and display the S.Procedure Scripts viewer.\n\n\n\n\n\n";
                    }
                    else
                    {
                        ResultsTextbox . Text = $"Execution of Stored procedure [ {SqlCommand . ToUpper ( )} ]\nusing [ {commandtype . ToUpper ( )} ] \n" +
                            $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                            $"\nhas been processed successfully...\n" +
                            $"\nHowever, the Execution did not appear to return any data, and did not return any item count .\n\n" +
                            $"Hit Escape to hide this results panel and display the S.Procedure Scripts viewer.\n\n\n\n\n\n";
                    }
                    ResultsTextbox . Background = FindResource ( "Green5" ) as SolidColorBrush;
                    ResultsTextbox . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
                    Utils2 . DoSuccessBeep ( );
                }
            }
        }
        private void ShowErrorPanel ( object Obj, int Count, string Err )
        {
            // Hadle ALL and any errors post S.P Execution by showing the Result TextBlock panel
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            if ( TextResult . Visibility == VISIBLE )
                TextResult . Visibility = COLLAPSED;
            //else if ( EditPanel . Visibility == VISIBLE )
            //    EditPanel . Visibility = COLLAPSED;
            ////// make results Listbox ViSIBLE
            ShowRt = true;
            ResultsTextbox . Background = FindResource ( "Yellow1" ) as SolidColorBrush;
            ResultsTextbox . Foreground = FindResource ( "Blue2" ) as SolidColorBrush;
            ResultsTextbox . Visibility = Visibility . Visible;
            Splitter_DragCompleted ( null, null );

            //if ( SprocCreationGrid . Visibility == VISIBLE )
            //SprocCreationGrid . Visibility = COLLAPSED;

            if ( Err == "" )
            {
                ResultsTextbox . Text = $"**** EXECUTION ERROR MESSAGE *** :\n\nExecution of Stored Procedure {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} " +
                    $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                    $"was completed without any reported errors, but it failed to return any resulting items ?\n\n" +
                    $"This may be because the Table(s) that were accessed were  Empty, or perhaps you passed one or more incorrect Arguments to the S.Procedure," +
                    $"or finally it is always possible that the actual S.Procedure is faulty in some way ?\n\n" +
                    $"You can Hit ESCAPE to close this Results panel, or right click to choose from the many Context Menu Options...\n\n\n\n\n\n";

                ExecResult . Background = FindResource ( "Blue4" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;

                StatusText = "No return value - Suggest you try a different  Execution Method !";
                ExecResult . UpdateLayout ( );
                //               ResultsTextHeight = ResultsTextbox . ActualHeight;
            }
            else
            {
                if ( Err [ 0 ] == '\n' ) Err = Err . Substring ( 1, Err . Length - 1 );
                if ( Err [ Err . Length - 1 ] == '\n' ) Err = Err . Substring ( 0, Err . Length - 1 );
                ResultsTextbox . Text = $"**** EXECUTION ERROR MESSAGE *** :\nExecution of Stored Procedure {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} " +
                    $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                    $"has reported an error as shown below :-\n[{Err}]\n\n" +
                    $"Please correct the reported problem if that is possible, but if it appears to be an error in the script, report the error message shown above to your Data Administrator.\n\n" +
                    $"You can Hit ESCAPE to close this Results panel, or right click to choose from the many Context Menu Options...\n\n\n\n\n\n";

                //                ResultsTextHeight = ResultsTextbox . ActualHeight;
            }
            //Thickness   th = new();
            //th . Top = 3;
            //ResultsTextbox . Margin = th;
            ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
            ResultsContainerTextblock . Visibility = VISIBLE;
            ResultsTextbox . Visibility = VISIBLE;
            ResultsTextbox . UpdateLayout ( );
            Utils2 . DoErrorBeep ( );
        }
        private void ShowNonErrorPanel ( object Obj, int Count, string Err )
        {
            // Hadle ALL and any results that have SUCCESS and no errors post S.P Execution by showing the Result TextBlock panel
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            if ( TextResult . Visibility == VISIBLE )
                TextResult . Visibility = COLLAPSED;
            else if ( EditPanel . Visibility == VISIBLE )
                EditPanel . Visibility = COLLAPSED;
            //// make results Listbox ViSIBLE
            ShowRt = true;
            TextResultsBanner . Text = "Results - Successful Execution !";
            ResultsTextbox . Background = FindResource ( "White4" ) as SolidColorBrush;
            ResultsTextbox . Foreground = FindResource ( "Blue2" ) as SolidColorBrush;
            ResultsTextbox . Visibility = Visibility . Visible;
            Splitter_DragCompleted ( null, null );

            //if ( SprocCreationGrid . Visibility == VISIBLE )
            //SprocCreationGrid . Visibility = COLLAPSED;
            ResultsTextbox . Text = $"****SUCCESSFULL EXECUTION MESSAGE *** :\n\nExecution of Stored Procedure {SProcsListbox . SelectedItem . ToString ( ) . ToUpper ( )} " +
                $"\nusing the Arguments you provided of [{SPArguments . Text . ToUpper ( )}]" +
                $"was completed without any reported errors.\n\n" +
                $"No results of any type were returned, which indicates that the operation concerned was completed successfully.\n\n" +
                $"You can Hit ESCAPE to close this Results panel, or right click to choose from the many Context Menu Options...\n\n\n\n\n\n";

            ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
            ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            StatusText = "No return value - Perhaps a different  Execution Method ?";
            ExecResult . UpdateLayout ( );
            Thickness   th = new();
            th . Top = 3;
            ResultsTextbox . Margin = th;
            ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
            //            icon1 . BringIntoView ( );
            Utils2 . DoSuccessBeep ( );
            return;
        }

        private void ShowResultsTextbox ( )
        {
            // Show Results TextBlock
            if ( ResultsContainerDatagrid . Visibility == VISIBLE )
                ResultsContainerDatagrid . Visibility = COLLAPSED;
            ResultsContainerListbox . Visibility = COLLAPSED;
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            if ( ResultsTextbox . Visibility == VISIBLE )
                ResultsTextbox . Visibility = COLLAPSED;
            ResultsTextbox . Visibility = Visibility . Visible;
            ResultsContainerDatagrid . Visibility = Visibility . Visible;
            ResultsContainerDatagrid . UpdateLayout ( );
            ResultsTextbox . FontSize = MainWindow . ScrollViewerFontSize;
            ResultsTextbox . Focus ( );
            Splitter_DragCompleted ( null, null );
        }
        private void ExecListbox_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
            MouseRightButtonDown = true;
            e . Handled = true;
        }

        #endregion ExecListBox Commands mouse handlers

        #endregion  END of  Stored Procedure Execution handers

        #region General supporting methods
        private void LoadSqlEditData ( object sender, RoutedEventArgs e )
        {
            InitDataEditWin ( CurrentDbName, dglayoutlist, SPDatagrid . SelectedIndex );
        }
        public void InitDataEditWin ( string SqlTable, List<DataGridLayout> Dglayoutlist, int currentindex )
        {
            int newweight = 0;
            CreateEditList ( CurrentSelectionIndex );
            // OriginalRecordData holds ORIGINAL data from record
            // so make true copy in datalist
            //         DuplicateRecordData = ObjectCopier . Clone ( OriginalRecordData );
            dglayoutlist = ObjectCopier . Clone ( Dglayoutlist );
            //           this . Topmost = true;
            CurrentDbName = SqlTable;
            reccount = OriginalRecordData . Count;
            // create cloned copy of original data so it doesn't get updated automatically
            UpdatedRecordData = ObjectCopier . Clone ( OriginalRecordData );
            currselection = currentindex;
            newweight = newweight = MainWindow . GetfontWeight ( "Normal" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            Data2 . SelectionLength = Data2 . Text . Length;
            Data2 . Select ( 0, Data2 . Text . Length );
            // clear all fields 1st off, it may be a different table we are reloading
            Clearfields ( );
            // enable all fields
            DisableNullFields ( true, true );
            // populate EdtPanel fields with current data
            populatedatafields ( dglayoutlist . Count );
            // disable unused fields
            DisableNullFields ( false );
        }
        private void populatedatafields ( int totalrecs )
        {
            int newweight = 0;
            // put record data into edit fields and fill a GenericClass object (OriginalRecord) with it as well
            for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        label1 . Content = dglayoutlist [ 0 ] . Fieldname;
                        Data1 . Text = OriginalRecordData [ x ];
                        Data1 . IsEnabled = true;
                        gclass . field1 = OriginalRecordData [ x ];
                        if ( dglayoutlist [ 0 ] . fieldname . ToUpper ( ) == "ID" )
                        {
                            label1 . Content = "(Automatic value)";
                            label1 . Foreground = FindResource ( "Orange2" ) as SolidColorBrush;
                            newweight = MainWindow . GetfontWeight ( "DemiBold" );
                            label1 . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
                        }
                        break;
                    case 1:
                        label2 . Content = dglayoutlist [ 1 ] . Fieldname;
                        Data2 . Text = OriginalRecordData [ x ];
                        Data2 . IsEnabled = true;
                        gclass . field2 = OriginalRecordData [ x ];
                        break;
                    case 2:
                        label3 . Content = dglayoutlist [ 2 ] . Fieldname;
                        Data3 . Text = OriginalRecordData [ x ];
                        Data3 . IsEnabled = true;
                        gclass . field3 = OriginalRecordData [ x ];
                        break;
                    case 3:
                        label4 . Content = dglayoutlist [ 3 ] . Fieldname;
                        Data4 . Text = OriginalRecordData [ x ];
                        Data4 . IsEnabled = true;
                        gclass . field4 = OriginalRecordData [ x ];
                        break;
                    case 4:
                        label5 . Content = dglayoutlist [ 4 ] . Fieldname;
                        Data5 . Text = OriginalRecordData [ x ];
                        Data5 . IsEnabled = true;
                        gclass . field5 = OriginalRecordData [ x ];
                        break;
                    case 5:
                        label6 . Content = dglayoutlist [ 5 ] . Fieldname;
                        Data6 . Text = OriginalRecordData [ x ];
                        Data6 . IsEnabled = true;
                        gclass . field6 = OriginalRecordData [ x ];
                        break;
                    case 6:
                        label7 . Content = dglayoutlist [ 6 ] . Fieldname;
                        Data7 . Text = OriginalRecordData [ x ];
                        Data7 . IsEnabled = true;
                        gclass . field7 = OriginalRecordData [ x ];
                        break;
                    case 7:
                        label8 . Content = dglayoutlist [ 7 ] . Fieldname;
                        Data8 . Text = OriginalRecordData [ x ];
                        Data8 . IsEnabled = true;
                        gclass . field8 = OriginalRecordData [ x ];
                        break;
                    case 8:
                        label9 . Content = dglayoutlist [ 8 ] . Fieldname;
                        Data9 . Text = OriginalRecordData [ x ];
                        Data9 . IsEnabled = true;
                        gclass . field9 = OriginalRecordData [ x ];
                        break;
                    case 9:
                        label10 . Content = dglayoutlist [ 9 ] . Fieldname;
                        Data10 . Text = OriginalRecordData [ x ];
                        Data10 . IsEnabled = true;
                        gclass . field10 = OriginalRecordData [ x ];
                        break;
                    case 10:
                        label11 . Content = dglayoutlist [ 10 ] . Fieldname;
                        Data11 . Text = OriginalRecordData [ x ];
                        Data11 . IsEnabled = true;
                        gclass . field11 = OriginalRecordData [ x ];
                        break;
                    case 11:
                        label12 . Content = dglayoutlist [ 11 ] . Fieldname;
                        Data12 . Text = OriginalRecordData [ x ];
                        Data12 . IsEnabled = true;
                        gclass . field12 = OriginalRecordData [ x ];
                        break;
                    case 12:
                        label13 . Content = dglayoutlist [ 12 ] . Fieldname;
                        Data13 . Text = OriginalRecordData [ x ];
                        Data13 . IsEnabled = true;
                        gclass . field13 = OriginalRecordData [ x ];
                        break;
                    case 13:
                        label14 . Content = dglayoutlist [ 13 ] . Fieldname;
                        Data14 . Text = OriginalRecordData [ x ];
                        Data14 . IsEnabled = true;
                        gclass . field14 = OriginalRecordData [ x ];
                        break;
                    case 14:
                        label15 . Content = dglayoutlist [ 14 ] . Fieldname;
                        Data15 . Text = OriginalRecordData [ x ];
                        Data15 . IsEnabled = true;
                        gclass . field15 = OriginalRecordData [ x ];
                        break;
                    case 15:
                        label16 . Content = dglayoutlist [ 15 ] . Fieldname;
                        Data16 . Text = OriginalRecordData [ x ];
                        Data16 . IsEnabled = true;
                        gclass . field16 = OriginalRecordData [ x ];
                        break;
                    case 16:
                        label17 . Content = dglayoutlist [ 16 ] . Fieldname;
                        Data17 . Text = OriginalRecordData [ x ];
                        Data17 . IsEnabled = true;
                        gclass . field17 = OriginalRecordData [ x ];
                        break;
                    case 17:
                        label18 . Content = dglayoutlist [ 17 ] . Fieldname;
                        Data18 . Text = OriginalRecordData [ x ];
                        Data18 . IsEnabled = true;
                        gclass . field18 = OriginalRecordData [ x ];
                        break;
                    case 18:
                        label19 . Content = dglayoutlist [ 18 ] . Fieldname;
                        Data19 . Text = OriginalRecordData [ x ];
                        Data19 . IsEnabled = true;
                        gclass . field19 = OriginalRecordData [ x ];
                        break;
                    case 19:
                        label20 . Content = dglayoutlist [ 19 ] . Fieldname;
                        Data20 . Text = OriginalRecordData [ x ];
                        Data20 . IsEnabled = true;
                        gclass . field20 = OriginalRecordData [ x ];
                        break;
                }
            }
            for ( int y = reccount + 1 ; y <= 20 ; y++ )
            {
                switch ( y )
                {
                    case 1:
                        label1 . IsEnabled = false;
                        break;
                    case 2:
                        label2 . IsEnabled = false;
                        break;
                    case 3:
                        label3 . IsEnabled = false;
                        break;
                    case 4:
                        label4 . IsEnabled = false;
                        break;
                    case 5:
                        label5 . IsEnabled = false;
                        break;
                    case 6:
                        label6 . IsEnabled = false;
                        break;
                    case 7:
                        label7 . IsEnabled = false;
                        break;
                    case 8:
                        label8 . IsEnabled = false;
                        break;
                    case 9:
                        label9 . IsEnabled = false;
                        break;
                    case 10:
                        label10 . IsEnabled = false;
                        break;
                    case 11:
                        label11 . IsEnabled = false;
                        break;
                    case 12:
                        label12 . IsEnabled = false;
                        break;
                    case 13:
                        label13 . IsEnabled = false;
                        break;
                    case 14:
                        label14 . IsEnabled = false;
                        break;
                    case 15:
                        label15 . IsEnabled = false;
                        break;
                    case 16:
                        label16 . IsEnabled = false;
                        break;
                    case 17:
                        label17 . IsEnabled = false;
                        break;
                    case 18:
                        label18 . IsEnabled = false;
                        break;
                    case 19:
                        label19 . IsEnabled = false;
                        break;
                    case 20:
                        label20 . IsEnabled = false;
                        break;
                }
                bdirty = false;
            }
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            newweight = MainWindow . GetfontWeight ( "Normal" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
        }
        private void Closewin ( object sender, RoutedEventArgs e )
        {
            int newweight = 0;
            Mouse . OverrideCursor = Cursors . Wait;
            editprompt . Text = "Just checking for unsaved  data changes ...";
            editprompt . UpdateLayout ( );
            GenericClass gc = new GenericClass();
            if ( bdirty && CheckForChanges ( "", -1, true ) == false )
            {
                Mouse . OverrideCursor = Cursors . Arrow;
                MessageBoxResult mbr = MessageBox.Show("There are changes in the current data being edited.\n\nDo you want to save them ?", "Unsaved changes ?", MessageBoxButton.YesNo);
                if ( mbr == MessageBoxResult . Yes )
                {
                    // create new list of data in newlist
                    GetUpdatedRecordData ( );
                    UpdateDb ( );
                    OriginalRecordData = ObjectCopier . Clone ( UpdatedRecordData );
                }
                else
                {
                    StatusText = "Changes were made to data but they have NOT been saved !";
                    newweight = MainWindow . GetfontWeight ( "Bold" );
                    if ( newweight != -1 )
                    {
                        editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
                        editprompt . Background = FindResource ( "Red5" ) as SolidColorBrush;
                        editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                    }
                }
                RefreshDatagrid ( CurrentDbName );
            }
            this . Close ( );
            Mouse . OverrideCursor = Cursors . Arrow;
        }
        private bool UpdateDb ( )
        {
            int currsel = SPDatagrid.SelectedIndex;
            Mouse . OverrideCursor = Cursors . Wait;
            List<string> newdata = GetUpdatedRecordData();
            for ( int x = 0 ; x < newdata . Count ; x++ )
            {
                if ( x == 0 )
                    newgclass . field1 = newdata [ x ];
                if ( x == 1 )
                    newgclass . field2 = newdata [ x ];
                if ( x == 2 )
                    newgclass . field3 = newdata [ x ];
                if ( x == 3 )
                    newgclass . field4 = newdata [ x ];
                if ( x == 4 )
                    newgclass . field5 = newdata [ x ];
                if ( x == 5 )
                    newgclass . field6 = newdata [ x ];
                if ( x == 6 )
                    newgclass . field7 = newdata [ x ];
                if ( x == 7 )
                    newgclass . field8 = newdata [ x ];
                if ( x == 8 )
                    newgclass . field9 = newdata [ x ];
                if ( x == 9 )
                    newgclass . field10 = newdata [ x ];
                if ( x == 1 )
                    newgclass . field11 = newdata [ x ];
                if ( x == 11 )
                    newgclass . field12 = newdata [ x ];
                if ( x == 12 )
                    newgclass . field13 = newdata [ x ];
                if ( x == 13 )
                    newgclass . field14 = newdata [ x ];
                if ( x == 14 )
                    newgclass . field15 = newdata [ x ];
                if ( x == 15 )
                    newgclass . field16 = newdata [ x ];
                if ( x == 16 )
                    newgclass . field17 = newdata [ x ];
                if ( x == 17 )
                    newgclass . field18 = newdata [ x ];
                if ( x == 18 )
                    newgclass . field19 = newdata [ x ];
                if ( x == 19 )
                    newgclass . field20 = newdata [ x ];

            }
            CreateNewSqlUpdateCommand ( UpdatedRecordData, newgclass );
            SProcsHandling . SqlCommand += $" where {dglayoutlist [ 0 ] . Fieldname}={UpdatedRecordData [ 0 ]}";
            Debug . WriteLine ( $"{SProcsHandling . SqlCommand}	" );

            if ( UpdateSqlTable ( SProcsHandling . SqlCommand ) == true )
            {
                bdirty = false;
                currsel = SPDatagrid . SelectedIndex;
                if ( currsel == -1 )
                    currsel = 0;
                RefreshDatagrid ( CurrentDbName );
                SPDatagrid . SelectedIndex = currsel;
                Utils2 . DoSuccessBeep ( 2 );
                SPDatagrid . UpdateLayout ( );
                editprompt . Text = $"The Sql Table [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] has been updated successfuly....";
                editprompt . Background = FindResource ( "Purple4" ) as SolidColorBrush;
                editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
            else
            {
                editprompt . Text = $"The Sql Table [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] was NOT updated ....";
                editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
                editprompt . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
            Mouse . OverrideCursor = Cursors . Arrow;
            return true;
        }
        public string Validatechars ( string dat )
        {
            string newstring = "";
            string validchars = "0123456789";
            if ( dat . Contains ( "'" ) )
                for ( int x = 0 ; x < dat . Length ; x++ )
                {
                    char dc = (char)dat[x];
                    if ( validchars . Contains ( dc ) == true )
                        newstring += dc;
                }
            else
                newstring = dat;
            dat = newstring;
            return newstring;
        }
        private List<string> GetUpdatedRecordData ( )
        {
            string dat = "";
            List<string> UpdatedRecordData = new List<string>();

            // Create UpdatedRecordData  list and genericclass with updated data
            for ( int x = 0 ; x < reccount ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        dat = Data1 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field1 = dat;
                        break;
                    case 1:
                        dat = Data2 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field2 = dat;
                        break;
                    case 2:
                        dat = Data3 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field3 = dat;
                        break;
                    case 3:
                        dat = Data4 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field4 = dat;
                        break;
                    case 4:
                        dat = Data5 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field5 = dat;
                        break;
                    case 5:
                        dat = Data6 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field6 = dat;
                        break;
                    case 6:
                        dat = Data7 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field7 = dat;
                        break;
                    case 7:
                        dat = Data8 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field8 = dat;
                        break;
                    case 8:
                        dat = Data9 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field9 = dat;
                        break;
                    case 9:
                        dat = Data10 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field10 = dat;
                        break;
                    case 10:
                        dat = Data11 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field11 = dat;
                        break;
                    case 11:
                        dat = Data12 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field12 = dat;
                        break;
                    case 12:
                        dat = Data13 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field13 = dat;
                        break;
                    case 13:
                        dat = Data14 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field14 = dat;
                        break;
                    case 14:
                        dat = Data15 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field15 = dat;
                        break;
                    case 15:
                        dat = Data16 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field16 = dat;
                        break;
                    case 16:
                        dat = Data17 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field17 = dat;
                        break;
                    case 17:
                        dat = Data18 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field18 = dat;
                        break;
                    case 18:
                        dat = Data19 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( dat );
                        newgclass . field19 = dat;
                        break;
                    case 19:
                        dat = Data20 . Text;
                        dat = Validatechars ( dat );
                        if ( dat == "" )
                            break;
                        if ( dat . Contains ( "/" ) )
                        {
                            dat = ConvertDataToSql ( dat );
                            UpdatedRecordData . Add ( dat );
                        }
                        else
                            UpdatedRecordData . Add ( Data20 . Text );
                        newgclass . field20 = Data20 . Text;
                        break;
                }
            }
            return UpdatedRecordData;
        }
        private void DisableAllfields ( )
        {
            for ( int x = reccount ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . IsEnabled = false;
                        break;
                    case 1:
                        Data2 . IsEnabled = false;
                        break;
                    case 2:
                        Data3 . IsEnabled = false;
                        break;
                    case 3:
                        Data4 . IsEnabled = false;
                        break;
                    case 4:
                        Data5 . IsEnabled = false;
                        break;
                    case 5:
                        Data6 . IsEnabled = false;
                        break;
                    case 6:
                        Data7 . IsEnabled = false;
                        break;
                    case 7:
                        Data8 . IsEnabled = false;
                        break;
                    case 8:
                        Data9 . IsEnabled = false;
                        break;
                    case 9:
                        Data10 . IsEnabled = false;
                        break;
                    case 10:
                        Data11 . IsEnabled = false;
                        break;
                    case 11:
                        Data12 . IsEnabled = false;
                        break;
                    case 12:
                        Data13 . IsEnabled = false;
                        break;
                    case 13:
                        Data14 . IsEnabled = false;
                        break;
                    case 14:
                        Data15 . IsEnabled = false;
                        break;
                    case 15:
                        Data16 . IsEnabled = false;
                        break;
                    case 16:
                        Data17 . IsEnabled = false;
                        break;
                    case 17:
                        Data18 . IsEnabled = false;
                        break;
                    case 18:
                        Data19 . IsEnabled = false;
                        break;
                    case 19:
                        Data20 . IsEnabled = false;
                        break;
                }
            }
        }
        public void CreateEditList ( int currselection )
        {
            if ( currselection == -1 )
                return;
            List<string> list = new();
            SPDatagrid . SelectedItem = currselection;
            GenericClass gc = SPDatagrid.Items[currselection] as GenericClass;
            if ( gc . field1 != null )
            {
                list . Add ( gc . field1 );
                label1 . IsEnabled = false;
            }
            else
                return;
            if ( gc . field2 != null )
                list . Add ( gc . field2 );
            else
            {
                label2 . IsEnabled = false;
            }
            if ( gc . field3 != null )
                list . Add ( gc . field3 );
            else
            {
                label3 . IsEnabled = false;
            }
            if ( gc . field4 != null )
                list . Add ( gc . field4 );
            else
            {
                label4 . IsEnabled = false;
            }
            if ( gc . field5 != null )
                list . Add ( gc . field5 );
            else
            {
                label5 . IsEnabled = false;
            }
            if ( gc . field6 != null )
                list . Add ( gc . field6 );
            else
            {
                label6 . IsEnabled = false;
            }
            if ( gc . field7 != null )
                list . Add ( gc . field7 );
            else
            {
                label7 . IsEnabled = false;
            }
            if ( gc . field8 != null )
                list . Add ( gc . field8 );
            else
            {
                label8 . IsEnabled = false;
            }
            if ( gc . field9 != null )
                list . Add ( gc . field9 );
            else
            {
                label9 . IsEnabled = false;
            }
            if ( gc . field10 != null )
                list . Add ( gc . field10 );
            else
            {
                label10 . IsEnabled = false;
            }


            if ( gc . field11 != null )
                list . Add ( gc . field11 );
            else
            {
                label11 . IsEnabled = false;
            }
            if ( gc . field12 != null )
                list . Add ( gc . field12 );
            else
            {
                label12 . IsEnabled = false;
            }
            if ( gc . field13 != null )
                list . Add ( gc . field13 );
            else
            {
                label13 . IsEnabled = false;
            }
            if ( gc . field14 != null )
                list . Add ( gc . field14 );
            else
            {
                label14 . IsEnabled = false;
            }
            if ( gc . field15 != null )
                list . Add ( gc . field15 );
            else
            {
                label15 . IsEnabled = false;
            }
            if ( gc . field16 != null )
                list . Add ( gc . field16 );
            else
            {
                label16 . IsEnabled = false;
            }
            if ( gc . field17 != null )
                list . Add ( gc . field17 );
            else
            {
                label17 . IsEnabled = false;
            }
            if ( gc . field18 != null )
                list . Add ( gc . field18 );
            else
            {
                label18 . IsEnabled = false;
            }
            if ( gc . field19 != null )
                list . Add ( gc . field19 );
            else
            {
                label19 . IsEnabled = false;
            }
            if ( gc . field20 != null )
                list . Add ( gc . field20 );
            else
            {
                label20 . IsEnabled = false;
            }
            //}
            OriginalRecordData = list;
            // create copy of original data for any changes  to be made  to
            UpdatedRecordData = list;
            populatedatafields ( dglayoutlist . Count );
        }
        private void Savedata ( object sender, RoutedEventArgs e )
        {
            // save data to sql table
            UpdateDb ( );
            bdirty = false;
        }
        public string ConvertDataToSql ( string datestring )
        {
            if ( datestring . Contains ( "/" ) )
            {
                string[] parts = datestring.Split(" ");
                string[] dateparts = parts[0].Split("/");
                string newdate = $"'{dateparts[2]}/{dateparts[1]}/{dateparts[0]}'";// {parts[1]}'";
                return newdate;
            }
            else
                return datestring;
        }
        private void NextRecord ( object sender, RoutedEventArgs e )
        {
            if ( currselection < SPDatagrid . Items . Count )
            {
                currselection++;
                SPDatagrid . SelectedIndex = currselection;
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
                CreateEditList ( currselection );
            }
            editprompt . Text = "Next record shown ... ";
            editprompt . Background = FindResource ( "Purple3" ) as SolidColorBrush;
            editprompt . Foreground = Brushes . White;
            bdirty = false;
        }
        private void PreviousRecord ( object sender, RoutedEventArgs e )
        {
            if ( currselection > 0 )
            {
                currselection--;
                SPDatagrid . SelectedIndex = currselection;
                SPDatagrid . ScrollIntoView ( SPDatagrid . SelectedItem );
                CreateEditList ( currselection );
            }
            editprompt . Text = "Previous record shown ... ";
            editprompt . Background = FindResource ( "Green3" ) as SolidColorBrush;
            editprompt . Foreground = Brushes . White;
            bdirty = false;
        }
        private void AddRecord ( object sender, RoutedEventArgs e )
        {
            GenericClass gc = new();
            for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
            {
                Data1 . IsEnabled = false;
                Data1 . Text = "Automatic";
                if ( x >= 1 )
                {
                    Data2 . Text = "";
                    Data2 . IsEnabled = true;
                }
                if ( x >= 2 )
                {
                    Data3 . Text = "";
                    Data3 . IsEnabled = true;
                }
                if ( x >= 3 )
                {
                    Data4 . Text = "";
                    Data4 . IsEnabled = true;
                }
                if ( x >= 4 )
                {
                    Data5 . Text = "";
                    Data5 . IsEnabled = true;
                }
                if ( x >= 5 )
                {
                    Data6 . Text = "";
                    Data6 . IsEnabled = true;
                }
                if ( x >= 6 )
                {
                    Data7 . Text = "";
                    Data7 . IsEnabled = true;
                }
                if ( x >= 7 )
                {
                    Data8 . Text = "";
                    Data8 . IsEnabled = true;
                }
                if ( x >= 8 )
                {
                    Data9 . Text = "";
                    Data9 . IsEnabled = true;
                }
                if ( x >= 19 )
                {
                    Data10 . Text = "";
                    Data10 . IsEnabled = true;
                }
                if ( x >= 10 )
                {
                    Data11 . Text = "";
                    Data11 . IsEnabled = true;
                }
                if ( x >= 11 )
                {
                    Data12 . Text = "";
                    Data12 . IsEnabled = true;
                }
                if ( x >= 12 )
                {
                    Data13 . Text = "";
                    Data13 . IsEnabled = true;
                }
                if ( x >= 13 )
                {
                    Data14 . Text = "";
                    Data14 . IsEnabled = true;
                }
                if ( x >= 14 )
                {
                    Data15 . Text = "";
                    Data15 . IsEnabled = true;
                }
                if ( x >= 15 )
                {
                    Data16 . Text = "";
                    Data16 . IsEnabled = true;
                }
                if ( x >= 16 )
                {
                    Data17 . Text = "";
                    Data17 . IsEnabled = true;
                }
                if ( x >= 17 )
                {
                    Data18 . Text = "";
                    Data18 . IsEnabled = true;
                }
                if ( x >= 18 )
                {
                    Data19 . Text = "";
                    Data19 . IsEnabled = true;
                }
                if ( x >= 19 )
                {
                    Data20 . Text = "";
                    Data20 . IsEnabled = true;
                }
                //               Data2 . Focus ( );

            }
            editprompt . Text = "Enter data for this new record";
            editprompt . Background = Brushes . LightYellow;
        }

        //++++++++++++++++++++++++++++++++//
        #endregion  END of  General supporting methods

        //        #region ContextMenu triggers
        //#################################//
        private void show_DataGrid ( object sender, RoutedEventArgs e )
        {
            // Switching to SQL tables view
            // configure display  for SQL Tables (only)
            SProcsListbox . Visibility = COLLAPSED;
            ExecList . Visibility = COLLAPSED;
            TextResult . Visibility = COLLAPSED;
            SPInfopanelGrid . Visibility = COLLAPSED;

            ArgumentsContainerGrid . Visibility = Visibility . Visible;
            SPDatagrid . Visibility = Visibility . Visible;
            EditPanel . Visibility = Visibility . Visible;
            ISGRIDVISIBLE = true;
            ShowDg = true;
            ShowSp = false;
            SPDatagrid . Visibility = Visibility . Visible;
            DgridInfo . Visibility = Visibility . Visible;
            Blanker . Visibility = Visibility . Visible;
            SetWindowTitleBar ( );
            ResetOptionsAccessColors ( );
            Splitter_DragCompleted ( null, null );
        }

        private void show_Sprocs ( object sender, RoutedEventArgs e )
        {
            // Switching to S.Proces view
            // configure display  for Stored Procedures (only)
            SPDatagrid . Visibility = COLLAPSED;
            DgridInfo . Visibility = COLLAPSED;
            Blanker . Visibility = COLLAPSED;
            EditPanel . Visibility = COLLAPSED;
            ArgumentsContainerGrid . Visibility = Visibility . Visible;
            SPInfopanelGrid . Visibility = Visibility . Visible;
            SProcsListbox . Visibility = Visibility . Visible;
            ExecList . Visibility = Visibility . Visible;
            TextResult . Visibility = Visibility . Visible;
            ISGRIDVISIBLE = false;
            ShowDg = false;
            ShowSp = true;
            SetWindowTitleBar ( );
            ResetOptionsAccessColors ( );
        }
        private void ResetOptionsAccessColors ( )
        {
            if ( ShowSp )
            {
                //Showing Stored Procs controls
                if ( IsLoading )
                    return;
                OptionControls . IsEnabled = true;
                ColorsCombo . IsEnabled = true;
                ColorsCombo . Opacity = 1.0;
                FontSizeCombo . IsEnabled = true;
                FontSizeCombo . Opacity = 1.0;
                FontSizeCombo . IsEnabled = true;
                GridCombo . IsEnabled = false;
                DVO . Opacity = 0.5;
                SCST . Opacity = 0.5;
                GridCombo . Opacity = 0.5;
                Scriptoptions . Opacity = 1.0;
                Hilitecolor . Opacity = 1.0;
                Blanker . Visibility = COLLAPSED;
            }
            else
            {
                //Showing Datagrid controls
                if ( IsLoading )
                    return;
                EditPanel . Opacity = 1.0;
                //useroptions . Opacity = 0.5;
                ColorsCombo . IsEnabled = false;
                ColorsCombo . Opacity = 0.5;
                FontSizeCombo . IsEnabled = true;
                FontSizeCombo . Opacity = 1.0;
                //colors combo
                GridCombo . IsEnabled = true;
                Scriptoptions . Opacity = 0.5;
                Hilitecolor . Opacity = 0.5;
                // Datagrid cobo
                DVO . Opacity = 1.0;
                SCST . Opacity = 1.0;
                GridCombo . Opacity = 1.0;
                OptionControls . IsEnabled = true;
                Blanker . Visibility = Visibility . Visible;
                if ( Data1 . Text == "" )
                {
                    SPDatagrid . SelectedIndex = 0;
                    SPDatagrid . SelectedItem = 0;
                    CreateEditList ( 0 );
                    populatedatafields ( SPDatagrid . Items . Count );
                    DisableNullFields ( false );
                }
            }
        }

        private void SetWindowTitleBar ( )
        {
            if ( ISGRIDVISIBLE )
            {
                ListTitle . Visibility = COLLAPSED;
                ExecTitle . Visibility = COLLAPSED;
                DgridInfo . Visibility = Visibility . Visible;
                if ( GridCombo . Items . Count > 0 )
                    DgridInfo . Text = $"Current SQL Table : [{GridCombo . SelectedItem . ToString ( ) . ToUpper ( )}] Total Records = {SqlTable . Count}";
                ExecTitle . Text = "";
            }
            else
            {
                DgridInfo . Visibility = COLLAPSED;
                ListTitle . Visibility = Visibility . Visible;
                ExecTitle . Visibility = Visibility . Visible;
                ListTitle . Text = $"All Stored Procedures - ({SProcsListbox . Items . Count})";
                ExecTitle . Text = "Available Execution Methods";
            }
        }


        #region UTILITY SUPPORT METHODS

        private void DisableNullFields ( bool doEnable, bool doall = false )
        {
            int startval = 1;
            if ( doall == false )
                startval = OriginalRecordData . Count;

            for ( int x = startval ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . IsEnabled = doEnable;
                        break;
                    case 1:
                        Data2 . IsEnabled = doEnable;
                        break;
                    case 2:
                        Data3 . IsEnabled = doEnable;
                        break;
                    case 3:
                        Data4 . IsEnabled = doEnable;
                        break;
                    case 4:
                        Data5 . IsEnabled = doEnable;
                        break;
                    case 5:
                        Data6 . IsEnabled = doEnable;
                        break;
                    case 6:
                        Data7 . IsEnabled = doEnable;
                        break;
                    case 7:
                        Data8 . IsEnabled = doEnable;
                        break;
                    case 8:
                        Data9 . IsEnabled = doEnable;
                        break;
                    case 9:
                        Data10 . IsEnabled = doEnable;
                        break;
                    case 10:
                        Data11 . IsEnabled = doEnable;
                        break;
                    case 11:
                        Data12 . IsEnabled = doEnable;
                        break;
                    case 12:
                        Data13 . IsEnabled = doEnable;
                        break;
                    case 13:
                        Data14 . IsEnabled = doEnable;
                        break;
                    case 14:
                        Data15 . IsEnabled = doEnable;
                        break;
                    case 15:
                        Data16 . IsEnabled = doEnable;
                        break;
                    case 16:
                        Data17 . IsEnabled = doEnable;
                        break;
                    case 17:
                        Data18 . IsEnabled = doEnable;
                        break;
                    case 18:
                        Data19 . IsEnabled = doEnable;
                        break;
                    case 19:
                        Data20 . IsEnabled = doEnable;
                        break;
                }
            }
        }
        private void Clearfields ( )
        {
            for ( int x = 0 ; x < 20 ; x++ )
            {
                switch ( x )
                {
                    case 0:
                        Data1 . Text = "";
                        break;
                    case 1:
                        Data2 . Text = "";
                        break;
                    case 2:
                        Data3 . Text = "";
                        break;
                    case 3:
                        Data4 . Text = "";
                        break;
                    case 4:
                        Data5 . Text = "";
                        break;
                    case 5:
                        Data6 . Text = "";
                        break;
                    case 6:
                        Data7 . Text = "";
                        break;
                    case 7:
                        Data8 . Text = "";
                        break;
                    case 8:
                        Data9 . Text = "";
                        break;
                    case 9:
                        Data10 . Text = "";
                        break;
                    case 10:
                        Data11 . Text = "";
                        break;
                    case 11:
                        Data12 . Text = "";
                        break;
                    case 12:
                        Data13 . Text = "";
                        break;
                    case 13:
                        Data14 . Text = "";
                        break;
                    case 14:
                        Data15 . Text = "";
                        break;
                    case 15:
                        Data16 . Text = "";
                        break;
                    case 16:
                        Data17 . Text = "";
                        break;
                    case 17:
                        Data18 . Text = "";
                        break;
                    case 18:
                        Data19 . Text = "";
                        break;
                    case 19:
                        Data20 . Text = "";
                        break;
                }
            }
        }

        //#################################//
        #endregion END of  UTILITY SUPPORT METHODS

        /// <summary>
        /// Shows the "TextResults" Scroll viewer panel 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void show_Spscript ( object sender, RoutedEventArgs e )
        {
            //Force display of the contents of the currently selected script
            ResultsContainerDatagrid . Visibility = COLLAPSED;
            ResultsContainerListbox . Visibility = COLLAPSED;
            ResultsContainerTextblock . Visibility = COLLAPSED;
            EditPanel . Visibility = COLLAPSED;
            TextResult . Visibility = Visibility . Visible;
        }

        #region main window menu Handlers
        //++++++++++++++++++++++++++++++++//

        private void Menu_MouseEnter ( object sender, MouseEventArgs e )
        {
            Type type = sender.GetType();
            if ( type == typeof ( Menu ) )
            {
                Menu mi = sender as Menu;
                if ( mi == null ) return;
                SphMenuControl spm = new SphMenuControl (this, AllowSplitterReset);
                if ( mi . Name == "viewsmenu" )
                    spm . ViewsMenuOpening ( sender, e, "ViewsMenuOpening" );
                if ( mi . Name == "optsmenu" )
                    spm . OptionsMenuOpening ( sender, e, "OptionsMenuOpening" );
                if ( mi . Name == "scriptsmenu" )
                    spm . ScriptsMenuOpening ( sender, e, "ScripsMenuOpening" );
                if ( mi . Name == "Helpmenu" )
                    spm . HelpMenuOpening ( sender, e, "HelpMenu" );
            }
            else if ( type == typeof ( MenuItem ) )
            {
                MenuItem mi = sender as MenuItem;
                if ( mi == null ) return;
                SphMenuControl spm = new SphMenuControl (this, AllowSplitterReset);
                if ( mi . Name == "viewsmenu" )
                    spm . ViewsMenuOpening ( sender, e, "ViewsMenuOpening" );
                if ( mi . Name == "optsmenu" )
                    spm . OptionsMenuOpening ( sender, e, "OptionsMenuOpening" );
                if ( mi . Name == "scriptsmenu" )
                    spm . ScriptsMenuOpening ( sender, e, "ScripsMenuOpening" );
                if ( mi . Name == "Helpmenu" )
                    spm . HelpMenuOpening ( sender, e, "HelpMenu" );
            }
        }

        private void MainWinmenu_MouseLeave ( object sender, MouseEventArgs e )
        {
            //MainWinmenu . Background = Brushes . Transparent;
        }

        private void Close_Click ( object sender, RoutedEventArgs e )
        {
            this . Close ( );
        }

        private void About_Click ( object sender, RoutedEventArgs e )
        {
            MessageBox . Show ( "I am working on this ...." );
        }

        private void ShowDgmenu_Click ( object sender, RoutedEventArgs e )
        {
            // Switch to SQL Table view from S.Procedure controls
            //if ( ISGRIDVISIBLE == false )
            //{
            //    SPDatagrid . Visibility = Visibility . Visible;
            //    TextResult . Visibility = COLLAPSED;
            //    SProcsListbox . Visibility = COLLAPSED;
            //    ShowSp = false;
            //    ShowDg = true;
            //    ExecList . Visibility = COLLAPSED;
            //    EditPanel . Visibility = Visibility . Visible;
            //    Blanker . Visibility = Visibility . Visible;
            //    ArgumentsContainerGrid . Visibility = Visibility . Visible;
            //    ISGRIDVISIBLE = true;
            //    SPInfopanelGrid . Visibility = COLLAPSED;
            //    CurrentMenuitem = ShowDgmenu;
            //    double SqlDataArea = SPFullDataContainerGrid . ActualHeight;
            //    // Set lower  panel to FULL Edit panel height
            //    SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( DefEditpanelHeight, GridUnitType . Pixel );
            //    // store parameters for SQL controls
            //    if ( SPDatagrid . SelectedIndex == -1 )
            //        SPDatagrid . SelectedIndex = 0;
            //    // Hide reopen panel button
            //    EditPanel . Visibility = Visibility . Visible;
            //    if ( SPDatagrid . Items . Count == 0 )
            //    {
            //        RefreshDatagrid ( "BANKACCOUNT" );
            //    }
            //    SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            //    SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );

            //    ResetOptionsAccessColors ( );
            //}
        }

        private void ShowSpmenu_Click ( object sender, RoutedEventArgs e )
        {
            //if ( ISGRIDVISIBLE == true )
            //{
            //    SPDatagrid . Visibility = COLLAPSED;
            //    Blanker . Visibility = COLLAPSED;
            //    EditPanel . Visibility = COLLAPSED;

            //    TextResult . Visibility = Visibility . Visible;
            //    SProcsListbox . Visibility = Visibility . Visible;
            //    ExecList . Visibility = Visibility . Visible;
            //    ListTitle . Visibility = Visibility . Visible;
            //    SPInfopanelGrid . Visibility = Visibility . Visible;
            //    ArgumentsContainerGrid . Visibility = Visibility . Visible;
            //    SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0, GridUnitType . Pixel );
            //    SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2, GridUnitType . Pixel );
            //    // store parameters for SQL controls
            //    RowHeight0 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height . Value;
            //    Splitterlastpos = RowHeight0;
            //    SPFullDataContainerGrid . UpdateLayout ( );
            //    ISGRIDVISIBLE = false;
            //    CurrentMenuitem = ShowSpmenu;
            //    ShowSp = true;
            //    ShowDg = false;
            //    ResetOptionsAccessColors ( );
            //}
        }

        private void showSPSchema_Click ( object sender, RoutedEventArgs e )
        {
            string sprocname = SProcsListbox.SelectedItem.ToString();
            ExecList . SelectedIndex = 0;
            SpToUse = "spGetProcedureschema";
            string args = CurrentDbName;

            //dynamic result = ExecuteArgument ( SpToUse, SProcs, args, ref Count, ref ResultString, ref Obj, ref Objtype, ref Err, currentSp );

            ExecListbox_MouseLeftButtonDown ( null, null );
            ResultsDatagrid . Background = FindResource ( "Cyan2" ) as SolidColorBrush;
            ResultsDatagrid . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
        }

        //#################################//
        #endregion END of main window menu Handlers

        #region ALL SQL View Handlers

        private void Data_LostFocus ( object sender, RoutedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            string[] parts = new string[2];

            if ( tb == null )
                return;
            tb . SelectionLength = tb . Text . Length;
            string fldname = tb.Name;
            string text = tb.Text;
            parts = fldname . Split ( "Data" );
            int fieldindex = Convert.ToInt32(parts[1]);

            if ( CheckForChanges ( text, fieldindex ) == false )
            {
                editprompt . Text = "Data now contains 1 or more changed fields... ";
                editprompt . Background = FindResource ( "Red4" ) as SolidColorBrush;
                editprompt . Foreground = Brushes . White;
                bdirty = true;
            }
            tb = sender as TextBox;
            tb . Background = FindResource ( "White0" ) as SolidColorBrush;
            int newweight = MainWindow.GetfontWeight("Normal");
            tb . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            SetButtonColors ( false );

        }

        private void Data1_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Enter )
                UpdateList ( sender, null );
        }

        /// <summary>
        /// Crucial method to maintain the SQL table edit panel at 
        /// the right height  so all fields are visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="caller"></param>
        /// 

        private bool CheckForChanges ( string arg, int index, bool CheckAll = false )
        {
            //for (int x = 0 ; x < OriginalRecordData.Count ; x++)
            if ( OriginalRecordData . Count == 0 || arg == "" )
                return false;
            if ( CheckAll == false )
            {
                if ( OriginalRecordData [ ( index - 1 ) ] != arg )
                    return false;
            }
            else
            {
                for ( int x = 0 ; x < OriginalRecordData . Count ; x++ )
                {
                    if ( OriginalRecordData [ x ] != UpdatedRecordData [ x ] )
                    {
                        // ignore date felds
                        if ( OriginalRecordData [ x ] . Contains ( "/" ) == false )
                            return false;
                    }
                }
            }
            return true;
        }
        private void Resetdata ( object sender, RoutedEventArgs e )
        {
            int newweight = 0;
            populatedatafields ( dglayoutlist . Count );
            editprompt . Text = $"All entries have been reset to original values for you...";
            editprompt . Background = FindResource ( "Orange3" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Red3" ) as SolidColorBrush;
            newweight = MainWindow . GetfontWeight ( "Bold" );
            editprompt . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
        }

        private void UpdateList ( object sender, TextChangedEventArgs e )
        {
            TextBox tb = sender as TextBox;
            string name = tb.Name;
            string[] parts = new string[2];
            parts = name . Split ( "Data" );
            try
            {
                switch ( Convert . ToInt32 ( parts [ 1 ] ) )
                {
                    case 1:
                        UpdatedRecordData [ 0 ] = tb . Text;
                        break;
                    case 2:
                        UpdatedRecordData [ 1 ] = tb . Text;
                        break;
                    case 3:
                        UpdatedRecordData [ 2 ] = tb . Text;
                        break;
                    case 4:
                        UpdatedRecordData [ 3 ] = tb . Text;
                        break;
                    case 5:
                        UpdatedRecordData [ 4 ] = tb . Text;
                        break;
                    case 6:
                        UpdatedRecordData [ 5 ] = tb . Text;
                        break;
                    case 7:
                        UpdatedRecordData [ 6 ] = tb . Text;
                        break;
                    case 8:
                        UpdatedRecordData [ 7 ] = tb . Text;
                        break;
                    case 9:
                        UpdatedRecordData [ 8 ] = tb . Text;
                        break;
                    case 10:
                        UpdatedRecordData [ 9 ] = tb . Text;
                        break;
                    case 11:
                        UpdatedRecordData [ 10 ] = tb . Text;
                        break;
                    case 12:
                        UpdatedRecordData [ 11 ] = tb . Text;
                        break;
                    case 13:
                        UpdatedRecordData [ 12 ] = tb . Text;
                        break;
                    case 14:
                        UpdatedRecordData [ 13 ] = tb . Text;
                        break;
                    case 15:
                        UpdatedRecordData [ 14 ] = tb . Text;
                        break;
                    case 16:
                        UpdatedRecordData [ 15 ] = tb . Text;
                        break;
                    case 17:
                        UpdatedRecordData [ 16 ] = tb . Text;
                        break;
                    case 18:
                        UpdatedRecordData [ 17 ] = tb . Text;
                        break;
                    case 19:
                        UpdatedRecordData [ 18 ] = tb . Text;
                        break;
                    case 20:
                        UpdatedRecordData [ 19 ] = tb . Text;
                        break;
                }
            }
            catch ( Exception ex )
            {
                Debug . WriteLine ( "Error encountered..." );
            }
            bdirty = true;
        }

        //#################################//
        #region SPDatagrid mouse handlers
        //++++++++++++++++++++++++++++++++//

        private void SPDatagrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LEFTMOUSEDOWN = true;
        }

        private void SPDatagrid_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            LEFTMOUSEDOWN = false;
        }

        private void SPDatagrid_PreviewMouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {

        }

        private void edit_dgItem ( object sender, MouseButtonEventArgs e )
        {
        }


        private void ShowDateEdtpanel_Click ( object sender, RoutedEventArgs e )
        {
            // forceit to be closed so the (show_Editpanel) method wil reopen it
            EditPanel . Visibility = COLLAPSED;
            show_Editpanel ( null, null );
        }

        //#################################//
        #endregion END of SPDatagrid mouse handlers
        //++++++++++++++++++++++++++++++++//


        //#################################//
        #endregion END of ALL SQL View Handlers

        private void SProcsViewer_MouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = false;
        }

        private void SProcsViewer_MouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
        {
            LeftMousePressed = true;
        }

        private void Data_GotFocus ( object sender, RoutedEventArgs e )
        {
            editprompt . Text = "Edit content of current record and click \"Update Table\" to save any changes,  or ESC to exit";
            editprompt . Background = FindResource ( "Yellow2" ) as SolidColorBrush;
            editprompt . Foreground = FindResource ( "Black0" ) as SolidColorBrush;
            TextBox tb = sender as TextBox;
            tb . Background = FindResource ( "Yellow0" ) as SolidColorBrush;
            int newweight = MainWindow.GetfontWeight("DemiBold");
            tb . FontWeight = FontWeight . FromOpenTypeWeight ( newweight );
            SetButtonColors ( true );
        }

        public Menu Removemainmenu ( Menu sender, MenuItem mainmenu, MenuItem item )
        {
            bool alldone = false;
            foreach ( MenuItem entry in sender . Items )
            {
                if ( entry . Name == mainmenu . Name )
                {
                    foreach ( MenuItem mitem in entry . Items )
                    {
                        if ( mitem . Name == item . Name )
                        {

                            alldone = true;
                            mitem . Visibility = COLLAPSED;
                            break;
                        }
                    }
                    if ( alldone )
                        break;
                }
            }
            return sender;
        }
        public Menu Addmainmenu ( Menu sender, MenuItem mainmenu, MenuItem item, string prompt )
        {
            bool alldone = false;
            foreach ( MenuItem entry in sender . Items )
            {
                if ( entry . Name == mainmenu . Name )
                {
                    foreach ( MenuItem mitem in entry . Items )
                    {
                        if ( mitem . Name == item . Name )
                        {
                            alldone = true;
                            mitem . Visibility = Visibility . Visible;
                            mitem . Header = prompt;
                            break;
                        }
                    }
                    if ( alldone )
                        break;
                }
            }
            return sender;
        }


        private void Overall_Click ( object sender, RoutedEventArgs e )
        {
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 1 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }

        private void UsingSQLPanel ( object sender, RoutedEventArgs e )
        {
            // open document in info panel window
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 2 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }

        private void UsingSprocsPanel ( object sender, RoutedEventArgs e )
        {
            InfoWindow infowin = null;
            WindowCollection v = Application.Current.Windows;
            foreach ( Window item in v )
            {
                if ( item . ToString ( ) . Contains ( "InfoWin" ) )
                {
                    item . BringIntoView ( );
                    item . Focus ( );
                    return;
                }
            }

            infowin = new InfoWindow ( 3 );
            SetValue ( TopmostProperty, false );
            infowin . Show ( );
        }


        private void SPDatagrid_PreviewKeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Up )
                SPDatagrid . SelectedIndex -= 1;
            else if ( e . Key == Key . Down )
                SPDatagrid . SelectedIndex += 1;
        }

        private void ReshowEditpane ( object sender, RoutedEventArgs e )
        {
            // reset flag so resize will work
            // Show edit panel full height in window
            MenuItem mi = sender as MenuItem;
            if ( mi == null ) return;
            if ( mi . Name . Contains ( "ShowSpmenu" ) || mi . Name . Contains ( "ShowDgmenu" ) )
            {
                // direct access to switch between SQL and Sprocs windows
                if ( mi . Name == "ShowDgmenu" )
                {
                    // close all S.Procs controls
                    ShowDg = true;
                    ShowSp = false;
                    SProcsListbox . Visibility = COLLAPSED;
                    ExecList . Visibility = COLLAPSED;
                    TextResult . Visibility = COLLAPSED;
                    SPDatagrid . Visibility = Visibility . Visible;
                    EditPanel . Visibility = Visibility . Visible;
                    SPDatagrid . UpdateLayout ( );
                    EditPanel . UpdateLayout ( );
                    ResetOptionsAccessColors ( );
                }
                else if ( mi . Name == "ShowSpmenu" )
                {
                    // Close alll DataGrid controls
                    ShowDg = false;
                    ShowSp = true;
                    SProcsListbox . Visibility = Visibility . Visible;
                    ExecList . Visibility = Visibility . Visible;
                    TextResult . Visibility = Visibility . Visible;
                    SPDatagrid . Visibility = COLLAPSED;
                    EditPanel . Visibility = COLLAPSED;
                    SProcsListbox . UpdateLayout ( );
                    ExecList . UpdateLayout ( );
                    TextResult . UpdateLayout ( );
                    ResetOptionsAccessColors ( );
                }
                SPFullDataContainerGrid . UpdateLayout ( );
            }
            else
            {
                if ( shrink1 . Text . Contains ( "Hide" ) )
                {
                    IsGridFullHeight = true;
                    ExpandSqlDataGrid ( sender, null );
                }
                else
                {
                    IsGridFullHeight = false;
                    EditPanel . Visibility = Visibility . Visible;
                    ResetPanelSplit ( sender, e );
                }
                this . Refresh ( );
            }
        }
        private void SPDatagrid_GotFocus ( object sender, RoutedEventArgs e )
        {
            SetButtonColors ( false );
            this . Focus ( );
        }
        private void ClearEditFocus ( )
        {
            //            Data1 . SelectedText = false;
        }
        private void SetButtonColors ( bool direction )
        {
            if ( direction == true )
            {
                SaveBtn . Style = FindResource ( "DiagonalRedButton" ) as Style;
                SaveBtn . BorderBrush = FindResource ( "Yellow0" ) as SolidColorBrush;
                Thickness th = new();
                th . Top = 10; th . Left = 4; th . Right = 4; th . Bottom = 10;
                SaveBtn . BorderThickness = th;
            }
            else
            {
                SaveBtn . Style = FindResource ( "DiagonalCyanButton" ) as Style;
                SaveBtn . BorderBrush = FindResource ( "White0" ) as SolidColorBrush;
                Thickness th = new();
                th . Top = 1; th . Left = 1; th . Right = 1; th . Bottom = 1;
                SaveBtn . BorderThickness = th;
            }
        }
        private void ResetHideBtnText ( int mode )
        {
            "" . sprocstrace ( 0 );
            if ( mode == 0 )
            {
                shrink1 . Text = " Hide Edit";
                shrink2 . Text = "   Panel ";
            }
            else if ( mode == 1 )
            {
                shrink1 . Text = "  Show Full ";
                shrink2 . Text = " Edit Panel ";
            }
            else if ( mode == 2 )
            {
                shrink1 . Text = "Reset Edit";
                shrink2 . Text = "   Panel ";
            }
            "" . sprocstrace ( 1 );
        }
        public List<string> GetSPCollectionData ( string sproc )
        {
            dynamic dynresult = null;
            dynamic dyn1 = new SingleColumnData();
            List<string > scd = new ( );
            SingleColumnData scoldata = new();
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == "" )
            {
                DapperSupport . CheckDbDomain ( "IAN1" );
                ConString = Flags . CurrentConnectionString;
            }
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    //SingleColumnData scd = new();
                    dynamic sdw = null;
                    //if ( SqlCommand == "" )
                    //{
                    IEnumerable<dynamic> query = db . Query<dynamic> ( sproc,
                            commandType: CommandType . StoredProcedure );
                    //                            . ToList ( );// as List<SingleColumnData>;
                    if ( dyn1 == null )
                        dynresult = db . Query ( sproc, param: null, transaction: null, buffered: true, commandType: CommandType . StoredProcedure );
                    //} 
                    string keyname="";
                    object keyvalue = null;
                    foreach ( var rows in query )
                    {
                        var fields = rows as IDictionary<string, object>;
                        if ( keyname == "" )
                        {
                            foreach ( KeyValuePair<string, object> pair in fields )
                            {
                                keyname = pair . Key . ToString ( );
                                break;
                            }
                        }
                        var sum = fields[keyname];
                        scd . Add ( sum . ToString ( ) );
                    }
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
                }
            }
            return scd;
        }

        private void ResultsEscapeKeyDown ( object sender, KeyEventArgs e )
        {
            // Close All of the execution results panel - if open
            //if ( ResultsListBox . Visibility != Visibility . Visible )
            //    return;
            if ( e . Key == Key . Escape )
            {
                ResultsContainerListbox . Visibility = COLLAPSED;
                ResultsListBox . Visibility = COLLAPSED;
                ResultsContainerTextblock . Visibility = COLLAPSED;
                ResultsTextbox . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                ResultsDatagrid . Visibility = COLLAPSED;
                TextResult . Visibility = Visibility . Visible;
            }
        }

        private void HideResultsPanel ( object sender, RoutedEventArgs e )
        {
            ResultsContainerListbox . Visibility = COLLAPSED;
            ResultsListBox . Visibility = COLLAPSED;
            ResultsContainerDatagrid . Visibility = COLLAPSED;
            ResultsDatagrid . Visibility = COLLAPSED;
            ResultsContainerTextblock . Visibility = COLLAPSED;
            ResultsTextbox . Visibility = COLLAPSED;
            if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
            else if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;
            //else if ( ShowSc )
            //SprocCreationGrid . Visibility = Visibility . Visible;
        }

        private void ColorsCombo_DropDownOpened ( object sender, EventArgs e )
        {
            Mouse . OverrideCursor = Cursors . Wait;
            Thread . Sleep ( 250 );
            Mouse . OverrideCursor = Cursors . Arrow;
            return;
        }

        //private void CloseApp_Click ( object sender, RoutedEventArgs e )
        //{
        //    if ( bdirty )
        //    {
        //        MessageBoxResult mbr =  MessageBox . Show ( "You have unsaved changes to the current table !  Are you sure you want to discard these changes ?","Possible Data Loss", MessageBoxButton.YesNoCancel);
        //        if ( mbr == MessageBoxResult . Cancel )
        //            return;
        //        else if ( mbr == MessageBoxResult . No )
        //            return;
        //        else if ( mbr == MessageBoxResult . Yes )
        //            Application . Current . Shutdown ( );
        //    }
        //    else
        //    {
        //        MessageBoxResult mbr =  MessageBox . Show ( "Are you quite sure you want to close this \napplication down completely ?","Confirm Close Down", MessageBoxButton.YesNo);
        //        if ( mbr == MessageBoxResult . No )
        //            return;
        //        Application . Current . Shutdown ( );
        //    }
        //}


        private void SPEditorKeyDown ( object sender, KeyEventArgs e )
        {
            if ( e . Key == Key . Escape )
            {
                //if ( CreateSprocTextbox .Text != NewSprocText )
                {
                    MessageBox . Show ( "Save the changes made to the S.Procedure ??" );
                }
                //SprocCreationGrid . Visibility = COLLAPSED;
                ResultsContainerTextblock . Visibility = Visibility . Visible;
                ResultsTextbox . Visibility = Visibility . Visible;
                Splitter_DragCompleted ( null, null );
            }
        }

        private void Execnewscript ( object sender, RoutedEventArgs e )
        {
            StatusText = "processing new SQL Script ......";
        }

        private void Closenewscript ( object sender, RoutedEventArgs e )
        {
            //SprocCreationGrid . Visibility = COLLAPSED;
            ResultsContainerTextblock . Visibility = Visibility . Visible;
            ResultsTextbox . Visibility = Visibility . Visible;
            Splitter_DragCompleted ( null, null );

        }

        public void SetLowerPanelHeight ( )
        {
            // set Dependency property for lower grid panel height
            //Debug . WriteLine ( $"Pre reset = {LowerPanelHeight}" );
            //if ( LowerPanelHeight == 0 )
            //    LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - ( RowHeight0] + hsplitterrow . ActualHeight );
            //else
            //    LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - ( RowHeight 2 ] + hsplitterrow . ActualHeight );
            //LowerPanelHeight = SPFullDataContainerGrid . ActualHeight - SProcsListbox . ActualHeight;
            ////           LowerPanelHeight += 25;
            //Debug . WriteLine ( $"Post reset = {LowerPanelHeight}" );
            //Debug . WriteLine ( $"{SPFullDataContainerGrid . ActualHeight} : = {RowHeight 0 ]} : {hsplitterrow . ActualHeight} : {RowHeight 2 ]} " );
            //    //$"{uppergridrow . ActualHeight + hsplitterrow . ActualHeight + RowHeight 0 ]}" );
            //lowergridrow . Height = new GridLength ( LowerPanelHeight );
        }
        private void ResetPanelControlSizes ( )
        {
            return;
            //"" . sprocstrace ( 0 );
            //if ( ShowSp )
            //{
            //    //SprocCreationGrid . Height = RowHeight 2 ];
            //    ResultsContainerDatagrid . Height = RowHeight 2 ];
            //    TextResult . Height = RowHeight 2 ] - 20;
            //    // results controls
            //    //ResultsDatagrid . Height = RowHeight 2 ];
            //    //ResultsContainerListbox . Height = RowHeight 2 ];
            //    //ResultsContainerTextblock . Height = RowHeight 2 ];
            //}
            //else
            //{
            //    EditPanel . Height = RowHeight 2 ];

            //}
            //"" . sprocstrace ( 1 );
        }

        private void ListAllControlHeights ( )
        {
            string output =LowerDataArea.ToString().PadRight(25) + $" : {LowerDataArea}";
            output += UpperDataArea . ToString ( ) . PadRight ( 25 ) + $" : {UpperDataArea}";
            output += EditPanel . ToString ( ) . PadRight ( 25 ) + $" : {EditPanel . Height}";
            output += SProcsListbox . ToString ( ) . PadRight ( 25 ) + $" : {SProcsListbox . ActualHeight}";
            output += SPDatagrid . ToString ( ) . PadRight ( 25 ) + $" : {SPDatagrid . Height}";
            output += TextResult . ToString ( ) . PadRight ( 25 ) + $" : {TextResult . Height}";
            output += $"";
            Debug . WriteLine ( output );
        }

        private void ShowpDg_MouseEnter ( object sender, MouseEventArgs e )
        {
            // ShowSp = true;
        }

        private void ShowpSp_MouseEnter ( object sender, MouseEventArgs e )
        {
            //ShowDg = true;
        }

        private void ShowSp_MouseLeave ( object sender, MouseEventArgs e )
        {
            //ShowSp = false;
        }

        private void ShowDg_MouseLeave ( object sender, MouseEventArgs e )
        {
            //ShowDg = false;
        }

        public static string GetDateTimeString ( )
        {
            string datestring="";
            DateTime dtime = DateTime.Now;
            string hour= dtime.Hour.ToString ( );
            if ( hour . Length == 1 ) hour = "0" + hour;
            string minute= dtime.Minute.ToString ( );
            if ( minute . Length == 1 ) minute = "0" + minute;
            string month = dtime.Month.ToString ( );
            if ( month . Length == 1 ) month = "0" + month;
            string day= dtime.Day.ToString ( );
            if ( day . Length == 1 ) day = "0" + day;
            return datestring = $"{hour}:{minute}-{day}/{month}/{dtime . Year}";
        }


        #region Resizing support methods
        private void show_Editpanel ( object sender, RoutedEventArgs e )
        {
            if ( EditPanel . Visibility == COLLAPSED )
            {
                EditPanel . Visibility = Visibility . Visible;
                TextResult . Visibility = COLLAPSED;
                PreEditsplitterheight1 = SPDatagrid . ActualHeight;
                PreEditsplitterheight2 = TextResult . ActualHeight;
                // reset splitter to suitable position so all fields are shown
                LoadSqlEditData ( null, null );
            }
            else
            {
                EditPanel . Visibility = COLLAPSED;
                TextResult . Visibility = Visibility . Visible;
                if ( AllowSplitterReset && ShowFullGrid == false )
                {
                    // reset splitter to original position if flag is true
                    if ( PreEditsplitterheight1 > 0 )
                        SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( PreEditsplitterheight1, GridUnitType . Pixel );
                    else
                        PreEditsplitterheight1 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight;
                }
                else if ( ShowFullGrid )
                {
                    GridLength gl2 = new();
                    double fullheight = SPFullDataContainerGrid.ActualHeight;
                    gl2 = new GridLength ( fullheight - 25, GridUnitType . Pixel );
                    SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = gl2;
                    PreEditsplitterheight1 = fullheight;
                }
                Clearfields ( );
            }
            Debug . WriteLine ( $"show_Editpaneln\nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        private void Win_SizeChanged ( object sender, SizeChangedEventArgs e )
        {
            // Working 4/2/23
            if ( IsLoading )
                return;
            double diff = 0, fullheight =0;
            "" . Track ( 0 );
            fullheight = SPFullDataContainerGrid . Height;
            diff = e . NewSize . Height - e . PreviousSize . Height;
            if ( diff == 0 )
                return;
            // Set new total working area
            RowHeight4 += diff;
            RowHeight0 += diff;
            if ( RowHeight0 < 0 )
                RowHeight0 = 0;
            //This is copied from DragCompleted, and WORKS CORRECTLY for row 2
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height =
                new GridLength ( ( SPFullDataContainerGrid . ActualHeight - 100 ) - ( SPFullDataContainerGrid . RowDefinitions [ 0 ] . ActualHeight + 185 ) );

            if ( RowHeight0 - splitht < 0 )
            {
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( 0 );
            }
            else
            {
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
            }
            SPFullDataContainerGrid . UpdateLayout ( );
            SPFullDataContainerGrid . UpdateLayout ( );
            // This is needed if the splitter gets "pushed down" by the size reduction, it corrects the lower panel  height change
            Splitter_DragCompleted ( sender, null );
            Debug . WriteLine ( $"\nWin_SizeChanged \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        private void TriggerFullHeight ( object sender, RoutedEventArgs e )
        {
            // large window
            double screenheight = SystemParameters.VirtualScreenHeight;
            this . Top = 0;
            if ( this . Height < screenheight )
                this . Height = screenheight;
            else
            {
                if ( WinSize == "SMALL" )
                    Showsmallwin ( null, null );
                if ( WinSize == "MED" )
                    Showmediumwin ( null, null );
                if ( WinSize == "LARGE" )
                    Showlargewin ( null, null );
            }
            Splitter_DragCompleted ( null, null );
            this . UpdateLayout ( );
        }

        private void Showlargewin ( object sender, RoutedEventArgs e )
        {
            // large window
            this . Top = 50;
            this . Left = 270;
            if ( this . Height <= 970 || this . Width <= 1350 )
            {
                WinSize = "LARGE";
                this . Height = 1000;
                this . Width = 1350;
                EditPanel . UpdateLayout ( );
                this . UpdateLayout ( );
            }
            Splitter_DragCompleted ( null, null );
            double screenheight = SystemParameters.VirtualScreenHeight;
        }

        private void Showmediumwin ( object sender, RoutedEventArgs e )
        {
            // medium window
            WinSize = "MED";
            this . Top = 100;
            this . Left = 415;
            this . Height = 900;
            this . Width = 1000;
            EditPanel . UpdateLayout ( );
            this . UpdateLayout ( );
            Splitter_DragCompleted ( null, null );
        }

        private void Showsmallwin ( object sender, RoutedEventArgs e )
        {
            // small window
            this . Top = 100;
            this . Left = 450;
            if ( this . Height >= 760 || this . Width >= 750 )
            {
                WinSize = "SMALL";
                this . Height = 760;
                this . Width = 900;
                EditPanel . UpdateLayout ( );
                this . UpdateLayout ( );
            }
            Splitter_DragCompleted ( null, null );
        }

        private void ExpandGridHeight_Click ( object sender, RoutedEventArgs e )
        {
            // All working 5/2/23
            // Set sql table datagrid to full height of window
            "" . sprocstrace ( 0 );

            // Works great 13/2/23
            ResetPanelSplit ( null, null );

            // Set Binding of top row  to full height - Splitter height
            //RowHeight0 = RowHeight4 - ( splitht + 65 );
            //SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( 20 );
            //SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight4 - ( splitht + 65 ) );
            //SPFullDataContainerGrid . UpdateLayout ( );
            //SPDatagrid . UpdateLayout ( );
            //EditPanel . UpdateLayout ( );
            "" . sprocstrace ( 1 );
        }

        private void ExpandSqlDataGrid ( object sender, RoutedEventArgs e )
        {
            // WORKING 24/1/23
            ExpandGridHeight_Click ( sender, null );
            IsGridFullHeight = true;
            IsEditFullHeight = false;
            ResetHideBtnText ( 1 );
        }

        public void SetDefLowerPanelHeight ( )
        {
            // set both panels to 50% height, (almost  anyway)
            // works well 15/2/23
            double btmval = DefEditpanelHeight;
            double topval = SPFullDataContainerGrid . Height - (btmval + SpUnusedSpace);
            RowHeight0 = topval;
            RowHeight2 = btmval;
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            SPFullDataContainerGrid . UpdateLayout ( );
            TextResult . Height = btmval - 55;
            EditPanel . Visibility = Visibility . Visible;
            EditPanel . UpdateLayout ( );
        }
        private void ResetPanelSplit ( object sender, RoutedEventArgs e )
        {
            "" . sprocstrace ( 0 );
            if ( RowHeight0 == 0 )
            {
                var v1 =  SPFullDataContainerGrid.RowDefinitions [ 0 ] . ActualHeight ;
            }
            RowHeight4 = SPFullDataContainerGrid . Height;
            UsablePanelsHeight = SPFullDataContainerGrid . Height - SpUnusedSpace;
            if ( ShowDg && RowHeight2 <= DefEditpanelHeight && sender != null )
            {
                SetDefLowerPanelHeight ( );
            }
            else
            {
                // Set datagrid to full height
                double btmval = 0;
                double topval = SPFullDataContainerGrid . Height - (SpUnusedSpace + 50);
                RowHeight0 = topval;
                RowHeight2 = btmval;
                SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
                SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );
                SPFullDataContainerGrid . UpdateLayout ( );
                SPDatagrid . UpdateLayout ( );
                EditPanel . UpdateLayout ( );
            }
            if ( ShowDg )
                ResetHideBtnText ( 0 );
            IsEditFullHeight = true;
            "" . sprocstrace ( 1 );
            Debug . WriteLine ( $"\nResetPanelSplit \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
            return;
        }

        private void ShowEditpanel_click ( object sender, RoutedEventArgs e )
        {
            // Show edit panel
            ExpandSqlDataGrid ( sender, null );
            if ( ShowDg )
            {
                // Hide reopen panel button
                ShowEditpanel . Visibility = Visibility . Visible;
                IsGridFullHeight = true;
                Splitter_DragCompleted ( null, null );
            }
        }
        private void ResetScriptPanelSplit ( object sender, RoutedEventArgs e )
        {
            // reset TextResulr position in window
            "" . sprocstrace ( 0 );
            if ( RowHeight0 == 0 )
            {
                var v1 =  SPFullDataContainerGrid.RowDefinitions [ 0 ] . ActualHeight ;
            }
            RowHeight2 = DefEditpanelHeight;
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );// + RowHeight1 );
            RowHeight0 = RowHeight4 - ( RowHeight2 + 180 );
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );// - ( DefEditpanelHeight ) );
            SPFullDataContainerGrid . UpdateLayout ( );
            //SprocCreationGrid . Height = RowHeight2;
            //CreateSprocTextbox .Height = RowHeight2 - 80;
            EditPanel . UpdateLayout ( );
            SPDatagrid . UpdateLayout ( );

            SetSplitterToMidMidposition ( );
            SPFullDataContainerGrid . UpdateLayout ( );

            IsEditFullHeight = true;
            "" . sprocstrace ( 1 );
            Debug . WriteLine ( $"\nResetScriptPanelSplit \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
            return;
        }

        private void SetSplitterToMidMidposition ( )
        {
            // called  by SP Creation panel refit option
            double percentages = 0.0;
            percentages = RowHeight4 / 5.0;
            RowHeight0 = ( percentages * 2 ) + RowHeight1;
            RowHeight2 = ( percentages * 3 - ( RowHeight1 + 90 ) );
            SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height = new GridLength ( RowHeight2 );
            SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height = new GridLength ( RowHeight0 );

            UpdateAllLowerControlSizes ( );
            RowHeight0 = SPFullDataContainerGrid . RowDefinitions [ 0 ] . Height . Value;
            RowHeight2 = SPFullDataContainerGrid . RowDefinitions [ 2 ] . Height . Value;
            Debug . WriteLine ( $"\nSetSplitterToMidMidposition\nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }
        private void UpdateAllLowerControlSizes ( )
        {
            TextResult . Height = RowHeight2;
            //if ( CreateSprocTextbox .Visibility == VISIBLE )
            {
                // lower pane  has Script editor  visible
                //SprocCreationGrid . Height = RowHeight2;// - 10;
                //CreateSprocTextbox .Height = RowHeight2; // - 10;
            }
            ResultsContainerDatagrid . Height = RowHeight2;
            ResultsDatagrid . Height = RowHeight2;
            ResultsContainerListbox . Height = RowHeight2;
            ResultsContainerTextblock . Height = RowHeight2;
            Debug . WriteLine ( $"\nUpdateAllLowerControlSizes ??? \nRowHeight4={RowHeight4}\nRowHeight0={RowHeight0}\n" +
                $"RowHeight2={RowHeight2} = " + $"{RowHeight0 + RowHeight2}" );
        }

        #endregion Resizing support methods

        #region  file save elements

        private void SaveResultsToFile ( object sender, RoutedEventArgs e )
        {
            // Save  the results from the Results panels to a user selected file
            bool IsListbox= false;
            bool IsDatagrid = false;
            bool IsTextbox= false;
            string output = "";
            string savepath="";
            string tmp="";
            MessageBoxResult mbr;

            if ( ResultsTextbox . Visibility == VISIBLE )
            {
                // Is it a text viewer ?
                MessageBoxResult mbr3 = MessageBox . Show ( "Do you really want to save the textual Data shown in the current Text viewer to a data file", "Save Script Execution Results", MessageBoxButton.YesNo );
                if ( mbr3 == MessageBoxResult . No ) { return; }
                else
                {
                    savepath = Utils2 . GetFilenameFromUser ( "ExecutionTextBoxResults.txt" );
                    IsTextbox = true;
                }
            }
            else if ( ResultsDatagrid . Visibility == VISIBLE )
            {
                // Is it a datagrid viewer ?
                mbr = MessageBox . Show ( "This will save the Columnar data in comma seperated CSV format.\nDepending on the amount of Data in the Datagrid,\nthis could create quite a large textual data file.\n\nAre You sure you want to do this ?.\n\nClick CANCEL if you want to limit the # of records saved to the CSV file first?", "Save Script Execution Results", MessageBoxButton . YesNoCancel );
                if ( mbr == MessageBoxResult . No ) { return; }
                else if ( mbr == MessageBoxResult . Cancel )
                {
                    // Get limit  total from user
                }
                tmp = "";

                // Get filename
                savepath = Utils2 . GetFilenameFromUser ( "ExecutionDatagridResults.txt" );
                IsDatagrid = true;

                // Call the print  system to either print the datagrid content, or convert  it
                // to a CSV and return the data to us to save or show the user as 
                StringBuilder SbResult = PrintingSupport . ParseDbTableToCSV ( ResultsDatagrid, false );
                if ( SbResult != null )
                {
                    string resultsdata = File . ReadAllText ( @"C:\MyDocumentation\TempPrintBuffer.dat" );
                    Directory . CreateDirectory ( @"C:\WpfmainData\" );

                    SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                    Nullable<bool> printresult = dialog.ShowDialog();
                    // Set all the parameters we want 
                    dialog . InitialDirectory = @"C:\WpfmainData\";
                    // It is being called from script editor
                    dialog . DefaultExt = "txt"; // Default file extension

                    dialog . Filter = "SQL Script (.sql) | *.sql | Text documents (*.txt) | *.txt | RTF Files {*.rtf) |*.rtf | All files (*.*) | *.*"; // Filter files by extension
                    dialog . FilterIndex = 1;
                    File . WriteAllText ( @"C:\WpfmainData\", resultsdata );
                }
                //else if ( type == typeof ( StringBuilder ) )
                //{
                //    // Got  the CSV data back in a StringBuilder
                //    // Write SB output to file
                //    string filename="";
                //    string savefilepath =Utils2. GetExportFileName ( "*.csv", filename );
                //    if ( savefilepath != "" )
                //    {
                //        StringBuilder sb = (StringBuilder)result;
                //        // convert StringBuilder to bytes
                //        byte[] barr = Encoding.ASCII.GetBytes(sb.ToString());
                //        File . WriteAllBytes ( savefilepath, barr );
                //    }
                //}
                else if ( ResultsListBox . Visibility == VISIBLE )
                {
                    // Processing for listbox
                    savepath = Utils2 . GetFilenameFromUser ( "ExecutionListBoxResults.txt" );
                    IsListbox = true;
                }

                // Now save it
                System . IO . Directory . CreateDirectory ( System . IO . Path . GetFullPath ( $@"C:\wpfmain\UserDataFiles" ) );
                if ( IsTextbox == true )
                {
                    // must be handling Textbox ?
                    if ( savepath == "" ) return;
                    tmp = $"Text  output from datagrid\n\n";
                    output = ResultsTextbox . Text;
                    File . WriteAllText ( savepath, output );
                    StatusText = $"Text Results text saved to file {savepath}";
                }
                else if ( IsDatagrid == true )
                {
                    // must be handling Datagrid?
                    if ( savepath == "" ) return;
                    tmp = $"Text output from datagrid\n\n";
                    GenericClass gc = new();
                    for ( int x = 0 ; x < ResultsDatagrid . Items . Count ; x++ )
                    {
                        gc = ResultsDatagrid . Items [ x ] as GenericClass;
                        tmp = ParseDatagridToText ( ResultsDatagrid, gc, x );
                        if ( tmp == "***END***" || tmp == "" ) break;
                        else output += tmp;
                    }
                    File . WriteAllText ( savepath, output );
                    StatusText = $"Datagrid Results as text saved to file {savepath}";
                }
                else if ( IsListbox == true )
                {
                    // drop thru to handle listbox
                    int indx = 0;
                    // create output buffer
                    if ( savepath == "" ) return;
                    tmp = $"Text  output from Listbox\n\n";
                    output = tmp;
                    foreach ( string str in ResultsListBox . Items )
                    {
                        if ( indx != 1
                            && str . Contains ( "Click ESCAPE" ) == false
                            && str != "" )
                        {
                            output += $"{str}\n";
                        }
                        indx++;
                    }
                    output += $"\nEND OF LISTBOX RESULTS from execution of the Stored Procedure \nshown above (Courtesy of the WpfMain Generic SQL/S.P Access Application\n\n";
                    savepath = System . IO . Path . GetFullPath ( $@"{savepath}" );

                    File . WriteAllText ( savepath, output );
                    StatusText = $"Results text saved to file {savepath}";
                }

                mbr = MessageBox . Show ( $"Close the Script Editor now ?.", "Data Saved", MessageBoxButton . YesNo, MessageBoxImage . Information );
                if ( mbr == MessageBoxResult . Yes )
                {
                    // Close ALL results panels    and clear their contents
                    ResultsContainerDatagrid . Visibility = COLLAPSED;
                    ResultsDatagrid . Visibility = COLLAPSED;
                    ResultsDatagrid . ItemsSource = null;
                    ResultsDatagrid . Items . Clear ( );
                    ResultsContainerListbox . Visibility = COLLAPSED;
                    ResultsListBox . Visibility = COLLAPSED;
                    ResultsListBox . Items . Clear ( );
                    ResultsContainerTextblock . Visibility = COLLAPSED;
                    ResultsTextbox . Visibility = COLLAPSED;
                    ResultsTextbox . Text = "";
                    if ( ShowSp )
                        TextResult . Visibility = VISIBLE;
                    else
                        EditPanel . Visibility = VISIBLE;
                }
                MessageBoxResult mbr2 = MessageBox . Show ( $"All the Data has been saved successfully to a file with the name\n {savepath}\n\nDo you want to view the new Output file now ?.", "Data Saved", MessageBoxButton . YesNo, MessageBoxImage . Information );
                if ( mbr2 == MessageBoxResult . Yes )
                {
                    ResultsContainerTextblock . Visibility = VISIBLE;
                    ResultsTextbox . Visibility = VISIBLE;
                    ResultsTextbox . Text = output;
                    Splitter_DragCompleted ( null, null );
                    ResultsTextbox . Text = output;
                }
            }
        }
        public static int GetLongestDataGridColumnWidth ( DataGrid dgrid )
        {
            int max = 0;
            int size = 0;
            int col = 0;
            for ( int x = 0 ; x < dgrid . Items . Count ; x++ )
            {
                GenericClass gc = new();
                gc = dgrid . Items [ x ] as GenericClass;
                if ( gc . field1 . GetType ( ) == typeof ( string ) )
                    max = gc . field1 . Length < max ? max : gc . field1 . Length;
                if ( gc . field2 == null ) continue;
                if ( gc . field2 . GetType ( ) == typeof ( string ) )
                    max = gc . field2 . Length < max ? max : gc . field2 . Length;
                if ( gc . field3 == null ) continue;
                if ( gc . field3 . GetType ( ) == typeof ( string ) )
                    max = gc . field3 . Length < max ? max : gc . field3 . Length;
                if ( gc . field4 == null ) continue;
                if ( gc . field4 . GetType ( ) == typeof ( string ) )
                    max = gc . field4 . Length < max ? max : gc . field4 . Length;
                if ( gc . field5 == null ) continue;
                if ( gc . field5 . GetType ( ) == typeof ( string ) )
                    max = gc . field5 . Length < max ? max : gc . field5 . Length;
                if ( gc . field6 == null ) continue;
                if ( gc . field6 . GetType ( ) == typeof ( string ) )
                    max = gc . field6 . Length < max ? max : gc . field6 . Length;
                if ( gc . field7 == null ) continue;
                if ( gc . field7 . GetType ( ) == typeof ( string ) )
                    max = gc . field7 . Length < max ? max : gc . field7 . Length;
                if ( gc . field8 == null ) continue;
                if ( gc . field8 . GetType ( ) == typeof ( string ) )
                    max = gc . field8 . Length < max ? max : gc . field8 . Length;
                if ( gc . field9 == null ) continue;
                if ( gc . field9 . GetType ( ) == typeof ( string ) )
                    max = gc . field9 . Length < max ? max : gc . field9 . Length;
                if ( gc . field10 == null ) continue;
                if ( gc . field10 . GetType ( ) == typeof ( string ) )
                    max = gc . field10 . Length < max ? max : gc . field10 . Length;
                if ( gc . field11 == null ) continue;
                if ( gc . field11 . GetType ( ) == typeof ( string ) )
                    max = gc . field11 . Length < max ? max : gc . field11 . Length;
                if ( gc . field12 == null ) continue;
                if ( gc . field12 . GetType ( ) == typeof ( string ) )
                    max = gc . field12 . Length < max ? max : gc . field12 . Length;
                if ( gc . field13 == null ) continue;
                if ( gc . field13 . GetType ( ) == typeof ( string ) )
                    max = gc . field13 . Length < max ? max : gc . field13 . Length;
                if ( gc . field14 == null ) continue;
                if ( gc . field14 . GetType ( ) == typeof ( string ) )
                    max = gc . field14 . Length < max ? max : gc . field14 . Length;
                if ( gc . field15 == null ) continue;
                if ( gc . field15 . GetType ( ) == typeof ( string ) )
                    max = gc . field15 . Length < max ? max : gc . field15 . Length;
                if ( gc . field16 == null ) continue;
                if ( gc . field16 . GetType ( ) == typeof ( string ) )
                    max = gc . field16 . Length < max ? max : gc . field16 . Length;
                if ( gc . field17 == null ) continue;
                if ( gc . field17 . GetType ( ) == typeof ( string ) )
                    max = gc . field17 . Length < max ? max : gc . field17 . Length;
                if ( gc . field18 == null ) continue;
                if ( gc . field18 . GetType ( ) == typeof ( string ) )
                    max = gc . field18 . Length < max ? max : gc . field18 . Length;
                if ( gc . field19 == null ) continue;
                if ( gc . field19 . GetType ( ) == typeof ( string ) )
                    max = gc . field19 . Length < max ? max : gc . field19 . Length;
                if ( gc . field20 == null ) continue;
                if ( gc . field20 . GetType ( ) == typeof ( string ) )
                    max = gc . field20 . Length < max ? max : gc . field20 . Length;
            }
            return max;
        }

        public static string ParseDatagridToText ( DataGrid dgrid, GenericClass gc, int index )
        {
            string output = "";
            if ( gc . field1 . ToString ( ) . Contains ( "Dbl-Click" ) == true )
                return "***END***";
            output = $"{gc . field1 . ToString ( )}, ";
            output += gc . field2 != null ? $"{gc . field2 . ToString ( )}, " : "";
            output += gc . field3 != null ? $"{gc . field3 . ToString ( )}, " : "";
            output += gc . field4 != null ? $"{gc . field4 . ToString ( )}, " : "";
            output += gc . field5 != null ? $"{gc . field5 . ToString ( )}, " : "";
            output += gc . field6 != null ? $"{gc . field6 . ToString ( )}, " : "";
            output += gc . field7 != null ? $"{gc . field7 . ToString ( )}, " : "";
            output += gc . field8 != null ? $"{gc . field8 . ToString ( )}, " : "";
            output += gc . field9 != null ? $"{gc . field9 . ToString ( )}, " : "";
            output += gc . field10 != null ? $"{gc . field10 . ToString ( )}, " : "";
            output += gc . field11 != null ? $"{gc . field11 . ToString ( )}, " : "";
            output += gc . field12 != null ? $"{gc . field12 . ToString ( )}, " : "";
            output += gc . field13 != null ? $"{gc . field13 . ToString ( )}, " : "";
            output += gc . field14 != null ? $"{gc . field14 . ToString ( )}, " : "";
            output += gc . field15 != null ? $"{gc . field15 . ToString ( )}, " : "";
            output += gc . field16 != null ? $"{gc . field16 . ToString ( )}, " : "";
            output += gc . field17 != null ? $"{gc . field17 . ToString ( )}, " : "";
            output += gc . field18 != null ? $"{gc . field18 . ToString ( )}, " : "";
            output += gc . field19 != null ? $"{gc . field19 . ToString ( )}, " : "";
            output += gc . field20 != null ? $"{gc . field20 . ToString ( )}n" : "\n";
            return output;
        }
        public static string CreateGridRecordAsText ( GenericClass gc, Type type )
        {
            string output = "";
            if ( type == typeof ( int ) || type == typeof ( double ) || type == typeof ( decimal ) )
            { output = $"{gc . field1 . PadRight ( 18 )}"; }
            else if ( type == typeof ( int ) || type == typeof ( double ) || type == typeof ( decimal ) )
            { output = $"{gc . field1 . PadRight ( 18 )}"; }

            return output;
        }

        #endregion  file save elements

        private void CloseResultsPanel ( object sender, RoutedEventArgs e )
        {
            /// Close most obvious Results Container or the Text viewer , which will always be on top !
            ShowRt = false;
            if ( ResultsContainerDatagrid . Visibility == VISIBLE && ResultsContainerTextblock . Visibility == VISIBLE
                || ResultsContainerListbox . Visibility == VISIBLE && ResultsContainerTextblock . Visibility == VISIBLE
                || ResultsContainerDatagrid . Visibility == VISIBLE && ResultsContainerTextblock . Visibility == VISIBLE )
            {
                ResultsContainerTextblock . Visibility = COLLAPSED;
                ResultsTextbox . Visibility = COLLAPSED;
            }
            else if ( ResultsContainerDatagrid . Visibility == VISIBLE )
            {
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                ResultsDatagrid . Visibility = COLLAPSED;
            }
            else if ( ResultsContainerListbox . Visibility == VISIBLE )
            {
                ResultsContainerListbox . Visibility = COLLAPSED;
                ResultsListBox . Visibility = COLLAPSED;
            }
            else if ( ResultsContainerTextblock . Visibility == VISIBLE )
            {
                ResultsContainerTextblock . Visibility = COLLAPSED;
                ResultsTextbox . Visibility = COLLAPSED;
            }
            if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
            else if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;

            if ( ShowSc )
            {
                //SprocCreationGrid . Visibility = Visibility . Visible;
                //CreateSprocTextbox .Visibility = Visibility . Visible;
            }
        }
        public ObservableCollection<GenericClass> ParseDapperToGenericClass ( IEnumerable<dynamic> reslt, Dictionary<string, object> dict )
        {
            ObservableCollection<GenericClass> objG = new ObservableCollection<GenericClass> ( );
            int dictcount = 0;
            int fldcount = 0;
            int colcount=0;
            string result="";
            bool IsSuccess = false;
            long zero= reslt.LongCount ();
            try
            {
                foreach ( var item in reslt )
                {
                    // trial to get a new  instance of an anonymous object    - fails	 for bankaccountciewmodel
                    //var  gcx = SqlServerCommands .deepClone( objtype);
                    GenericClass gc = new GenericClass();
                    try
                    {
                        //	Create a dictionary entry for each row of data then add it as a row to the Generic (ObervableCollection<xxxxxx>) Class
                        gc = ParseDapperRowGen ( item, dict, out colcount );
                        dictcount = 1;
                        fldcount = dict . Count;
                        //if ( fldcount == 0 )
                        //{
                        //	//TODO - Oooops, maybe, we will use a Datatable or osething
                        //	//return null;
                        //}
                        foreach ( var pair in dict )
                        {
                            try
                            {
                                if ( pair . Key != null && pair . Value != null )
                                {
                                    DapperSupport . AddDictPairToGeneric<GenericClass> ( gc, pair, dictcount++ );
                                }

                            }
                            catch ( Exception ex )
                            {
                                Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                                result = ex . Message;
                            }
                        }
                        IsSuccess = true;
                    }
                    catch ( Exception ex )
                    {
                        result = $"SQLERROR : {ex . Message}";
                        Debug . WriteLine ( result );
                    }

                    objG . Add ( gc );
                    dict . Clear ( );
                    dictcount = 1;
                }
                return objG;
            }
            catch ( Exception ex ) { return null; }
        }

        //*************************************************************************************//
        //Create a new  Observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
        //*************************************************************************************//
        public ObservableCollection<GenericClass> CreateCollectionFromDynamic ( IEnumerable<dynamic> dyn )
        {
            // Working
            // Create a new  observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
            int index = 0;
            ObservableCollection<GenericClass>  querytable = new();
            foreach ( var rows in dyn )
            {
                index = 1;
                GenericClass gc = new();
                var fields = rows as IDictionary<string, object>;
                foreach ( KeyValuePair<string, object> item in fields )
                {
                    if ( item . Value == null )
                    {
                        index++;
                        break;
                    }
                    string data  = item . Value.ToString();
                    switch ( index )
                    {
                        case 1:
                            gc . field1 = data; break;
                        case 2:
                            gc . field2 = data; break;
                        case 3:
                            gc . field3 = data; break;
                        case 4:
                            gc . field4 = data; break;
                        case 5:
                            gc . field5 = data; break;
                        case 6:
                            gc . field6 = data; break;
                        case 7:
                            gc . field7 = data; break;
                        case 8:
                            gc . field8 = data; break;
                        case 9:
                            gc . field9 = data; break;
                        case 10:
                            gc . field10 = data; break;
                        case 11:
                            gc . field11 = data; break;
                        case 12:
                            gc . field12 = data; break;
                        case 13:
                            gc . field13 = data; break;
                        case 14:
                            gc . field14 = data; break;
                        case 15:
                            gc . field15 = data; break;
                        case 16:
                            gc . field16 = data; break;
                        case 17:
                            gc . field17 = data; break;
                        case 18:
                            gc . field18 = data; break;
                        case 19:
                            gc . field19 = data; break;
                        case 20:
                            gc . field20 = data; break;
                        default:
                            break;
                    }
                    index++;
                }
                querytable . Add ( gc );
            }
            GenericClass gc2 = new();
            gc2 . field1 = "Dbl-Click this row, or Hit ESCAPE";
            querytable . Add ( gc2 );
            gc2 = new ( );
            gc2 . field1 = "to close the results datagrid display";
            querytable . Add ( gc2 );
            return querytable;
        }

        public List<string> CreateListFromDynamic ( IEnumerable<dynamic> dyn )
        {
            // Working
            // Create a new  observablecolleciton<GenericClass> from  IEnumerable<dynamic> data
            List<string>  resultslist = new();
            string currentline="";
            int dupecount=0;
            int total=0;
            try
            {
                foreach ( var entry in dyn )
                {
                    if ( currentline != "" && entry == currentline )
                    {
                        dupecount++;
                    }
                    currentline = entry . ToString ( );
                    resultslist . Add ( entry );
                    total++;
                }
                if ( ++dupecount == total )
                {
                    resultslist . Add ( "INFO:" );
                    resultslist . Add ( "As all values returned are identical" );
                    resultslist . Add ( "a better result may be achiieved by using the" );
                    resultslist . Add ( "'SP returning a List<string>' Execution Method or " );
                    resultslist . Add ( "'SP returning a table as ObservableCollection' ?" );
                }
                return resultslist;
            }
            catch ( Exception ex ) { Debug . WriteLine ( "Failed to parse data from dynamic result" ); }
            return resultslist;
        }


        private void ResultsDatagrid_PreviewMouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            GenericClass gc = new();
            DataGrid dg = sender as DataGrid;
            if ( dg == null ) { return; }
            if ( dg . SelectedItem == null ) { return; }
            gc = dg . SelectedItem as GenericClass;
            string  selection =gc.field1.ToString();
            if ( selection . Contains ( "Dbl-Click this row" )
                || selection . Contains ( "to close the results" ) )
            {
                ResultsDatagrid . Visibility = COLLAPSED;
                ResultsContainerDatagrid . Visibility = COLLAPSED;
                TextResult . Visibility = Visibility . Visible;
            }

        }

        private void ResultsDatagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            GenericClass gc = new();
            DataGrid dg = sender as DataGrid;
            if ( dg == null ) { return; }
            if ( dg . SelectedItem == null ) { return; }
            gc = dg . SelectedItem as GenericClass;
            string  selection =gc.field1.ToString();
        }

        private void ResultsListBox_SelectionChanged ( object sender, MouseButtonEventArgs e )
        {
            ListBox lb = sender   as ListBox;
            Type type = sender.GetType();
            if ( lb . SelectedItem == null )
                return;
            string selection = lb.SelectedItem . ToString();
            if ( selection . Contains ( "Dbl-Click this line" ) == true )
            {
                ResultsListBox . Items . Clear ( );
                ResultsListBox . Visibility = COLLAPSED;
                ResultsContainerListbox . Visibility = COLLAPSED;
                TextResult . Visibility = Visibility . Visible;
            }

        }

        #region printing support
        private void printscript_click ( object sender, RoutedEventArgs e )
        {
            //Request to print someting
            bool istopmost = this.Topmost;
            //this . Topmost = false;
            this . BringIntoView ( );

            PrintingSupport psupport = new();
            //default save path for data to be printed
            string filepath = @"C:\Wpfmain\SqlScripts\";

            // get calling menu
            MenuItem mitem = sender  as MenuItem;
            if ( mitem . Name == "MainPrintScript" || mitem . Name == "spprintscript" )
            {
                // called from main window menu
                ResetFlowDocForprint ( "FLOWDOC", FlowDoc: true );
            }

            if ( mitem . Name == "mainprintscript" )
            {
                // called from main window menu
                ResetFlowDocForprint ( "FLOWDOC", FlowDoc: true );
            }
            //else if ( mitem . Name . Contains ( "ScriptPrint" ) )
            //{
            //    File . WriteAllText ( $@"C:\WpfMain\PrintFiles\Printfile.txt", //CreateSprocTextbox .Text );
            //    psupport .SpPrintItem ( this, documenttext: //CreateSprocTextbox .Text );
            //}
            else if ( mitem . Name . Contains ( "printscript" ) )
            {
                string text="";

                //new  Script printing
                if ( ResultsTextbox . Visibility == VISIBLE )
                    text = ResultsTextbox . Text;       // resullts  textbox
                                                        //else //if ( CreateSprocTextbox .Visibility == VISIBLE )
                                                        //    text = //CreateSprocTextbox .Text;       // script
                else if ( EditFileTextbox . Visibility == VISIBLE )
                    text = EditFileTextbox . Text;      // data   files
                else if ( TextResult . Visibility == VISIBLE )
                {
                    FlowDocument fdoc = TextResult.Document;
                    PrintingSupport . SpPrintItem ( this, flowDocument: fdoc );
                    TextResult . Background = FindResource ( "Black0" ) as SolidColorBrush;
                    TextResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                }
            }
            if ( istopmost )
            {
                this . Topmost = true;
            }
        }
        public void ResetFlowDocForprint ( string callertype, bool FlowDoc = false, string filename = "", string TextTobePrinted = "" )
        {
            string sptext="";
            PrintingSupport psupport = new();

            Brush fore=MainWindow . ScrollViewerFground;
            Brush back=MainWindow . ScrollViewerBground;

            if ( FlowDoc == true )
            {
                // *** WORKING 12/2/22 *** //
                // We are printing the current Script in viewer
                FlowDocument fd = flowDoc;
                fd . Blocks . Clear ( );
                FetchStoredProcedureCode ( SProcsListbox . SelectedItem . ToString ( ), ref sptext );
                // we have to switch color parameters for Flowdoc to Black on Whiite to be able to print it.
                MainWindow . ScrollViewerBground = fd . Background = FindResource ( "White0" ) as SolidColorBrush;
                MainWindow . ScrollViewerFground = fd . Foreground = FindResource ( "Black0" ) as SolidColorBrush;

                fd = processsprocs . CreatePrintFlowDoc ( fd, sptext, "" );
                PrintingSupport . SpPrintItem ( this, flowDocument: fd );
                // This switches the default flowdoc colors back again
                MainWindow . ScrollViewerBground = back;
                MainWindow . ScrollViewerFground = fore;
                return;
            }
            else if ( TextTobePrinted != "" )
            {
                PrintingSupport . SpPrintItem ( this, documenttext: TextTobePrinted );
                return;
            }
            else if ( filename != "" )
            {
                string currtext = File.ReadAllText(filename);
                PrintingSupport . SpPrintItem ( this, documenttext: currtext );
                return;
            }
        }


        private void PrintCurrentToResultsPanel ( )
        {
            PrintResultsPanel ( null, null );
        }
        private bool PrintResultsPanel ( object sender, RoutedEventArgs e )
        {
            // print content of current results panel
            string originalline="", output="";
            PrintingSupport psupport = new();
            ListBox lb = null;
            DataGrid dg = null;
            if ( ResultsTextbox . Visibility == VISIBLE )
            {
                output = $"Execution Text Resuts Printout :\n\n";
                output += ResultsTextbox . Text;
            }
            else if ( ResultsListBox . Visibility == VISIBLE )
            {
                output = $"     Execution ListBox Resuts Printout :\n\n";

                lb = this . ResultsListBox;
                foreach ( var item in lb . Items )
                {
                    originalline = item . ToString ( );
                    string line = originalline .ToUpper ();
                    if ( line . Contains ( "EXECUTION RESULT FOR" )
                        || line . Contains ( "DBL-CLICK THIS LINE" ) )
                        continue;
                    else
                        output += $"        {originalline}\n";
                }
            }
            else if ( ResultsDatagrid . Visibility == VISIBLE )
            {
                //                MessageBox . Show ( "You cannot currently print data from a datagrid...", "", MessageBoxButton . OK, MessageBoxImage . Warning );
                PrintingSupport . ParseDbTableToCSV ( ResultsDatagrid, true );
                //             return true;
            }
            bool istopmost =this . Topmost;
            this . Topmost = false;

            PrintingSupport . SpPrintItem ( this, documenttext: output );

            if ( istopmost )
                this . Topmost = true;
            ExecResult . Background = FindResource ( "Green4" ) as SolidColorBrush;
            ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            StatusText = "Contents of Results panel have been printed successfully.....";
            Utils2 . DoSuccessBeep ( );
            return true;
        }
        #endregion printing support

        private void HSplitter_PreviewKeyDown ( object sender, KeyEventArgs e )
        {
            // stop splitter from responding to keyboard
            e . Handled = true;
        }

        private void ShowDataFiles_Click ( object sender, RoutedEventArgs e )
        {
            string fullfilenamepath = "";
            // show all  the files saved in C:\wpfmain\USERDATAFILES
            string path  = System .IO.Path .GetFullPath( $@"C:\wpfmain\UserDataFiles");

            // NB:  file type = "Text documents (.txt)|*.txt|All Files (*.*)|*.*"
            fullfilenamepath = Utils2 . GetFileName ( path, "", filetypes: "", deffiletype: "" );

            // Process open file dialog box results
            if ( fullfilenamepath != "" )
            {
                // Open document
                string output = File . ReadAllText ($"{fullfilenamepath}" );
                EditFileContainerGrid . Visibility = Visibility . Visible;
                EditFileTextbox . Visibility = Visibility . Visible;
                Splitter_DragCompleted ( null, null );
                EditFileTextbox . Text = output;
                StatusText = "Selected Data file displayed in Text Viewer above....";
                ExecResult . Background = FindResource ( "Orange3" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
                Splitter_DragCompleted ( null, null );
                Utils2 . DoSuccessBeep ( );
            }
        }

        private void CloseTextResultsPanel ( object sender, RoutedEventArgs e )
        {
            if ( ResultsContainerTextblock . Visibility == VISIBLE )
            {
                ResultsContainerTextblock . Visibility = COLLAPSED;
                ResultsTextbox . Visibility = COLLAPSED;
            }
        }

        private void ResetEditPanelHeight ( object sender, RoutedEventArgs e )
        {
            // TODO ??????? wrong command ?
            ResetPanelSplit ( null, null );
        }

        #region New Script handling
        private string CreateNewSprocEditorTemplate ( )
        {
            string Output="";
            Output = $"/* STANDARD  HEADER SETUP:\n" +
                $"  Modify as you wish ....\n/* NB: This script will be executed by  the system using the \n" +
                $"  \"1. SP returning a Table as ObservableCollection\" option, so\n" +
                $"  you should expect the script to return some form of SQL table contents  */\n\nUse [IAN1]  /* the default database ! */\nGO\nSET  ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\n-- -- Start of users script :-\n\n\n";
            NewSprocText = Output;
            return Output;
        }
        //private void ShowSprocCreationGrid ( object sender, RoutedEventArgs e )
        //{
        // Show script creation panel
        // set global flag so we know editor is open
        //if ( CreateSprocTextbox .Text != "" )
        //{
        //MessageBoxResult mbr =  MessageBox . Show ( "There is a hidden script editor already open !\nClick YES to show the existing Script , or NO if want to discard it and open a new script ?.", "Show new Sql Script", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No );
        //if ( mbr == MessageBoxResult . No )
        //{
        //    // discard  existing script
        //    //CreateSprocTextbox .Text = CreateNewSprocEditorTemplate ( );
        //    //SprocCreationGrid . Visibility = Visibility . Visible;
        //    //CreateSprocTextbox .Visibility = Visibility . Visible;
        //}
        //else
        //{
        //Just Show existing Script
        //SprocCreationGrid . Visibility = Visibility . Visible;
        //CreateSprocTextbox .Visibility = Visibility . Visible;
        //Splitter_DragCompleted ( null, null );
        //}
        //}
        //else
        //{
        //CreateSprocTextbox .Text = CreateNewSprocEditorTemplate ( );
        //SprocCreationGrid . Visibility = Visibility . Visible;
        //CreateSprocTextbox .Visibility = Visibility . Visible;
        //}

        //CreateSprocTextbox .BringIntoView ( );
        //CreateSprocTextbox .UpdateLayout ( );
        //    Splitter_DragCompleted ( null, null );
        //    ShowSc = true;
        //}
        private void ShowExistingScript ( object sender, RoutedEventArgs e )
        {
            // select and load  existing SQL script
            //if ( CreateSprocTextbox .Text == "" )
            {
                string path=System.IO.Path.GetFullPath($@"C:\Wpfmain\SqlScripts\");
                string DefFileName="", filetypes = "SQL Script (*.sql) | *.sql | All files {*.*) | *.*";
                string deffiletype = ".sql";
                DefFileName = Utils2 . GetFileName ( path, DefFileName, filetypes, deffiletype );
                if ( DefFileName == "" ) return;
                //CreateSprocTextbox .Text = File . ReadAllText ( DefFileName );
                //if (CreateSprocTextbox .Text == "" ) return;
            }
            //SprocCreationGrid . Visibility = Visibility . Visible;
            //CreateSprocTextbox .Visibility = Visibility . Visible;
            ShowSc = true;
        }
        private void HideSprocCreationGrid ( object sender, RoutedEventArgs e )
        {
            // Only Hide the sql editor panel, it keeps it's data
            //SprocCreationGrid . Visibility = COLLAPSED;
            //CreateSprocTextbox .Visibility = COLLAPSED;
            if ( ShowDg )
                EditPanel . Visibility = Visibility . Visible;
            else if ( ShowSp )
                TextResult . Visibility = Visibility . Visible;
        }
        private void DiscardSprocCreationGrid ( object sender, RoutedEventArgs e )
        {
            // Close Script editor - loosing all data
            MessageBoxResult  mbr = MessageBox . Show ( $"This Script will be totally deleted, please ensure that you really want to proceed ?\n\nNB - Please Confirm that you understand that any changes made will be LOST !" ,"Deleting user's Sql Script?",MessageBoxButton.YesNo,MessageBoxImage.Question);
            if ( mbr == MessageBoxResult . No )
                return;
            //CreateSprocTextbox .Text = "";
            //CreateSprocTextbox .Visibility = COLLAPSED;
            //SprocCreationGrid . Visibility = COLLAPSED;
            ShowSc = false;
        }
        private void ExecuteNewSprocs ( object sender, RoutedEventArgs e )
        {
            // try to Execute the  new SQL Script
            //MessageBoxResult  mbr = MessageBox . Show ( $"Your new Script will be executed using the \n[{ExecList.SelectedItem.ToString().ToUpper()} method\nwhich is the currently selected Execution Method.\n\nPlease confirm that you want to proceed ?\n\nNB - Please make SURE you understand that any changes it may make to your SQL data system, as they may NOT necessarily be REVERSIBLE !" ,"Executing user's Stored procedure ?",MessageBoxButton.YesNo,MessageBoxImage.Question);
            //if ( mbr == MessageBoxResult . No )
            //    return;
            //// TODO Add code to run the script and show results.
            //string txt = CreateSprocTextbox.Text;
            //File . WriteAllText ( System . IO . Path . GetFullPath ( $@"C:\wpfmain\tempscript" ), txt );
            //txt = //CreateSprocTextbox .Text;
            //SqlCommand = txt;
            //SqlCommand = SProcsListbox . SelectedValue . ToString ( );
            //// try to call the execution system from here. the return value is our result
            //int Count=0;
            //string ResultString="", Err="";
            //object Obj=null;
            //Type Objtype = null;
            //SpExecution spe = new("DUMMY");
            //spe = spe . GetSpExecution ( );
            //if ( spe == null )
            //{
            //    Debug . WriteLine ( "Failed   to load SpExecution () !!!!" );
            //}
            //int selvalue = Convert . ToInt32 ( ExecList.SelectedItem.ToString() . Substring ( 0, 1 ) );
            //// Execute the new script
            //dynamic dyn = spe.Execute_click(SqlCommand, ref Count, ref ResultString, ref Objtype, ref Obj, out Err,selvalue);
            //if ( Err != "" )
            //    Debug . WriteLine ( "" );
        }
        //private void SaveNewSprocs ( object sender, RoutedEventArgs e )
        //{
        //    // Save  script and close Script editor
        //    MenuItem mi = sender as MenuItem;
        //    string filename="";
        //    if ( mi == null ) return;
        //    string newpath = System.IO.Path.GetFullPath($@"C:\Wpfmain\SqlScripts");
        //    Directory . CreateDirectory ( newpath );

        //    // Create  & Show save file dialog box
        //    Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
        //    Nullable<bool> result = dialog.ShowDialog();

        //    // Set all the parameters we want 
        //    dialog . InitialDirectory = newpath;// + "\\";
        //    if ( mi . Name == "spsaveclosespeditor" || mi . Name == "saveclosespeditor" || mi . Name == "dgsaveclosescripteditor" )
        //    {
        //        // It is being called from script editor
        //        dialog . DefaultExt = ".sql"; // Default file extension
        //        dialog . Filter = "SQL Script (.sql) | *.sql | Text documents (*.txt) | *.txt | All files (*.*) | *.*"; // Filter files by extension
        //        dialog . FilterIndex = 2;
        //    }
        //    else
        //    {
        //        // Save text from "somewhere" and close panel
        //        dialog . FileName = "newuserfile.txt"; // Default file name
        //        dialog . DefaultExt = ".txt"; // Default file extension
        //        dialog . Filter = "Text documents (.txt)|*.txt | SQL Script (.sql) *.sql | All files (*.*) *.*|"; // Filter files by extension
        //        dialog . FilterIndex = 2;
        //        dialog . AddExtension = true;
        //    }

        //    if ( result == true )
        //    {
        //        // Process save file dialog box results
        //        //Get the path of file specified 
        //        filename = dialog . FileName;

        //        if ( filename != null && filename != "" )
        //        {
        //            string content = //CreateSprocTextbox .Text ;
        //            bool isdupe = content . Contains ( filename . ToUpper ( ) );
        //            if ( isdupe == false )
        //            {
        //                // has not been saved as this name before
        //                // So add time stamp line
        //                DateTime dtime = DateTime.Now;
        //                string hour= dtime.Hour.ToString ( );
        //                if ( hour . Length == 1 ) hour = "0" + hour;
        //                string minute= dtime.Minute.ToString ( );
        //                if ( minute . Length == 1 ) minute = "0" + minute;
        //                string stamp = GetDateTimeString ( );
        //                content += $"\n-- Script saved by system as {filename . ToUpper ( )} on {stamp}";
        //                //CreateSprocTextbox .Text = content;
        //            }
        //            else
        //            {
        //                // been saved before, so remove stamp line and replace it
        //                if ( content . Contains ( "\n-- Script saved by system" ) )
        //                    content = content . Substring ( 0, content . IndexOf ( "\n-- Script saved by system" ) );
        //                else if ( content . Contains ( "\n-- Script Re-saved by system" ) )
        //                    content = content . Substring ( 0, content . IndexOf ( "\n-- Script Re-saved by system" ) );
        //                string stamp = GetDateTimeString ( );
        //                content += $"\n-- Script Re-saved by system as {dialog . FileName . ToUpper ( )} on {stamp}";
        //                //CreateSprocTextbox .Text = content;
        //            }
        //            File . WriteAllText ( filename, //CreateSprocTextbox .Text );
        //            MessageBox . Show ( $"New Script saved as [ {filename . ToUpper ( )}]", "Script Saved" );
        //        }
        //    }
        //    //CreateSprocTextbox .Text = "";
        //    //CreateSprocTextbox .Visibility = COLLAPSED;
        //    //SprocCreationGrid . Visibility = COLLAPSED;
        //    ShowSc = false;
        //}
        private bool runSqlScriptFile ( string pathStoreProceduresFile, string connectionString )
        {
            //// Execute an SQL script
            try
            {
                string script = File.ReadAllText(pathStoreProceduresFile);

                // split script on GO command
                System.Collections.Generic.IEnumerable<string> commandStrings = Regex.Split(script, @"^\s*GO\s*$",
                                     RegexOptions.Multiline | RegexOptions.IgnoreCase);
                using ( SqlConnection connection = new SqlConnection ( connectionString ) )
                {
                    connection . Open ( );
                    foreach ( string commandString in commandStrings )
                    {
                        if ( commandString . Trim ( ) != "" )
                        {
                            using ( var command = new SqlCommand ( commandString, connection ) )
                            {
                                try
                                {
                                    command . ExecuteNonQuery ( );
                                }
                                catch ( SqlException ex )
                                {
                                    string spError = commandString.Length > 100 ? commandString.Substring(0, 100) + " ...\n..." : commandString;
                                    MessageBox . Show ( string . Format ( "Please check the SqlServer script.\nFile: {0} \nLine: {1} \nError: {2} \nSQL Command: \n{3}", pathStoreProceduresFile, ex . LineNumber, ex . Message, spError ), "Warning", MessageBoxButton . OK, MessageBoxImage . Warning );
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch ( Exception ex )
            {
                MessageBox . Show ( ex . Message, "Warning", MessageBoxButton . OK, MessageBoxImage . Warning );
                return false;
            }
        }

        #endregion New Script handling

        private void LoadColorsComboFromFile ( )
        {
            // All  configured correctly (Background/Foreground matching completed)
            // #79 is Red, the default background item.
            // load stored color data for my colored background combo
            string[] names = File . ReadAllLines ( @"C:\wpfmain\combos-names.txt" );
            string[] fcolors = File . ReadAllLines ( @"C:\wpfmain\combos-foreground.txt" );
            string[] bcolors = File . ReadAllLines ( @"C:\wpfmain\combos-colors.txt" );
            int index =0;
            foreach ( string name in names )
            {
                comboclrs cc = new();
                cc . name = names [ index ];
                cc . Fground = ( SolidColorBrush ) new BrushConverter ( ) . ConvertFrom ( fcolors [ index ] );
                cc . Bground = ( SolidColorBrush ) new BrushConverter ( ) . ConvertFrom ( bcolors [ index ] );
                CbColorsList . Add ( cc );
                index++;
            }
            ColorsCombo . ItemsSource = CbColorsList;
            ColorsCombo . SelectedIndex = 79;
            ColorsCombo . SelectedItem = 79;
        }

        #region menu redirection
        private void MenuControl_ExecShowContextMenu ( object sender, MouseButtonEventArgs e )
        {
            if ( ShowSp )
                sphMenus . ShowSpContextMenu ( sender, e, FindResource ( "SProcsContextmenu" ) as ContextMenu, "SProcsContextmenu" );
            else if ( ShowDg )
                sphMenus . ShowDgridContextMenu ( sender, e, FindResource ( "DgridContextmenu" ) as ContextMenu, "DgridContextmenu" );
        }
        private void SphMenus_ExecShowDgridContextMenu ( object sender, MouseButtonEventArgs e )
        {
            if ( ShowDg )
                sphMenus . ShowDgridContextMenu ( sender, e, FindResource ( "DgridContextmenu" ) as ContextMenu, "DgridContextmenu" );
            else if ( ShowSp )
                sphMenus . ShowSpContextMenu ( sender, e, FindResource ( "SProcsContextmenu" ) as ContextMenu, "SProcsContextmenu" );
        }

        private void SphMenu_ViewsMenuOpening ( object sender, RoutedEventArgs e )
        {
            sphMenus . ViewsMenuOpening ( sender, e, "ViewsMenuOpening" );
        }

        //private void sphmenu_ScriptsMenuOpening ( object sender, RoutedEventArgs e )
        //{
        //    sphMenus . ViewsMenuOpening ( sender, e, "ScriptsMenuOpening" );
        //}

        private void SphMenus_ScriptsMenuOpening ( object sender, RoutedEventArgs e )
        {
            sphMenus . ScriptsMenuOpening ( sender, e, "ScriptsMenuOpening" );
        }

        #endregion menu redirection

        public bool HaveSecondaryPanelsVisible ( )
        {
            if ( ResultsContainerDatagrid . Visibility == VISIBLE ||
                ResultsContainerListbox . Visibility == VISIBLE ||
                ResultsContainerTextblock . Visibility == VISIBLE ||
                //SprocCreationGrid . Visibility == VISIBLE ||
                EditFileContainerGrid . Visibility == VISIBLE )
                return true;
            else
                return false;
        }
        private void Hidepane_click ( object sender, RoutedEventArgs e )
        {
            if ( HaveSecondaryPanelsVisible ( ) )
            {
                EditPanel . Visibility = COLLAPSED;
                EditPanel . UpdateLayout ( );
                Splitter_DragCompleted ( null, null );
            }
            else
            {
                Utils2 . DoErrorBeep ( );
                StatusText = "Cannot Hide, No other Viewer is available";
                ExecResult . Background = FindResource ( "Red5" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "White0" ) as SolidColorBrush;
            }
        }

        private void CloseDataFiles_Click ( object sender, RoutedEventArgs e )
        {
            MessageBoxResult mbr = MessageBox.Show("The data file in the editor will be lost/discarded.\nAre you sure you want to proceeed ?","Close/Discard data  file", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
            if ( mbr == MessageBoxResult . Cancel )
                return;

            EditFileContainerGrid . Visibility = COLLAPSED;
            EditFileTextbox . Visibility = COLLAPSED;
            EditFileTextbox . Text = "";
        }

        private void SaveDataFiles_Click ( object sender, RoutedEventArgs e )
        {
            // save data file from file editor
            string filename = Utils2. GetFilenameFromUser ( "");
            if ( filename != "" )
            {
                File . WriteAllText ( filename, EditFileTextbox . Text );
            }
            EditFileContainerGrid . Visibility = COLLAPSED;
            EditFileTextbox . Visibility = COLLAPSED;
            EditFileTextbox . Text = "";
        }
        private void LoadShowScript ( object sender, RoutedEventArgs e )
        {
            // loading a new script  template
            //if ( CreateSprocTextbox .Text == "" )
            //CreateSprocTextbox .Text = CreateNewSprocEditorTemplate ( );
            // else
            {
                MessageBoxResult mbr = MessageBox.Show("The script editor is open and contains data.\nThat data will be lost if you continue ?.\nAre you sure you want to proceeed ?","Close/Discard data  file", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                if ( mbr == MessageBoxResult . Cancel )
                    return;
                //CreateSprocTextbox .Text = CreateNewSprocEditorTemplate ( );
            }
            //SprocCreationGrid . Visibility = Visibility . Visible;
            //CreateSprocTextbox .Visibility = Visibility . Visible;
            Splitter_DragCompleted ( null, null );
        }
        //private void LoadExistingScript ( object sender, RoutedEventArgs e )
        //{
        //    string filename = Utils2. GetFilenameFromUser ( "" );
        //    else
        //    {
        //        MessageBoxResult mbr = MessageBox.Show("The script editor is open and contains data.\nThat data will be lost if you continue ?.\nAre you sure you want to proceeed ?","Close/Discard data  file", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
        //        if ( mbr == MessageBoxResult . Cancel )
        //            return;
        //    }
        //    if ( filename == "" ) return;
        //    Splitter_DragCompleted ( null, null );
        //}

        private void CloseCreationGrid ( object sender, RoutedEventArgs e )
        {
            MessageBoxResult mbr = MessageBox.Show("Closing the script editor means the data will be lost if you continue ?.\nAre you sure you want to proceeed ?","Close/Discard data  file", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
            if ( mbr == MessageBoxResult . Cancel )
                return;
            //CreateSprocTextbox .Text = "";
            //SprocCreationGrid . Visibility = COLLAPSED;
            //CreateSprocTextbox .Visibility = COLLAPSED;
            Splitter_DragCompleted ( null, null );

        }

        private void CloseApp_Click ( object sender, RoutedEventArgs e )
        {
            if ( sph . bdirty )
            {
                MessageBoxResult mbr =  MessageBox . Show ( "You have unsaved changes to the current table !  Are you sure you want to discard these changes ?","Possible Data Loss", MessageBoxButton.YesNoCancel);
                if ( mbr == MessageBoxResult . Cancel )
                    return;
                else if ( mbr == MessageBoxResult . No )
                    return;
                else if ( mbr == MessageBoxResult . Yes )
                    Application . Current . Shutdown ( );
            }
            else
            {
                MessageBoxResult mbr =  MessageBox . Show ( "Are you quite sure you want to close this \napplication down completely ?","Confirm Close Down", MessageBoxButton.YesNo);
                if ( mbr == MessageBoxResult . No )
                    return;
                Application . Current . Shutdown ( );
            }
        }

        private void Hidescriptpane_click ( object sender, RoutedEventArgs e )
        {
            //SprocCreationGrid . Visibility = COLLAPSED;
            StatusText = "SQL Script editor panel hidden.....";
        }
        public static MenuItem CreateMenuSplitter ( )
        {
            MenuItem mi = new MenuItem();
            mi . Width = 300;
            mi . HorizontalAlignment = HorizontalAlignment . Center;
            mi . Header = "^^^^^^^^^^^^^^^^^^^^^^^^^";
            mi . FontSize = 6;
            mi . Padding = new Thickness ( 0, -1, 0, 0 );
            mi . Height = 6;
            mi . IsHitTestVisible = false;
            mi . Background = Application . Current . FindResource ( "Black0" ) as SolidColorBrush;
            mi . Foreground = Application . Current . FindResource ( "White0" ) as SolidColorBrush;
            mi . IsEnabled = false;
            mi . Visibility = Visibility . Visible;
            return mi;
        }

        private void printnewscript_click ( object sender, RoutedEventArgs e )
        {
            //Request from menus to print a NEW sql script currently in viewer 
            //Request to print someting
            PrintingSupport psupport = new();
            bool istopmost = this.Topmost;
            this . Topmost = false;
            // get calling menu
            MenuItem mitem = sender  as MenuItem;

            //          if ( mitem . Name . Contains ( "spprintnewscript" ) )
            //          {
            //              string text="";
            //              //new  Script printing
            //              //if ( CreateSprocTextbox .Visibility == VISIBLE )
            //                  text = //CreateSprocTextbox .Text;       // resullts  textbox
            ////              psupport.SpPrintItem ( this, documenttext: text );
            //          }
            if ( istopmost )
            {
                this . Topmost = true;
            }
        }

        private void PrintFileViewer ( object sender, RoutedEventArgs e )
        {
            PrintingSupport psupport = new();
            string filetext = EditFileTextbox . Text;
            PrintingSupport . SpPrintItem ( this, filetext );
        }

        private void CloseFileViewer ( object sender, RoutedEventArgs e )
        {
            EditFileContainerGrid . Visibility = COLLAPSED;
            EditFileTextbox . Visibility = COLLAPSED;
            EditFileTextbox . Text = "";
        }

        private void Execute_Click ( object sender, RoutedEventArgs e )
        {
            // Execute an S.P
            ExecListbox_SelectionChanged ( null, null );
        }

        private void ExecListbox_SelChanged ( object sender, SelectionChangedEventArgs e )
        {
            return;
            if ( ExecList . SelectedItem == null ) return;
            string   execommand = ExecList.SelectedItem.ToString();
            if ( execommand . Contains ( "*** No Execution" ) == true )
            {
                ExecuteBtn . IsEnabled = false;
                ExecuteBtn . Opacity = 0.6;
                return;
            }
            else
            {
                ExecuteBtn . IsEnabled = true;
                ExecuteBtn . Opacity = 1.0;
            }
        }
        public ObservableCollection<GenericClass> CreateCollectionFromIEnumerableCollection ( IEnumerable<dynamic> ienum, ref int columnscount )
        {
            // parse the data received into GenericClass records
            GenericClass gc = new();
            Sprocs = new ObservableCollection<GenericClass> ( );
            Dictionary<string, object> dict = new();
            List<string> list = new();
            List<string> Fldnames = new();
            int colcount = 0;
            ArrayList SprocsData = new ArrayList();
            foreach ( var item in ienum )
            {
                // get data from DapperRow as Dictionary{string>, object>
                dict = new ( );
                dict = DapperGeneric . ParseDapperRowGen ( item, dict, out colcount );
                // return a list of data as Fieldname=Data:"
                list = GetGenericListFromDictionary ( dict );
                // Convert List<string> into GenericClass Record with only one column
                genclass = ConvertListToGenericClassRecord ( list, colcount, out Fldnames );
                // Add GenericClass to our public Observable collection
                Sprocs . Add ( genclass );
                //// Add dict to our collection of these dicts (for later indexing)
                //SpNamesExecIndex . Add ( genclass . field1 . ToString ( ), 0 );
                //SProcsListbox . ItemsSource = null;
                //SProcsListbox . Items . Clear ( );
                //// Add sp name to an ArrayList 
                //SprocsData = LoadListBoxData ( Sprocs );
            }
            columnscount = colcount;
            return Sprocs;
        }
        public static GenericClass ConvertListToGenericClass ( List<string> Recordslist, int colcount, out List<string> Fldnames )
        {
            // called after a list<string> contaning a single row of GenericClass data
            // to create a GenericClass Record
            int count = 0;
            // Fldnames is NOT used in this version of sinlge column (S.Procs list) mode
            Fldnames = new List<string> ( );
            while ( count < colcount )
            {
                foreach ( var item in Recordslist )
                {
                    string[] buff = new string[3];
                    buff = item . Split ( "=" );
                    genclass . field1 = buff [ 1 ];
                    count++;
                }
            }
            return genclass;

        }

        private void ExecResult_PreviewTextInput ( object sender, TextCompositionEventArgs e )
        {
            Storyboard s = (Storyboard)TryFindResource("LookAtMe");
            s . Begin ( );  // Start animation-->
        }

        private void IsUpdated ( object sender, DataTransferEventArgs e )
        {
            if ( ExecResult . Text . Contains ( "Arguments are required" ) == true )
            {
                ExecResult . Background = FindResource ( "Yellow1" ) as SolidColorBrush;
                ExecResult . Foreground = FindResource ( "Red4" ) as SolidColorBrush;
                ExecResult . UpdateLayout ( );
                //Utils2 . DoWarningBeep ( );
            }
        }

        private void ExecResult_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
        {
            //          Storyboard s = (Storyboard)TryFindResource("LookatMe");
            //           s . Begin ( ); 	// Start animation-->
        }

        private void ShowExternalscript_click ( object sender, RoutedEventArgs e )
        {
            string script = "";
            FetchStoredProcedureCode ( SProcsListbox . SelectedItem . ToString ( ), ref script );
            ScriptViewerWin scriptwin = new ScriptViewerWin(SProcsListbox . SelectedItem . ToString ( ), script);
            scriptwin . ScriptTextEditor . Text = script;
            scriptwin . Show ( );
        }

        private void ShownewscriptEditor_click ( object sender, RoutedEventArgs e )
        {
            ScriptViewerWin scriptwin = new();
            scriptwin . Show ( );
        }

        private void EditExistingScript ( object sender, RoutedEventArgs e )
        {
            // open current SP script in editor window
            string script = "";
            FetchStoredProcedureCode ( SProcsListbox . SelectedItem . ToString ( ), ref script );
            ScriptViewerWin scriptwin = new(SProcsListbox . SelectedItem . ToString ( ), script);
            //            scriptwin . ScriptText . Text = script;
            scriptwin . Show ( );
        }

        public void LoadAllSprocs ( )
        {
            int currentselection=0;
            // Get IEnumerable<dynamic> collection of the data requested using S.Proc
            IeSprocs = LoadAllSprocs ( ConString, CurrentDbName, "spGetStoredProcs" );
            GenericClass gc = new();
            if( SProcsListbox . Items.Count > 0 )
            {
                currentselection = SProcsListbox . SelectedIndex;
            }
            // Now parse the data received into GenericClass records
            Sprocs = new ObservableCollection<GenericClass> ( );
            // Create list for any sql tble size info (dglayout)
            Dictionary<string, object> dict = new();
            List<string> list = new();
            List<string> Fldnames = new();
            int dbselect = 0;
            ArrayList SprocsData = new ArrayList();
            SpNamesExecIndex . Clear ( );
            int colcount = 0;
            // Create an internal Obscollection of s.Procedure names entries
            if ( IeSprocs != null )
            {
                foreach ( var item in IeSprocs )
                {
                    // get data from DapperRow as Dictionary{string>, object>
                    dict = new ( );
                    dict = DapperGeneric . ParseDapperRowGen ( item, dict, out colcount );
                    // return a list of data as Fieldname=Data:"
                    list = GetGenericListFromDictionary ( dict );
                    // Convert List<string> into GenericClass Record with only one column
                    genclass = ConvertListToSingleFieldGenericClass ( list, colcount, out Fldnames );
                    // Add GenericClass to our public Observable collection
                    Sprocs . Add ( genclass );
                    // Add dict to our collection of these dicts (for later indexing)
                    SpNamesExecIndex . Add ( genclass . field1 . ToString ( ), 0 );
                    SProcsListbox . ItemsSource = null;
                    SProcsListbox . Items . Clear ( );
                    // Add sp name to an ArrayList 
                    SprocsData = LoadListBoxData ( Sprocs );
                }

                SProcsListbox . ItemsSource = SprocsData;
                Task task = Task . Run (  ( ) =>
                {
                    this . Dispatcher . Invoke ( async ( ) =>
                    {
                        await LoadSpExecutionIndexing ( );
                        SProcsListbox . SelectedIndex = currentselection;
                        SetExecutionIndex ( "", currentselection );
                    } );
                });
                SProcsListbox . SelectedIndex = currentselection;
                SetExecutionIndex ( "", currentselection );
                ListTitle . Text = $"All Stored Procedures - ({SProcsListbox . Items . Count})";
            }
        }
        private void ReloadProcs_Click ( object sender, RoutedEventArgs e )
        {
            LoadAllSprocs ( );
            ExecResult . Text = $"Stored Procedures List reloaded successfully";
            ExecResult . UpdateLayout ( );
        }

        private void Printtablecontents_click ( object sender, RoutedEventArgs e )
        {
            // Called by main menu item, so just print it
            "" . Track ( 0 );
            PrintDatagrid ( ResultsDatagrid );
            "" . Track ( 1 );
        }

        public bool PrintDatagrid ( DataGrid dGrid = null )
        {
            "" . Track ( 0 );
            if ( dGrid == null )
            {
                "" . Track ( 1 );
                return false;
            }
            StringBuilder Sbreslt = PrintingSupport . ParseDbTableToCSV ( dGrid, PrintData: true );
            Type type = Sbreslt.GetType();
            if ( type == typeof ( StringBuilder ) )
            {
                string defprintfilepath = @"C:\\MyDocumentation\\TempPrintBuffer.dat";
                // sb now contains full csv format of the data in the table
                // convert StringBuilder to bytes and write to disk
                byte[] barr = Encoding.ASCII.GetBytes(Sbreslt.ToString());
                File . WriteAllBytes ( defprintfilepath, barr );
                var res = PrintingSupport. GeneralPrintItem ( null, filename: defprintfilepath );
                "" . Track ( 1 );
                return true;
            }
            "" . Track ( 1 );
            return false;
            //if ( PrintData )
            //{
            //    if ( defprintfilepath == null )
            //        defprintfilepath = @"C:\\MyDocumentation\\TempPrintBuffer.dat";
            //    // sb now contains full csv format of the data in the table
            //    // convert StringBuilder to bytes
            //    byte[] barr = Encoding.ASCII.GetBytes(sb.ToString());
            //    File . WriteAllBytes ( defprintfilepath, barr );
            //    // Request that data is printed
            //    return res;
            //}
            //else if ( ShowCsv )
            //{
            //    "" . Track ( 1 );
            //    return sb;
            //}
            //else
            //{
            //    "" . Track ( 1 );
            //    return false;
            //}

            //if ( ( bool ) reslt == true )
            //{
            //    PrintingSupport . GeneralPrintItem ( null, filename: @"C:\MyDocumentation\TempPrintBuffer.dat" );
            //    sph . ExecResult . Text = "Contents of the Datagrid have been printed successfully ....";
            //    sph . ExecResult . UpdateLayout ( );
            //    "" . Track ( 1 );
            //    return true;
            //}
            //}
            //    "" . Track ( 1 );
            //    return false;
        }

        private void PrintResultPanel ( object sender, RoutedEventArgs e )
        {
            if ( ResultsDatagrid . Visibility == Visibility . Visible )
            {
                PrintDatagrid ( ResultsDatagrid );
            }
        }

        private void DeleteScript ( object sender, RoutedEventArgs e )
        {
            // delete  current script
            string sptext = "";
            string procname = SProcsListbox . SelectedItem . ToString ( );
            bool result = LoadShowMatchingSproc(this, TextResult, procname, ref sptext);
            if ( result )
            {
                /// We need to do the following :-
                // 1st Create new file as copy with backup name
                //string scriptresult = "";
  //              CreateBackupScript ( procname, sptext , out string scriptresult);
                // that  worked, so we have a backup !!
                string delprocname = SProcsListbox . SelectedItem . ToString ( );
                string crresult="";
                ScriptViewerWin . DeleteProcedure (sptext  , delprocname , out crresult );
                if ( crresult == "SUCCESS" )
                {
                    // Reload list here ....
                    int  currsel = SProcsListbox . SelectedIndex;
                    ReloadProcs_Click ( null, null );
                    SProcsListbox . SelectedIndex = currsel;
                    SProcsListbox . UpdateLayout ( );
                    MessageBox . Show ( $"It appears that the Script [ {delprocname} ] has been deleted successfully.\n\nThe Listbox has been updated for you" );
                }
            }
        }
        public bool CreateBackupScript ( string procname, string sptext, out string result)
        {
            string[] parts1 = sptext.ToUpper().Split("CREATE PROCEDURE ");
            string buff = parts1[1];
            string[] root = buff . Split("\r\n");
            string newname = root [0] .Trim()+ "_backup";
            string newscript = $"CREATE PROCEDURE {newname}\r\n";
            for(int x = 1; x < root.Length; x++)
            {
                newscript += $"{root [x].Trim()}\r\n";
            }
            ScriptViewerWin . CreateNewStoredProcedure ( newscript,  out result , $"{newname}_Backup");
            return true;
        }

        private void OpenDataFile ( object sender, RoutedEventArgs e )
        {
            string  contents = "";
            ScriptViewerWin svw=null;
            string filename = FileHandling . GetOpenFileName ( "*.*", "", "" );
            if ( filename == "" || filename == "*.*" ) return;
            string suffix = filename. Substring (filename. Length - 4 ).ToUpper();
            if ( suffix == ".TXT" || suffix == ".XAML" || suffix == ".SQL" || suffix == ".CS" || suffix == ".SNIPPET" )
            {
                contents = File . ReadAllText ( filename );
                svw = new ScriptViewerWin ( filename, contents );
            }
            else if ( suffix == ".RTF" )    // Script viewer will load ths automatically !
                svw = new ScriptViewerWin ( filename, contents );
            else
                svw = new ScriptViewerWin ( filename, contents );

            svw . Show ( );
        }
    }
}