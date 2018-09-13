using System;
namespace k8stodoapi.Models
{
    public class TodoItem
    {
        public TodoItem()
        {
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get;
            set;
        }

        public DateTime DueDate
        {
            get;
            set;
        }

        public DateTime CompletedDate
        {
            get;
            set;
        }
    }
}
