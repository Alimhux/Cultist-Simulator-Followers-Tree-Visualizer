namespace FollowerProcessing
{
    /// <summary>
    /// Класс, реализующий базу данных последователей.
    /// </summary>
    public class DataBase
    {
        private Dictionary<string, Follower> _followers = new();

        /// <summary>
        /// Конструктор без параметрво для инициализации базы данных.
        /// </summary>
        public DataBase() {_followers = new(); }

        /// <summary>
        /// Возвращает словарь последователей.
        /// </summary>
        public Dictionary<string, Follower> Followers => _followers;
        
        /// <summary>
        /// Проверяет базу данных на наличие в ней последователей.
        /// </summary>
        public bool IsEmpty => _followers.Count == 0;
        
        /// <summary>
        /// Загружает данные о последователях.
        /// </summary>
        /// <param name="sourse">Источних данных</param>
        public void LoadData(int sourse)
        {
            _followers = DataHandler.GetFollowersInfo(sourse);
            Console.WriteLine("Данные были успешно считаны!");
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Вызывает методы для записи данных в консоль или файл, в зависимости от предпочтений пользователя
        /// </summary>
        /// <param name="way">Способ для вывода даных</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteData(int way)
        {

            if (!IsEmpty)
            {
                switch (way)
                {
                    case 1:
                        JsonParser.WriteJson(_followers);
                        Console.WriteLine("\nВсе данные выведены в консоль!");
                        Thread.Sleep(1000);
                        break;
                    case 2:
                        string pathTo = DataHandler.GetFileNameOrFilePathFromUser(false);
                        JsonParser.WriteJson(_followers, $"../../../../{pathTo}");
                        Console.WriteLine("Запись в файл успешно завершена! Он находится в папке с решением.");
                        Thread.Sleep(1000);
                        break;
                }
                throw new TimeoutException();
            }
            else
            {
                throw new ArgumentOutOfRangeException("Чтобы вывести данные, сначала их загрузить из консоль/файла");
            }            
        }


        /// <summary>
        /// Фильтрует данные по заданому полю и подстроке
        /// </summary>
        /// <param name="field">Поле по которому будет проводиться фильтрация</param>
        /// <param name="substring">Подстрока, которой должны соответстовавать поля в объектах</param>
        /// <exception cref="TimeoutException"></exception>
        public void FilterData(string field, string substring)
        {
            Console.WriteLine($"\nФильтрация по полю {field}:");

            Dictionary<string, Follower> filteredFollowers = new();

            foreach(string key in _followers.Keys)
            {
                if (_followers[key].GetField(field) != null && _followers[key].GetField(field)!.Contains(substring))
                {
                    filteredFollowers[key] = _followers[key];
                }
            }
            ShowLoadAnimation();
            if (filteredFollowers.Count == 0)
            {
                Console.WriteLine("К сожалению, ни одного удовлетворяющего объекта не найдено(");
            }
            else
            {
                Console.WriteLine("\nДанные успешно отфильтрованы! Теперь вы можете записать их в файл/вывести в консоль!\n");
                _followers = filteredFollowers;
            }
            throw new TimeoutException();           
        }
        

        /// <summary>
        /// Сортирует последователей в зависимости от выбранного направления сортировки.
        /// </summary>
        /// <param name="field">Поле по которому будет произведена сортировка</param>
        /// <param name="sortType">Направление сортировки</param>
        /// <exception cref="TimeoutException"></exception>
        public void SortData(string field, int sortType) 
        {
            Console.WriteLine($"Сортировка по полю {field}:");

            Dictionary<string, Follower> sortedFollowers = new();

            foreach (string key in _followers.Keys)
            {
                if (_followers[key].GetField(field) != null)
                {
                    sortedFollowers[key] = _followers[key];
                }
            }

            var items = sortedFollowers.ToList();

            switch (sortType) 
            {
                case 1:
                    items = items.OrderBy(item => item.Value.GetField(field)).ToList();
                    break;
                case 2:
                    items = items.OrderByDescending(item => item.Value.GetField(field)).ToList();
                    break;
                default:
                    throw new ArgumentException("Некорректный тип сортировки. Допустимые значения: 1 (по возрастанию) или 2 (по убыванию).");
            }

            sortedFollowers = items.ToDictionary(item => item.Key, item => item.Value);

            ShowLoadAnimation();
            if (sortedFollowers.Count == 0)
            {
                Console.WriteLine("К сожалению, ни одного удовлетворяющего объекта не найдено(");
            }
            else
            {
                Console.WriteLine("\nДанные успешно отсортированы! Теперь вы можете записать их в файл/вывести в консоль!\n");
                _followers = sortedFollowers;
            }
            throw new TimeoutException();
        }


        /// <summary>
        /// Получает список доступных полей для проведения операций над объектами.
        /// </summary>
        /// <returns>Список доступных полей для операций</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string[] GetOperationFields()
        {
            if (IsEmpty)
            {
                throw new ArgumentOutOfRangeException();
            }
            List<string> fields = [];
            foreach (string key in _followers.Keys)
            {
                foreach (string field in _followers[key].GetAllFields())
                {
                    if (!fields.Contains(field) && !(field == "xtriggers" || field == "aspects"))
                    {
                        fields.Add(field);
                    }
                }
            }
            return fields.ToArray();
        }

        /// <summary>
        /// Отрисовывывает загрузку, имитируя процесс выполнения операции.
        /// </summary>
        private void ShowLoadAnimation()
        {
            Console.Write("\nЗагрузка#");
            for (int i = 0; i < 4; i++)
            {
                Console.Write("#");
                Thread.Sleep(400);
            }
            Console.WriteLine();
        }

    }
}
