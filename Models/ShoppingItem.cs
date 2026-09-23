using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace WeeklyShoppingListWPF.Models
{
    public class ShoppingItem : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private double _quantity = 1;
        private string _unit = "шт";
        private decimal _price = 0;
        private string _category = "Інше";
        private bool _isPurchased = false;
        private DateTime _createdAt = DateTime.Now;
        private DateTime? _archivedAt = null;

        public string Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public double Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(QuantityWithUnit));
                }
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                if (SetField(ref _unit, value))
                {
                    OnPropertyChanged(nameof(QuantityWithUnit));
                }
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                if (SetField(ref _price, value))
                {
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        public string Category
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        public bool IsPurchased
        {
            get => _isPurchased;
            set => SetField(ref _isPurchased, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetField(ref _createdAt, value);
        }

        public DateTime? ArchivedAt
        {
            get => _archivedAt;
            set => SetField(ref _archivedAt, value);
        }

        [JsonIgnore]
        public decimal TotalPrice => (decimal)_quantity * _price;

        [JsonIgnore]
        public string QuantityWithUnit => $"{_quantity} {_unit}";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
