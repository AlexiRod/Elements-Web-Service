using ElementsClassLibrary;
using System.Collections.Generic;

namespace ElementsWebAPI.Interfaces
{
    public interface IPropertyRepository
    {
        public Task<string> GetLiteratureReference(string id);
        public Task<Tuple<Dictionary<string, Unit>, Dictionary<string, string>>> GetUnitsAndPairs();

        public Task<IEnumerable<Element>> GetAllElements();
        public Task<IEnumerable<Property>> GetAllPropertiesNames();

        public Task<IEnumerable<IProperty>> GetAllPropertiesOfElementById(int id);


        public Task<IEnumerable<IProperty>> GetGivenPropertyOfElementByIds(int elemId, string propId);
        public Task<IEnumerable<IProperty>> GetGivenPropertiesOfElementById(int elemId, IEnumerable<string> propsId);


        public Task<IEnumerable<IProperty>> GetGivenPropertyValuesById(
            string propId, bool isRecomended, decimal? left, decimal? right);
        public Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropertiesValues(
            IEnumerable<string> propsId, bool isRecomended);


        public Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropertiesValuesWithQuery(
            IEnumerable<string> propsId, bool isRecomended, IEnumerable<decimal> lefts, IEnumerable<decimal> rights);
    }
}
