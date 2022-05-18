using Dapper;
using ElementsClassLibrary;
using ElementsWebAPI.Interfaces;
using System.Globalization;

namespace ElementsWebAPI.Entities
{
    /// <summary>
    /// Класс репозитория, работающего с БД
    /// </summary>
    public class PropertyRepository : IPropertyRepository
    {
        #region Глобальные переменные и конструктор

        /// <summary>
        /// Контекст для работы с базой данных.
        /// </summary>
        private readonly DapperContext _context;
        
        /// <summary>
        /// Список Id свойств ионного радиуса, имеющими дополнительные значения.
        /// </summary>
        private static readonly List<string> riPropIds = new List<string>() { "S11", "S12", "S13", "S14", "S15" };
        /// <summary>
        /// Список Id таблиц для организации запросов к БД.
        /// </summary>
        private static readonly List<string> propIds = new List<string>() { "Properties", "S11", "S12", "S13", "S14", "S15" };
        
        /// <summary>
        /// Конструктор с определением контекста, списка таблиц и конвертируемых единиц измерения.
        /// </summary>
        /// <param name="context"></param>
        public PropertyRepository(DapperContext context)
        {
            _context = context;
        }

        #endregion





        #region Get-методы для получения данных из БД


        /// <summary>
        /// Метод получения информации об источнике по заданной ссылке
        /// </summary>
        /// <param name="id">Номер ссылки</param>
        /// <returns>Информация об источнике по ссылке</returns>
        public async Task<string> GetLiteratureReference(string id)
        {
            using (var connection = _context.CreateConnection())
            {

                string query = @$"SELECT * FROM ((SELECT [Article], [Source], [Vol], [Num], [Page1], [Page2] FROM [dbo].[bData] WHERE N = {id}) dat
                                LEFT JOIN (SELECT TOP 1 N, [Year1], [Year2] FROM [dbo].[bYears] WHERE N = {id}) yer ON 1=1
                                LEFT JOIN (SELECT TOP 1 N, [DOI] FROM [dbo].bPdf WHERE N = {id}) doi ON 1=1)";
                LiteratureReference litref = connection.Query<LiteratureReference>(query).FirstOrDefault();
                if (litref == null)
                    return "No data";

                query = $"SELECT DISTINCT [Author] FROM [dbo].[bAuthors] WHERE N = {id}";
                List<string> authors = connection.Query<string>(query).ToList();
                litref.Authors = authors;


                return litref.ToString();

            }
        }

        /// <summary>
        /// Метод чтения из БД и установки пар конвертируемых ЕИ а также уравнения конвертации.
        /// </summary>
        /// /// <remarks>
        /// Важно: Так как готовые таблицы были сочтены некорректными и неподходящими для работы, было принято решение 
        /// о создании двух вспомогательных таблиц rUnits и rUnitConv для правильной и удобной работой с ЕИ.
        /// Одна хранит Id конвертируемой величины, первую ЕИ, вторую ЕИ, уравнение конвертации.
        /// Вторая хранит Id свойства, его название, Id конвертируемой величины, в которой измеряется свойство.
        /// </remarks>
        public async Task<Tuple<Dictionary<string, Unit>, Dictionary<string, string>>> GetUnitsAndPairs()
        {
            Dictionary<string, Unit> units = new Dictionary<string, Unit>();
            Dictionary<string, string> unitPairs = new Dictionary<string, string>();

            using (var connection = _context.CreateConnection())
            {
                string query = @"SELECT Unit1, Unit2, Equation FROM [dbo].[rUnits]
                               WHERE Equation IS NOT NULL ORDER BY Id";
                List<Unit> list = connection.Query<Unit>(query).ToList();
                foreach (var unit in list)
                {
                    units.TryAdd(unit.Unit1, unit);
                    unitPairs.TryAdd(unit.Unit1, unit.Unit2);
                }
                return new Tuple<Dictionary<string, Unit>, Dictionary<string, string>>(units, unitPairs);
            }
        }


        /// <summary>
        /// Все элементы таблицы Менделеева с основными свойствами.
        /// </summary>
        /// <returns>Список всех элементов.</returns>
        public async Task<IEnumerable<Element>> GetAllElements() 
        {
            using (var connection = _context.CreateConnection())
            {
                var elems = new List<Element>();
                string query = @"SELECT el.Id, el.Elem as Symbol, aw.AtomicWeight, den.Density, melt.MeltingTemp, boil.BoilingTemp FROM [dbo].[xElements] el
                                INNER JOIN (SELECT * FROM (SELECT a.Symbol, a.Value as AtomicWeight, (ROW_NUMBER() OVER (PARTITION BY Symbol ORDER BY Symbol DESC)) row  
                                FROM [dbo].[A6] a) un WHERE row = 1) aw ON aw.Symbol = el.Elem 
                                INNER JOIN (SELECT * FROM (SELECT a.Symbol, a.Value as Density, (ROW_NUMBER() OVER (PARTITION BY Symbol ORDER BY Symbol DESC)) row  
                                FROM [dbo].[I5] a) un WHERE row = 1) den ON den.Symbol = el.Elem 
                                INNER JOIN (SELECT * FROM (SELECT a.Symbol, a.Value as MeltingTemp, (ROW_NUMBER() OVER (PARTITION BY Symbol ORDER BY Symbol DESC)) row  
                                FROM [dbo].[C1] a) un WHERE row = 1) melt ON melt.Symbol = el.Elem 
                                INNER JOIN (SELECT * FROM (SELECT a.Symbol, a.Value as BoilingTemp, (ROW_NUMBER() OVER (PARTITION BY Symbol ORDER BY Symbol DESC)) row  
                                FROM [dbo].[C2] a) un WHERE row = 1) boil ON boil.Symbol = el.Elem 
                                ORDER BY el.Id";
                elems.AddRange(await connection.QueryAsync<Element>(query));
                return elems;
            }
        }


        /// <summary>
        /// Все свойства из БД в формате Id, Название, Тип
        /// </summary>
        /// <returns>Список всех свойств, где Id, Name и Value - значения Id, Названия и Типа свойства.</returns>
        public async Task<IEnumerable<Property>> GetAllPropertiesNames()
        {
            using (var connection = _context.CreateConnection())
            {
                string query = @"SELECT PropertyNumber as Id, PropertyName as Name, PropertyType as Value, Unit
                                FROM[dbo].[PropertiesNames] 
								INNER JOIN (SELECT NProp, Unit1 as Unit from [dbo].[rPropUnit] 
								INNER JOIN [dbo].[rUnits] ON UnitId = Id) as u ON PropertyNumber = u.NProp  ORDER BY Name";
                return await connection.QueryAsync<Property>(query);
            }
        }

        /// <summary>
        /// Все свойства выбранного из таблицы элемента.
        /// </summary>
        /// <param name="id">Id элемента.</param>
        /// <returns>Список всех значений всех свойств выбранного элемента.</returns>
        public async Task<IEnumerable<IProperty>> GetAllPropertiesOfElementById(int id)
        {
            using (var connection = _context.CreateConnection())
            {
                var allProps = new List<IProperty>();
                foreach (string propId in propIds)
                {
                    string RIField = riPropIds.Contains(propId) ? ", Charge" : "";
                    string RIShanonField = propId == "S15" ? ", CN" : "";
                    string query = $@"SELECT pr.NProp as Id, Symbol as ElementSymbol, PropertyName as Name, Value, Unit, Comments, 
                                    Nref as Reference{RIField}{RIShanonField} FROM [dbo].[{propId}] pr
                                    INNER JOIN [dbo].[PropertiesNames] pn ON pr.Nprop = pn.PropertyNumber
                                    INNER JOIN (SELECT NProp, Unit1 as Unit from [dbo].[rPropUnit] 
									INNER JOIN [dbo].[rUnits] ON UnitId = Id) as u ON pr.Nprop = u.NProp 
                                    WHERE Symbol = (SELECT Elem FROM xElements el WHERE el.Id = @Id) ORDER BY Name";
                    allProps.AddRange(riPropIds.Contains(propId) ?
                        (await connection.QueryAsync<RIProperty>(query, new { id })).OrderBy(x => x.Id) :
                        (await connection.QueryAsync<Property>(query, new { id })));
                }
                return allProps;
            }
        }



        /// <summary>
        /// Конкретное свойство по Id для выбранного элемента по Id.
        /// </summary> /// 
        /// <param name="elemId">Id элемента.</param>
        /// <param name="propId">Id свойства.</param>
        /// <returns>Список всех значений конкретного свойства для выбранного элемента.</returns>
        public async Task<IEnumerable<IProperty>> GetGivenPropertyOfElementByIds(int elemId, string propId)
        {
            string tableFrom = "Properties";
            string RIField = "";
            string RIShanonField = propId == "S15" ? ", CN" : "";
            if (riPropIds.Contains(propId))
            {
                tableFrom = propId;
                RIField = ", Charge";
            }

            string query = $@"SELECT pr.NProp as Id, Symbol as ElementSymbol, PropertyName as Name, Value, Unit, Comments, 
                    Nref as Reference{RIField}{RIShanonField} FROM [dbo].[{tableFrom}] pr
                    INNER JOIN [dbo].[PropertiesNames] pn ON pr.Nprop = pn.PropertyNumber
                    INNER JOIN (SELECT NProp, Unit1 as Unit from [dbo].[rPropUnit] 
					INNER JOIN [dbo].[rUnits] ON UnitId = Id) as u ON pr.Nprop = u.NProp 
                    WHERE pr.NProp = @propId AND Symbol = (SELECT Elem FROM xElements el WHERE el.Id = @elemId)";

            using (var connection = _context.CreateConnection())
            {
                var props = riPropIds.Contains(propId) ?
                        (await connection.QueryAsync<RIProperty>(query, new { propId, elemId })).OrderBy(x => x.Id) :
                        (await connection.QueryAsync<Property>(query, new { propId, elemId }));
                return props;
            }
        }

        /// <summary>
        /// Выбранные свойства выбранного элемента.
        /// </summary>
        /// <param name="elemId">Id элемента.</param>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <returns>Список выбранных свойств выбранного элемента.</returns>
        public async Task<IEnumerable<IProperty>> GetGivenPropertiesOfElementById(int elemId, IEnumerable<string> propsId)
        {
            List<IProperty> props = new List<IProperty>();
            foreach (string propId in propsId)
            {
                var prop = await GetGivenPropertyOfElementByIds(elemId, propId);
                props.AddRange(prop);
            }
            return props;
        }



        /// <summary>
        /// Значения конкретного свойства для всех элементов с возможностью выбора рекомендованного значения и диапозона значений.
        /// </summary>
        /// <param name="propId">Id свойства.</param>
        /// <param name="isRecomended">Флаг: все значения | рекомендованные значения.</param>
        /// <param name="left">Левая граница диапозона (опционально).</param>
        /// <param name="right">Правая граница диапозона (опционально).</param>
        /// <returns>Список всех значений свойства.</returns>
        public async Task<IEnumerable<IProperty>> GetGivenPropertyValuesById(
            string propId, bool isRecomended, decimal? left, decimal? right)
        {
            string tableFrom = "Properties";
            string RIField = "";
            string RIShanonField = propId == "S15" ? ", CN" : "";
            string queryRec = isRecomended ? " AND Rec = 1" : "";
            string queryLeft = left.HasValue ? $" AND Value >= {left.Value.ToString(CultureInfo.InvariantCulture)}" : "";
            string queryRight = right.HasValue ? $" AND Value <= {right.Value.ToString(CultureInfo.InvariantCulture)}" : "";
            if (riPropIds.Contains(propId))
            {
                tableFrom = propId;
                RIField = ", Charge";
            }

            string query = $@"SELECT pr.NProp as Id, Symbol as ElementSymbol, PropertyName as Name, Value, Unit, Comments, 
                    Nref as Reference, el.Id as ElId{RIField}{RIShanonField} FROM [dbo].[{tableFrom}] pr
                    INNER JOIN [dbo].[PropertiesNames] pn ON pr.Nprop = pn.PropertyNumber
                    INNER JOIN (SELECT NProp, Unit1 as Unit from [dbo].[rPropUnit] 
					INNER JOIN [dbo].[rUnits] ON UnitId = Id) as u ON pr.Nprop = u.NProp 
                    INNER JOIN [dbo].[xElements] el ON el.Elem = pr.Symbol
                    WHERE (pr.NProp = @propId){queryRec}{queryLeft}{queryRight} order by ElId";

            using (var connection = _context.CreateConnection())
            {
                var props = riPropIds.Contains(propId) ?
                    (await connection.QueryAsync<RIProperty>(query, new { propId })).OrderBy(x => x.Id) :
                    (await connection.QueryAsync<Property>(query, new { propId }));
                return props.ToList();
            }
        }

        /// <summary>
        /// Значения выбранных свойств для всех элементов с возможностью выбора рекомендованного значения.
        /// </summary>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <param name="isRecomended">Флаг: все значения | рекомендованные значения.</param>
        /// <returns>Словарь Id - List<Property> (все).</returns>
        public async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropertiesValues(
            IEnumerable<string> propsId, bool isRecomended)
        {
            Dictionary<string, IEnumerable<IProperty>> dict = new Dictionary<string, IEnumerable<IProperty>>();
            foreach (var prop in propsId)
            {
                var vals = await GetGivenPropertyValuesById(prop, isRecomended, null, null);
                dict.Add(prop, vals);
            }
            return dict;
        }



        /// <summary>
        /// Выбранные свойства со значениями из заданного диапозона значений с возможностью выбора рекомендованного значения.
        /// </summary>
        /// <remarks>
        /// Важно: Предполагается, что длины списков заданы корректно, то есть равны.
        /// </remarks>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <param name="isRecomended">Флаг: все значения | рекомендованные значения.</param>
        /// <param name="lefts">Список левых границ диапозона для каждого свойства из списка (опционально).</param>
        /// <param name="rights">Список правых границ диапозона для каждого свойства из списка (опционально).</param>
        /// <returns>Словарь Id - List<Property> (все).</returns>
        public async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropertiesValuesWithQuery(
            IEnumerable<string> propsId, bool isRecomended, IEnumerable<decimal>? lefts, IEnumerable<decimal>? rights)
        {
            if (lefts != null && rights != null && (propsId.Count() != lefts.Count() || lefts.Count() != rights.Count()))
                throw new ArgumentException("Длины списков Id свойств и границ диапозонов не равны.");

            List<IProperty> res = new List<IProperty>();
            for (int i = 0; i < propsId.Count(); i++)
            {
                List<IProperty> vals = (await GetGivenPropertyValuesById(
                    propsId.ElementAt(i), isRecomended, lefts?.ElementAt(i), rights?.ElementAt(i))).AsList();

                if (i == 0) 
                {
                    res = vals;
                    continue;
                }

                List<IProperty> intersected = new List<IProperty>();
                List<string> addedElemSymbols = new List<string>();
                foreach (var val in vals)
                {
                    if (addedElemSymbols.Contains(val.ElementSymbol))
                    {
                        intersected.Add(val);
                        continue;
                    }

                    var a = res.FindAll(y => y.ElementSymbol == val.ElementSymbol);
                    if (a.Count != 0) 
                    {
                        intersected.AddRange(a);
                        intersected.Add(val);
                        addedElemSymbols.Add(val.ElementSymbol);
                    }
                }
                res = intersected;
            }

            Dictionary<string, IEnumerable<IProperty>> dict = new Dictionary<string, IEnumerable<IProperty>>();
            foreach (var prop in res)
                if (dict.ContainsKey(prop.ElementSymbol))
                    dict[prop.ElementSymbol].AsList().Add(prop);
                else dict.Add(prop.ElementSymbol, new List<IProperty>() { prop });
            return dict;
        }

        #endregion
    }
}


