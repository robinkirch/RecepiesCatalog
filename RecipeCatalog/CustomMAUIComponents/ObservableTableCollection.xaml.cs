﻿using RecipeCatalog.Helper;
using RecipeCatalog.Resources.Language;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RecipeCatalog.CustomMAUIComponents
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObservableTableCollection: ContentView
    {
        private ObservableCollection<object> _itemsSource;

        /// <summary>
        /// Sets the items source for the table and rebuilds the table based on the provided data.
        /// </summary>
        /// <value>
        /// The collection of objects to be displayed in the table.
        /// </value>
        public ObservableCollection<object> ItemsSource
        {
            get => _itemsSource;
            set
            {
                _itemsSource = value;
                OnPropertyChanged();
                BuildTable();
            }
        }

        public ObservableTableCollection()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Builds the table UI based on the provided items source.
        /// Dynamically generates columns and rows based on the properties of the items in the collection.
        /// </summary>
        /// <param name="title">The title to display at the top of the table (optional).</param>
        public void BuildTable(string title = "")
        {
            TableTitle.Text = title;

            TableGrid.Children.Clear();


            var itemType = _itemsSource.FirstOrDefault()?.GetType();
            if (_itemsSource == null || _itemsSource.Count == 0)
            {
                var nothingLabel = new Label
                {
                    Text = "",
                };
                var noContentLabel = new Label
                {
                    Text = AppLanguage.NoEntry,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                TableGrid.Add(nothingLabel, 0, 2);
                TableGrid.Add(noContentLabel, 0, 3);
                Grid.SetColumnSpan(noContentLabel, TableGrid.ColumnDefinitions.Count);
                return;
            }
            if (itemType == null) return;
            var properties = itemType.GetProperties();

            double totalwidth = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                string propertyName = GetTranslatedPropertyName(property);
                string longestText = "";

                for (int rowIndex = 0; rowIndex < _itemsSource.Count; rowIndex++)
                {
                    var item = _itemsSource[rowIndex];
                    var cellValue = property.GetValue(item)?.ToString() ?? string.Empty;

                    if (cellValue.Length > longestText.Length)
                    {
                        longestText = cellValue;
                    }
                }
                if (longestText.Length < propertyName.Length)
                {
                    longestText = propertyName;
                }

                double estimatedWidth = longestText.Length * 9; // 10 ca. per character + spacing
                totalwidth += estimatedWidth;
                TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(estimatedWidth, GridUnitType.Absolute) });
                if(i == 0) // for BoxView Border
                    TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Absolute) });

                TableGrid.Add(new Label
                {
                    Text = propertyName,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Start
                }, i > 0 ? i+1 : 0, 0);// for BoxView Border

                if(i == properties.Length - 1)
                {
                    TableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                }
            }

            var bottomBorder = new BoxView { HeightRequest = 1, Color = Colors.Gray };
            TableGrid.Add(bottomBorder, 0, 1);
            Grid.SetColumnSpan(bottomBorder, (int)totalwidth+10);

            // data
            for (int rowIndex = 0; rowIndex < _itemsSource.Count; rowIndex++)
            {
                var item = _itemsSource[rowIndex];
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
                    else if (property.PropertyType == typeof(int))
                    {
                        var intValue = (int)(property.GetValue(item) ?? 0);
                        var shortEntry = new Entry { Keyboard = Keyboard.Numeric, Text = intValue.ToString(), WidthRequest = 30 };
                        shortEntry.TextChanged += (s, e) =>
                        {
                            if (int.TryParse(e.NewTextValue, out int newValue))
                                property.SetValue(item, newValue);
                            else
                                shortEntry.Text = intValue.ToString();
                        };
                        cellContent = shortEntry;
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

        /// <summary>
        /// Retrieves the translated name for the given property using the TranslationAttribute.
        /// If no translation is found, the default property name is returned, split into words.
        /// </summary>
        /// <param name="property">The property to get the translated name for.</param>
        /// <returns>The translated property name, or the default name if no translation is found.</returns>
        private string GetTranslatedPropertyName(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<TranslationAttribute>();
            if (attribute != null)
            {
                return AppLanguage.ResourceManager.GetString(attribute.TranslationKey) ?? string.Join(" ", Regex.Split(property.Name.ToString(), @"(?=[A-Z])")); ;
            }
            return string.Join(" ", Regex.Split(property.Name.ToString(), @"(?=[A-Z])"));
        }
    }
}