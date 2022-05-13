namespace ElementsMVCApp.Models
{
    public class PropertyTableViewModel
    {

        public List<string> Elements { get; set; }
        public List<string> Properties { get; set; }
        public List<string> PropertiesNames { get; set; }
        public List<List<decimal>> Values { get; set; }

        public PropertyTableViewModel(List<string> elements, List<string> properties, List<string> propsNames) 
        {
            Elements = elements; 
            Properties = properties;
            PropertiesNames = propsNames;
            Values = new List<List<decimal>>();
            foreach (var item in elements)
                Values.Add(new List<decimal>(new decimal[properties.Count]));
        }
        public PropertyTableViewModel() { }

        public void AddValue(string elem, string prop, decimal value)
        {
            int i = Elements.IndexOf(elem);
            int j = PropertiesNames.IndexOf(prop);
            Values[i][j] = value;
        }
    }
}
