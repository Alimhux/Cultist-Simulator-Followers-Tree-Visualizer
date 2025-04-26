using System.Text;
namespace FollowerProcessing
{

    /// <summary>
    /// Основной класс для парсинга и записи данных в json-файлах
    /// </summary>
    public static class JsonParser
    {
        /// <summary>
        /// Перечесление состояний машины состояний
        /// </summary>
        private enum JsonStates
        {
            Start,
            ReadingArray,
            ReadingObject,
            ReadingHeader,
            ReadingValue,
            ReadingAspectsValue,
            End
        }

        /// <summary>
        /// Парсит данные из json-файла с путём <path>
        /// </summary>
        /// <param name="path">Путь к файлу для чтения данных</param>
        /// <returns>Список словарей представляющих json-объекты</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Dictionary<string, string>[] ReadJson(string path)
        {

            JsonStates state = JsonStates.Start;    
            TextReader general = Console.In;
            
            List<Dictionary<string, string>> contents = new List<Dictionary<string, string>>();
            int counter = 0;

            StringBuilder item = new StringBuilder();
            StringBuilder header = new StringBuilder();

            using(StreamReader reader = new StreamReader(path)) 
            {
                Console.SetIn(reader);
                bool isQuote = false;
                int read = Console.Read();
                while (read != -1)
                {
                    char letter = (char)read;
                    switch (letter)
                    {
                        case '[' when state == JsonStates.Start:
                            state = JsonStates.ReadingArray;
                            break;
                        case '{' when state == JsonStates.ReadingArray:
                            state = JsonStates.ReadingObject;
                            //Пытаемся добавить новый объект на текущий индекс, если не вызывается ошибка, значит
                            //предыдущий объект не был создан и мы продолжаем работать с этой же ячейкой массива.
                            try 
                            {
                                contents[counter] = new Dictionary<string, string>();
                            }
                            catch {
                                contents.Add(new Dictionary<string, string>());
                            }
                            break;
                        case '"' when state == JsonStates.ReadingObject:
                            state = JsonStates.ReadingHeader;
                            isQuote = !isQuote;
                            break;
                        case '"':
                            isQuote = !isQuote;
                            break;
                        case ':' when state == JsonStates.ReadingHeader:
                            state = JsonStates.ReadingValue;
                            break;
                        case '{' when state == JsonStates.ReadingValue:
                            state = JsonStates.ReadingAspectsValue;
                            break;
                        case '}' when state == JsonStates.ReadingAspectsValue:
                            state = JsonStates.ReadingObject;
                            item.Append(letter);
                            contents[counter].Add(header.ToString(), item.ToString());
                            header.Clear();
                            item.Clear();
                            break;
                        case ',' when state == JsonStates.ReadingValue && !isQuote:
                            state = JsonStates.ReadingObject;
                            contents[counter].Add(header.ToString(), item.ToString());
                            header.Clear();
                            item.Clear();
                            continue;
                        case '}' when state == JsonStates.ReadingValue:
                            contents[counter].Add(header.ToString(), item.ToString().Trim());
                            if (contents[counter].ContainsKey("id") && contents[counter]["id"] != "") //если объект имеет поле id то обновляем счётчик
                            {   
                                counter++;
                            }
                            header.Clear();
                            item.Clear();
                            state = JsonStates.ReadingArray;
                            break;
                        case '}' when state == JsonStates.ReadingObject:
                            state = JsonStates.ReadingArray;
                            if (contents[counter].ContainsKey("id")) 
                            {
                                counter++;
                            }
                            break;
                        case ']' when state == JsonStates.ReadingArray:
                            state = JsonStates.End;
                            break;
                        }

                        switch (state)
                        {
                            case JsonStates.ReadingHeader:
                                if (letter != '"' && isQuote)
                                {
                                    header.Append(letter);
                                }
                                break;
                            case JsonStates.ReadingAspectsValue:
                                item.Append(letter);
                                break;
                            case JsonStates.ReadingValue:
                                if (letter != ':' && letter != '"')
                                {
                                    if (!(item.Length == 0 && letter == ' '))
                                    {
                                        item.Append(letter);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        
                        read = Console.Read();
                }
                Console.SetIn(general);
                if (isQuote || state != JsonStates.End)
                {
                    throw new ArgumentException("Некорректная структура файла. Данные не сохранены.");
                }
                return contents.ToArray();
            }
        }

        /// <summary>
        /// Выводит в консоль данные об объектах последователей
        /// </summary>
        /// <param name="followers">Список Последователей</param>
        public static void WriteJson(Dictionary<string, Follower> followers)
        {   
            Console.WriteLine("{\r\n    \"elements\": [");
            try {
                foreach(string key in followers.Keys)
                {
                    if (key == followers.Keys.Last())
                    {
                        Console.WriteLine(followers[key].ToJson());
                        Console.WriteLine("    ]");
                    }
                    else
                    {
                        Console.WriteLine($"{followers[key].ToJson()},");
                    }
                }
            Console.Write("}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
            
        }   

        /// <summary>
        /// Выводит в файл с путём <pathTo> данные об объектах последователей
        /// </summary>
        /// <param name="followers">Список "Последователей"</param>
        /// <param name="pathTo">Путь к файлу</param>
        public static void WriteJson(Dictionary<string, Follower> followers, string pathTo)
        {
            TextWriter general = Console.Out;
            using (StreamWriter writer = new StreamWriter(pathTo))
            {
                Console.SetOut(writer);
                WriteJson(followers);
                Console.SetOut(general);
            }
        }


        /// <summary>
        /// Парсит Xtriggers, переводя их из строки в словарь (использует для этого уже готовый парсер ParseXtriggers)
        /// </summary>
        /// <param name="s">строка из объекта Json</param>
        /// <returns>Словарь aspects</returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, int> ParseAspects(string s)
        {
            Dictionary<string, string> xtriggers = ParseXtriggers(s);
            Dictionary<string, int> aspects = new Dictionary<string, int>();

            foreach (string key in xtriggers.Keys)
            {
                aspects[key] = int.Parse(xtriggers[key]);
            }

            return aspects;
        }

        /// <summary>
        /// Парсит Xtriggers, переводя их из строки в словарь 
        /// </summary>
        /// <param name="s">строка из объекта Json</param>
        /// <returns>Словарь xtriggers</returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, string> ParseXtriggers(string s)
        {
            Dictionary<string, string> xtriggers = new Dictionary<string, string>();

            JsonStates state = JsonStates.Start;

            StringBuilder value = new StringBuilder();
            StringBuilder header = new StringBuilder();

            bool isQuote = false;

            try
            {
                foreach (char letter in s)
                {
                    switch (letter)
                    {
                        case '\r': continue;
                        case '{' when state == JsonStates.Start:
                            state = JsonStates.ReadingHeader;
                            break;
                        case '"' when state == JsonStates.ReadingHeader:
                            if (isQuote)
                            {
                                state = JsonStates.ReadingValue;
                            }
                            isQuote = !isQuote;
                            break;
                        case ',' when state == JsonStates.ReadingValue:
                            state = JsonStates.ReadingHeader;
                            xtriggers[header.ToString().Trim('\n', ' ')] = value.ToString();
                            header.Clear();
                            value.Clear();
                            break;
                        case '}' when state == JsonStates.ReadingValue || state == JsonStates.ReadingHeader:
                            xtriggers[header.ToString().Trim('\n', ' ')] = value.ToString();
                            header.Clear();
                            value.Clear();
                            state = JsonStates.End;
                            break;
                        default:
                            break;
                    }

                    switch (state)
                    {
                        case JsonStates.ReadingHeader:
                            if (letter != '\n' && letter != '"' && letter != ':' && letter != ' ' && letter != '{' && letter != '}' && letter != ',')
                            {
                                header.Append(letter);
                            }
                            break;
                        case JsonStates.ReadingValue:
                            if (letter != '\n' && letter != ':' && letter != ' ' && letter != '"' && letter != ',')
                            {
                                value.Append(letter);
                            }
                            break;
                        case JsonStates.End:
                            break;
                        default:
                            break;
                    }

                }
            }
            catch (FormatException)
            {
                throw new Exception("Невеные данные в поле xtriggers или aspects");
            }
            return xtriggers;
        }
    }
}
