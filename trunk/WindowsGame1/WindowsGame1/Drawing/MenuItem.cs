using System.Collections.Generic;
using System.Linq;

namespace WGiBeat.Drawing
{
    public class MenuItem
    {
        private readonly List<string> _optionDisplays;
        private readonly List<object> _optionValues;

        public string ItemText { get; set; }
        private int _selectedOption;

        public MenuItem()
        {
            _optionValues = new List<object>();
            _optionDisplays = new List<string>();
            _selectedOption = 0;
        }

        public void AddOption(string display, object value)
        {
            _optionDisplays.Add(display);
            _optionValues.Add(value);
        }

        public void RemoveOption(string display)
        {
            int idx = _optionDisplays.IndexOf(display);
            _optionDisplays.RemoveAt(idx);
            _optionValues.RemoveAt(idx);
        }

        public void IncrementSelected()
        {
            if (_optionValues.Count > 0)
            {
                _selectedOption += 1;
                _selectedOption %= _optionValues.Count;
            }
        }

        public void DecrementSelected()
        {
            if (_optionValues.Count > 0)
            {
                _selectedOption -= 1;
                if (_selectedOption < 0)
                {
                    _selectedOption = _optionValues.Count - 1;
                }
            }
        }

        public string SelectedText()
        {
            if (_optionDisplays.Count == 0)
            {
                return "";
            }
            return _optionDisplays[_selectedOption];

        }

        public object SelectedValue()
        {
            if (_optionValues.Count == 0)
            {
                return null;
            }
            return _optionValues[_selectedOption];
        }

        public void SetSelectedByValue(object value)
        {
            var selected = (from e in _optionValues where value.Equals(e) select e).FirstOrDefault();
            if (selected != null)
            {
                _selectedOption = _optionValues.IndexOf(selected);
            }
        }


    }
}
