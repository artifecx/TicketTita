using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Exceptions
{
    public class TeamExceptions
    {
        public class TeamException : Exception
        {
            public string Id { get; }
            public TeamException(string message) : base(message) { }
            public TeamException(string message, string id) : base(message)
            {
                Id = id;
            }
        }

        public class TeamNameAlreadyExistsException : TeamException
        {
            public TeamNameAlreadyExistsException(string message) : base(message) { }
            public TeamNameAlreadyExistsException(string message, string id) : base(message) { }
        }

        public class TeamHasUnresolvedTicketsException : TeamException
        {
            public TeamHasUnresolvedTicketsException(string message) : base(message) { }
            public TeamHasUnresolvedTicketsException(string message, string id) : base(message) { }
        }

        public class TeamHasMembersException : TeamException
        {
            public TeamHasMembersException(string message) : base(message) { }
            public TeamHasMembersException(string message, string id) : base(message) { }
        }

        public class NoAgentSelectedException : TeamException
        {
            public NoAgentSelectedException(string message) : base(message) { }
            public NoAgentSelectedException(string message, string id) : base(message) { }
        }
    }
}
