-- STANDARD  HEADER SETUP:
-- Modify as you wish ....
/* NB: This script will be executed by  the system using the 
"1. SP returning a Table as ObservableCollection" option, so
you should expect the script to return some form of SQL table contents  */

Use [IAN1]  -- the default database !
GO
SET  ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Start of user script :-

select top(10) * from customer where fname like 'turner'


-- Script saved by system as C:\WPFMAIN\SQLSCRIPTS\SQLSCRIPT4555454545454.SQL on 18:55-13/02/2023