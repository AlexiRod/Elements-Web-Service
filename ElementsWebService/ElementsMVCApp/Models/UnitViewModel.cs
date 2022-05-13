using ElementsClassLibrary;

namespace ElementsMVCApp.Models
{
    public class UnitViewModel
    {
        public string Id { get; set; }
        public bool IsSelected { get; set; }

        public UnitViewModel(string id, bool isSelected)
        {
            Id = id;
            IsSelected = isSelected;
        }
    }
}
