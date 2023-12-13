using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AplikacjaBankowaTest
{
    public class User
    {
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Login { get; set; }
        public byte[] HashedPassword { get; set; }
        public int KwotaPodstawowa { get; set; }

        public User(string imie, string nazwisko, string login, string haslo, int kwotaPodstawowa)
        {
            Imie = imie;
            Nazwisko = nazwisko;
            Login = login;
            HashedPassword = HashPassword(haslo);
            KwotaPodstawowa = kwotaPodstawowa;
        }

        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPassword(string password)
        {
            byte[] hashed = HashPassword(password);
            return HashedPassword.SequenceEqual(hashed);
        }
    }

    internal class Program
    {
        static List<User> users = new List<User>();
        static string linia = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Witam w aplikacji Bankowej!!!");
            Thread.Sleep(3000);
            Console.Clear();

            while (true)
            {
                int kwotaPodstawowa = 500;
                int userInput = DisplayMainMenu();

                if (userInput == 1)
                {
                    CreateUser(kwotaPodstawowa);
                }
                else if (userInput == 2)
                {
                    LoginUser();
                }
                else if (userInput == 3)
                {
                    AdminMenu();
                }
                else if (userInput == 4)
                {
                    break;
                }
            }
        }

        static int DisplayMainMenu()
        {
            Console.WriteLine("1. Stwórz konto");
            Console.WriteLine("2. Zaloguj się");
            Console.WriteLine("3. Konto administracji");
            Console.WriteLine("4. Zamknij aplikację\n");

            int userInput;
            if (!int.TryParse(Console.ReadLine(), out userInput) || userInput < 1 || userInput > 4)
            {
                Console.WriteLine("Wprowadzono nieprawidłową liczbę. Spróbuj ponownie.");
                Thread.Sleep(2000);
                Console.Clear();
                return DisplayMainMenu();
            }
            return userInput;
        }

        static void CreateUser(int kwotaPodstawowa)
        {
            Console.Clear();
            Console.WriteLine("STWÓRZ KONTO\n\n");
            Console.Write("Imię: ");
            string imie = Console.ReadLine();
            Console.Write("Nazwisko: ");
            string nazwisko = Console.ReadLine();
            Console.Write("Login: ");
            string login = Console.ReadLine();

            if (users.Any(user => user.Login == login))
            {
                Console.WriteLine("Login jest już zajęty. Wybierz inny.");
                Thread.Sleep(2000);
                Console.Clear();
                return;
            }

            Console.Write("Hasło: ");
            string haslo = Console.ReadLine();
            Console.Clear();

            User newUser = new User(imie, nazwisko, login, haslo, kwotaPodstawowa);
            users.Add(newUser);

            try
            {
                using (StreamWriter streamWriter = new StreamWriter("ZałożoneKonta.txt", true))
                {
                    streamWriter.WriteLine("Dane");
                    streamWriter.WriteLine("Imie: " + imie);
                    streamWriter.WriteLine("Nazwisko: " + nazwisko);
                    streamWriter.WriteLine("Login: " + login);
                    streamWriter.WriteLine("Hasło: " + Convert.ToBase64String(newUser.HashedPassword));
                    streamWriter.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wyjątek: " + ex.Message);
                Console.Clear();
            }

            Console.WriteLine("Super! Założyłeś konto");
            Console.WriteLine("W podzięce dostajesz od nas " + kwotaPodstawowa + " zł.");
            Thread.Sleep(5000);
            Console.Clear();
        }

        static void LoginUser()
        {
            Console.Clear();
            Console.Write("Podaj login: ");
            string login = Console.ReadLine();

            Console.Write("Podaj hasło: ");
            string haslo = Console.ReadLine();
            Console.Clear();

            User loggedInUser = users.FirstOrDefault(user => user.Login == login && user.VerifyPassword(haslo));

            if (loggedInUser != null)
            {
                UserLoggedInMenu(loggedInUser);
            }
            else
            {
                Console.WriteLine("Błędne dane logowania!!!");
                Thread.Sleep(1000);
                Console.Clear();
            }
        }

        static void UserLoggedInMenu(User loggedInUser)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Witamy w banku!!!\n");
                Console.WriteLine("1. Sprawdz stan konta");
                Console.WriteLine("2. Wpłata gotówki");
                Console.WriteLine("3. Wypłata gotówki");
                Console.WriteLine("4. Wyloguj się");

                int userInput3;
                if (!int.TryParse(Console.ReadLine(), out userInput3) || userInput3 < 1 || userInput3 > 4)
                {
                    Console.WriteLine("Wprowadzono nieprawidłową liczbę. Spróbuj ponownie.");
                    Thread.Sleep(2000);
                    Console.Clear();
                    continue;
                }

                if (userInput3 == 1)
                {
                    Console.Clear();
                    Console.WriteLine("Twój stan konta to: " + loggedInUser.KwotaPodstawowa + " zł.");
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (userInput3 == 2)
                {
                    DepositMoney(loggedInUser);
                }
                else if (userInput3 == 3)
                {
                    WithdrawMoney(loggedInUser);
                }
                else if (userInput3 == 4)
                {
                    Console.Clear();
                    break;
                }
            }
        }

        static void DepositMoney(User user)
        {
            Console.Clear();
            Console.Write("Jaką kwotę wpłacić: ");
            int kwotaDodatnia;
            if (!int.TryParse(Console.ReadLine(), out kwotaDodatnia) || kwotaDodatnia <= 0)
            {
                Console.WriteLine("Wprowadzono nieprawidłową kwotę. Spróbuj ponownie.");
                Thread.Sleep(2000);
                Console.Clear();
                return;
            }

            user.KwotaPodstawowa += kwotaDodatnia;
            Console.WriteLine("Wpłata udana!!!");
            Thread.Sleep(2000);
            Console.Clear();
        }

        static void WithdrawMoney(User user)
        {
            Console.Clear();
            Console.Write("Jaką kwotę wypłacić: ");
            int kwotaUjemna;
            if (!int.TryParse(Console.ReadLine(), out kwotaUjemna) || kwotaUjemna <= 0)
            {
                Console.WriteLine("Wprowadzono nieprawidłową kwotę. Spróbuj ponownie.");
                Thread.Sleep(2000);
                Console.Clear();
                return;
            }

            if (user.KwotaPodstawowa < kwotaUjemna)
            {
                Console.WriteLine("Brak wystarczających środków na koncie.");
                Thread.Sleep(2000);
                Console.Clear();
                return;
            }

            user.KwotaPodstawowa -= kwotaUjemna;
            Console.WriteLine("Wypłata udana!!!");
            Thread.Sleep(2000);
            Console.Clear();
        }

        static void AdminMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Konto administracji");
                Console.WriteLine("1. Wyświetl zapisanych użytkowników.");
                Console.WriteLine("2. Wyloguj się.");

                int userInput4;
                if (!int.TryParse(Console.ReadLine(), out userInput4) || userInput4 < 1 || userInput4 > 2)
                {
                    Console.Clear();
                    Console.WriteLine("Wprowadzono nieprawidłową liczbę. Spróbuj ponownie.");
                    Thread.Sleep(2000);
                    Console.Clear();
                    continue;
                }

                if (userInput4 == 1)
                {
                    DisplaySavedUsers();
                }
                else if (userInput4 == 2)
                {
                    Console.Clear();
                    break;
                }
            }
        }

        static void DisplaySavedUsers()
        {
            Console.Clear();
            try
            {
                using (StreamReader streamReader = new StreamReader("ZałożoneKonta.txt"))
                {
                    linia = streamReader.ReadLine();

                    while (linia != null)
                    {

                        Console.WriteLine(linia);
                        linia = streamReader.ReadLine();
                    }
                }
                Console.ReadLine();
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wyjątek: " + ex.Message);
                Console.Clear();
            }
        }
    }
}
