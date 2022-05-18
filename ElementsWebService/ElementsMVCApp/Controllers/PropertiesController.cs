using ElementsClassLibrary;
using ElementsMVCApp.Models;
using ElementsMVCApp.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using Tavis.UriTemplates;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.

namespace ElementsMVCApp.Controllers
{
    /// <summary>
    /// Контроллер для работы с запросами по свойствам.
    /// </summary>
    public class PropertiesController : Controller
    {

        /// <summary>
        /// Логгер для отлавливания ошибок сервиса.
        /// </summary>
        private readonly ILogger<HomeController> _logger;
        /// <summary>
        /// Http клиент для взаимодействия с API.
        /// </summary>
        private readonly HttpClient client = new HttpClient();
        /// <summary>
        /// Настройки сериализации
        /// </summary>
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        /// <summary>
        /// Список всех химических элементов.
        /// </summary>
        private static List<Element> elements;
        /// <summary>
        /// Построенная на основе информации из БД таблица Менделеева.
        /// </summary>
        private static List<List<Element>> table;
        /// <summary>
        /// Список Id, названий и единиц измерения всех свойств.
        /// </summary>
        private static List<Property> propertiesNames;
        /// <summary>
        /// Список пар Id-Название свойства.
        /// </summary>
        private static List<Tuple<string, string>> propertiesPairs;


        /// <summary>
        /// Конструктор контроллера с параметрами конфигурации и логгера.
        /// </summary>
        /// <param name="logger">Экземпляр ILogger.</param>
        /// <param name="configuration">Экземпляр IConfiguration.</param>
        public PropertiesController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            client.BaseAddress = new Uri(configuration.GetConnectionString("DefaultHost"));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            propertiesNames ??= GetAllPropertiesNames().Result;
            propertiesPairs ??= propertiesNames.Select(x => new Tuple<string, string>(x.Name, x.Id)).ToList();
            elements ??= GetAllElements().Result;
            table ??= TableWorker.MakeTable(elements);
        }



        /// <summary>
        /// Установка настроек единиц измерения по-умолчанию в сессию клиента.
        /// </summary>
        public async Task<IActionResult> SetSession()
        {
            try
            {
                if (HttpContext.Session.Get<Dictionary<string, Unit>>("units") == default ||
                        HttpContext.Session.Get<Dictionary<string, string>>("unitpairs") == default)
                {

                    HttpResponseMessage response = await client.GetAsync("api/properties/units");
                    if (response.IsSuccessStatusCode)
                    {
                        string stringDeserialized = await response.Content.ReadAsAsync<string>();
                        Tuple<Dictionary<string, Unit>, Dictionary<string, string>> pair =
                            JsonConvert.DeserializeObject<Tuple<Dictionary<string, Unit>, Dictionary<string, string>>>
                            (stringDeserialized, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                        var units = pair.Item1;
                        var unitPairs = pair.Item2;
                        HttpContext.Session.Set("units", units);
                        HttpContext.Session.Set("unitpairs", unitPairs);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }
        

        /// <summary>
        /// Метод установки настроек при первом обращении к приложению.
        /// </summary>
        /// <returns></returns>
        public IActionResult SetParams()
        {
            return SetSession().Result;
        }


        /// <summary>
        /// Главная страница контроллера
        /// </summary>
        /// <returns>Переадресация на главную HomeController</returns>
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }



        /// <summary>
        /// Получение из сессии выбранных настроек пользователя и отображение страницы с их изменением. 
        /// </summary>
        [HttpGet]
        public IActionResult Settings()
        {
            try
            {
                Dictionary<string, string> unitPairs = HttpContext.Session.Get<Dictionary<string, string>>("unitpairs");
                Dictionary<string, Unit> units = HttpContext.Session.Get<Dictionary<string, Unit>>("units");

                if (units == null || unitPairs == null)
                    return NoContent(); // this won't happen 

                List<Tuple<UnitViewModel, UnitViewModel>> settingPairs = new List<Tuple<UnitViewModel, UnitViewModel>>();
                foreach (var pair in unitPairs)
                {
                    bool isFirstSelected = units[pair.Key].Unit1.Trim() == pair.Key.Trim();
                    Tuple<UnitViewModel, UnitViewModel> unitViewModel = new Tuple<UnitViewModel, UnitViewModel>(
                        new UnitViewModel(pair.Key, isFirstSelected), new UnitViewModel(pair.Value, !isFirstSelected));
                    settingPairs.Add(unitViewModel);
                }

                return View(new SettingsViewModel() { Units = settingPairs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Подтверждение выбранных пользователем настроек и изменение их в сессии.
        /// </summary>
        /// <param name="model">Выбранные настройки.</param>
        public IActionResult Settings(SettingsViewModel model)
        {
            UpdateUnits(model.SelectedUnits);
            return View("SettingsChanged");
        }
     

        /// <summary>
        /// Отображение информации об источнике по заданной ссылке.
        /// </summary>
        /// <param name="id">Ссылка на источник.</param>
        public IActionResult LitRef(string? id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return NoContent();

                string res = GetLiteratureReference(id).Result;
                ViewBag.Lit = res;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }



        /// <summary>
        /// Отображение страницы с интерактивной таблицей Менделеева.
        /// </summary>
        /// <param name="mode">Режим работы пользователя.</param>
        public IActionResult Mendel(int? mode)
        {
            try
            {
                ViewBag.Mode = mode;
                return View(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Отображение интерактивного списка для выбора свойств.
        /// </summary>
        /// <param name="mode">Режим работы пользователя.</param>
        /// <param name="elemId">Id выбранного элемента (Опционально)</param>
        [HttpGet("Properties/PropsList/{mode}/{elemId?}")]
        public IActionResult PropsList(int? mode, int? elemId)
        {
            try
            {
                List<SelectListItem> selectedList = propertiesPairs.Select(
                       x => new SelectListItem() { Text = x.Item1, Value = x.Item2 }).ToList();

                ViewBag.Mode = mode;
                ViewBag.ElemId = elemId;
                return View(new PropertyListViewModel() { AvailableProps = selectedList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Переадресация на нужный метод после выбора свойств из списка.
        /// </summary>
        /// <param name="mode">Режим работы пользователя.</param>
        /// <param name="elemId">Id выбранного элемента (Опционально)</param>
        /// <param name="propsList">Список выбранных свойств.</param>
        [HttpPost("Properties/PropsList/{mode}/{elemId?}")]
        public IActionResult PropsList(int? mode, int? elemId, PropertyListViewModel propsList)
        {
            try
            {
                if (ModelState.IsValid && mode != null)
                {
                    switch (mode.Value)
                    {
                        case 2:
                            return GivenProps(elemId, propsList);
                        case 3:
                            return PropsValues(propsList, 3);
                        case 4:
                            return RecProps(propsList);
                        case 5:
                            return QueryProps(propsList);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Отображение списка свойств заданного элемента.
        /// </summary>
        /// <param name="props">Список выбранных свойств.</param>
        /// <param name="elemId">Id выбранного элемента.</param>
        /// <param name="element">Символ выбранного элемента.</param>
        /// <param name="mode">Режим работы пользователя.</param>
        public IActionResult ElementPropsList(List<IProperty> props, int elemId, string element, int mode)
        {
            try
            {
                List<Property> properties = new List<Property>();
                List<RIProperty> riproperties = new List<RIProperty>();
                props.ForEach(x => { if (x is RIProperty) riproperties.Add((RIProperty)x); else properties.Add((Property)x); });
                ViewBag.Properties = properties;
                ViewBag.RIProperties = riproperties;
                ViewBag.ElemId = elemId;
                ViewBag.Element = element;
                ViewBag.Mode = mode;

                return View("ElementPropsList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }
        

        /// <summary>
        /// Отображение частичного представления при выборе свойства из интерактивного выпадающего списка.
        /// </summary>
        /// <param name="id">Id выбранного свойства.</param>
        /// <param name="mode">Режим работы пользователя.</param>
        public ActionResult Partial(string id, int? mode)
        {
            try
            {
                if (id == null || mode == null)
                    return NotFound();


                List<IProperty> model = mode == 3 ? GetGivenPropValuesById(id).Result : GetGivenPropOfGivenElemRec(id).Result;
                if (model == null || model.Count == 0)
                    return NotFound();

                ViewBag.Mode = mode.Value;
                return PartialView("Partial/PartialValsTable", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }



        /// <summary>
        /// Получение всех свойств выбранного элемента (Режим 1).
        /// </summary>
        /// <param name="id">Id элемента.</param>
        public IActionResult AllProps(int? id)
        {
            try
            {
                ViewBag.Mode = 1;
                if (id == null)
                    return View("ElementNotFound");

                var props = GetAllPropsOfGivenElem(id.Value).Result;
                if (props == null || props.Count == 0)
                    return View("ElementNotFound");

                return ElementPropsList(props, id.Value, props[0].ElementSymbol, 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Получение значений выбранных из списка свойств выбранного элемента (Режим 2).
        /// </summary>
        /// <param name="elemId">Id элемента.</param>
        /// <param name="propsList">Список выбранных свойств/</param>
        public IActionResult GivenProps(int? elemId, PropertyListViewModel propsList)
        {
            try
            {
                ViewBag.Mode = 2;
                if (!elemId.HasValue || elemId.Value < 1 || elemId.Value > elements.Count || propsList == null)
                    return View("ElementNotFound");

                List<string> propIds = propsList.SelectedProps.ToList();
                var props = GetGivenPropsOfGivenElem(elemId.Value, propIds).Result;
                return ElementPropsList(props, elemId.Value, elements[elemId.Value - 1].Symbol, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Получение значений выбранных из списка свойств для всех элементов таблицы Менделеева (Режим 3).
        /// </summary>
        /// <param name="propsList">Список выбранных свойств.</param>
        /// <param name="mode">Режим работы пользователя.</param>
        public IActionResult PropsValues(PropertyListViewModel propsList, int mode)
        {
            try
            {
                if (propsList == null)
                    return NotFound();

                string firstId = "";
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (var id in propsList.SelectedProps)
                {
                    firstId = firstId == "" ? id : firstId;
                    dict[id] = propertiesNames.FirstOrDefault(x => x.Id == id)?.Name;
                }

                ViewBag.Dict = dict;
                ViewBag.FirstId = firstId;
                ViewBag.Mode = mode;
                return View("PropsValues");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Отображение таблицы рекомендованных значений выбранных свойств (Режим 4).
        /// </summary>
        /// <param name="propsList">Список выбранных свйоств.</param>
        [HttpGet]
        public IActionResult RecProps(PropertyListViewModel propsList)
        {
            try
            {
                if (propsList == null)
                    return NotFound();

                Dictionary<string, Unit> units = HttpContext.Session.Get<Dictionary<string, Unit>>("units");
                Dictionary<string, IEnumerable<IProperty>> dict = GetGivenPropsOfGivenElemRec(propsList.SelectedProps.ToList()).Result;
                
                var props = propertiesNames.Where(y => propsList.SelectedProps.Contains(y.Id)).
                    Select(x => x.Name + (string.IsNullOrEmpty(x.Unit) ? "" :
                    $" {(units?.ContainsKey(x.Unit) == true ? units?[x.Unit].Unit1 : "")}")).ToList();

                var propNames = propertiesNames.Where(y => propsList.SelectedProps.Contains(y.Id))
                    .Select(x => x.Name).ToList();
                
                PropertyTableViewModel table = new PropertyTableViewModel(
                    elements.Select(x => x.Symbol.Trim()).ToList(), props, propNames);
                
                foreach (var list in dict.Values)
                    list.DistinctBy(x => x.ElementSymbol).ToList().
                        ForEach(x => table.AddValue(x.ElementSymbol.Trim(), x.Name, x.Value));

                TempData["table"] = JsonConvert.SerializeObject(table);
                return View("RecProps", table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Отображение элементов со свойствами, значения которых попадают в заданные диапазоны.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Query(QueryListViewModel query)
        {
            try
            {
                if (query == null)
                    return NotFound();

                List<string> propIds = new List<string>();
                List<decimal> lefts = new List<decimal>();
                List<decimal> rights = new List<decimal>();

                query.Queries.ForEach(x => { propIds.Add(x.PropId); lefts.Add(x.Left); rights.Add(x.Right); });
                var dict = GetGivenPropsOfGivenElemQuery(propIds, lefts, rights).Result;

                return View("QueryResult", dict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Выбор диапазонов значений для выбранных свойств.
        /// </summary>
        /// <param name="propsList">Список выбранных свойств.</param>
        public IActionResult QueryProps(PropertyListViewModel propsList)
        {
            try
            {
                if (propsList == null)
                    return NotFound();

                List<string> units = new List<string>();
                var props = propertiesNames.Where(y => propsList.SelectedProps.Contains(y.Id)).ToList();
                List<QueryViewModel> queries = new List<QueryViewModel>();
                foreach (var item in props)
                {
                    queries.Add(new QueryViewModel(item.Id, item.Name));
                    units.Add(item.Unit?.Trim('(', ')', '[', ']'));
                }

                ViewBag.Units = units;
                return View("Query", new QueryListViewModel() { Queries = queries });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Экспортирование данных в формат CSV
        /// </summary>
        /// <returns>CSV файл для скачивания.</returns>
        public IActionResult Export()
        {
            try
            {
                if (!TempData.ContainsKey("table"))
                    return NotFound();

                PropertyTableViewModel table = JsonConvert.DeserializeObject<PropertyTableViewModel>(TempData["table"].ToString());

                if (table == null)
                    return NotFound();

                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(stream: ms, encoding: new UTF8Encoding(true)))
                    {
                        using (var cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        {
                            cw.WriteField("Element");
                            table.Properties.ForEach(x => cw.WriteField(x));
                            cw.NextRecord();
                            for (int i = 0; i < table.Elements.Count; i++)
                            {
                                cw.WriteField(table.Elements[i]);
                                table.Values[i].ForEach(x => cw.WriteField(x.ToString("0.##############", CultureInfo.InvariantCulture)));
                                cw.NextRecord();
                            }
                        }
                        return File(ms.ToArray(), "text/csv", $"props_{DateTime.Now.Ticks}.csv");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }




        /// <summary>
        /// Изменение настроек единиц измерения.
        /// </summary>
        /// <param name="newUnits">Новые единицы измерения.</param>
        private void UpdateUnits(List<string> newUnits)
        {
            Dictionary<string, Unit> units = HttpContext.Session.Get<Dictionary<string, Unit>>("units");
            if (units == default) // This won't happen...
            {
                var res = SetSession().Result;
                units = HttpContext.Session.Get<Dictionary<string, Unit>>("units");
            }

            foreach (string unit in newUnits)
            {
                var found = units.FirstOrDefault(x => x.Value.Unit2 == unit.Trim());
                if (/*unitPairs.ContainsKey(unit) ||*/ found.Value == null)
                    continue;

                found.Value.ChangeUnits();
            }

            HttpContext.Session.Set("units", units);
        }


        /// <summary>
        /// Получение библиотечной информации по заданной литературной ссылке.
        /// </summary>
        /// <param name="id">Id ссылки.</param>
        /// <returns>Строка с информацией по данному ресурсу.</returns>
        private async Task<string> GetLiteratureReference(string id)
        {
            string res = "No data";
            HttpResponseMessage response = await client.GetAsync($"api/properties/refs/{id}");
            if (response.IsSuccessStatusCode)
                res = await response.Content.ReadAsAsync<string>();
            return res;
        }


        /// <summary>
        /// Получение всех элементов с основыми свойствами.
        /// </summary>
        /// <returns>Список всех элементов таблицы Менделеева.</returns>
        private async Task<List<Element>> GetAllElements()
        {
            List<Element> elems = new List<Element>();
            HttpResponseMessage response = await client.GetAsync("api/properties/elems");
            if (response.IsSuccessStatusCode)
            {
                string stringDeserialized = await response.Content.ReadAsAsync<string>();
                elems = JsonConvert.DeserializeObject<List<Element>>(stringDeserialized,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            return elems;
        }


        /// <summary>
        /// Получение всех свойств с их Id, названием и типом.
        /// </summary>
        /// <returns>Список всех свойств в формате Id, Название, Тип.</returns>
        private async Task<List<Property>> GetAllPropertiesNames()  
        {
            List<Property> props = new List<Property>();
            HttpResponseMessage response = await client.GetAsync("api/properties/propsnames");
            if (response.IsSuccessStatusCode)
            {
                string stringDeserialized = await response.Content.ReadAsAsync<string>();
                props = JsonConvert.DeserializeObject<List<Property>>(stringDeserialized,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            return props;
        }


        /// <summary>
        /// Взаимодействие с API (Режим 1).
        /// </summary>
        private async Task<List<IProperty>> GetAllPropsOfGivenElem(int id)
        {
            return await GetPropsFromApi($"api/properties/allprops/{id}");
        }


        /// <summary>
        /// Взаимодействие с API (Подрежим 2).
        /// </summary>
        private async Task<List<IProperty>> GetGivenPropOfGivenElem(int elemId, string propId)
        {
            var url = new UriTemplate("api/properties/givenprops/{elemId},{propId}")
            .AddParameter("elemId", elemId).AddParameter("propId", propId).Resolve();

            return await GetPropsFromApi(url);
        }

        /// <summary>
        /// Взаимодействие с API (Режим 2).
        /// </summary>
        private async Task<List<IProperty>> GetGivenPropsOfGivenElem(int elemId, List<string> propsId)
        {
            var url = new UriTemplate($"api/properties/givenprops/{elemId}?" +
                $"{string.Join('&', propsId.Select(x => "propsId=" + x))}").Resolve();

            return await GetPropsFromApi(url);
        }

        
        /// <summary>
        /// Взаимодействие с API (Подрежим 3).
        /// </summary>
        private async Task<List<IProperty>> GetGivenPropValuesById(string propId)
        {
            return await GetPropValuesByPath(propId, "propsvalues");
        }


        /// <summary>
        /// Взаимодействие с API (Режим 3).
        /// </summary>
        private async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropsValues(List<string> propsId)
        {
            return await GetGivenPropsValuesByPath(propsId, "propsvalues");
        }


        /// <summary>
        /// Взаимодействие с API (Подрежим 4).
        /// </summary>
        private async Task<List<IProperty>> GetGivenPropOfGivenElemRec(string propId)
        {
            return await GetPropValuesByPath(propId, "recprops");
        }


        /// <summary>
        /// Взаимодействие с API (Режим 4).
        /// </summary>
        private async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropsOfGivenElemRec(List<string> propsId)
        {
            return await GetGivenPropsValuesByPath(propsId, "recprops");
        }


        /// <summary>
        /// Взаимодействие с API (Подрежим 5).
        /// </summary>
        private async Task<List<IProperty>> GetGivenPropOfGivenElemQuery(string propId, decimal left, decimal right)
        {
            var url = $"api/properties/queryprops/{propId}/{left.ToString().Replace(',', '.')}/{right.ToString().Replace(',', '.')}/";

            return await GetPropsFromApi(url);
        }


        /// <summary>
        /// Взаимодействие с API (Режим 5).
        /// </summary>
        private async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropsOfGivenElemQuery(
            List<string> propsId, List<decimal> lefts, List<decimal> rights)
        {
            Dictionary<string, IEnumerable<IProperty>> propsValues = new Dictionary<string, IEnumerable<IProperty>>();
            List<string> leftsStr = lefts.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList();
            List<string> rightsStr = rights.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToList();

            var url = new UriTemplate("api/properties/queryprops/{?propsId*,lefts*,rights*}")
                .AddParameters(new { propsId = propsId, lefts = leftsStr, rights = rightsStr }).Resolve();

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string stringDeserialized = await response.Content.ReadAsAsync<string>();
                propsValues = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<IProperty>>>(stringDeserialized,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                foreach (IEnumerable<IProperty> list in propsValues.Values)
                    ConvertPropertiesUnits(list.ToList());
            }
            return propsValues;
        }



        /// <summary>
        /// Вспомогательный метод для вызова API.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<List<IProperty>> GetPropsFromApi(string url)
        {
            List<IProperty> props = new List<IProperty>();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string stringDeserialized = await response.Content.ReadAsAsync<string>();
                props = JsonConvert.DeserializeObject<List<IProperty>>(stringDeserialized,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                ConvertPropertiesUnits(props);
            }
            return props;
        }

 
        /// <summary>
        /// Вспомогательный метод для подрежимов 3-4.
        /// </summary>
        private async Task<List<IProperty>> GetPropValuesByPath(string propId, string path)
        {
            var url = new UriTemplate($"api/properties/{path}/{{propId}}").AddParameter("propId", propId).Resolve();

            return await GetPropsFromApi(url);
        }


        /// <summary>
        /// Вспомогательный метод для режимов 3-4.
        /// </summary>
        private async Task<Dictionary<string, IEnumerable<IProperty>>> GetGivenPropsValuesByPath(List<string> propsId, string path)
        {
            Dictionary<string, IEnumerable<IProperty>> propsValues = new Dictionary<string, IEnumerable<IProperty>>();

            var url = new UriTemplate($"api/properties/{path}/{{?propsId*}}").AddParameter("propsId", propsId).Resolve();

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string stringDeserialized = await response.Content.ReadAsAsync<string>();
                propsValues = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<IProperty>>>(stringDeserialized,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                foreach (IEnumerable<IProperty> list in propsValues.Values)
                    ConvertPropertiesUnits(list.ToList());
            }
            return propsValues;
        }


        /// <summary>
        /// Вспомогательный метод конвертации величин свойств.
        /// </summary>
        private void ConvertPropertiesUnits(List<IProperty> props)
        {
            Dictionary<string, Unit> units = HttpContext.Session.Get<Dictionary<string, Unit>>("units");
            if (units == default)
                return;

            foreach (Property prop in props)
            {
                if (prop.Unit == null)
                    continue;

                var unit = units.GetValueOrDefault(prop.Unit);
                if (unit != null && unit.Unit2 == prop.Unit)
                {
                    prop.Value = unit.Convert(prop.Value);
                    prop.Unit = unit.Unit1;
                }
            }
        }
    }
}

#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
