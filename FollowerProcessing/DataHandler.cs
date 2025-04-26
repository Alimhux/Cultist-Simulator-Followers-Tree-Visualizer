namespace FollowerProcessing
{
    /// <summary>
    /// Класс для обработки данных, используемых программой.
    /// </summary>
    public static class DataHandler
    {
        /// <summary>
        /// Получает информацию о введённых Последователях из Json-файла
        /// </summary>
        /// <param name="sourse">Источник данных</param>
        /// <returns>Словарь Последователей, где ключ - id последователя, значение - сам последователь</returns>
        public static Dictionary<string, Follower> GetFollowersInfo(int sourse)
        {
            
            // Console.SetIn(Console.OpenStandardInput());
            switch (sourse)
            {
                case 1:
                    Console.WriteLine("Вставьте JSON, введите *** для окончания ввода и нажмите enter");
                    string s = "";
                    string str = "";
                    while (true) 
                    {
                        if (s == "***") {
                            break;
                        }
                        str += s + '\n';
                        s = Console.ReadLine()!;
                    }
                    File.WriteAllText("temp_output.json", str); // Записываем в файл
                    Dictionary<string, string>[] followersJsonConsole = JsonParser.ReadJson("temp_output.json");
                    return JsonDictToFollowerDict(followersJsonConsole);

                case 2:
                    string pathTo = GetFileNameOrFilePathFromUser();
                    Dictionary<string, string>[] followersJson = JsonParser.ReadJson(pathTo);
                    return JsonDictToFollowerDict(followersJson);

                default: return new Dictionary<string, Follower>();
            }
        }

        /// <summary>
        /// Рисует дерево возможных "продвижений" последователя,
        /// начиная с указанного пользователем
        /// </summary>
        /// <param name="followers"></param>
        /// <param name="currentId"></param>
        /// <param name="depth"></param>
        public static void DrawPossibleTransformationTree(Dictionary<string, Follower> followers, string currentId, int depth, string indent = "")
        {
            if (!followers.ContainsKey(currentId))
            {
                Console.WriteLine("Последователя с таким id нет в базе даных");
                return;
            }

            Follower curFol = followers[currentId];

            if (depth == 0)
            {
                // Выводим текущий уровень дерева
                Console.WriteLine("\nПоследователь: " + curFol.GetField("id") + " (" + curFol.GetField("label") + ")");
            }
            
            // Рекурсивно обрабатываем все аспекты текущего последователя
            foreach (var aspect in curFol.XTriggers)
            {
                bool islast = aspect.Key == curFol.XTriggers.Keys.Last();

                string prefix;
                if ((depth == 0 && curFol.XTriggers.Count == 1) || islast)
                {
                    prefix = "└──";
                }
                else
                {
                    prefix = "├──";
                }

                Console.WriteLine(indent + prefix + aspect.Key + " --> " + aspect.Value +

                    (followers.ContainsKey(aspect.Value) ? " (" + followers[aspect.Value].GetField("label") + ")" : ""));

                string curIndent = islast ? "    " : "│    ";

                // Если аспект указывает на другого последователя, рекурсивно вызываем DrawPossibleTransformationTree
                if (followers.ContainsKey(aspect.Value))
                {
                    DrawPossibleTransformationTree(followers, aspect.Value, depth + 1, indent + curIndent);
                }
            }

        }



        /// <summary>
        /// Получает путь к файлу от пользователя
        /// </summary>
        /// <param name="toRead">Является ли файл входным или выходным</param>
        /// <returns>Имя файла/Путь к файлу, в зависимости </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static string GetFileNameOrFilePathFromUser(bool toRead = true)
        {

            Console.Write($"Введите {(toRead == true ? "путь к файлу для ввода " : "имя файла для вывода ")}");
            string path;
            while (true)
            {
                try
                {
                    path = Console.ReadLine()!.Trim('"');

                    if (string.IsNullOrWhiteSpace(path))
                        throw new ArgumentException("Путь к файлу не может быть пустым или состоять только из пробелов.");

                    if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                        throw new ArgumentException("Путь к файлу содержит недопустимые символы.");

                    if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != ".json")
                        throw new ArgumentException("Файл должен иметь расширение .json.");

                    if (toRead == true && !File.Exists(path))
                    {
                        throw new FileNotFoundException("Исходный файл не найден.", path);
                    }
                    return path;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.Write("Введите путь к файлу ещё раз:   ");
                }
            }
        }

        /// <summary>
        /// Преобразовывает массив словарей с полями объектов в словарь "Последователей"
        /// </summary>
        /// <param name="dict">Массив словарей типа <string, string> с объектами Json-файла</param>
        /// <returns>Словарь последователйе</returns>
        public static Dictionary<string, Follower> JsonDictToFollowerDict(Dictionary<string, string>[] dict)
        {
            Dictionary<string, Follower> followers = new Dictionary<string, Follower>();
            foreach (Dictionary<string, string> obj in dict)
            {
                Follower fol = new Follower(obj);
                followers.Add(fol.GetField("id")!, fol);
            }
            return followers;
        }
    }
}

