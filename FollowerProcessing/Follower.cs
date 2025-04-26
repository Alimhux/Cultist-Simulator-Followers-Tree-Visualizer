using System.Text;
namespace FollowerProcessing
{   
    /// <summary>
    /// Интерфейс, который реализуют классы, связанные с json-объектами
    /// </summary>
    public interface IJSONObject
    {
        IEnumerable<string> GetAllFields();

        string? GetField(string fieldName);

        void SetField(string fieldName, string value);
    }

    /// <summary>
    /// Класс, Представляющий объект "Последователь".
    /// </summary>
    public struct Follower : IJSONObject
    {
        private Dictionary<string, string> _fields;

        private string? _id;
        private string? _label;
        private Dictionary<string, int>? _aspects;
        private string? _description;
        private Dictionary<string, string>? _xtriggers;
        private string? _uniquenessGroup;
        private string? _icon;
        private int _lifeTime;
        private string? _decayTo;
        private string? _comments;
        public Dictionary<string, string> XTriggers { get => _xtriggers == null ? new() :_xtriggers; }

        /// <summary>
        /// Конструктор без параметров.
        /// </summary>
        public Follower()
        {
            _fields = new();
            _id = null;
            _label = null;
            _aspects = null;
            _description = null;
            _xtriggers = null;
            _uniquenessGroup = null;
            _icon = null;
            _lifeTime = 0;
            _decayTo = null;
            _comments = null;
        }

        /// <summary>
        /// Конструктор с параметрами.
        /// </summary>
        /// <param name="fields">Словарь, представляющий json-объект</param>
        /// <exception cref="ArgumentException"></exception>
        public Follower(Dictionary<string, string> fields)
        {
            _fields = fields;
            
            string[] correctHeaders = { "id", "label", "aspects", "description", "xtriggers", "uniquenessgroup", "icon", "lifetime", "decayTo", "comments" };

            //Если в объекте есть некорректные поля, то не создаём объект.
            foreach (string header in fields.Keys)
            {
                if (!correctHeaders.Contains(header))
                {
                    throw new ArgumentException($"В файле есть объекты с некорректными полями ({header}), попробуйте исправить файл и попробовать снова.");
                }
            }
            _id = fields["id"];
            _label = fields.ContainsKey("label") ? fields["label"] : null;
            _description = fields.ContainsKey("description") ? fields["description"] : null;
            _uniquenessGroup = fields.ContainsKey("uniquenessgroup") ? fields["uniquenessgroup"] : null;
            _icon = fields.ContainsKey("icon") ? fields["icon"] : null;
            _lifeTime = fields.ContainsKey("lifetime") ? int.Parse(fields["lifetime"]) : 0;
            _decayTo = fields.ContainsKey("decayTo") ? fields["decayTo"] : null;
            _comments = fields.ContainsKey("comments") ? fields["comments"] : null;

            //Проверяем на наличие аспектов и триггеров и парсим их в строку, если они есть.
            try
            {
                if (fields.ContainsKey("aspects"))
                {
                    _aspects = JsonParser.ParseAspects(fields["aspects"]);
                }

                if (fields.ContainsKey("xtriggers"))
                {
                    _xtriggers = JsonParser.ParseXtriggers(fields["xtriggers"]);
                }
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Неверные данные в полях aspects или xtriggers");
            }
        }

        /// <summary>
        /// Возвращает список всех полей объекта.
        /// </summary>
        /// <returns>Список строк - полей объекта</returns>
        public IEnumerable<string> GetAllFields()
        {
            List<string> fields = new List<string>();   
            foreach (string key in _fields.Keys)
            {
                if (GetField(key) != null)
                {
                    fields.Add(key);
                }
            }
            return fields;

        }

        /// <summary>
        /// Позвращает в виде строки представление выбранного пользователем поля.
        /// </summary>
        /// <param name="fieldName">Поле которое хочет получить пользователь</param>
        /// <returns>Строку, представляющую поле</returns>
        /// <exception cref="FormatException"></exception>
        public string? GetField(string fieldName)
        {
            switch (fieldName.ToLower())
            {
                case "id": return _id;
                case "label": return _label;
                case "aspects": return _fields["aspects"];
                case "description": return _description;
                case "xtriggers": return _fields["xtriggers"];
                case "uniquenessgroup": return _uniquenessGroup;
                case "icon": return _icon;
                case "lifetime": return _lifeTime.ToString();
                case "decayto": return _decayTo;
                case "comments": return _comments;
                default: Console.WriteLine(fieldName + fieldName + fieldName);
                    throw new FormatException("Нет такого поля у объекта");
            };
        }

        /// <summary>
        /// Устанавливает желаемое пользователем значение в выбранное поле.
        /// </summary>
        /// <param name="fieldName">Поле для изменения</param>
        /// <param name="value">Значение, которое будет присвоено полю</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetField(string fieldName, string value)
        {
            switch (fieldName.ToLower())
            {
                case "id": _id = value; break;
                case "label": _label = value; break;
                case "aspects": _fields["aspects"] = value; break;
                case "description": _description = value; break;
                case "xtriggers": _fields["xtriggers"] = value; break;
                case "uniquenessgroup": _uniquenessGroup = value; break;
                case "icon": _icon = value; break;
                case "lifetime":
                    try
                    {
                        _lifeTime = int.Parse(value);
                        break;
                    }
                    catch { throw new ArgumentException("Неверные данные в Json-объекте"); }
                case "decayto": _decayTo = value; break;
                case "comments": _comments = value; break;
            };
        }

        /// <summary>
        /// Преобразует объект "Последователь" в строку, со структуой как у Json-файла
        /// </summary>
        /// <returns>Строка, представляющая Последователя в виде Json</returns>
        public string ToJson()
        {
            StringBuilder str = new StringBuilder("        {\n");
            foreach (string key in _fields.Keys) 
            {
                if (GetField(key) != null)
                {
                    if (key == "aspects" || key == "xtriggers" || key == "lifetime")
                    {
                        str.Append($"            \"{key}\": {GetField(key)}");
                    }
                    else
                    {
                        str.Append($"            \"{key}\": \"{GetField(key)}\"");
                    }
                    if (key != _fields.Keys.Last())
                    {
                        str.Append(",");
                    }
                    str.Append("\n");

                }
            }
            str.Append("        }");
            return str.ToString();
        }

        /// <summary>
        /// Получает путь к изображению по id Последователя.
        /// </summary>
        /// <param name="iconsFolderPath">Путь к директории с изображениями</param>
        /// <returns>Путь к изображению для текущего последователя или путь к дефолтной картинке если картики для последователя нет</returns>
        public string GetIconPath(string iconsFolderPath)
        {
            string iconFileName = _id + ".png";
            string fullIconPath = Path.Combine(iconsFolderPath, iconFileName);

            if (!File.Exists(fullIconPath))
            {
                return @"default.png";
            }
            return fullIconPath;
        }

    }
}
