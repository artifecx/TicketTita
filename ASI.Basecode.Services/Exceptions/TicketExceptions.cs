using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Exceptions
{
    public class TicketExceptions
    {
        public class TicketException : Exception
        {
            public string Id { get; }
            public TicketException(string message) : base(message) { }
            public TicketException(string message, string id) : base(message)
            {
                Id = id;
            }
        }

        public class InvalidFileException : TicketException
        {
            public InvalidFileException(string message, string id) : base(message, id) { }
        }

        public class NoChangesException : TicketException
        {
            public NoChangesException(string message) : base(message) { }
            public NoChangesException(string message, string id) : base(message, id) { }
        }

        public class CannotReassignToSameAgentException : TicketException
        {
            public CannotReassignToSameAgentException(string message) : base(message) { }
            public CannotReassignToSameAgentException(string message, string id) : base(message, id) { }
        }

        public class DuplicateTicketException : TicketException
        {
            public DuplicateTicketException(string message) : base(message) { }
            public DuplicateTicketException(string message, string id) : base(message, id) { }
        }

        public class NotFoundException : TicketException
        {
            public NotFoundException(string message) : base(message) { }
            public NotFoundException(string message, string id) : base(message, id) { }
        }
    }
}
