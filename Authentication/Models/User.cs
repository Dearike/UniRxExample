using System.Collections.Generic;
using System.Linq;
using Planner.Utils;
using UniRx;

namespace Planner.Authentication.Models
{
    /*!
     \ingroup authentication
     \brief Модель пользователя
     
     Здесь содержится оснавная информация о пользователе
     */
    public class User
    {
        public readonly StringReactiveProperty Name;
        public readonly StringReactiveProperty Surname;
        
        /// <summary>
        /// Права пользователя
        /// </summary>
        public readonly ReactiveCollection<UserRight> Rights;
        
        public readonly string Email;
        public readonly int Id;

        public readonly Token Token;

        public struct Memento
        {
            public string Name;
            public string Surname;
            public int[] Rights;
            public string Email;
        }

        public User(string name, string surname, IEnumerable<int> rights, Token token, string email, int id)
        {
            Id = id;
            Name = new StringReactiveProperty(name);
            Surname = new StringReactiveProperty(surname);
            
            Rights = new ReactiveCollection<UserRight>();
            foreach (var rightNumber in rights)
            {
                var isConverted = EnumConverter.TryParse<UserRight>(rightNumber, out var right);
                if (isConverted)
                    Rights.Add(right);
            }

            Email = email; 
            
            Token = token;
        }

        public string GetFullName()
        {
            return $"{Name.Value} {Surname.Value}";
        }

        public bool HasRights(params UserRight[] userRights)
        {
            return userRights.All(userRight => Rights.Contains(userRight));
        }

        public Memento GetMemento()
        {
            return new Memento
            {
                Name = Name.Value,
                Surname = Surname.Value,
                Rights = Rights.Select(x => (int) x).ToArray()
            };
        }

        public void SetMemento(Memento memento)
        {
            Name.Value = memento.Name;
            Surname.Value = memento.Surname;
            
            Rights.Clear();
            foreach (var mementoRight in memento.Rights)
            {
                var isConverted = EnumConverter.TryParse<UserRight>(mementoRight, out var right);
                if (isConverted)
                    Rights.Add(right);
            }
        }
    }
}