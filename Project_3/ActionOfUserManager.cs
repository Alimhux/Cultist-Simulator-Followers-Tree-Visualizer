using FollowerProcessing;
using TreeProcessing;

namespace Project_mod_3
{
    /// <summary>
    /// Статический класс, реализующий логику программы
    /// </summary>
    public static class ActionOfUserManager
    {
        /// <summary>
        /// Выполняет взаимодействие с пользователем и запускает основные методы.
        /// </summary>
        public static void InitiateMainOperations()
        {
            DataBase dataBase = new();
            while (true)
            {
                // Отображение меню и получение выбора пользователя
                int key = Menu.ProcessCurrentMenu("Выберите функцию", "",
                    $"{(dataBase.IsEmpty ? "Загрузить данные (консоль/файл)" : "Изменить данные")}",
                    "Отфильтровать данные",
                    "Отсортировать данные",
                    "Основная задача",
                    "Доп задача",
                    "Вывести/Записать данные в консоль/файл",
                    "Выход");
                try
                {
                    // Обработка выбора пункта меню.
                    switch (key)
                    {
                        case 1:
                            // Выбор источника данных (консоль или файл)
                            int inputSourse = Menu.ProcessCurrentMenu("Выберите источник для считывания данных",
                                  "Перемещайте курсор стрелочками Вверх/Вниз", "Из консоли", "Из файла");
                            dataBase.LoadData(inputSourse); break;
                        case 2:
                            // Фильтрация данных по выбранному полю
                            string[] filtrationFields = dataBase.GetOperationFields();
                            int filterField = Menu.ProcessCurrentMenu("Выберите поле, по которому будет проходить фильтрация:",
                                "Перемещайте курсор стрелочками Вверх/Вниз", filtrationFields);
                            string substring = GetStrFromUser("Введите строку, которую должно содержать поле " +
                                $"{filtrationFields[filterField - 1]} каждого из объектов: ",
                                "Строка некорректна. Пожалуйста, введите строку, содержащую только буквы.");
                            dataBase.FilterData(filtrationFields[filterField - 1], substring);
                            break;
                        case 3:
                            // Сортировка данных по выбранному полю и направлению
                            string[] sortingFields = dataBase.GetOperationFields();
                            int sortField = Menu.ProcessCurrentMenu("Выберите поле, по которому будет проходить сортировка:",
                                "Перемещайте курсор стрелочками Вверх/Вниз", sortingFields);
                            int sortType = Menu.ProcessCurrentMenu($"Выберите направление сортировки по полю {sortingFields[sortField - 1]}:",
                                "Перемещайте курсор стрелочками Вверх/Вниз",
                                "Сортировка по алфавиту", "Обратная сортировка по алфавиту");
                            dataBase.SortData(sortingFields[sortField - 1], sortType);
                            break;
                        case 4:
                            // Основная задача: отображение дерева преобразований
                            if (dataBase.IsEmpty)
                            {
                                throw new ArgumentOutOfRangeException();
                            }

                            try
                            {
                                string name = GetStrFromUser("Введите имя последователя: ", "Имя не может быть пустым, пожалуйста повторите ввод");
                                DataHandler.DrawPossibleTransformationTree(dataBase.Followers!, name, 0);
                            }
                            catch (KeyNotFoundException)
                            {
                                Console.WriteLine("Последователя с таким id нет, попробуйте снова");
                            }
                            throw new TimeoutException(); // Используется для возврата в меню
                        case 5:
                            // Дополнительная задача: визуализация дерева с изображениями
                            if (dataBase.IsEmpty)
                            {
                                throw new ArgumentOutOfRangeException();
                            }

                            string rootId = GetStrFromUser("Введите имя последователя: ", "Имя не может быть пустым, пожалуйста повторите ввод");
                            if (!dataBase.Followers.ContainsKey(rootId))
                            {
                                throw new KeyNotFoundException();
                            }

                            string iconsPath = GetDirectoryFromUser(); // Получение пути к папке с иконками
                            TreeVisualizer visualizer = new();
                            visualizer.VisualizeTree(dataBase.Followers, rootId, iconsPath);
                            Console.WriteLine("Визуализация сохранена в файл tree_output.png! (Он находится в папке с решением)");
                            throw new TimeoutException();
                        case 6:
                            // Вывод данных в консоль или файл
                            int outputSourse = Menu.ProcessCurrentMenu("Выберите способ для вывода данных",
                                  "Перемещайте курсор стрелочками Вверх/Вниз", "В консоль", "В файл");
                            dataBase.WriteData(outputSourse); break;
                        case 7: return; // Выход из программы
                    }
                }
                catch (TimeoutException)
                {
                    AwaitExit("Нажмите Enter, чтобы продолжить", ConsoleKey.Enter);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine("Cначала нужно загрузить данные из консоли/файла");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("Последователя с таким id нет, попробуйте снова");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (PathTooLongException)
                {
                    Console.WriteLine("Путь к файлу слишком длинный.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("Директорий не найден.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Доступ ");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Файл не найден.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Неверный формат данных.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (IOException)
                {
                    Console.WriteLine("Ошибка ввода-вывода. \nВозможно, файл, над которым производятся действия используется другим процессом.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("Недостаточно памяти.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
                catch (Exception)
                {
                    Console.WriteLine("Неизвестная ошибка.");
                    AwaitExit("Для продолжения нажмите Enter", ConsoleKey.Enter);
                }
            }
        }

        /// <summary>
        /// Получает путь к директории от пользователя.
        /// </summary>
        /// <returns>Путь к директории с изображениями</returns>
        public static string GetDirectoryFromUser()
        {
            Console.Write("Введите абсолютный путь к папке с изображениями:  ");
            string inputPath, fullPath;
            while (true)
            {
                inputPath = Console.ReadLine()!.Trim('"');

                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    Console.Write("Путь не может быть пустым, попробуйте снова:    ");
                }

                try
                {
                    // Преобразование в абсолютный путь
                    fullPath = Path.GetFullPath(inputPath);

                    if (!Directory.Exists(fullPath))
                    {
                        Console.Write($"Папка не найдена: {fullPath}.\nПожалуйста, повторите ввод:    ");
                        continue;
                    }

                    return fullPath;
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
                {
                    Console.Write("Некорректный формат пути, попробуйсте снова:    ");
                }
            }
        }

        /// <summary>
        /// Показывает анимацию завершения программы.
        /// </summary>
        public static void ShowEndMessage()
        {
            Console.WriteLine("Спасибо, что воспользовались программой, до встречи!");
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Получает строку от пользователя.
        /// </summary>
        /// <param name="mainText">Главный текст, поясняющий что конкретно надо вводить</param>
        /// <param name="errorText">Текст об ошибке, если строка пуста</param>
        /// <returns>Введённая строка</returns>
        private static string GetStrFromUser(string mainText, string errorText)
        {
            while (true)
            {
                Console.Write(mainText);
                string input = Console.ReadLine()!;
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                else
                {
                    Console.WriteLine(errorText);
                }
            }
        }

        /// <summary>
        /// Выводит на экран указанное сообщение и останавливает выполнение программы до момента нажатия запрашиваемой клавиши.
        /// </summary>
        /// <param name="text">Сообщение для вывода</param>
        /// <param name="key">Клавиша, которую нужно нажать для продолжения</param>
        private static void AwaitExit(string text, ConsoleKey key)
        {
            Console.WriteLine(text);
            while (Console.ReadKey().Key != key)
            {
                continue;
            }
        }
    }
}