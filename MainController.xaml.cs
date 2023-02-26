using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

using SprocsProcessing;

using ViewModels;

using Wpfmain . Models;

namespace Wpfmain
{
    //****************************************************************************************************//
    //**************************************TABITEMVIEWMODEL*****************************************//
    //****************************************************************************************************//

    //public class TabItemViewModel
    //{
    //    // this is the title of the tab,
    //    private string _header;
    //    public string Header
    //    {
    //        get { return _header; }
    //        set  {_header = value; }
    //    }

    //    // these are the items that will be shown in the list view
    //    public ObservableCollection<string> TabCtrlItems { get; }
    //        = new ObservableCollection<string> ( ) { "Stored Procedures", "Sql Tables", "Spare" };
    //}

    //****************************************************************************************************//
    //***************************************MAINCONTROLLER*****************************************//
    //****************************************************************************************************//
    public partial class MainController : Window, INotifyPropertyChanged
    {

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
                    //IsUpdated ( null, null );
                }
            }
        }
        //#################################//
        #region ALL DECLARATIONS
        //++++++++++++++++++++++++++++++++//

        //public TabItemViewModel TabViewModel = new TabItemViewModel ();

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

        #region  DP's

        #endregion  DP's

        #endregion ALL DECLARATIONS
        public ObservableCollection<string> TabItems { get; }

        // store our list of tabs in an ObservableCollection
        // so that the UI is notified when tabs are added/removed
        //public ObservableCollection<TabItemViewModel> Tabs { get; }
        //    = new ObservableCollection<TabItemViewModel> ( );

        public MainController ( )
        {
            this . DataContext = this;
            InitializeComponent ( );
    }
        private void LoadTabls ( )
        {
        }

 
    }
}
