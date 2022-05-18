using Microsoft.AspNetCore.Mvc;
using ElementsWebAPI.Interfaces;
using ElementsWebAPI.Entities;
using ElementsClassLibrary;
using Newtonsoft.Json;

namespace ElementsWebAPI.Controllers
{
    [ApiController]
    [Route("api/properties")]
    //[Produces("application/json")]
    public class PropertiesController : Controller
    {
        #region Глобальные переменные и конструктор

        /// <summary>
        /// Логгер для отлавливания ошибок сервера.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Репозиторий для работы с базой данных.
        /// </summary>
        private readonly IPropertyRepository _propertyRepository;

        /// <summary>
        /// Настройки сериализатора с автоматическим TypeNameHandling
        /// </summary>
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Настройки сериализатора с объектным TypeNameHandling
        /// </summary>
        private readonly JsonSerializerSettings objectSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
            Formatting = Formatting.Indented
        };


        public PropertiesController(ILogger<PropertiesController> logger, IPropertyRepository propertyRepository)
        {
            _logger = logger;
            _propertyRepository = propertyRepository;
        }

        #endregion



        #region Вспомогательные методы контроллера

        /// <summary>
        /// Вспомогательный метод для получения всех значений выбранного свойства с опцией рекомендуемых и диапозона.
        /// </summary>
        /// <param name="propId">Id свойства.</param>
        /// <param name="isRecomended">Флаг, показывающий, необходимо ли взять все значения или только рекомендуемые.</param>
        /// <param name="left">Левая граница диапозона (опционально).</param>
        /// <param name="right">Правая граница диапозона (опционально).</param>
        /// <returns>Список (в зависимости от флага) значений свойства.</returns>
        private async Task<IActionResult> GivenPropValuesByIdOrRecQuery(
            string propId, bool isRecomended, decimal? left = null, decimal? right = null)
        {
            try
            {
                var prop = await _propertyRepository.GetGivenPropertyValuesById(propId, isRecomended, left, right);
                var res = JsonConvert.SerializeObject(prop, objectSettings);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// Вспомогательный метод для получения всех значений выбранных из списка свойств с опцией рекомендуемых.
        /// </summary>
        /// <param name="propsId">Список Id выбранных свойств.</param>
        /// <param name="isRecomended">Флаг, показывающий, необходимо ли взять все значения или только рекомендуемые.</param>
        /// <returns>Словарь Id - List<Property> (в зависимости от флага).</returns>
        private async Task<IActionResult> GivenPropsValuesOrRecQuery(
            List<string> propsId, bool isRecomended)
        {
            try
            {
                if (propsId == null)
                    return BadRequest("Список Id свойств обязательно должен быть указан.");
                var dict = await _propertyRepository.GetGivenPropertiesValues(propsId, isRecomended);
                
                var res = JsonConvert.SerializeObject(dict, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                    Formatting = Formatting.Indented
                }).Replace("[[ElementsClassLibrary.Property, ElementsClassLibrary]]",
                "[[ElementsClassLibrary.IProperty, ElementsClassLibrary]]");

                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        #endregion



        #region Get-запросы на получение данных


        /// <summary>
        /// Получение библиотечной информации по заданной литературной ссылке.
        /// </summary>
        /// <param name="id">Id ссылки.</param>
        /// <returns>Строка с информацией по данному ресурсу.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("refs/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetLiteratureReference(string id)
        {
            try
            {
                string res = await _propertyRepository.GetLiteratureReference(id);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Получение единиц измерения.
        /// </summary>
        /// <returns>Два словаря - пары ЕИ1-ЕИ2 и пары Выбранная_ЕИ-Unit.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("units/")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUnits()
        {
            try
            {
                var pair = await _propertyRepository.GetUnitsAndPairs();
                var res = JsonConvert.SerializeObject(pair, settings);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Получение всех элементов с основыми свойствами.
        /// </summary>
        /// <returns>Список всех элементов таблицы Менделеева.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("elems/")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllElements()
        {
            try
            {
                var elems = await _propertyRepository.GetAllElements();
                var res = JsonConvert.SerializeObject(elems, settings);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }


        /// <summary>
        /// Получение всех свойств с их Id, названием и типом.
        /// </summary>
        /// <returns>Список всех свойств в формате Id, Название, Тип.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("propsnames/")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPropertiesNames()
        {
            try
            {
                var props = await _propertyRepository.GetAllPropertiesNames();
                var res = JsonConvert.SerializeObject(props, settings);
                return Ok(res);
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
        /// <returns>Список всех значений всех свойств выбранного элемента.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("allprops/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllPropsOfGivenElem(int id)
        {
            try
            {
                var props = await _propertyRepository.GetAllPropertiesOfElementById(id);
                var res = JsonConvert.SerializeObject(props, settings);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }




        /// <summary>
        /// Получение значения выбранного свойства выбранного элемента (Подрежим 2).
        /// </summary>
        /// <remarks>
        /// Важно: Возвращается список, так как для одного Id свойства могут быть разные значения. Например:
        /// Melting temperature (разные значения из разных источников) Radii Ionic ... (разные значения для разных зарядов)
        /// </remarks>
        /// <param name="elemId">Id элемента.</param>
        /// <param name="propId">Id свойства.</param>
        /// <returns>Список всех значений выбранного свойства для выбранного элемента.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("givenprops/{elemId},{propId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropOfGivenElem(int elemId, string propId)
        {
            try
            {
                var prop = await _propertyRepository.GetGivenPropertyOfElementByIds(elemId, propId);
                return Ok(JsonConvert.SerializeObject(prop, objectSettings));
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
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <returns>Список выбранных свойств выбранного элемента.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="400">Список Id свойств не указан.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("givenprops/{elemId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropsOfGivenElem(int elemId, [FromQuery] List<string> propsId)
        {
            try
            {
                if (propsId == null)
                    return BadRequest("Список Id свойств обязательно должен быть указан.");
                var props = await _propertyRepository.GetGivenPropertiesOfElementById(elemId, propsId);
                return Ok(JsonConvert.SerializeObject(props, settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }
        }




        /// <summary>
        /// Получение значений выбранного свойства для всех элементов таблицы Менделеева (Подрежим 3).
        /// </summary>
        /// <param name="propId">Id свойства</param>
        /// <returns>Список всех значений свойства.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("propsvalues/{propId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropValuesById(string propId)
        {
            return await GivenPropValuesByIdOrRecQuery(propId, false);
        }


        /// <summary>
        /// Получение значений выбранных из списка свойств для всех элементов таблицы Менделеева (Режим 3).
        /// </summary>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <returns>Словарь значений свойств.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="400">Список Id свойств не указан.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("propsvalues/")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
       
        public async Task<IActionResult> GetGivenPropsValues([FromQuery] List<string> propsId)
        {
            return await GivenPropsValuesOrRecQuery(propsId, false);
        }




        /// <summary>
        /// Получение рекомендуемых значений выбранного свойства для всех элементов таблицы Менделеева (Подрежим 4).
        /// </summary>
        /// <param name="propId">Id свойства.</param>
        /// <returns>Список рекомендуемых значений свойства.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("recprops/{propId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropOfGivenElemRec(string propId)
        {
            return await GivenPropValuesByIdOrRecQuery(propId, true);
        }

        /// <summary>
        /// Получение рекомендуемых значений выбранных из списка свойств для всех элементов таблицы Менделеева (Режим 4).
        /// </summary>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <returns>Словарь рекомендуемых значений.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="400">Список Id свойств не указан.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("recprops/")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropsOfGivenElemRec([FromQuery] List<string> propsId)
        {
            return await GivenPropsValuesOrRecQuery(propsId, true);
        }




        /// <summary>
        /// Получение значений выбранного свойства из заданного диапозона для всех элементов таблицы Менделеева (Подрежим 5).
        /// </summary>
        /// <param name="propId">Id свойства.</param>
        /// <param name="left">Левая граница диапозона (опционально).</param>
        /// <param name="right">Правая граница диапозона (опционально).</param>
        /// <returns>Список рекомендуемых значений свойства.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("queryprops/{propId}/{left}/{right}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropOfGivenElemQuery(string propId, decimal left, decimal right)
        {
            return await GivenPropValuesByIdOrRecQuery(propId, true, left, right);
        }

        /// <summary>
        /// Получение значений выбранных из списка свойств из заданного диапозона для всех элементов таблицы Менделеева (Режим 5).
        /// </summary>
        /// <param name="propsId">Список Id выбранных свойств</param>
        /// <param name="lefts">Левая граница диапозона (опционально).</param>
        /// <param name="rights">Правая граница диапозона (опционально).</param>
        /// <returns>Словарь рекомендуемых значений с учетом диапозона.</returns>
        /// <response code="200">Успешное выполнение.</response>
        /// <response code="400">Параметры списков для запроса заданы неверно.</response>
        /// <response code="500">Ошибка на стороне сервиса.</response>
        [HttpGet("queryprops")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGivenPropsOfGivenElemQuery(
            [FromQuery] List<string> propsId, [FromQuery] List<decimal> lefts, [FromQuery] List<decimal> rights)
        {
            try
            {
                if (propsId.Count != lefts.Count || lefts.Count != rights.Count)
                    return BadRequest("Длины списков Id свойств и границ диапозонов не равны.");
                var dict = await _propertyRepository.GetGivenPropertiesValuesWithQuery(propsId, true, lefts, rights);
                return Ok(JsonConvert.SerializeObject(dict, settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace);
            }

        }

        #endregion
    }
}
