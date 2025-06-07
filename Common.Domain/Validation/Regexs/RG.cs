using System.Text.RegularExpressions;

namespace Common.Domain.Validation.Regexs
{
    public static class RG
    {

        public static void validateNameSurname(string input)
        {
            if (input.Length > 15)
            {
                throw new ArgumentException("amsigrdze saxeli ar arsebobs");
            }
            Regex rg = new Regex("^[A-Za-z]{2,13}");
            if (!rg.IsMatch(input))
            {
                throw new ArgumentException($"saxeli an gvari ara swor formatshi{input}");

            }
        }

        public static void ValidateBirth(DateTime input)
        {
            if (input > DateTime.Now)
            {
                throw new ArgumentException("dabadebis taigi agemateba dgevandels");
            }
            if (input.Year < 1950)
            {
                throw new ArgumentException("yvavi xar? swori tarigi dawere");
            }
            if (input.Year > DateTime.Now.Year - 10)
            {
                throw new ArgumentException("minimum 10 wlis unda iyo");
            }
        }

        public static void validatePersonal(string personal)
        {
            if (!personal.All(io => char.IsDigit(io)) || personal.Length != 11)
            {
                throw new ArgumentException("piradi nomeri unda iyos sigrdzeshi 11");
            }
        }

        public static void validateEmail(string email)
        {
            Regex reg = new Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$");
            if (!reg.Match(email).Success)
            {
                throw new ArgumentException("meilie formati arasworia");
            }
        }

        public static void ValidatePhone(string phone)
        {
            Regex reg = new Regex("^5(?!([0-9])1+$)d{8}$");
            if (!reg.IsMatch(phone))
            {
                throw new ArgumentException("telefonis formati arasworia");
            }
        }

        public static void Validatepassword(string password)
        {
            Regex reg = new Regex("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$");
            if (!reg.IsMatch(password))
            {
                throw new ArgumentException("paroli ar aris sando, an swori formatis");
            }
        }

        public static void valdateUsername(string user)
        {
            Regex reg = new Regex("^[a-zA-Z0-9]+([._]?[a-zA-Z0-9]+)*$");

            if (!reg.IsMatch(user) || user.Length <= 6)
            {
                throw new ArgumentException("Username ar aris validuri");
            }

        }
    }
}
