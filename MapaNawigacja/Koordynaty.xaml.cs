using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MapaNawigacja
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Koordynaty : Page
    {
        public Koordynaty()
        {
            this.InitializeComponent();
            GdzieJaNaMapie();
        }



        async void GdzieJaNaMapie()
        {
            Geolocator mojGPS = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High
            };
            Geoposition mojeZGPS = await mojGPS.GetGeopositionAsync();
            double dlugosc = mojeZGPS.Coordinate.Point.Position.Longitude;
            double szerokosc = mojeZGPS.Coordinate.Point.Position.Latitude;
            tbGPS.Text = $"długość: {dlugosc:F4} | szerokość: {szerokosc:F4}";
            DaneGeograficzne.pktStartowy = mojeZGPS.Coordinate.Point.Position;
            DaneGeograficzne.opisCelu = "";
            BasicGeoposition geoposition = new BasicGeoposition
            {
                Latitude = dlugosc,
                Longitude = szerokosc
            };
        }


        private void goBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void szukajCelu(object sender, RoutedEventArgs e)
        {
            DaneGeograficzne.opisCelu = txAdres.Text;
            if (DaneGeograficzne.opisCelu == "")
                return;
            var wynik = await MapLocationFinder.FindLocationsAsync(DaneGeograficzne.opisCelu, new Geopoint(DaneGeograficzne.pktStartowy));
            
            if (wynik.Status == MapLocationFinderStatus.Success)
            {
                try
                {
                    
                    double dlg = wynik.Locations[0].Point.Position.Longitude;
                    double szer = wynik.Locations[0].Point.Position.Latitude;
                    tbDlg.Text = $"{dlg:F4}";
                    tbSzer.Text = $"{szer:F4}";
                    DaneGeograficzne.pktDocelowy = wynik.Locations[0].Point.Position;
                    MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(
                        new Geopoint(DaneGeograficzne.pktStartowy),
                        new Geopoint(DaneGeograficzne.pktDocelowy),
                        MapRouteOptimization.Time,
                        MapRouteRestrictions.None);
                    DaneGeograficzne.distance = (routeResult.Route.LengthInMeters / 1000).ToString();
                }
                catch (Exception)
                {
                    var dlg = new Windows.UI.Popups.MessageDialog(wynik.Status.ToString(), DaneGeograficzne.opisCelu);
                    await dlg.ShowAsync();
                    DaneGeograficzne.opisCelu = wynik.Status.ToString();
                }
            }
            else
            {
                var dlg = new Windows.UI.Popups.MessageDialog(wynik.Status.ToString(), DaneGeograficzne.opisCelu);
                await dlg.ShowAsync();
                DaneGeograficzne.opisCelu = wynik.Status.ToString();
            }
        }
    }
}
