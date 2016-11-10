using System;

namespace TestClient
{
    public class SelectItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public string Show
        {
            get { return String.Format("{0:dd.MM.yyyy HH:mm}: {1}", Date, Name); }
        }
    }
}