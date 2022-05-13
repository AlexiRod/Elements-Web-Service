using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElementsMVCApp.Models
{
    public class PropertyListViewModel
    {
        public IList<string> SelectedProps { get; set; }
        public IList<SelectListItem> AvailableProps { get; set; }

        public PropertyListViewModel()
        {
            SelectedProps = new List<string>();
            AvailableProps = new List<SelectListItem>();
        }
    }
}
