using System.Text;
namespace Project_mod_3
{
    public class Program
    {
        private static void Main()
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.UTF8;
            while (true)
            {
                while (true)
                {
                    int key = Menu.ProcessCurrentMenu("Добро пожаловать!",
                        "Для смены позиции указателя используйте стрелочки (вверх/вниз). Для выбора пункта нажмите Enter",
                        "Старт", "Выйти");
                    switch (key)
                    {
                        case 1:
                            ActionOfUserManager.InitiateMainOperations();
                            break;
                        case 2:
                            ActionOfUserManager.ShowEndMessage();
                            return;
                    }
                }
            }

        }
    }
}
