using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using log4net;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;

namespace eWamLauncher.Views
{
   /// <summary>
   /// Interaction logic for EwamView.xaml
   /// This view presents an Ewam object
   /// </summary>
   public partial class EwamView : UserControl
   {

      private static readonly ILog log = LogManager.GetLogger(typeof(EnvironmentView));

      public EwamView()
      {
         InitializeComponent();
      }

      #region binariesSets actions

      /// <summary>
      /// Creates and adds a new empty binaries set in the binaries sets list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnNewBinariesSet(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            BinariesSet binariesSet = new BinariesSet();
            binariesSet.name = "new set of binaries " + ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Count.ToString();
            ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
            lbBinariesSets.SelectedItem = binariesSet;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      /// <summary>
      /// Duplicates the selected binaries set and adds the duplicate to the binaries set list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDuplicateBinariesSet(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbBinariesSets.SelectedItem == null)
            {
               return;
            }

            BinariesSet binariesSet = (BinariesSet)((BinariesSet)lbBinariesSets.SelectedItem).Clone();
            binariesSet.name += " (clone)";
            ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
            lbBinariesSets.SelectedItem = binariesSet;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      /// <summary>
      /// Delete selected launcher
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeleteBinariesSet(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            int curSelection = lbBinariesSets.SelectedIndex;
            if (lbBinariesSets.SelectedItem == null)
            {
               return;
            }

         ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Remove((BinariesSet)lbBinariesSets.SelectedItem);
            lbBinariesSets.SelectedIndex = curSelection;
            if (lbBinariesSets.SelectedIndex == -1)
               lbBinariesSets.SelectedIndex = curSelection - 1;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      /// <summary>
      /// Move selected binaries set up in the binaries set list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpBinariesSet(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbBinariesSets.SelectedItem == null)
            {
               return;
            }

            int binariesSetsIndex = ((Ewam)this.DataContext).binariesSets.IndexOf(
               (BinariesSet)lbBinariesSets.SelectedItem);

            if (binariesSetsIndex > 0)
            {
               ((Ewam)this.DataContext).binariesSets.Move(binariesSetsIndex, binariesSetsIndex - 1);
            }
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      /// <summary>
      /// Move selected binaries set down in the binaries set list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownBinariesSet(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbBinariesSets.SelectedItem == null)
            {
               return;
            }

            int binariesSetIndex = ((Ewam)this.DataContext).binariesSets.IndexOf(
               (BinariesSet)lbBinariesSets.SelectedItem);

            if (binariesSetIndex < ((Ewam)this.DataContext).binariesSets.Count() - 1)
            {
               ((Ewam)this.DataContext).binariesSets.Move(binariesSetIndex, binariesSetIndex + 1);
            }
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      //public void OnImportBinariesSets(object sender, RoutedEventArgs e)
      //{
      //   OpenFileDialog fileBrowser = new OpenFileDialog();

      //   fileBrowser.Filter = "Any file|*.exe;*.dll";
      //   fileBrowser.FilterIndex = 1;
      //   fileBrowser.RestoreDirectory = true;

      //   if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      //   {

      //      string envPath = Path.GetDirectoryName(Path.GetDirectoryName(fileBrowser.FileName));

      //      EwamImporter importer = new EwamImporter((Profile)this.profile);
      //      importer.ImportFromPath(envPath);
      //   }
      //}

      #endregion


   }
}
