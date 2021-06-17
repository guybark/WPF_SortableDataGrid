using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WPFDataGridSorted
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Populate the sample DataGrid with data.
            ObservableCollection<SampleItem> sampleItems = GetData();
            SampleDataGrid.DataContext = sampleItems;

            // Create to the DataGird bring resorted, once the sort is complete.
            SampleDataGrid.Sorted += OnSorted;
        }

        private ObservableCollection<SampleItem> GetData()
        {
            var samplesItems = new ObservableCollection<SampleItem>();

            var samplesItem1 = new SampleItem();
            samplesItem1.Label = "Hummingbird";
            samplesItem1.LabelId = "HB";
            samplesItem1.Description = "A small bird.";
            samplesItems.Add(samplesItem1);

            var samplesItem2 = new SampleItem();
            samplesItem2.Label = "Sparrow";
            samplesItem2.LabelId = "SP";
            samplesItem2.Description = "A medium bird.";
            samplesItems.Add(samplesItem2);

            var samplesItem3 = new SampleItem();
            samplesItem3.Label = "Towhee";
            samplesItem3.LabelId = "TH";
            samplesItem3.Description = "A bigger bird.";
            samplesItems.Add(samplesItem3);

            return samplesItems;
        }

        public class SampleItem
        {
            public string Label { get; set; }
            public string LabelId { get; set; }
            public string Description { get; set; }
        }

        private void OnSorted(object sender, ValueEventArgs<DataGridColumn> valueEventArgs)
        {
            var sortedColumn = valueEventArgs.Value;

            // Get all the column headers in this DataGrid.
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(sender as DataGrid);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                // If this is not the sorted column, clear any item status that may already exist 
                // due to the DataGrid havng been previoulsy sorted by this column. If this sample
                // app, the ItemStatus is not use for any purpose other than conveying the current
                // sort order.
                if (columnHeader.Column != sortedColumn)
                {
                    AutomationProperties.SetItemStatus(columnHeader, "");
                }
                else
                {
                    // Get the current UIA ItemStatus from the header element. ​
                    string oldStatus = AutomationProperties.GetItemStatus(columnHeader);

                    // Set the new status based on the current sort order.
                    string newStatus = columnHeader.SortDirection.ToString();

                    // Now set the new UIA ItemStatus on the header element.
                    AutomationProperties.SetItemStatus(columnHeader, newStatus);

                    // Having just set the new ItemStatus, raise a UIA property changed event.
                    // Note that the peer may be null here unless a UIA client app such as
                    // Narrator or the Accessibility Insights for Windows tool are running.
                    var peer = FrameAutomationPeer.FromElement(columnHeader);
                    if (peer != null)
                    {
                        peer.RaisePropertyChangedEvent(
                            AutomationElementIdentifiers.ItemStatusProperty,
                            oldStatus,
                            newStatus);
                    }
                }
            }
        }

        // GetVisualChildCollection() and GetVisualChildCollection() exist to get the 
        // headers associate with the columns in the DataGrid. If you have your own 
        // preferred way to accessing the column headers, you could use that.
        public List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
    }

    // The WPF DataGrid has a Sorting event, but no Sorted event. In the Sorting event handler
    // it's only possible to know the sort order of the column being sorted, as it was before 
    // the sort. That not useful this to app, as it needs to set the ItemStatus of the header
    // to be the final sort order. As such, create a custom DataGrid which can take the necessary
    // action once the sort is complete.

    // IMPORTANT: If your own app has a way to determine the target sort order at the time the 
    // the Sorting event handler is called, take all the action required to react to the re-sort
    // in the Sorting event handler and don't bother creating the custom DataGrid.

    public class DataGridWithSortedEvent : DataGrid
    {
        public event EventHandler<ValueEventArgs<DataGridColumn>> Sorted;

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);

            if (Sorted != null)
            {
                Sorted(this, new ValueEventArgs<DataGridColumn>(eventArgs.Column));
            }
        }
    }

    public class ValueEventArgs<T> : EventArgs
    {
        public ValueEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
