﻿using System;
using System . Collections;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Windows;

using CommunityToolkit . Mvvm . ComponentModel;

using Dapper;

using Microsoft . Data . SqlClient;

using ViewModels;

namespace Wpfmain . Dapper
{

    [Serializable]
    public partial class DapperGeneric : ObservableObject
    {
        // All these declarations are required
        public static ObservableCollection<GenericClass> objG = new ObservableCollection<GenericClass> ( );
        //public static ObservableCollection<BankAccountViewModel>  objB = new ObservableCollection<BankAccountViewModel>();
        //public static ObservableCollection<CustomerViewModel>  objC = new ObservableCollection<CustomerViewModel>();
        //public static ObservableCollection<DetailsViewModel>  objD = new ObservableCollection<DetailsViewModel>();
        public static GenericClass objGclass = new GenericClass ( );
        public static List<string> objL = new List<string> ( );

        // Handles SP and Txt requests.....
        public static IEnumerable<dynamic> ExecuteSPGenericClass<IEnumerable> (
            ObservableCollection<IEnumerable> collection,
            string constring,
            string SqlCommand,
            string Arguments,
            string WhereClause,
            string OrderByClause,
            out List<string> genericlist,
            out string errormsg )
        {

            genericlist = new List<string> ( );
            errormsg = "";

            Debug . WriteLine ( $"objG.Count = {objG . Count ( )}" );
            //====================================
            // Use DAPPER to run a Stored Procedure
            //====================================
            string result = "";
            errormsg = "";
            string ConString = Flags . CurrentConnectionString;
            ConString = constring;
            if ( ConString == null || ConString == "" )
            {
                ConString = Flags . CurrentConnectionString;
            }
            genericlist = new List<string> ( );

            string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
            Dictionary<string , object> dict = new Dictionary<string , object> ( );
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    // Use DAPPER to run  Stored Procedure
                    try
                    {
                        // Parse out the arguments and put them in correct order for all SP's
                        if ( Arguments . Contains ( "," ) )
                        {
                            string [ ] args = Arguments . Trim ( ) . Split ( ',' );
                            //string[] args = DbName.Split(',');
                            for ( int x = 0 ; x < args . Length ; x++ )
                            {
                                switch ( x )
                                {
                                    case 0:
                                        arg1 = args [ x ];
                                        if ( arg1 . Contains ( "," ) )              // trim comma off
                                            arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                                        break;
                                    case 1:
                                        arg2 = args [ x ];
                                        if ( arg2 . Contains ( "," ) )              // trim comma off
                                            arg2 = arg2 . Substring ( 0, arg2 . Length - 1 );
                                        break;
                                    case 2:
                                        arg3 = args [ x ];
                                        if ( arg3 . Contains ( "," ) )         // trim comma off
                                            arg3 = arg3 . Substring ( 0, arg3 . Length - 1 );
                                        break;
                                    case 3:
                                        arg4 = args [ x ];
                                        if ( arg4 . Contains ( "," ) )         // trim comma off
                                            arg4 = arg4 . Substring ( 0, arg4 . Length - 1 );
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // One or No arguments
                            arg1 = Arguments;
                            if ( arg1 . Contains ( "," ) )              // trim comma off
                                arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                        }
                        // Create our aguments using the Dynamic parameters provided by Dapper
                        var Params = new DynamicParameters ( );
                        if ( arg1 != "" )
                            Params . Add ( "Arg1", arg1, DbType . String, ParameterDirection . Input, arg1 . Length );
                        if ( arg2 != "" )
                            Params . Add ( "Arg2", arg2, DbType . String, ParameterDirection . Input, arg2 . Length );
                        if ( arg3 != "" )
                            Params . Add ( "Arg3", arg3, DbType . String, ParameterDirection . Input, arg3 . Length );
                        if ( arg4 != "" )
                            Params . Add ( "Arg4", arg4, DbType . String, ParameterDirection . Input, arg4 . Length );
                        // Call Dapper to get results using it's StoredProcedures method which returns
                        // a Dynamic IEnumerable that we then parse via a dictionary into collection of GenericClass  records
                        int colcount = 0;
                        // process a standard sql query string
                        if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
                        {
                            Debug . WriteLine ( $"Running Select db.Query" );
                            //***************************************************************************************************************//
                            var reslt = db . Query ( SqlCommand , CommandType . Text );
                            //***************************************************************************************************************//

                            if ( reslt == null )
                            {
                                errormsg = "DT";
                                return default ( IEnumerable<dynamic> );
                            }
                            else
                            {
                                return reslt;
                                /*
                                                                return reslt;
                                                                //Although this is duplicated  with the one below we CANNOT make it a method()
                                                                errormsg = "DYNAMIC";
                                                                int dictcount = 0;
                                                                int fldcount = 0;
                                                                try
                                                                {

                                                                    foreach ( var item in reslt )
                                                                    {
                                                                        // create new obs<T>
                                                                        // Create an instance - Activator.CreateInstance will call the default constructor.
                                                                        // This is equivalent to calling new ObservableCollection<T>().

                                                                        var openType = typeof ( ObservableCollection<T> );
                                                                        Type[] tArgs = {typeof( ObservableCollection <T>) };
                                                                        Type target = openType.MakeGenericType(tArgs);
                                                                        var gc = (GenericClass)Activator.CreateInstance (target);

                                                                        try
                                                                        {
                                                                            // we need to create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
                                                                            string buffer = "";
                                                                            gc = ParseDapperRowGen ( gc , dict , out colcount );
                                                                            // Create a dictionary entry for each record
                                                                            StringBuilder sb = new StringBuilder();
                                                                            int indx=2;
                                                                            colcount = 0;
                                                                            foreach ( var item2 in item )
                                                                            {
                                                                                try
                                                                                {
                                                                                    if ( item . Key == "" || item . Value == null )
                                                                                        break;
                                                                                    dict . Add ( item . Key , item . Value );
                                                                                }
                                                                                catch ( Exception ex )
                                                                                {
                                                                                    MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
                                                                                }
                                                                            }
                                                                            colcount = indx;
                                                                            dictcount = 1;
                                                                            fldcount = dict . Count;
                                                                            string tmp="";
                                                                            foreach ( var pair in dict )
                                                                            {
                                                                                try
                                                                                {
                                                                                    if ( pair . Key != null && pair . Value != null )
                                                                                    {
                                                                                        DapperSupport . AddDictPairToGeneric<GenericClass> ( gc , pair , dictcount++ );
                                                                                        tmp = pair . Key . ToString ( ) + "=" + pair . Value . ToString ( );
                                                                                        buffer += tmp + ",";
                                                                                    }
                                                                                }
                                                                                catch ( Exception ex )
                                                                                {
                                                                                    Debug . WriteLine ( $"Dictionary ERROR : {ex . Message}" );
                                                                                    result = ex . Message;
                                                                                }
                                                                            }
                                                                            //remove trailing comma
                                                                            string s = buffer . Substring (0, buffer . Length - 1 );
                                                                            buffer = s;
                                                                            genericlist . Add ( buffer );
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
                                                                }
                                                                catch ( Exception ex )
                                                                {
                                                                    Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
                                                                    result = ex . Message;
                                                                }
                                                                errormsg = $"DYNAMIC:{fldcount}";
                                                                return null;
                                */
                            }
                        }
                        else
                        {
                            // probably a stored procedure ?  							
                            bool IsSuccess = false;
                            Debug . WriteLine ( $"Running SP db.Query" );

                            //***************************************************************************************************************//
                            //WORKING  JUST FINE FOR OBSERVABLECOLLECTION<GENERICCLASS>
                            var reslt = db . Query ( SqlCommand ,
                                                                               Params
                                                                               , commandType: CommandType . StoredProcedure );
                            //***************************************************************************************************************//
                            // Generic trial 	     Both work as long as classes are marked as [Serializable]
                            //                          var Tobjtype = typeof(T );
                            //							var  z = SqlServerCommands .deepClone( collection);

                            if ( reslt != null )
                            {
                                //Although this is duplicated  with the one above we CANNOT make it a method()
                                int dictcount = 0;
                                int fldcount = 0;
                                long zero = reslt . LongCount ( );
                                try
                                {
                                    foreach ( var item in reslt )
                                    {
                                        GenericClass gc = new GenericClass ( );
                                        try
                                        {
                                            //	Create a dictionary for each row of data then add it to a GenericClass row then add row to Generics Db
                                            gc = ParseDapperRowGen ( item, dict, out colcount );
                                            dictcount = 1;
                                            fldcount = dict . Count;
                                            if ( fldcount == 0 )
                                            {
                                                //no problem, we will get a Datatable anyway
                                                return null;
                                            }
                                            foreach ( var pair in dict )
                                            {
                                                try
                                                {
                                                    if ( pair . Key != null && pair . Value != null )
                                                    {
                                                        DapperSupport . AddDictPairToGeneric ( gc, pair, dictcount++ );
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
                                }
                                catch ( Exception ex )
                                {
                                    Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
                                    if ( ex . Message . Contains ( "not find stored procedure" ) )
                                    {
                                        result = $"SQL PARSE ERROR - [{ex . Message}]";
                                        errormsg = $"{result}";
                                    }
                                    else
                                    {
                                        long x = reslt . LongCount ( );
                                        if ( x == ( long ) 0 )
                                        {
                                            result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
                                            errormsg = $"DYNAMIC:0";
                                            return null;
                                        }
                                        else
                                        {
                                            result = ex . Message;
                                            errormsg = $"UNKNOWN :{ex . Message}";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ( IsSuccess == false )
                                    Debug . WriteLine ( $"Dapper request returned zero results" );
                            }
                        }
                    }
                    //	else
                    //{
                    //	errormsg = "DT";
                    //	return null;
                    //}
                    catch ( Exception ex )
                    {
                        Debug . WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
                        result = ex . Message;
                        errormsg = $"SQLERROR : {result}";
                    }
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"Sql Error, {ex . Message}, {ex . Data}" );
                    result = ex . Message;
                }
            }
            return null;
        }


        public static Dictionary<string, object> ParseDapperRowGen ( dynamic buff, Dictionary<string, object> dict, out int colcount )
        {
            //$"Entering " . dcwinfo();
            colcount = 0;
            foreach ( var item in buff )
            {
                try
                {
                    if ( item . Key == "" || item . Value == null )
                        dict . Add ( item . Key, "");
                    else
                        dict . Add ( item . Key, item . Value );
                    colcount++;
                }
                catch ( Exception ex )
                {
                    MessageBox . Show ( $"ParseDapper error was : \n{ex . Message}\nKey={item . Key} Value={item . Value . ToString ( )}" );
                    break;
                }
            }
            return dict;
        }



        public static ObservableCollection<GenericClass> ExecuteSPFullGenericClass<U1, T> (
                 ref ObservableCollection<T> collection,
                string Connstring,
                string SqlCommand,
                string Arguments,
                string WhereClause,
                string OrderByClause,
                ref List<GenericClass> genericlist,
                out string errormsg )
        {
            errormsg = "";
#pragma warning disable CS0219 // The variable 'ArgType' is assigned but its value is never used
            int ArgType = 0;
#pragma warning restore CS0219 // The variable 'ArgType' is assigned but its value is never used
            Debug . WriteLine ( $"objG.Count = {objG . Count ( )}" );
            //====================================
            // Use DAPPER to run a Stored Procedure or Select query
            //====================================
            string result = "";
#pragma warning disable CS0219 // The variable 'HasArgs' is assigned but its value is never used
            bool HasArgs = false;
#pragma warning restore CS0219 // The variable 'HasArgs' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'argcount' is assigned but its value is never used
            int argcount = 0;
#pragma warning restore CS0219 // The variable 'argcount' is assigned but its value is never used
            //DbToOpen = "";
            errormsg = "";
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == null || ConString == "" )
            {
                DapperSupport . CheckDbDomain ( "IAN1" );
                ConString = Flags . CurrentConnectionString;
            }

            string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
            Dictionary<string , object> dict = new Dictionary<string , object> ( );
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    // Use DAPPER to run  Stored Procedure
                    try
                    {
                        if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) == false )
                        // Parse out the arguments and put them in correct order for all SP's
                        {
                            if ( Arguments . Contains ( "," ) )
                            {
                                string [ ] args = Arguments . Trim ( ) . Split ( ',' );
                                //string[] args = DbName.Split(',');
                                for ( int x = 0 ; x < args . Length ; x++ )
                                {
                                    switch ( x )
                                    {
                                        case 0:
                                            arg1 = args [ x ];
                                            if ( arg1 . Contains ( "," ) )              // trim comma off
                                                arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                                            break;
                                        case 1:
                                            arg2 = args [ x ];
                                            if ( arg2 . Contains ( "," ) )              // trim comma off
                                                arg2 = arg2 . Substring ( 0, arg2 . Length - 1 );
                                            break;
                                        case 2:
                                            arg3 = args [ x ];
                                            if ( arg3 . Contains ( "," ) )         // trim comma off
                                                arg3 = arg3 . Substring ( 0, arg3 . Length - 1 );
                                            break;
                                        case 3:
                                            arg4 = args [ x ];
                                            if ( arg4 . Contains ( "," ) )         // trim comma off
                                                arg4 = arg4 . Substring ( 0, arg4 . Length - 1 );
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                // SP with One or No arguments
                                arg1 = Arguments;
                                if ( arg1 . Contains ( "," ) )              // trim comma off
                                    arg1 = arg1 . Substring ( 0, arg1 . Length - 1 );
                            }
                            {
                                // Create our aguments using the Dynamic parameters provided by Dapper
                                var Params = new DynamicParameters ( );
                                if ( arg1 != "" )
                                    Params . Add ( "Arg1", arg1, DbType . String, ParameterDirection . Input, arg1 . Length );
                                if ( arg2 != "" )
                                    Params . Add ( "Arg2", arg2, DbType . String, ParameterDirection . Input, arg2 . Length );
                                if ( arg3 != "" )
                                    Params . Add ( "Arg3", arg3, DbType . String, ParameterDirection . Input, arg3 . Length );
                                if ( arg4 != "" )
                                    Params . Add ( "Arg4", arg4, DbType . String, ParameterDirection . Input, arg4 . Length );
                                // Call Dapper to get results using it's StoredProcedures method which returns
                                // a Dynamic IEnumerable that we then parse via a dictionary into collection of GenericClass  records
                                int colcount = 0;
                                var reslt = db . Query ( SqlCommand ,
                                                                               null
                                                                               , commandType: CommandType . StoredProcedure );


                                // How to test the type if a generic
                                //if ( typeof ( T ) == typeof ( ObservableCollection<GenericClass> ) )
                                //{
                                //}
                                if ( reslt != null )
                                {
                                    //Although this is duplicated  with the one above we CANNOT make it a method()
                                    int dictcount = 0;
                                    int fldcount = 0;
                                    //									int colcount=0;
                                    long zero = reslt . LongCount ( );
                                    try
                                    {
                                        foreach ( var item in reslt )
                                        {
                                            // trial to get a new  instance of an anonymous object    - fails	 for bankaccountciewmodel
                                            GenericClass gc = new GenericClass ( );
                                            try
                                            {
                                                //	Create a dictionary entry for each row of data then add it as a row to the Generic (ObervableCollection<xxxxxx>) Class
                                                gc = ParseDapperRowGen ( item, dict, out colcount );
                                                dictcount = 1;
                                                fldcount = dict . Count;
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
                                                //IsSuccess = true;
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
                                    catch ( Exception ex )
                                    {
                                        Debug . WriteLine ( $"OUTER DICT/PROCEDURE ERROR : {ex . Message}" );
                                        if ( ex . Message . Contains ( "not find stored procedure" ) )
                                        {
                                            result = $"SQL PARSE ERROR - [{ex . Message}]";
                                            errormsg = $"{result}";
                                        }
                                        else
                                        {
                                            long x = reslt . LongCount ( );
                                            if ( x == ( long ) 0 )
                                            {
                                                result = $"ERROR : [{SqlCommand}] returned ZERO records... ";
                                                errormsg = $"DYNAMIC:0";
                                                return null;
                                            }
                                            else
                                            {
                                                result = ex . Message;
                                                errormsg = $"UNKNOWN :{ex . Message}";
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        // process a standard sql query string
                        else if ( SqlCommand . ToUpper ( ) . Contains ( "SELECT " ) )
                        {
                            Debug . WriteLine ( $"Running Select db.Query" );
                            //***************************************************************************************************************//
                            // NO Full Generic type received, so we want the Generic structure returned
                            //***************************************************************************************************************//
                            //if ( typeof ( T ) == typeof ( GenericClass ) )
                            //{
                            var reslt = db . Query<GenericClass> ( SqlCommand , CommandType . Text ) . ToList ( );
                            List<GenericClass> lo = new List<GenericClass> ( );
                            // just gotta process the Enumbarable 
                            genericlist = reslt . Select ( r => new List<GenericClass> ( ) ) as List<GenericClass>;
                            foreach ( var item in reslt )
                            {
                                lo . Add ( item as GenericClass );
                                //genericlist . Add ( item );
                            }

                            //collection = lo as ObservableCollection < BankAccountViewModel >;
                            return null;
                            //                                }
                        }
                    }
                    catch ( Exception ex ) { }
                }
                catch ( Exception ex ) { }
            }
            return null;
        }

        public static dynamic ExecuteWithDapper ( string SqlCommand, List<string [ ]> ArgsList, out int result, out string error )
        {
            //Arguments is a string[3] where [0] & [1] are used
            result = -1;
            error = "";
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == null || ConString == "" )
            {
                DapperSupport . CheckDbDomain ( "IAN1" );
                ConString = Flags . CurrentConnectionString;
            }

            string arg1 = "", arg2 = "", arg3 = "", arg4 = "";
            Dictionary<string , object> dict = new Dictionary<string , object> ( );
            dynamic execresult =null;
            using ( IDbConnection db = new SqlConnection ( ConString ) )
            {
                try
                {
                    // Use DAPPER to run  Stored Procedure
                    // Create our aguments using the Dynamic parameters provided by Dapper
                    var Params = new DynamicParameters ( );
                    Params = SProcsHandling.ParseNewSqlArgs ( Params, ArgsList, out error );
   
                        // Call Dapper to get results using it's StoredProcedures method which returns
                        // a Dynamic IEnumerable that we then parse via a dictionary into collection of GenericClass  records
                    //********************************************************************************//
                    execresult = db . Execute ( SqlCommand, Params, commandType: CommandType . StoredProcedure );
                    //********************************************************************************//
                    result = 1;
                    //if ( execresult . Count > 0 )
                    //    CounterCreationData = 0;
                }
                catch ( Exception ex )
                {
                    Debug . WriteLine ( $"STORED PROCEDURE ERROR : {ex . Message}" );
                    error = $"SQLERROR : {ex.Message}";
                }
                return execresult;
            }
        }

        public static DbType GetDbType ( string Argument )
        {
            if ( Argument == "STRING" )
                return DbType . String;
            else if ( Argument == "INTEGER" )
                return DbType . Int32;
            else if ( Argument == "BOOL" )
                return DbType . Boolean;
            else if ( Argument == "DOUBLE" )
                return DbType . Double;
            else if ( Argument == "CURRENCY" )
                return DbType . Currency;
            else if ( Argument == "DECIMAL" )
                return DbType . Decimal;
            else if ( Argument == "OBJECT" )
                return DbType . Object;

            return DbType . Object;
        }
    }
}