using System.Collections.ObjectModel;
using System.Reflection;

namespace RecipeCatalog.CustomMAUIComponents
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObservableTableCollection : ContentView
    {
        public ObservableTableCollection()
        {
            InitializeComponent();
        }

        public void BuildTable<T>(ObservableCollection<T> items, string title = "")
        {
            TableTitle.Text = title;

            TableGrid.Children.Clear();
            if (items == null || items.Count == 0) return;

            var properties = typeof(T).GetProperties();

            double totalwidth = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                string longestText = "";

                for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
                {
                    var item = items[rowIndex];
                    var cellValue = property.GetValue(item)?.ToString() ?? string.Empty;

                    if (cellValue.Length > longestText.Length)
                    {
                        longestText = cellValue;
                    }
                }
                if(longestText.Length < properties[i].Name.ToString().Length)
                    longestText = properties[i].Name.ToString();

                double estimatedWidth = longestText.Length * 11; // 10 ca. per character
                totalwidth += estimatedWidth;
                TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(estimatedWidth, GridUnitType.Absolute) });
                if(i == 0) // for BoxView Border
                    TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Absolute) });

                TableGrid.Add(new Label
                {
                    Text = properties[i].Name,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Start
                }, i > 0 ? i+1 : 0, 0);// for BoxView Border
            }

            var bottomBorder = new BoxView { HeightRequest = 1, Color = Colors.Gray };
            TableGrid.Add(bottomBorder, 0, 1);
            Grid.SetColumnSpan(bottomBorder, (int)totalwidth+10);

            // data
            for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
            {
                var item = items[rowIndex];
                for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                {
                    var property = properties[colIndex];
                    var cellValue = property.GetValue(item)?.ToString() ?? string.Empty;

                    View cellContent;

                    if (property.PropertyType == typeof(bool))
                    {
                        var checkBox = new CheckBox { IsChecked = (bool)property.GetValue(item) };
                        checkBox.CheckedChanged += (s, e) =>
                        {
                            property.SetValue(item, e.Value);
                        };
                        cellContent = checkBox;
                    }
                    else
                    {
                        cellContent = new Label { Text = cellValue, VerticalTextAlignment = TextAlignment.Center };
                    }

                    if (colIndex == 0)
                    {
                        TableGrid.Add(cellContent, colIndex, rowIndex + 2);
                        var rightBorder = new BoxView { WidthRequest = 1, Color = Colors.Gray };
                        TableGrid.Add(rightBorder, 1, rowIndex + 2);
                    }
                    else
                    {
                        TableGrid.Add(cellContent, colIndex+1, rowIndex + 2);
                    }

                }
            }
        }
    }
}