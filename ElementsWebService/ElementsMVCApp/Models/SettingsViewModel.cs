namespace ElementsMVCApp.Models
{
    public class SettingsViewModel
    {
        public List<Tuple<UnitViewModel, UnitViewModel>> Units { set; get; }
        public List<string> SelectedUnits { get; set; }
    }
}
