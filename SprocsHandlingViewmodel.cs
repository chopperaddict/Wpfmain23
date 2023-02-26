using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;

using CommunityToolkit . Mvvm . ComponentModel;

namespace Wpfmain
{
    public partial  class SprocsHandlingViewmodel : ObservableObject
    {
        SProcsHandling sph;


        public SprocsHandlingViewmodel()
        {
        }
        public SprocsHandlingViewmodel( SProcsHandling  sph)
        {
            sph = sph;
        }
        private void CloseBtn_Click ( object sender, RoutedEventArgs e )
        {
            sph . Close ( );
        }
 

    }
}
