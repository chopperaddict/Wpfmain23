using System;
using System . Globalization;
using System . Windows . Data;
using System . Windows . Media . Imaging;

namespace Wpfmain . Converters
{
    [ValueConversion ( typeof ( string ) , typeof ( bool ) )]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance =
            new HeaderToImageConverter ( );

        public object Convert ( object value , Type targetType ,
            object parameter , CultureInfo culture )
        {
            if ( ( value as string ) . Contains ( @"\" ) )
            {
                Uri uri = new Uri
                ( "pack://application:,,,/Icons/diskdrive.png" );
                BitmapImage source = new BitmapImage ( uri );
                return source;
            }
            else
            {
                Uri uri = new Uri ( "pack://application:,,,/Images/executable.png" );
                BitmapImage source = new BitmapImage ( uri );
                return source;
            }
        }

        public object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
        {
            throw new NotImplementedException ( );
        }
    }
}
